using Game.Gameplay;
using Godot;
using System;
using System.Threading.Tasks; // Pour utiliser Task.Delay (pauses entre les messages)

public partial class BattleScene : Node2D
{
	[Export] public PokemonResource PlayerPokemon;
	[Export] public PokemonResource EnemyPokemon;

	// Variables pour suivre les PV actuels pendant le combat
	private int _currentPlayerHP;
	private int _currentEnemyHP;

	// Références aux nodes
	private Sprite2D _playerSprite, _enemySprite;
	private Label _playerName, _playerLevel, _enemyName, _enemyLevel;
	private TextureProgressBar _playerHPBar, _enemyHPBar;
	private RichTextLabel _dialogueText;
	private Control _actionMenu;

	public override void _Ready()
	{
		// Récupération des Sprites
		_enemySprite = GetNode<Sprite2D>("BattlePositions/EnemySpawn/EnemySprite");
		_playerSprite = GetNode<Sprite2D>("BattlePositions/PlayerSpawn/PlayerSprite");

		// Récupération de l'UI
		_enemyName = GetNode<Label>("UI/EnemyHUD/Name");
		_enemyLevel = GetNode<Label>("UI/EnemyHUD/Level");
		_enemyHPBar = GetNode<TextureProgressBar>("UI/EnemyHUD/HP");

		_playerName = GetNode<Label>("UI/PlayerHUD/Name");
		_playerLevel = GetNode<Label>("UI/PlayerHUD/Level");
		_playerHPBar = GetNode<TextureProgressBar>("UI/PlayerHUD/HP");

		_dialogueText = GetNode<RichTextLabel>("UI/DialogueLabel");
		_actionMenu = GetNode<Control>("UI/ActionMenu");

		// Connexion du bouton Attaque (Assure-toi que le nom du node est exact)
		GetNode<Button>("UI/ActionMenu/Attaque").Pressed += OnAttackPressed;

		SetupBattle();
	}

	public void SetupBattle()
	{
		// Initialisation des PV locaux
		_currentPlayerHP = PlayerPokemon.BaseHp;
		_currentEnemyHP = EnemyPokemon.BaseHp;

		// Configuration Ennemi
		_enemySprite.Texture = EnemyPokemon.FrontSprite;
		_enemyName.Text = EnemyPokemon.Name.ToUpper();
		_enemyLevel.Text = "Lv5";
		_enemyHPBar.MaxValue = EnemyPokemon.BaseHp;
		_enemyHPBar.Value = EnemyPokemon.BaseHp;

		// Configuration Joueur
		_playerSprite.Texture = PlayerPokemon.BackSprite;
		_playerName.Text = PlayerPokemon.Name.ToUpper();
		_playerLevel.Text = "Lv5";
		_playerHPBar.MaxValue = PlayerPokemon.BaseHp;
		_playerHPBar.Value = PlayerPokemon.BaseHp;

		// Reset de l'UI
		UpdateHPBarColor(_playerHPBar, _currentPlayerHP, PlayerPokemon.BaseHp);
		UpdateHPBarColor(_enemyHPBar, _currentEnemyHP, EnemyPokemon.BaseHp);

		_dialogueText.Text = $"Un {EnemyPokemon.Name} sauvage apparaît !";
	}

	// --- LOGIQUE DE COMBAT ---

	private async void OnAttackPressed()
	{
		_actionMenu.Hide(); // Cache le menu pendant l'attaque

		// 1. Tour du Joueur
		_dialogueText.Text = $"{PlayerPokemon.Name.ToUpper()} attaque !";

		// Calcul simple : Attaque vs Défense (minimum 1 dégât)
		int damage = Math.Max(1, PlayerPokemon.BaseAttack - EnemyPokemon.BaseDefense);
		_currentEnemyHP = Math.Max(0, _currentEnemyHP - damage);

		await Task.Delay(1000); // Pause pour lire le texte

		UpdateHealthVisual(_enemyHPBar, _currentEnemyHP, EnemyPokemon.BaseHp);

		if (_currentEnemyHP <= 0)
		{
			_dialogueText.Text = $"{EnemyPokemon.Name.ToUpper()} est K.O. !";
			return;
		}

		// 2. Tour de l'Ennemi (IA basique)
		await Task.Delay(1500);
		_dialogueText.Text = $"{EnemyPokemon.Name.ToUpper()} sauvage attaque !";

		int enemyDamage = Math.Max(1, EnemyPokemon.BaseAttack - PlayerPokemon.BaseDefense);
		_currentPlayerHP = Math.Max(0, _currentPlayerHP - enemyDamage);

		await Task.Delay(1000);
		UpdateHealthVisual(_playerHPBar, _currentPlayerHP, PlayerPokemon.BaseHp);

		if (_currentPlayerHP <= 0)
		{
			_dialogueText.Text = "Vous avez perdu le combat...";
		}
		else
		{
			await Task.Delay(1000);
			_actionMenu.Show(); // Redonne la main au joueur
			_dialogueText.Text = $"Que doit faire {_playerName.Text} ?";
		}
	}

	// --- FONCTIONS UTILITAIRES ---

	private void UpdateHealthVisual(TextureProgressBar bar, int currentHP, int maxHP)
	{
		// Animation de la barre (Tween)
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(bar, "value", currentHP, 0.5f).SetTrans(Tween.TransitionType.Circ);

		UpdateHPBarColor(bar, currentHP, maxHP);
	}

	private void UpdateHPBarColor(TextureProgressBar bar, int currentHP, int maxHP)
	{
		float ratio = (float)currentHP / maxHP;
		if (ratio < 0.2f) bar.TintProgress = Colors.Red;
		else if (ratio < 0.5f) bar.TintProgress = Colors.Orange;
		else bar.TintProgress = Colors.Blue;
	}
}