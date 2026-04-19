using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;
using System;
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

	private BattlePokemon _playerBattle, _enemyBattle;
	private bool _isBattleOver = false;
	private bool _battleWon = false;
	private PlayerInventory _playerInventory = new();

	[ExportCategory("UI Nodes")]
	[Export] private Sprite2D _playerSprite, _enemySprite;
	[Export] private Label _playerName, _enemyName;
	[Export] private Label _playerLevel, _enemyLevel;
	[Export] private TextureProgressBar _playerHPBar, _enemyHPBar;
	[Export] private Label _playerHPText, _enemyHPText;
	[Export] private RichTextLabel _dialogueText;
	[Export] private Control _actionMenu, _moveMenu, _itemsMenu;
	[Export] private GridContainer _movesGrid, _itemsGrid;
	[Export] private Control _battleEndScreen;
	[Export] private Label _battleEndText;

	public override void _Ready()
	{
		// Récupération des nœuds avec gestion des cas où ils n'existent pas
		_enemySprite ??= GetNodeOrNull<Sprite2D>("BattlePositions/EnemySpawn/EnemySprite");
		_playerSprite ??= GetNodeOrNull<Sprite2D>("BattlePositions/PlayerSpawn/PlayerSprite");
		_enemyName ??= GetNodeOrNull<Label>("UI/EnemyHUD/Name");
		_enemyLevel ??= GetNodeOrNull<Label>("UI/EnemyHUD/Level");
		_enemyHPBar ??= GetNodeOrNull<TextureProgressBar>("UI/EnemyHUD/HP");
		_enemyHPText ??= GetNodeOrNull<Label>("UI/EnemyHUD/HPText");
		_playerName ??= GetNodeOrNull<Label>("UI/PlayerHUD/Name");
		_playerLevel ??= GetNodeOrNull<Label>("UI/PlayerHUD/Level");
		_playerHPBar ??= GetNodeOrNull<TextureProgressBar>("UI/PlayerHUD/HP");
		_playerHPText ??= GetNodeOrNull<Label>("UI/PlayerHUD/HPText");
		_dialogueText ??= GetNodeOrNull<RichTextLabel>("UI/DialogueLabel");
		_actionMenu ??= GetNodeOrNull<Control>("UI/ActionMenu");
		_moveMenu ??= GetNodeOrNull<Control>("UI/MoveMenu");
		_movesGrid ??= GetNodeOrNull<GridContainer>("UI/MoveMenu/MovesGrid");
		_itemsMenu ??= GetNodeOrNull<Control>("UI/ItemsMenu");
		_itemsGrid ??= GetNodeOrNull<GridContainer>("UI/ItemsMenu/ItemsGrid");
		_battleEndScreen ??= GetNodeOrNull<Control>("UI/BattleEndScreen");
		_battleEndText ??= GetNodeOrNull<Label>("UI/BattleEndScreen/Label");

		// Connexion des boutons du menu principal
		var attackBtn = GetNodeOrNull<Button>("UI/ActionMenu/Attaque");
		if (attackBtn != null) attackBtn.Pressed += () => ShowMoveMenu(true);

		var itemBtn = GetNodeOrNull<Button>("UI/ActionMenu/Objet");
		if (itemBtn != null) itemBtn.Pressed += () => ShowItemsMenu(true);

		var pokemonBtn = GetNodeOrNull<Button>("UI/ActionMenu/Pokemon");
		if (pokemonBtn != null) pokemonBtn.Pressed += OnPokemonMenuPressed;

		var fleeBtn = GetNodeOrNull<Button>("UI/ActionMenu/Fuite");
		if (fleeBtn != null) fleeBtn.Pressed += OnFleePressed;

		// Connexion des boutons de retour
		var returnMoveBtn = GetNodeOrNull<Button>("UI/MoveMenu/Retour");
		if (returnMoveBtn != null) returnMoveBtn.Pressed += () => ShowMoveMenu(false);

		var returnItemBtn = GetNodeOrNull<Button>("UI/ItemsMenu/Retour");
		if (returnItemBtn != null) returnItemBtn.Pressed += () => ShowItemsMenu(false);

		var continueBtn = GetNodeOrNull<Button>("UI/BattleEndScreen/ContinueButton");
		if (continueBtn != null) continueBtn.Pressed += OnBattleEnd;

		SetupBattle();
	}

	private void SetupBattle()
	{
		// Sécurité : Si les données n'ont pas encore été envoyées, on ne fait rien
		if (PlayerPokemon == null || EnemyPokemon == null)
		{
			_dialogueText?.AppendText("Erreur : Pokémon non assignés !\n");
			return;
		}

		// Création des instances de battle
		_playerBattle = new BattlePokemon(PlayerPokemon, PlayerLevel, PlayerMoves);
		_enemyBattle = new BattlePokemon(EnemyPokemon, EnemyLevel, EnemyMoves);

		// Affichage des sprites
		if (_playerSprite != null) _playerSprite.Texture = PlayerPokemon.BackSprite;
		if (_enemySprite != null) _enemySprite.Texture = EnemyPokemon.FrontSprite;

		// Affichage des noms et niveaux
		if (_playerName != null) _playerName.Text = PlayerPokemon.Name.ToUpper();
		if (_playerLevel != null) _playerLevel.Text = $"Niv. {PlayerLevel}";
		if (_enemyName != null) _enemyName.Text = EnemyPokemon.Name.ToUpper();
		if (_enemyLevel != null) _enemyLevel.Text = $"Niv. {EnemyLevel}";

		// Setup des barres de PV
		UpdateHealthBars();

		// Génération des boutons d'attaque
		if (_movesGrid != null)
		{
			foreach (Node child in _movesGrid.GetChildren()) child.QueueFree();

			foreach (var moveWithPP in _playerBattle.Moves)
			{
				Button moveBtn = new Button { Text = $"{moveWithPP.Move.Name} ({moveWithPP.CurrentPP}/{moveWithPP.MaxPP})" };
				moveBtn.Pressed += () => OnMoveSelected(moveWithPP);
				_movesGrid.AddChild(moveBtn);
			}
		}

		if (_moveMenu != null) _moveMenu.Hide();
		if (_battleEndScreen != null) _battleEndScreen.Hide();
		_dialogueText.Text = $"Un {EnemyPokemon.Name} sauvage apparaît !\n";
	}

	private void UpdateHealthBars()
	{
		// Player
		if (_playerHPBar != null)
		{
			_playerHPBar.MaxValue = _playerBattle.MaxHP;
			_playerHPBar.Value = _playerBattle.CurrentHP;
		}
		if (_playerHPText != null)
			_playerHPText.Text = $"{_playerBattle.CurrentHP}/{_playerBattle.MaxHP}";

		// Enemy
		if (_enemyHPBar != null)
		{
			_enemyHPBar.MaxValue = _enemyBattle.MaxHP;
			_enemyHPBar.Value = _enemyBattle.CurrentHP;
		}
		if (_enemyHPText != null)
			_enemyHPText.Text = $"{_enemyBattle.CurrentHP}/{_enemyBattle.MaxHP}";
	}

	private void ShowMoveMenu(bool show)
	{
		if (_actionMenu != null && _moveMenu != null)
		{
			if (show)
			{
				_actionMenu.Hide();
				_moveMenu.Show();
				RefreshMoveButtons();
			}
			else
			{
				_actionMenu.Show();
				_moveMenu.Hide();
			}
		}
	}

	private void ShowItemsMenu(bool show)
	{
		if (_actionMenu != null && _itemsMenu != null)
		{
			if (show)
			{
				_actionMenu.Hide();
				_itemsMenu.Show();
				RefreshItemsButtons();
			}
			else
			{
				_actionMenu.Show();
				_itemsMenu.Hide();
			}
		}
	}

	private void RefreshItemsButtons()
	{
		if (_itemsGrid == null) return;

		foreach (Node child in _itemsGrid.GetChildren()) child.QueueFree();

		var items = _playerInventory.GetAllItems();
		if (items.Count == 0)
		{
			Label noItemsLabel = new Label { Text = "Pas d'items disponibles" };
			_itemsGrid.AddChild(noItemsLabel);
			return;
		}

		foreach (var item in items.Values)
		{
			Button itemBtn = new Button
			{
				Text = $"{item.Name} (x{item.Quantity})\n{item.Description}"
			};
			itemBtn.Pressed += () => OnItemSelected(item.Type);
			_itemsGrid.AddChild(itemBtn);
		}
	}

	private async void OnItemSelected(PlayerInventory.ItemType itemType)
	{
		if (_isBattleOver) return;

		// Utilise l'item
		bool success = UseItemInBattle(itemType);

		if (success)
		{
			_playerInventory.UseItem(itemType);
			_dialogueText.Text += $"\nUtilisé {GetItemName(itemType)} !";
			await Task.Delay(800);

			// Le joueur a utilisé un item, l'ennemi joue
			if (!_isBattleOver) await EnemyAiTurn();
			UpdateHealthBars();
			RefreshItemsButtons();

			if (!_isBattleOver)
			{
				ShowItemsMenu(false);
				_actionMenu?.Show();
			}
		}
		else
		{
			_dialogueText.Text += $"\nCet item ne peut pas être utilisé ici !";
			await Task.Delay(800);
		}
	}

	private bool UseItemInBattle(PlayerInventory.ItemType itemType)
	{
		// Récupère les PV selon le type d'item
		int hpRestore = itemType switch
		{
			PlayerInventory.ItemType.Potion => 20,
			PlayerInventory.ItemType.SuperPotion => 50,
			PlayerInventory.ItemType.HyperPotion => 100,
			PlayerInventory.ItemType.FullHeal => _playerBattle.MaxHP,
			_ => 0
		};

		if (hpRestore > 0)
		{
			_playerBattle.Heal(hpRestore);
			return true;
		}

		return false;
	}

	private string GetItemName(PlayerInventory.ItemType type)
	{
		return type switch
		{
			PlayerInventory.ItemType.Potion => "Potion",
			PlayerInventory.ItemType.SuperPotion => "Super Potion",
			PlayerInventory.ItemType.HyperPotion => "Hyper Potion",
			PlayerInventory.ItemType.FullHeal => "Full Heal",
			_ => "Item"
		};
	}

	private void OnPokemonMenuPressed()
	{
		// Cette fonctionnalité sera implémentée plus tard (switch Pokemon)
		_dialogueText.Text += "\nFonctionnalité de switch Pokémon à venir !";
	}

	private async void OnFleePressed()
	{
		if (_isBattleOver) return;

		_dialogueText.Text += "\nTentative de fuite...";
		await Task.Delay(600);

		// 50% de chance de réussir à fuir
		if (GD.Randf() > 0.5f)
		{
			_dialogueText.Text += "\nVous avez réussi à fuir !";
			await Task.Delay(800);
			_isBattleOver = true;
			_battleWon = false;
			OnBattleEnd();
		}
		else
		{
			_dialogueText.Text += "\nEchec de la fuite !";
			await Task.Delay(800);

			if (!_isBattleOver) await EnemyAiTurn();
			UpdateHealthBars();
			RefreshMoveButtons();

			if (!_isBattleOver) _actionMenu?.Show();
		}
	}

	private async void OnMoveSelected(MoveWithPP moveWithPP)
	{
		if (_isBattleOver || !moveWithPP.CanUse)
		{
			_dialogueText.Text += "\nCette attaque n'a plus de PP !";
			await Task.Delay(800);
			return;
		}

		if (_moveMenu != null) _moveMenu.Hide();
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

		UpdateHealthBars();
		RefreshMoveButtons();

		if (!_isBattleOver && _actionMenu != null) _actionMenu.Show();
	}

	private void RefreshMoveButtons()
	{
		if (_movesGrid == null) return;

		foreach (Node child in _movesGrid.GetChildren()) child.QueueFree();

		foreach (var moveWithPP in _playerBattle.Moves)
		{
			Button moveBtn = new Button { Text = $"{moveWithPP.Move.Name} ({moveWithPP.CurrentPP}/{moveWithPP.MaxPP})" };
			moveBtn.Pressed += () => OnMoveSelected(moveWithPP);
			_movesGrid.AddChild(moveBtn);
		}
	}

	private async Task EnemyAiTurn()
	{
		// IA Simple : choisit une attaque aléatoire parmi celles disponibles
		var availableMoves = new System.Collections.Generic.List<MoveWithPP>();
		foreach (var move in _enemyBattle.Moves)
		{
			if (move.CanUse)
				availableMoves.Add(move);
		}

		if (availableMoves.Count == 0)
		{
			_dialogueText.Text += $"\n{_enemyBattle.Resource.Name} n'a plus d'attaques !";
			return;
		}

		var randomIndex = (int)(GD.Randi() % availableMoves.Count);
		var selectedMove = availableMoves[randomIndex];
		selectedMove.UsePP();

		await ExecuteTurn(_enemyBattle, _playerBattle, selectedMove.Move, _playerSprite, false);
	}

	private async Task ExecuteTurn(BattlePokemon attacker, BattlePokemon target, MoveResource move, Sprite2D targetSprite, bool isAttackerPlayer)
	{
		// Animation de l'attaquant qui s'approche
		await AnimateAttacker(isAttackerPlayer);

		// Message d'attaque
		_dialogueText.Text += $"\n{attacker.Resource.Name} utilise {move.Name.ToUpper()} !";
		await Task.Delay(600);

		// Calcul des dégâts
		var damageResult = DamageCalculator.CalculateDamage(attacker, target, move);

		if (damageResult.IsMiss)
		{
			_dialogueText.Text += $"\nL'attaque a échoué !";
		}
		else if (damageResult.Damage == 0 && move.Power > 0)
		{
			_dialogueText.Text += $"\nCe n'était pas très efficace...";
		}
		else
		{
			// Application des dégâts
			target.TakeDamage(damageResult.Damage);

			// Texte flottant de dégâts
			if (damageResult.Damage > 0)
			{
				FloatingText.Create(targetSprite.GetParent(), targetSprite.GlobalPosition, damageResult.Damage.ToString(), Colors.Red);
			}

			// Message d'efficacité
			if (damageResult.Damage > 0)
			{
				string effectMsg = DamageCalculator.GetEffectivenessMessage(damageResult.Effectiveness);
				if (!string.IsNullOrEmpty(effectMsg))
					_dialogueText.Text += $"\n{effectMsg}";

				if (damageResult.IsCritical)
					_dialogueText.Text += "\nCoup critique !";
			}

			// Animation de choc
			ApplyShake(targetSprite);
			await Task.Delay(500);
		}

		// Animation de l'attaquant qui se retire
		await AnimateAttacker(isAttackerPlayer, true);

		UpdateHealthBars();

		// Vérification de la mort
		if (CheckDeath(isAttackerPlayer ? target : attacker))
		{
			_isBattleOver = true;
			_battleWon = isAttackerPlayer;
		}

		await Task.Delay(800);
	}

	private async Task AnimateAttacker(bool isPlayer, bool retreat = false)
	{
		var sprite = isPlayer ? _playerSprite : _enemySprite;
		if (sprite == null) return;

		Tween tween = GetTree().CreateTween();
		Vector2 originalPos = sprite.Position;
		Vector2 attackPos = originalPos + (isPlayer ? Vector2.Right : Vector2.Left) * 50;

		if (!retreat)
			tween.TweenProperty(sprite, "position", attackPos, 0.2f);
		else
			tween.TweenProperty(sprite, "position", originalPos, 0.2f);

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

	private bool CheckDeath(BattlePokemon pokemon)
	{
		if (pokemon.IsFainted)
		{
			_dialogueText.Text += $"\n{pokemon.Resource.Name} est K.O. !";

			if (_battleWon)
				_dialogueText.Text += $"\nVous avez remporté le combat !";
			else
				_dialogueText.Text += $"\nVous avez perdu le combat...";

			if (_actionMenu != null) _actionMenu.Hide();
			if (_moveMenu != null) _moveMenu.Hide();
			if (_battleEndScreen != null)
			{
				_battleEndScreen.Show();
				if (_battleEndText != null)
					_battleEndText.Text = _battleWon ? "VICTOIRE !" : "DÉFAITE...";
			}
			return true;
		}
		return false;
	}

	private void OnBattleEnd()
	{
		if (_battleWon)
		{
			// Gain d'expérience
			int expGain = _enemyBattle.Resource.BaseExperience;
			_playerBattle.Experience += expGain;
			_dialogueText.Text += $"\nVous avez reçu {expGain} points d'expérience !";
		}

		// Retour au monde
		var player = GameManager.GetPlayer();
		if (player != null)
		{
			player.SetProcess(true);
			player.Show();

			// Réactive la caméra du joueur
			var playerCamera = player.GetNodeOrNull<Camera2D>("Camera2D");
			if (playerCamera != null)
			{
				playerCamera.MakeCurrent();
			}
		}

		// Réaffiche le niveau
		var currentLevel = SceneManager.GetCurrentLevel();
		if (currentLevel != null)
		{
			currentLevel.Show();
		}

		// Retire la scène de combat
		GetParent().RemoveChild(this);
		QueueFree();
	}
}