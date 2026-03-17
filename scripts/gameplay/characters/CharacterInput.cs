using Godot;
<<<<<<< HEAD
using System;
using Game.Core;
=======
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd

namespace Game.Gameplay;

public abstract partial class CharacterInput : Node
{
    [Signal]
    public delegate void WalkEventHandler();

    [Signal]
    public delegate void TurnEventHandler();

    [ExportCategory("Common Input")]
    [Export]
    public Vector2 Direction = Vector2.Zero;

    [Export]
    public Vector2 TargetPosition = Vector2.Zero;
<<<<<<< HEAD
}
=======
}
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd
