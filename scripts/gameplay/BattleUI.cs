using Game.Gameplay;
using Godot;
using System;
using System.Collections.Generic;

public partial class BattleUI : CanvasLayer
{
	// Événements pour communiquer avec BattleScene
	public event Action<int> OnMoveClicked;
	public event Action OnActionAttackPressed;
	public event Action OnActionItemPressed;
	public event Action OnActionPokemonPressed;
	public event Action OnActionFleePressed;
	public event Action OnReturnPressed;
	public event Action OnBattleEndContinue;

	[ExportCategory("HUD")]
	[Export] private Label _playerName, _enemyName;
	[Export] private Label _playerLevel, _enemyLevel;
	[Export] private TextureProgressBar _playerHPBar, _enemyHPBar;
	[Export] private Label _playerHPText, _enemyHPText;
	[Export] private RichTextLabel _dialogueText;

	[ExportCategory("Boutons Menu Principal")]
	[Export] private Button _boutonAttaque;
	[Export] private Button _boutonObjet;
	[Export] private Button _boutonPokemon;
	[Export] private Button _boutonFuite;

	[ExportCategory("Boutons Navigation")]
	[Export] private Button _boutonRetourAttaque;
	[Export] private Button _boutonRetourObjet;
	[Export] private Button _boutonContinuer;

	[ExportCategory("Menus")]
	[Export] private Control _actionMenu, _moveMenu, _itemsMenu, _battleEndScreen;
	[Export] private GridContainer _movesGrid;
	[Export] private Label _battleEndText;

	private Button[] _moveButtons = new Button[4];

	public override void _Ready()
	{
		// A. ON NETTOIE L'UI (Pour ne plus voir "Fin du combat" au début)
		if (_battleEndScreen != null) _battleEndScreen.Hide();
		if (_moveMenu != null) _moveMenu.Hide();
		if (_itemsMenu != null) _itemsMenu.Hide();
		if (_actionMenu != null) _actionMenu.Show();

		// B. INITIALISATION DES BOUTONS D'ATTAQUE
		for (int i = 0; i < 4; i++)
		{
			int index = i;
			var btn = new Button();
			btn.Hide();
			btn.Pressed += () => OnMoveClicked?.Invoke(index);
			_movesGrid.AddChild(btn);
			_moveButtons[i] = btn;
		}

		if (_boutonAttaque != null) _boutonAttaque.Pressed += () => OnActionAttackPressed?.Invoke();
		if (_boutonObjet != null) _boutonObjet.Pressed += () => OnActionItemPressed?.Invoke();
		if (_boutonPokemon != null) _boutonPokemon.Pressed += () => OnActionPokemonPressed?.Invoke();
		if (_boutonFuite != null) _boutonFuite.Pressed += () => OnActionFleePressed?.Invoke();

		if (_boutonRetourAttaque != null) _boutonRetourAttaque.Pressed += () => OnReturnPressed?.Invoke();
		if (_boutonRetourObjet != null) _boutonRetourObjet.Pressed += () => OnReturnPressed?.Invoke();
		if (_boutonContinuer != null) _boutonContinuer.Pressed += () => OnBattleEndContinue?.Invoke();
	}

	// --- MÉTHODES APPELÉES PAR BATTLESCENE ---

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
				_playerName.Text = safeName;

			if (_playerLevel == null)
				GD.PrintErr("CRASH ÉVITÉ : La case 'Player Level' est vide dans l'inspecteur du nœud UI !");
			else
				_playerLevel.Text = $"Niv. {level}";
		}
		else
		{
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

	public void UpdateHealthBar(bool isPlayer, int currentHP, int maxHP)
	{
		var bar = isPlayer ? _playerHPBar : _enemyHPBar;
		var text = isPlayer ? _playerHPText : _enemyHPText;

		bar.MaxValue = maxHP;
		bar.Value = currentHP;
		if (text != null) text.Text = $"{currentHP}/{maxHP}";
	}

	public void ShowDialogue(string text, bool append = false)
	{
		if (append) _dialogueText.AppendText("\n" + text);
		else _dialogueText.Text = text;
	}

	public void RefreshMoves(List<MoveWithPP> moves)
	{
		for (int i = 0; i < 4; i++)
		{
			if (i < moves.Count)
			{
				_moveButtons[i].Text = $"{moves[i].Move.Name}\n{moves[i].CurrentPP}/{moves[i].MaxPP}";
				_moveButtons[i].Disabled = !moves[i].CanUse;
				_moveButtons[i].Show();
			}
			else
			{
				_moveButtons[i].Hide();
			}
		}
	}

	public void ShowActionMenu(bool show)
	{
		_actionMenu.Visible = show;
		if (show)
		{
			_moveMenu.Hide();
			_itemsMenu.Hide();
		}
	}

	public void ToggleMoveMenu(bool show)
	{
		_moveMenu.Visible = show;
		_actionMenu.Visible = !show;
	}

	public void ToggleItemsMenu(bool show)
	{
		_itemsMenu.Visible = show;
		_actionMenu.Visible = !show;
	}

	public void ShowBattleEndScreen(bool win)
	{
		_actionMenu.Hide();
		_moveMenu.Hide();
		_itemsMenu.Hide();
		_battleEndScreen.Show();
		_battleEndText.Text = win ? "VICTOIRE !" : "DÉFAITE...";
	}
}
