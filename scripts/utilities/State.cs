using Godot;
using System;
using Game.Core;
using Game.Utilities;

namespace Game.Utilities
{
    public abstract partial class State : Node
    {
        [Export] public Node StateOwner;
        public virtual void EnterState()
        {
            Game.Core.Logger.Info($"Entering {GetType().Name} state...");
        }
        public virtual void ExitState()
        {
            Game.Core.Logger.Info($"Exiting {GetType().Name} state...");
        }
    }
}