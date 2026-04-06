using Game.Gameplay;
using Godot;
using System;
using System.Threading.Tasks;

public partial class BattleScene : Node2D
{
	// [Export] : On glisse les fichiers "PokemonResource" (statistiques) ici.
	[Export] public PokemonResource PlayerPokemon;
	[Export] public PokemonResource EnemyPokemon;

	// 'int' = Nombre entier. On stocke les PV ici pour ne pas abîmer le fichier original.
	private int _currentPlayerHP;
	private int _currentEnemyHP;

	// Les "Variables de nœuds" : Ce sont des raccourcis vers les éléments visuels de ton écran.
	private Sprite2D _playerSprite, _enemySprite;
	private Label _playerName, _playerLevel, _enemyName, _enemyLevel;
	private TextureProgressBar _playerHPBar, _enemyHPBar;
	private RichTextLabel _dialogueText;
	private Control _actionMenu;

	/// <summary>
	/// _Ready : Le code va chercher chaque élément sur l'écran au démarrage du combat.
	/// </summary>
	public override void _Ready()
	{
		// GetNode = "Va chercher l'objet qui s'appelle comme ça dans la liste à gauche dans Godot".
		_enemySprite = GetNode<Sprite2D>("BattlePositions/EnemySpawn/EnemySprite");
		_playerSprite = GetNode<Sprite2D>("BattlePositions/PlayerSpawn/PlayerSprite");

		_enemyName = GetNode<Label>("UI/EnemyHUD/Name");
		_enemyLevel = GetNode<Label>("UI/EnemyHUD/Level");
		_enemyHPBar = GetNode<TextureProgressBar>("UI/EnemyHUD/HP");

		_playerName = GetNode<Label>("UI/PlayerHUD/Name");
		_playerLevel = GetNode<Label>("UI/PlayerHUD/Level");
		_playerHPBar = GetNode<TextureProgressBar>("UI/PlayerHUD/HP");

		_dialogueText = GetNode<RichTextLabel>("UI/DialogueLabel");
		_actionMenu = GetNode<Control>("UI/ActionMenu");

		// ÉCOUTEUR : Quand on appuie sur le bouton "Attaque", lance la fonction OnAttackPressed.
		GetNode<Button>("UI/ActionMenu/Attaque").Pressed += OnAttackPressed;

		SetupBattle(); // Configure les noms et les barres de vie.
	}

	/// <summary>
	/// INITIALISATION : Remplit les étiquettes de texte avec les vraies infos du Pokémon.
	/// </summary>
	public void SetupBattle()
	{
		if (PlayerPokemon == null || EnemyPokemon == null) return;

		// On prend les PV de base définis dans le fichier de ressource.
		_currentPlayerHP = PlayerPokemon.BaseHp;
		_currentEnemyHP = EnemyPokemon.BaseHp;

		// On met les images correspondantes.
		_enemySprite.Texture = EnemyPokemon.FrontSprite;
		_playerSprite.Texture = PlayerPokemon.BackSprite;

		// .ToUpper() transforme le texte en MAJUSCULES.
		_enemyName.Text = EnemyPokemon.Name.ToUpper();
		_enemyHPBar.MaxValue = EnemyPokemon.BaseHp; // La barre est pleine au max des PV.
		_enemyHPBar.Value = EnemyPokemon.BaseHp;

		_playerName.Text = PlayerPokemon.Name.ToUpper();
		_playerHPBar.MaxValue = PlayerPokemon.BaseHp;
		_playerHPBar.Value = PlayerPokemon.BaseHp;

		// On écrit le texte de bienvenue dans la boîte de dialogue.
		_dialogueText.Text = $"Un {EnemyPokemon.Name.ToUpper()} sauvage apparaît !";
		_actionMenu.Show(); // On montre les boutons "Attaquer/Fuir".
	}

	/// <summary>
	/// BOUCLE DE TOUR : Le joueur attaque, puis l'ennemi répond.
	/// </summary>
	private async void OnAttackPressed()
	{
		_actionMenu.Hide(); // On cache le menu pour pas qu'il clique 50 fois pendant l'animation.

		// --- 1. TOUR DU JOUEUR ---
		_dialogueText.Text = $"{PlayerPokemon.Name.ToUpper()} attaque !";

		// CALCUL : (Attaque de l'assaillant - Défense de la cible). 
		// Math.Max(1, ...) permet de faire au moins 1 dégât même si l'ennemi a une défense énorme.
		int damage = Math.Max(1, PlayerPokemon.BaseAttack - EnemyPokemon.BaseDefense);
		_currentEnemyHP = Math.Max(0, _currentEnemyHP - damage); // On descend les PV sans aller sous 0.

		await Task.Delay(1000); // On attend 1 seconde pour que l'utilisateur lise.

		UpdateHealthVisual(_enemyHPBar, _currentEnemyHP, EnemyPokemon.BaseHp);

		// Si l'ennemi n'a plus de PV
		if (_currentEnemyHP <= 0)
		{
			_dialogueText.Text = $"{EnemyPokemon.Name.ToUpper()} est K.O. !";
			return; // Le combat s'arrête ici.
		}

		// --- 2. TOUR DE L'ENNEMI ---
		await Task.Delay(1500); // Pause entre les deux attaques.
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
			_actionMenu.Show(); // Le joueur a survécu, on réaffiche les boutons.
			_dialogueText.Text = $"Que doit faire {_playerName.Text} ?";
		}
	}

	/// <summary>
	/// ANIMATION : Fait descendre la barre de vie doucement au lieu d'un coup sec.
	/// </summary>
	private void UpdateHealthVisual(TextureProgressBar bar, int currentHP, int maxHP)
	{
		// On crée un Tween (moteur d'animation fluide).
		Tween tween = GetTree().CreateTween();
		// On anime la propriété "value" de la barre jusqu'aux nouveaux PV en 0.5 secondes.
		tween.TweenProperty(bar, "value", currentHP, 0.5f).SetTrans(Tween.TransitionType.Circ);

		UpdateHPBarColor(bar, currentHP, maxHP);
	}

	/// <summary>
	/// COULEUR : Change la couleur de la barre selon la vie.
	/// </summary>
	private void UpdateHPBarColor(TextureProgressBar bar, int currentHP, int maxHP)
	{
		float ratio = (float)currentHP / maxHP; // Calcul du pourcentage (ex: 0.5 pour 50%).

		if (ratio < 0.2f) // Moins de 20%
		{
			bar.TintProgress = Colors.Red; // Rouge (danger)
		}
		else if (ratio < 0.5f) // Moins de 50%
		{
			bar.TintProgress = Colors.Orange; // Orange
		}
		else
		{
			bar.TintProgress = Colors.Blue; // Bleu (santé OK)
		}
	}
}