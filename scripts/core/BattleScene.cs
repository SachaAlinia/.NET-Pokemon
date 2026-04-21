using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic; // Important pour List<>
using System.Threading.Tasks;

public partial class BattleScene : Node2D
{
	[ExportCategory("Data")]
	[Export] public PokemonResource PlayerPokemon;
	[Export] public PokemonResource EnemyPokemon;
	[Export] public int PlayerLevel = 5;
	[Export] public int EnemyLevel = 5;
	[Export] public Array<MoveResource> PlayerMoves = new();
	[Export] public Array<MoveResource> EnemyMoves = new();

	private Vector2 _playerStartPos;
	private Vector2 _enemyStartPos;

	[ExportCategory("External Managers")]
	[Export] private BattleUI _battleUI;

	private BattlePokemon _playerBattle, _enemyBattle;
	private bool _isBattleOver = false;
	private bool _battleWon = false;
	private PlayerInventory _playerInventory = new();

	[ExportCategory("Visuals")]
	[Export] private Sprite2D _playerSprite, _enemySprite;

	public override void _Ready()
	{
		if (_battleUI == null)
		{
			GD.PrintErr("ERREUR: BattleUI n'est pas assigné dans l'inspecteur !");
			return;
		}

		// Abonnement aux événements de l'UI
		_battleUI.OnMoveClicked += OnMoveSelected;
		_battleUI.OnActionAttackPressed += () => _battleUI.ToggleMoveMenu(true);
		_battleUI.OnActionItemPressed += () => _battleUI.ToggleItemsMenu(true);
		_battleUI.OnActionFleePressed += OnFleePressed;
		_battleUI.OnActionPokemonPressed += OnPokemonMenuPressed;
		_battleUI.OnReturnPressed += () => _battleUI.ToggleMoveMenu(false);
		_battleUI.OnBattleEndContinue += OnBattleEnd;

		SetupBattle();
	}

	private void SetupBattle()
	{
		if (_playerSprite != null) _playerStartPos = _playerSprite.Position;
		if (_enemySprite != null) _enemyStartPos = _enemySprite.Position;

		// 1. Initialisation des instances de combat
		_playerBattle = new BattlePokemon(PlayerPokemon, PlayerLevel, PlayerMoves);
		_enemyBattle = new BattlePokemon(EnemyPokemon, EnemyLevel, EnemyMoves);

		// 2. Mise à jour visuelle des Sprites
		if (_playerSprite != null) _playerSprite.Texture = PlayerPokemon.BackSprite;
		if (_enemySprite != null) _enemySprite.Texture = EnemyPokemon.FrontSprite;

		// 3. Envoi des infos à l'UI
		_battleUI.SetupPokemonInfo(true, PlayerPokemon.Name, PlayerLevel);
		_battleUI.SetupPokemonInfo(false, EnemyPokemon.Name, EnemyLevel);

		UpdateHealthBars();

		// 4. CHARGEMENT DES ATTAQUES (Le point critique)
		GD.Print($"BattleScene: {PlayerPokemon.Name} possède {_playerBattle.Moves.Count} attaques.");
		_battleUI.RefreshMoves(_playerBattle.Moves);

		_battleUI.ShowDialogue($"Un {EnemyPokemon.Name} sauvage apparaît !");
	}

	private void UpdateHealthBars()
	{
		_battleUI.UpdateHealthBar(true, _playerBattle.CurrentHP, _playerBattle.MaxHP);
		_battleUI.UpdateHealthBar(false, _enemyBattle.CurrentHP, _enemyBattle.MaxHP);
	}

	private async void OnMoveSelected(int moveIndex)
	{
		if (_isBattleOver) return;

		var moveWithPP = _playerBattle.Moves[moveIndex];

		if (!moveWithPP.CanUse)
		{
			_battleUI.ShowDialogue("Cette attaque n'a plus de PP !", true);
			return;
		}

		_battleUI.ToggleMoveMenu(false);
		moveWithPP.UsePP();

		bool playerFirst = _playerBattle.Speed >= _enemyBattle.Speed;

		if (playerFirst)
		{
			await ExecuteTurn(_playerBattle, _enemyBattle, moveWithPP.Move, _enemySprite, true);
			if (!_isBattleOver) await EnemyAiTurn();
		}
		else
		{
			await EnemyAiTurn();
			if (!_isBattleOver) await ExecuteTurn(_playerBattle, _enemyBattle, moveWithPP.Move, _enemySprite, true);
		}

		if (!_isBattleOver)
		{
			UpdateHealthBars();
			_battleUI.RefreshMoves(_playerBattle.Moves);
			_battleUI.ShowActionMenu(true);
		}
	}

