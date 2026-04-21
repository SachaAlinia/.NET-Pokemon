using System.Collections.Generic;
using Game.Core;
using Godot;
using Godot.Collections;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Représente un niveau du jeu.
/// </summary>
public partial class Level : Node2D
{
	// Nom du niveau.
	[ExportCategory("Level Basics")]
	[Export]
	public LevelName LevelName;

	// Taux de rencontre de Pokémon dans ce niveau.
	[Export(PropertyHint.Range, "0,100")]
	public int EncounterRate;

	// Limite haute de la caméra.
	[ExportCategory("Camera Limits")]
	[Export]
	public int Top;

	// Limite basse de la caméra.
	[Export]
	public int Bottom;

	// Limite gauche de la caméra.
	[Export]
	public int Left;

	// Limite droite de la caméra.
	[Export]
	public int Right;

	// Active l’affichage des aides de debug.
	[ExportCategory("Debugging")]
	[Export]
	public bool DebugLayerOn = false;

	[Export] public Array<PokemonResource> WildPokemons; // Glisse tes .tres (Pikachu, Arbok, etc.) ici

	// Cases réservées pour les déplacements en cours.
	private readonly HashSet<Vector2> reserverdTiles = [];

	// Grille de pathfinding du niveau.
	public AStarGrid2D Grid;
	// Position cible du personnage.
	public Vector2 TargetPosition = Vector2.Zero;
	// Points de patrouille actuellement actifs.
	public Array<Vector2> CurrentPatrolPoints = [];

	/// <summary>
	/// Initialise le niveau au démarrage.
	/// </summary>
	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");

		// Configurer le layer de debug si présent.
		var debugLayer = GetNodeOrNull<LevelDebugger>("DebugLayer");

		if (debugLayer != null)
		{
			debugLayer.DebugOn = DebugLayerOn;
		}
		else
		{
			Logger.Info($"Note: No DebugLayer found in {LevelName}. Skipping debug setup.");
		}
	}

	/// <summary>
	/// Met à jour le niveau chaque frame.
	/// </summary>
	/// <param name="delta">Temps écoulé depuis la dernière frame.</param>
	public override void _Process(double delta)
	{
		// Initialiser la grille si elle n'existe pas et que le joueur est présent.
		if (Grid == null && GameManager.GetPlayer() != null)
		{
			SetupGrid();
		}
	}

	/// <summary>
	/// Configure la grille A* pour le pathfinding.
	/// </summary>
	public void SetupGrid()
	{
		Logger.Info("Setting up A* Grid ...");

		// Créer la grille avec les paramètres appropriés.
		Grid = new()
		{
			Region = new Rect2I(0, 0, Right, Bottom),
			CellSize = new Vector2(Globals.GRID_SIZE, Globals.GRID_SIZE),
			DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never
		};

		Grid.Update();

		var mapHeight = Bottom / Globals.GRID_SIZE;
		var mapWidth = Right / Globals.GRID_SIZE;

		// Parcourir chaque cellule pour marquer les obstacles.
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				Vector2I cell = new(x, y);
				Vector2 worldPosition = new(x * Globals.GRID_SIZE, y * Globals.GRID_SIZE);

				// Vérifier les collisions à cette position.
				var (_, collisions) = GameManager.GetPlayer().GetNode<CharacterMovement>("Movement").GetTargetColliders(worldPosition);

				foreach (var collision in collisions)
				{
					var collider = (Node)(GodotObject)collision["collider"];
					var colliderType = collider.GetType().Name;

					// Ignorer certains types de colliders.
					if (colliderType == "TallGrass" || colliderType == "Player")
					{
						continue;
					}

					if (colliderType == "Npc")
					{
						// PNJ en patrouille ou errance peuvent être traversés.
						switch (((Npc)collider).NpcInputConfig.NpcMovementType)
						{
							case NpcMovementType.Patrol:
								continue;
							case NpcMovementType.Wander:
								continue;
						}
					}

					// Marquer la cellule comme solide.
					Grid.SetPointSolid(cell, true);
				}
			}
		}
	}

	/// <summary>
	/// Réserve une case pour un déplacement.
	/// </summary>
	/// <param name="position">Position de la case à réserver.</param>
	/// <returns>True si la réservation a réussi.</returns>
	public bool ReserveTile(Vector2 position)
	{
		if (reserverdTiles.Contains(position))
			return false;

		reserverdTiles.Add(position);
		return true;
	}

	/// <summary>
	/// Vérifie si une case est libre.
	/// </summary>
	/// <param name="position">Position de la case à vérifier.</param>
	/// <returns>True si la case est libre.</returns>
	public bool IsTileFree(Vector2 position)
	{
		return !reserverdTiles.Contains(position);
	}

	/// <summary>
	/// Libère une case réservée.
	/// </summary>
	/// <param name="position">Position de la case à libérer.</param>
	public void ReleaseTile(Vector2 position)
	{
		reserverdTiles.Remove(position);
	}
}