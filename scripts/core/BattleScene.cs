// Fichier : BattleScene.cs
// Rôle : Gère la logique d'un combat Pokémon (initialisation, tour des attaques, UI, animations, fin du combat).
// Remarques générales :
// - C'est un script Godot (hérite de Node2D) ; il est attaché à une scène qui représente l'arène de combat.
// - Beaucoup de fonctions sont asynchrones (async/await) pour gérer des délais et animations sans bloquer l'interface.

using Game.Core; // Utilise des éléments du cœur du jeu (ex: SceneManager, GameManager).
using Game.Gameplay; // Types liés au gameplay (ex: BattlePokemon, MoveResource, DamageCalculator).
using Godot; // API Godot (Node2D, Sprite2D, Tween, etc.).
using Godot.Collections; // Fournit la classe Array spécifique de Godot utilisée ici.
using System; // Espace de noms système de base.
using System.Collections.Generic; // Important pour List<>
using System.Threading.Tasks; // Permet l'utilisation de Task et await (attente asynchrone).

public partial class BattleScene : Node2D
{
	// --------------------------
	// Données exportées (configurables dans l'éditeur Godot)
	// --------------------------
	[ExportCategory("Data")]
	[Export] public PokemonResource PlayerPokemon; // Ressource du Pokémon du joueur (données statiques : sprites, nom, stats de base).
	[Export] public PokemonResource EnemyPokemon; // Ressource du Pokémon ennemi.
	[Export] public int PlayerLevel = 5; // Niveau du Pokémon du joueur (modifiable dans l'inspecteur).
	[Export] public int EnemyLevel = 5; // Niveau du Pokémon ennemi.
	[Export] public Array<MoveResource> PlayerMoves = new(); // Liste des attaques disponibles pour le joueur (respects de Godot.Array).
	[Export] public Array<MoveResource> EnemyMoves = new(); // Liste des attaques disponibles pour l'ennemi.

	// Positions de départ des sprites (stockées pour les animations de retour)
	private Vector2 _playerStartPos;
	private Vector2 _enemyStartPos;

	[ExportCategory("External Managers")]
	[Export] private BattleUI _battleUI; // Référence à l'interface utilisateur du combat (va recevoir les callbacks, afficher dialogues, barres, etc).

	// Objets logic liés au combat (instances)
	private BattlePokemon _playerBattle, _enemyBattle; // Contiennent les PV actuels, attaques (avec PP), stats calculées à partir des ressources.
	private bool _isBattleOver = false; // Indique si le combat est terminé (pour bloquer les actions).
	private bool _battleWon = false; // Indique si le joueur a gagné (utilisé lors de l'écran de fin).
	private PlayerInventory _playerInventory = new(); // Référence à l'inventaire du joueur (peut être utilisé pour objets).

	[ExportCategory("Visuals")]
	[Export] private Sprite2D _playerSprite, _enemySprite; // Sprites affichant les Pokémon (front/back).

	// Méthode Godot appelée lorsque le nœud est prêt (équivalent à "Start" ou constructeur d'instance visible).
	public override void _Ready()
	{
		// Vérification basique : le UI doit être connecté dans l'éditeur pour que le script fonctionne correctement.
		if (_battleUI == null)
		{
			GD.PrintErr("ERREUR: BattleUI n'est pas assigné dans l'inspecteur !");
			return; // On stoppe l'initialisation pour éviter des NullReferenceException plus loin.
		}

		// Abonnement aux événements de l'UI (les lambdas et méthodes seront appelées quand l'utilisateur interagit).
		_battleUI.OnMoveClicked += OnMoveSelected;
		_battleUI.OnActionAttackPressed += () => _battleUI.ToggleMoveMenu(true); // Affiche le menu des attaques
		_battleUI.OnActionItemPressed += () => _battleUI.ToggleItemsMenu(true); // Affiche le menu d'objets
		_battleUI.OnActionFleePressed += OnFleePressed; // Tentative de fuite
		_battleUI.OnActionPokemonPressed += OnPokemonMenuPressed; // Switch de Pokémon (non implémenté ici)
		_battleUI.OnReturnPressed += () => _battleUI.ToggleMoveMenu(false); // Retour depuis menu d'attaques
		_battleUI.OnBattleEndContinue += OnBattleEnd; // Quand l'utilisateur continue après écran de fin

		SetupBattle(); // Configure le combat (instancie les BattlePokemon, met à jour UI, etc).
	}

