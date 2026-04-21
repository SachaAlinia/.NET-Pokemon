using Game.Gameplay;
using Godot;
using System;
using System.Collections.Generic;
using Game.Core; // Ensure this matches the namespace where GameManager is defined

// BattleUI est la couche d'interface utilisateur pour les combats.
// Ce script expose des événements que BattleScene écoute et fournit
// des méthodes pour mettre à jour l'affichage (barres de vie, dialogues, boutons, etc.).
public partial class BattleUI : CanvasLayer
{
	// Événements pour communiquer avec BattleScene
	// - OnMoveClicked(int) : déclenché quand l'utilisateur clique sur une attaque (index 0-3)
	// - OnActionXxxxPressed : événements pour chaque action du menu principal
	public event Action<int> OnMoveClicked;
	public event Action OnActionAttackPressed;
	public event Action OnActionItemPressed;
	public event Action OnActionPokemonPressed;
	public event Action OnActionFleePressed;
	public event Action OnReturnPressed;
	public event Action OnBattleEndContinue;

	// 1. L'événement qui prévient quand on clique sur un objet
	//    Ce callback transmet l'ItemResource sélectionné.
	public event Action<ItemResource> OnItemUsed;

	// 2. Le lien vers ton GridContainer d'objets (assigné depuis l'éditeur Godot)
	[Export] private GridContainer _itemsGrid;

	// 3. Le lien vers le menu des objets lui-même pour pouvoir le cacher/montrer
	[Export] private Control _itemsMenu;

	[ExportCategory("HUD")]
	[Export] private Label _playerName, _enemyName; // Labels pour afficher noms des Pokémon
	[Export] private Label _playerLevel, _enemyLevel; // Labels pour afficher niveaux
	[Export] private TextureProgressBar _playerHPBar, _enemyHPBar; // Barres graphiques des PV
	[Export] private Label _playerHPText, _enemyHPText; // Textes montrant PV numériques
	[Export] private RichTextLabel _dialogueText; // Zone pour afficher les dialogues/messages

	[ExportCategory("Boutons Menu Principal")]
	[Export] private Button _boutonAttaque; // Bouton "Attaque"
	[Export] private Button _boutonObjet; // Bouton "Objet"
	[Export] private Button _boutonPokemon; // Bouton "Pokemon" (switch)
	[Export] private Button _boutonFuite; // Bouton "Fuite"

	[ExportCategory("Boutons Navigation")]
	[Export] private Button _boutonRetourAttaque; // Bouton pour revenir depuis le menu attaques
	[Export] private Button _boutonRetourObjet; // Bouton pour revenir depuis le menu objets
	[Export] private Button _boutonContinuer; // Bouton "Continuer" sur l'écran de fin

	[ExportCategory("Menus")]
	[Export] private Control _actionMenu, _moveMenu, _battleEndScreen; // Conteneurs UI
	[Export] private GridContainer _movesGrid; // Conteneur qui accueillera dynamiquement les boutons d'attaque
	[Export] private Label _battleEndText; // Texte affiché sur l'écran de fin (victoire/défaite)

	// Tableau local de 4 boutons qui représentent les attaques du Pokémon (index 0..3)
	private Button[] _moveButtons = new Button[4];

	// Méthode Godot appelée quand le nœud est prêt : on initialise l'UI et les connexions aux boutons.
	public override void _Ready()
	{
		// A. ON NETTOIE L'UI (Pour ne plus voir "Fin du combat" au début)
		if (_battleEndScreen != null) _battleEndScreen.Hide(); // Cache l'écran de fin si présent
		if (_moveMenu != null) _moveMenu.Hide(); // Cache le menu d'attaques
		if (_itemsMenu != null) _itemsMenu.Hide(); // Cache le menu d'objets
		if (_actionMenu != null) _actionMenu.Show(); // Montre le menu d'actions principal

		// B. INITIALISATION DES BOUTONS D'ATTAQUE
		// On crée 4 boutons à la volée, on les cache et on les ajoute au GridContainer.
		for (int i = 0; i < 4; i++)
		{
			int index = i; // Capture de la variable pour le lambda (évite le piège de la boucle)
			var btn = new Button();
			btn.Hide(); // Par défaut invisible : sera montré quand une attaque correspondante existe
			btn.Pressed += () => OnMoveClicked?.Invoke(index); // Déclenche l'événement avec l'index
			_movesGrid.AddChild(btn); // Ajout au conteneur visuel
			_moveButtons[i] = btn; // Stockage local pour mise à jour future (RefreshMoves)
		}

		// Connexions simples : quand un bouton est pressé, invoquer l'événement correspondant.
		if (_boutonAttaque != null) _boutonAttaque.Pressed += () => OnActionAttackPressed?.Invoke();
		if (_boutonObjet != null) _boutonObjet.Pressed += () => OnActionItemPressed?.Invoke();
		if (_boutonPokemon != null) _boutonPokemon.Pressed += () => OnActionPokemonPressed?.Invoke();
		if (_boutonFuite != null) _boutonFuite.Pressed += () => OnActionFleePressed?.Invoke();

		if (_boutonRetourAttaque != null) _boutonRetourAttaque.Pressed += () => OnReturnPressed?.Invoke();
		if (_boutonRetourObjet != null) _boutonRetourObjet.Pressed += () => OnReturnPressed?.Invoke();
		if (_boutonContinuer != null) _boutonContinuer.Pressed += () => OnBattleEndContinue?.Invoke();
	}