	private async Task EnemyAiTurn()
	{
		var availableMoves = _enemyBattle.Moves.FindAll(m => m.CanUse);

		if (availableMoves.Count == 0)
		{
			_battleUI.ShowDialogue($"{_enemyBattle.Resource.Name} n'a plus d'attaques !", true);
			return;
		}

		var selectedMove = availableMoves[(int)(GD.Randi() % availableMoves.Count)];
		selectedMove.UsePP();

		await ExecuteTurn(_enemyBattle, _playerBattle, selectedMove.Move, _playerSprite, false);
	}

	private async Task ExecuteTurn(BattlePokemon attacker, BattlePokemon target, MoveResource move, Sprite2D targetSprite, bool isAttackerPlayer)
	{
		// Animation d'attaque
		await AnimateAttacker(isAttackerPlayer, false);

		_battleUI.ShowDialogue($"{attacker.Resource.Name} utilise {move.Name.ToUpper()} !", true);
		await Task.Delay(600);

		// Calcul et application des dégâts
		var damageResult = DamageCalculator.CalculateDamage(attacker, target, move);

		if (damageResult.IsMiss)
		{
			_battleUI.ShowDialogue("L'attaque a échoué !", true);
		}
		else
		{
			target.TakeDamage(damageResult.Damage);

			// Feedback visuel (Floating text + Shake)
			if (damageResult.Damage > 0)
				FloatingText.Create(targetSprite.GetParent(), targetSprite.GlobalPosition, damageResult.Damage.ToString(), Colors.Red);

			string effectMsg = DamageCalculator.GetEffectivenessMessage(damageResult.Effectiveness);
			if (!string.IsNullOrEmpty(effectMsg)) _battleUI.ShowDialogue(effectMsg, true);
			if (damageResult.IsCritical) _battleUI.ShowDialogue("Coup critique !", true);

			ApplyShake(targetSprite);
			await Task.Delay(500);
		}

		// Retour à la position initiale
		await AnimateAttacker(isAttackerPlayer, true);
		UpdateHealthBars();

		// --- VÉRIFICATION DE LA MORT (CORRIGÉE) ---
		if (target.IsFainted)
		{
			_isBattleOver = true;
			// Si la cible est morte et que l'attaquant était le joueur -> VICTOIRE
			_battleWon = isAttackerPlayer;

			CheckDeath(target);
			return; // On arrête le tour ici
		}

		// Au cas où l'attaquant meurt (ex: futur poison ou recul)
		if (attacker.IsFainted)
		{
			_isBattleOver = true;
			_battleWon = !isAttackerPlayer;
			CheckDeath(attacker);
			return;
		}

		await Task.Delay(800);
	}

	private bool CheckDeath(BattlePokemon pokemon)
	{
		if (pokemon.IsFainted)
		{
			_battleUI.ShowDialogue($"{pokemon.Resource.Name} est K.O. !", true);
			_battleUI.ShowBattleEndScreen(_battleWon);
			return true;
		}
		return false;
	}

