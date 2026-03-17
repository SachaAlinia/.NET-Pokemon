using Godot;
using System;
using Game.Core;
using Game.Utilities;

namespace Game.Gameplay;

public partial class Player : CharacterBody2D
{
    [Export] public StateMachine StateMachine;

    public override void _Ready()
    {	
		//GD.Print(Character.Name);
        StateMachine.ChangeState(StateMachine.GetNode<State>("Roam"));
		
    }
}