	// Configure toute la scène de combat : positions, instances logiques, sprites, UI, etc.
	private void SetupBattle()
	{
		// Sauvegarde la position initiale des sprites pour pouvoir les ramener après une animation.
		if (_playerSprite != null) _playerStartPos = _playerSprite.Position;
		if (_enemySprite != null) _enemyStartPos = _enemySprite.Position;

		// 1. Initialisation des instances de combat (objets avec logique: PV, stats, PP...).
		_playerBattle = new BattlePokemon(PlayerPokemon, PlayerLevel, PlayerMoves);
		_enemyBattle = new BattlePokemon(EnemyPokemon, EnemyLevel, EnemyMoves);

		// 2. Mise à jour visuelle des Sprites avec les textures issues des ressources Pokémon (dos pour joueur, face pour ennemi).
		if (_playerSprite != null) _playerSprite.Texture = PlayerPokemon.BackSprite;
		if (_enemySprite != null) _enemySprite.Texture = EnemyPokemon.FrontSprite;

		// 3. Envoi des infos à l'UI pour afficher noms / niveaux / barres de PV.
		_battleUI.SetupPokemonInfo(true, PlayerPokemon.Name, PlayerLevel);
		_battleUI.SetupPokemonInfo(false, EnemyPokemon.Name, EnemyLevel);

		UpdateHealthBars(); // Met à jour les barres de vie affichées.

		// 4. CHARGEMENT DES ATTAQUES (Le point critique)
		// On log à des fins de debug combien d'attaques le Pokémon possède.
		GD.Print($"BattleScene: {PlayerPokemon.Name} possède {_playerBattle.Moves.Count} attaques.");
		_battleUI.RefreshMoves(_playerBattle.Moves); // Envoie la liste d'attaques au UI pour affichage des boutons.

		_battleUI.ShowDialogue($"Un {EnemyPokemon.Name} sauvage apparaît !"); // Message initial.
	}

	// Met à jour les barres de PV dans l'UI (pour joueur et ennemi).
	private void UpdateHealthBars()
	{
		_battleUI.UpdateHealthBar(true, _playerBattle.CurrentHP, _playerBattle.MaxHP);
		_battleUI.UpdateHealthBar(false, _enemyBattle.CurrentHP, _enemyBattle.MaxHP);
	}

	// Handler quand le joueur choisit une attaque depuis l'UI.
	private async void OnMoveSelected(int moveIndex)
	{
		if (_isBattleOver) return; // Si le combat est terminé, ignorer l'action.

		var moveWithPP = _playerBattle.Moves[moveIndex]; // Récupère l'attaque choisie (objet contenant MoveResource + PP).

		if (!moveWithPP.CanUse)
		{
			_battleUI.ShowDialogue("Cette attaque n'a plus de PP !", true);
			return; // On arrête si la PP est à 0.
		}

		_battleUI.ToggleMoveMenu(false); // Cache le menu des attaques pour montrer l'action.
		moveWithPP.UsePP(); // Décrémente la PP de l'attaque choisie.

		// Détermine qui agit en premier selon la vitesse.
		bool playerFirst = _playerBattle.Speed >= _enemyBattle.Speed;

		if (playerFirst)
		{
			// Le joueur agit d'abord : on exécute son tour puis, si le combat continue, l'IA ennemie.
			await ExecuteTurn(_playerBattle, _enemyBattle, moveWithPP.Move, _enemySprite, true);
			if (!_isBattleOver) await EnemyAiTurn();
		}
		else
		{
			// L'ennemi agit d'abord
			await EnemyAiTurn();
			if (!_isBattleOver) await ExecuteTurn(_playerBattle, _enemyBattle, moveWithPP.Move, _enemySprite, true);
		}

		// Si le combat continue après ces actions, on met à jour l'UI et réaffiche le menu d'actions.
		if (!_isBattleOver)
		{
			UpdateHealthBars();
			_battleUI.RefreshMoves(_playerBattle.Moves); // Met à jour les PP visibles.
			_battleUI.ShowActionMenu(true);
		}
	}

	// Tour de l'IA ennemie : choisit une attaque disponible aléatoirement et l'utilise.
	private async Task EnemyAiTurn()
	{
		var availableMoves = _enemyBattle.Moves.FindAll(m => m.CanUse); // Filtre les attaques avec PP > 0.

		if (availableMoves.Count == 0)
		{
			_battleUI.ShowDialogue($"{_enemyBattle.Resource.Name} n'a plus d'attaques !", true);
			return;
		}

		// Choix aléatoire d'une attaque parmi celles disponibles.
		var selectedMove = availableMoves[(int)(GD.Randi() % availableMoves.Count)];
		selectedMove.UsePP(); // Décrémente la PP.

		// Exécute le tour en tant qu'attaquant l'ennemi vers le joueur.
		await ExecuteTurn(_enemyBattle, _playerBattle, selectedMove.Move, _playerSprite, false);
	}

