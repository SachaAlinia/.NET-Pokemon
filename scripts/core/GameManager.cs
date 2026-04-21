using Game.Gameplay;
using Game.UI;
using Godot;
using System.Collections.Generic;

namespace Game.Core;

public partial class GameManager : Node
{
	// Singleton pour accès rapide
	public static GameManager Instance { get; private set; }

	[ExportCategory("Nodes")]
	[Export]
	public SubViewport GameViewPort; // Zone de rendu du monde du jeu

	[ExportCategory("Vars")]
	[Export]
	public Player Player; // Référence vers l'objet Joueur

	/// <summary>
	/// Initialise l'instance et lance le premier niveau.
	/// </summary>
	public override void _Ready()
	{
		Instance = this;

		Logger.Info("Loading game manager ...");

		// Demande au SceneManager de charger le niveau par défaut au démarrage
		SceneManager.ChangeLevel(spawn: true);
	}

	/// <summary>
	/// Permet de récupérer le viewport de n'importe où.
	/// </summary>
	public static SubViewport GetGameViewPort()
	{
		return Instance.GameViewPort;
	}

	/// <summary>
	/// Enregistre le joueur dans le système et l'ajoute au monde.
	/// </summary>
	public static Player AddPlayer(Player player)
	{
		Instance.GameViewPort.AddChild(player);
		Instance.Player = player;
		return Instance.Player;
	}

	/// <summary>
	/// Permet de récupérer l'objet joueur de n'importe où.
	/// </summary>
	public static Player GetPlayer()
	{
		return Instance.Player;
	}

	// Dans GameManager.cs
	public static Dictionary<ItemResource, int> Inventory = new();

	public static void AddItem(ItemResource item, int amount = 1)
	{
		if (Inventory.ContainsKey(item))
			Inventory[item] += amount;
		else
			Inventory[item] = amount;
	}
}
