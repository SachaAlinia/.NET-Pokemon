using Game.Utilities;
using Godot;

namespace Game.Gameplay;

/// <summary>
/// Point d’entrée du joueur dans le jeu.
/// </summary>
public partial class Player : CharacterBody2D
{
// Machine à états utilisée par ce joueur ou PNJ.
    [Export]
    public StateMachine StateMachine;

    public override void _Ready()
    {
        StateMachine.Customer = this;
        StateMachine.ChangeState(StateMachine.GetNode<State>("Roam"));
    }
}
