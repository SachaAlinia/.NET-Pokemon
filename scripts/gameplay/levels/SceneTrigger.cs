using Game.Core;
using Godot;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Déclenche le chargement d'un autre niveau lorsque le joueur entre.
/// </summary>
public partial class SceneTrigger : Area2D
{
	// Niveau cible du déclencheur.
	[ExportCategory("Target Scene Vars")]
	[Export]
	public LevelName TargetLevelName;

	// Point de départ dans le niveau cible.
	[Export]
	public int TargetLevelTrigger = 0;

	// Point de trigger associé au niveau actuel.
	[ExportCategory("Current Scene Vars")]
	[Export]
	public int CurrentLevelTrigger = 0;

	// Direction d'entrée du joueur dans le nouveau niveau.
	[Export]
	public Vector2 EntryDirection;

	// Indique si le déclencheur est verrouillé.
	[Export]
	public bool Locked = false;

	/// <summary>
	/// Initialise le déclencheur en connectant le signal de collision.
	/// </summary>
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	/// <summary>
	/// Gère l'entrée d'un corps dans la zone du déclencheur.
	/// Change de niveau si c'est le joueur et que le déclencheur n'est pas verrouillé.
	/// </summary>
	/// <param name="body">Le nœud qui est entré dans la zone.</param>
	public void OnBodyEntered(Node2D body)
	{
		// Vérifier si c'est le joueur qui entre.
		if (body.Name != "Player")
			return;

		// Si verrouillé, afficher un message d'erreur.
		if (Locked)
			Logger.Info("Uh oh!  The door is locked ...");

		// Changer de niveau vers la cible.
		SceneManager.ChangeLevel(levelName: TargetLevelName, trigger: TargetLevelTrigger);
	}

	/// <summary>
	/// Ajoute le déclencheur au groupe des triggers de scène lors de l'entrée dans l'arbre.
	/// </summary>
	public override void _EnterTree()
	{
		AddToGroup(LevelGroup.SCENETRIGGERS.ToString());
	}

	/// <summary>
	/// Retire le déclencheur du groupe des triggers de scène lors de la sortie de l'arbre.
	/// </summary>
	public override void _ExitTree()
	{
		RemoveFromGroup(LevelGroup.SCENETRIGGERS.ToString());
	}
}