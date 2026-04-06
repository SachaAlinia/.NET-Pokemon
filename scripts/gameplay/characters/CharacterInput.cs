using Godot;

namespace Game.Gameplay;

/// <summary>
/// Représente l’entrée de contrôle du personnage.
/// </summary>
public abstract partial class CharacterInput : Node
{
    [Signal]
    public delegate void WalkEventHandler();

    [Signal]
    public delegate void TurnEventHandler();

    [ExportCategory("Common Input")]
    [Export]
    // Direction actuelle de l'entrée du personnage.
    public Vector2 Direction = Vector2.Zero;

    [Export]
    // Position vers laquelle le personnage doit se déplacer.
    public Vector2 TargetPosition = Vector2.Zero;
}
