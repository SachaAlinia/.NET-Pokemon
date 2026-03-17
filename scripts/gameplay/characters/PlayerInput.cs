using Godot;
using System;
using Game.Core;

namespace Game.Gameplay;

public partial class PlayerInput : CharacterInput
{
    [ExportCategory("Player Input")]
    [Export]
    public double HoldThreshold = 0.2f;

    [Export]
    public double HoldTime = 0.0f;

    public override void _Ready()
    {
		Game.Core.Logger.Info("Loading player input component ...");
    }
}