	// Exécute un tour complet d'attaque : animation, message, calcul dégâts, application, vérifications.
	private async Task ExecuteTurn(BattlePokemon attacker, BattlePokemon target, MoveResource move, Sprite2D targetSprite, bool isAttackerPlayer)
	{
		// Animation d'attaque : avance et revient.
		await AnimateAttacker(isAttackerPlayer, false);

		_battleUI.ShowDialogue($"{attacker.Resource.Name} utilise {move.Name.ToUpper()} !", true);
		await Task.Delay(600); // Petit délai pour laisser le message s'afficher.

		// Calcul et application des dégâts via une classe utilitaire DamageCalculator.
		var damageResult = DamageCalculator.CalculateDamage(attacker, target, move);

		if (damageResult.IsMiss)
		{
			_battleUI.ShowDialogue("L'attaque a échoué !", true);
		}
		else
		{
			// Application des dégâts à la cible (modifie CurrentHP interne).
			target.TakeDamage(damageResult.Damage);

			// Feedback visuel : texte flottant et shake si dégâts > 0.
			if (damageResult.Damage > 0)
				FloatingText.Create(targetSprite.GetParent(), targetSprite.GlobalPosition, damageResult.Damage.ToString(), Colors.Red);

			// Message d'efficacité (super efficace / peu efficace) si applicable.
			string effectMsg = DamageCalculator.GetEffectivenessMessage(damageResult.Effectiveness);
			if (!string.IsNullOrEmpty(effectMsg)) _battleUI.ShowDialogue(effectMsg, true);
			if (damageResult.IsCritical) _battleUI.ShowDialogue("Coup critique !", true);

			// Petite secousse du sprite cible pour l'impact.
			ApplyShake(targetSprite);
			await Task.Delay(500); // Pause pour laisser le joueur voir l'impact.
		}

		// Retour à la position initiale de l'attaquant.
		await AnimateAttacker(isAttackerPlayer, true);
		UpdateHealthBars(); // Met à jour les barres après application des dégâts.

		// --- VÉRIFICATION DE LA MORT (CORRIGÉE) ---
		if (target.IsFainted)
		{
			_isBattleOver = true;
			// Si la cible est morte et que l'attaquant était le joueur -> VICTOIRE
			_battleWon = isAttackerPlayer;

			CheckDeath(target); // Affiche messages et écran de fin.
			return; // On arrête le tour ici (plus d'actions après la mort).
		}

		// Au cas où l'attaquant meurt (ex: futur poison ou recul) on le gère aussi.
		if (attacker.IsFainted)
		{
			_isBattleOver = true;
			_battleWon = !isAttackerPlayer; // Si l'attaquant était le joueur et meurt => défaite
			CheckDeath(attacker);
			return;
		}

		await Task.Delay(800); // Pause finale avant de rendre le contrôle à l'UI.
	}

	// Vérifie et affiche l'écran si un Pokémon est K.O.
	private bool CheckDeath(BattlePokemon pokemon)
	{
		if (pokemon.IsFainted)
		{
			_battleUI.ShowDialogue($"{pokemon.Resource.Name} est K.O. !", true);
			_battleUI.ShowBattleEndScreen(_battleWon); // Affiche l'écran de fin avec victoire/défaite.
			return true;
		}
		return false;
	}

	// Anime l'attaquant : avance puis revient. Si retreat == false => avancer, true => revenir.
	private async Task AnimateAttacker(bool isPlayer, bool retreat)
	{
		var sprite = isPlayer ? _playerSprite : _enemySprite;
		if (sprite == null) return; // Sécurité : si pas de sprite assigné, on ne fait rien.

		Tween tween = GetTree().CreateTween(); // Crée un Tween pour animer des propriétés sur le temps.

		// On définit la direction de l'impact : joueur frappe vers la droite, ennemi vers la gauche.
		Vector2 direction = isPlayer ? Vector2.Right : Vector2.Left;
		Vector2 originalPos = isPlayer ? new Vector2(208, 416) : new Vector2(816, 176);
		// Note : idealement on utiliserait _playerStartPos/_enemyStartPos pour des positions dynamiques.

		if (!retreat)
		{
			// PHASE 1 : L'attaque (Avancer rapidement)
			Vector2 targetPos = sprite.Position + direction * 50;
			tween.TweenProperty(sprite, "position", targetPos, 0.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		}
		else
		{
			// PHASE 2 : Le retour (Revenir à la base)
			// Ici on utilise les positions sauvegardées depuis SetupBattle.
			Vector2 basePos = isPlayer ? _playerStartPos : _enemyStartPos;
			tween.TweenProperty(sprite, "position", basePos, 0.2f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		}

		// Attendre la fin de l'animation (signal "finished" du tween).
		await ToSignal(tween, "finished");
	}

	// Petite secousse visuelle du sprite ciblé (utilisée à l'impact).
	private void ApplyShake(Sprite2D target)
	{
		if (target == null) return;
		Tween tween = GetTree().CreateTween();
		Vector2 originalPos = target.Position;
		tween.TweenProperty(target, "position", originalPos + new Vector2(8, 0), 0.05f);
		tween.TweenProperty(target, "position", originalPos - new Vector2(8, 0), 0.05f);
		tween.TweenProperty(target, "position", originalPos, 0.05f);
		// Remarque : on ne "await" pas ici, l'animation se déroule mais la méthode retourne tout de suite.
	}

	// Gestion de la fuite demandée par le joueur.
	private async void OnFleePressed()
	{
		// 1. Désactiver le menu pour éviter de cliquer deux fois
		_battleUI.ShowActionMenu(false);

		_battleUI.ShowDialogue("Tentative de fuite...", true);
		await Task.Delay(600);

		// Chance de fuite (0.5f = 50%, mets 0.0f pour réussir à 100% pendant tes tests)
		if (GD.Randf() > 0.5f)
		{
			_battleUI.ShowDialogue("Vous avez réussi à fuir !", true);
			await Task.Delay(800);

			_isBattleOver = true;
			OnBattleEnd(); // Fin du combat (transition)
		}
		else
		{
			_battleUI.ShowDialogue("Echec de la fuite ! L'ennemi bloque le passage !", true);
			await Task.Delay(800);

			// Le combat continue, l'ennemi attaque immédiatement.
			await EnemyAiTurn();

			// Si le joueur n'est pas mort, on réaffiche le menu.
			if (!_isBattleOver)
			{
				_battleUI.ShowActionMenu(true);
			}
		}
	}

	// Placeholder pour le menu Pokémon (switch) : pas implémenté.
	private void OnPokemonMenuPressed() => _battleUI.ShowDialogue("Switch non implémenté !", true);

	// Méthode appelée quand le combat est terminé et que l'utilisateur veut revenir au monde.
	private async void OnBattleEnd()
	{
		if (_isBattleOver == false) return; // Sécurité : ne pas exécuter si le combat n'est pas fini.

		GD.Print("Fin du combat : Lancement de la transition via SceneManager...");

		// 1. TRANSITION SORTIE (Le rectangle noir s'affiche)
		// On appelle le FadeOut que tu as déjà défini dans SceneManager
		await SceneManager.Instance.FadeOut();

		// 2. MUSIQUE : On relance la musique du niveau actuel
		var currentLevel = SceneManager.GetCurrentLevel();
		var musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer"); // Accès au MusicPlayer global (singleton typique en Godot).

		if (currentLevel != null && musicPlayer != null)
		{
			// On utilise ton switch de musique basé sur le nom du niveau
			switch (currentLevel.LevelName)
			{
				case LevelName.small_town:
					musicPlayer.PlayMusic("res://assets/audio/music/music1.mp3", -22.0f);
					break;
				case LevelName.small_town_cave:
					musicPlayer.PlayMusic("res://assets/audio/music/music2.mp3", -17.0f);
					break;
				// Ajoute les autres cases ici si tu en as besoin
				default:
					musicPlayer.PlayMusic("res://assets/audio/music/music1.mp3", -22.0f);
					break;
			}
		}

		// 3. RÉAFFICHAGE DU MONDE (le niveau précédent devient visible)
		if (currentLevel != null)
		{
			currentLevel.Show();
		}

		// 4. RÉACTIVATION DU JOUEUR (réactive le personnage et sa caméra)
		var player = GameManager.GetPlayer();
		if (player != null)
		{
			player.Show();
			player.SetProcess(true);
			player.SetPhysicsProcess(true);

			// On remet la caméra du joueur en mode "current" pour que celle-ci suive le joueur.
			var playerCamera = player.GetNodeOrNull<Camera2D>("Camera2D");
			if (playerCamera != null)
			{
				playerCamera.MakeCurrent();
			}
		}

		// 5. DESTRUCTION DE LA SCÈNE DE COMBAT
		// On le fait AVANT le FadeIn pour ne plus voir le combat quand le noir se retire
		QueueFree();

		// 6. TRANSITION ENTRÉE (Le noir s'efface)
		await SceneManager.Instance.FadeIn();
	}

	// Petite fonction utilitaire pour relancer la musique selon ton switch (utilisée ailleurs si besoin).
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