	private async Task AnimateAttacker(bool isPlayer, bool retreat)
	{
		var sprite = isPlayer ? _playerSprite : _enemySprite;
		if (sprite == null) return;

		Tween tween = GetTree().CreateTween();

		// On définit la direction de l'impact
		Vector2 direction = isPlayer ? Vector2.Right : Vector2.Left;
		Vector2 originalPos = isPlayer ? new Vector2(208, 416) : new Vector2(816, 176);
		// Note : Idéalement, utilise sprite.Position au tout début du combat pour stocker ces valeurs.

		if (!retreat)
		{
			// PHASE 1 : L'attaque (Avancer rapidement)
			Vector2 targetPos = sprite.Position + direction * 50;
			tween.TweenProperty(sprite, "position", targetPos, 0.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		}
		else
		{
			// PHASE 2 : Le retour (Revenir à la base)
			// Ici on ne fait pas de calcul relatif, on donne la position FIXE d'origine
			// Pour être sûr, on peut utiliser une variable privée _playerStartPos définie dans SetupBattle
			Vector2 basePos = isPlayer ? _playerStartPos : _enemyStartPos;
			tween.TweenProperty(sprite, "position", basePos, 0.2f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		}

		await ToSignal(tween, "finished");
	}

	private void ApplyShake(Sprite2D target)
	{
		if (target == null) return;
		Tween tween = GetTree().CreateTween();
		Vector2 originalPos = target.Position;
		tween.TweenProperty(target, "position", originalPos + new Vector2(8, 0), 0.05f);
		tween.TweenProperty(target, "position", originalPos - new Vector2(8, 0), 0.05f);
		tween.TweenProperty(target, "position", originalPos, 0.05f);
	}

	private async void OnFleePressed()
	{
		_battleUI.ShowDialogue("Tentative de fuite...", true);
		_battleUI.ShowActionMenu(false);
		await Task.Delay(600);

		// Chance de fuite 
		if (GD.Randf() > 0.3f)
		{
			_battleUI.ShowDialogue("Vous avez réussi à fuir !", true);
			await Task.Delay(800);
			OnBattleEnd(); // <--- ICI on quitte la scène
		}
		else
		{
			_battleUI.ShowDialogue("Echec de la fuite ! L'ennemi attaque !", true);
			await Task.Delay(800);
			await EnemyAiTurn();
			if (!_isBattleOver) _battleUI.ShowActionMenu(true);
		}
	}

	private void OnPokemonMenuPressed() => _battleUI.ShowDialogue("Switch non implémenté !", true);

	private async void OnBattleEnd()
	{
		GD.Print("Fin du combat. Transition et remise en route de la musique de la ville...");

		// 1. TRANSITION (On essaie de récupérer l'AnimationPlayer du SceneManager)
		// On utilise SceneManager.Instance si c'est un Singleton, sinon on le cherche.
		var sceneManager = GetTree().Root.FindChild("SceneManager", true, false);
		var animPlayer = sceneManager?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

		if (animPlayer != null)
		{
			// On lance le fondu au noir
			// IMPORTANT : Vérifie dans ton AnimationPlayer si le nom est bien "FadeToBlack"
			animPlayer.Play("FadeToBlack");
			await ToSignal(animPlayer, "animation_finished");
		}

		// 2. RÉAFFICHAGE DU MONDE
		var currentLevel = SceneManager.GetCurrentLevel();
		if (currentLevel != null)
		{
			currentLevel.Show();

			// 3. REMISE EN ROUTE DE LA MUSIQUE (Via ton switch)
			// On récupère le nom du level actuel pour relancer la bonne musique
			string levelName = currentLevel.Name; // Ou SceneManager.GetCurrentLevelName()
			RestartWorldMusic(levelName);
		}

		// 4. RÉACTIVATION DU JOUEUR
		var player = GameManager.GetPlayer();
		if (player != null)
		{
			player.Show();
			player.SetProcess(true);
			player.SetPhysicsProcess(true);
			player.GetNodeOrNull<Camera2D>("Camera2D")?.MakeCurrent();
		}

		// 5. FIN DE TRANSITION (Ouverture)
		if (animPlayer != null)
		{
			animPlayer.Play("FadeFromBlack");
		}

		// 6. DESTRUCTION DE LA SCÈNE DE COMBAT
		QueueFree();
	}

	// Petite fonction utilitaire pour relancer la musique selon ton switch
	private void RestartWorldMusic(string levelName)
	{
		// On récupère ton MusicPlayer global
		var musicPlayerNode = GetTree().Root.FindChild("MusicPlayer", true, false);

		// On suppose que ton MusicPlayer a une fonction "PlayMusic" comme tu l'as montré
		// Si musicPlayerNode est ton script MusicPlayer :
		if (musicPlayerNode is MusicPlayer mp)
		{
			// On adapte le nom au format de ton Enum ou de tes cases
			if (levelName.ToLower().Contains("small_town"))
			{
				mp.PlayMusic("res://assets/audio/music/music1.mp3", -22.0f);
			}
			// Ajoute les autres cas ici si besoin
		}
	}
}