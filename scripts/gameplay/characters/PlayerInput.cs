<<<<<<< HEAD
using Godot;
using System;
using Game.Core;
=======
using Game.Core;
using Godot;
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd

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