	// --- MÉTHODES APPELÉES PAR BATTLESCENE ---
	// Ces méthodes sont publiques pour que BattleScene puisse mettre à jour l'UI.

	public void SetupPokemonInfo(bool isPlayer, string name, int level)
	{
		// Sécurité 1 : Si le nom du Pokémon est vide dans son fichier .tres
		string safeName = string.IsNullOrEmpty(name) ? "NOM_MANQUANT" : name.ToUpper();

		if (isPlayer)
		{
			// Sécurité 2 : On vérifie si tu as bien assigné les textes dans l'inspecteur
			if (_playerName == null)
				GD.PrintErr("CRASH ÉVITÉ : La case 'Player Name' est vide dans l'inspecteur du nœud UI !");
			else
				_playerName.Text = safeName; // Met à jour le label avec le nom en majuscules

			if (_playerLevel == null)
				GD.PrintErr("CRASH ÉVITÉ : La case 'Player Level' est vide dans l'inspecteur du nœud UI !");
			else
				_playerLevel.Text = $"Niv. {level}"; // Affiche "Niv. X"
		}
		else
		{
			// Même logique pour l'ennemi
			if (_enemyName == null)
				GD.PrintErr("CRASH ÉVITÉ : La case 'Enemy Name' est vide dans l'inspecteur du nœud UI !");
			else
				_enemyName.Text = safeName;

			if (_enemyLevel == null)
				GD.PrintErr("CRASH ÉVITÉ : La case 'Enemy Level' est vide dans l'inspecteur du nœud UI !");
			else
				_enemyLevel.Text = $"Niv. {level}";
		}
	}

	// Met à jour une barre de vie (player ou enemy)
	public void UpdateHealthBar(bool isPlayer, int currentHP, int maxHP)
	{
		var bar = isPlayer ? _playerHPBar : _enemyHPBar; // Choix de la barre
		var text = isPlayer ? _playerHPText : _enemyHPText; // Choix du texte

		bar.MaxValue = maxHP; // Valeur maximale de la barre
		bar.Value = currentHP; // Valeur actuelle
		if (text != null) text.Text = $"{currentHP}/{maxHP}"; // Texte "X/Y"
	}

	// Affiche un message dans la zone de dialogue ; append = true ajoute au texte existant
	public void ShowDialogue(string text, bool append = false)
	{
		if (append) _dialogueText.AppendText("\n" + text);
		else _dialogueText.Text = text;
	}

	// Met à jour l'affichage des attaques (noms + PP) d'après la liste Moves fournie
	public void RefreshMoves(List<MoveWithPP> moves)
	{
		for (int i = 0; i < 4; i++)
		{
			if (i < moves.Count)
			{
				// Affiche le nom de l'attaque et les PP restants, désactive le bouton si PP == 0
				_moveButtons[i].Text = $"{moves[i].Move.Name}\n{moves[i].CurrentPP}/{moves[i].MaxPP}";
				_moveButtons[i].Disabled = !moves[i].CanUse;
				_moveButtons[i].Show();
			}
			else
			{
				// Pas d'attaque pour cet index -> cacher le bouton
				_moveButtons[i].Hide();
			}
		}
	}

	// Reconstruit la liste d'objets à partir de GameManager.Inventory
	public void RefreshItems()
	{
		// 1. On vide l'ancien affichage (si tu as un container spécifique pour les items)
		foreach (Node child in _itemsGrid.GetChildren())
		{
			child.QueueFree(); // Supprime visuellement les anciens boutons
		}

		// 2. On parcourt le dictionnaire du GameManager
		//    GameManager.Inventory est un Dictionary<ItemResource, int>
		foreach (var entry in GameManager.Inventory)
		{
			ItemResource item = entry.Key;
			int count = entry.Value;

			var btn = new Button();
			btn.Text = $"{item.Name} x{count}"; // Nom + quantité

			// Quand on clique sur l'objet on déclenche OnItemUsed avec la ressource
			btn.Pressed += () => OnItemUsed?.Invoke(item);

			_itemsGrid.AddChild(btn); // Ajout du bouton au GridContainer
		}
	}

	// Affiche/masque le menu d'action principal et cache les sous-menus si on montre le menu principal
	public void ShowActionMenu(bool show)
	{
		_actionMenu.Visible = show;
		if (show)
		{
			_moveMenu.Hide();
			_itemsMenu.Hide();
		}
	}

	// Affiche/masque le menu des attaques
	public void ToggleMoveMenu(bool show)
	{
		_moveMenu.Visible = show;
		_actionMenu.Visible = !show; // Inverse la visibilité du menu d'action
	}

	// Affiche/masque le menu des objets
	public void ToggleItemsMenu(bool show)
	{
		_itemsMenu.Visible = show;
		_actionMenu.Visible = !show;
	}

	// Affiche l'écran de fin avec victoire ou défaite
	public void ShowBattleEndScreen(bool win)
	{
		_actionMenu.Hide();
		_moveMenu.Hide();
		_itemsMenu.Hide();
		_battleEndScreen.Show();
		_battleEndText.Text = win ? "VICTOIRE !" : "DÉFAITE..."; // Texte selon le résultat
	}
}