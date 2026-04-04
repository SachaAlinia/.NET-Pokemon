using Game.Core;
using Godot;

namespace Game.Utilities;

public abstract partial class State : Node
{
    [Export] public Node StateOwner;
    [Export] public StateMachine StateMachine;

    public virtual void EnterState()
    {
        Game.Core.Logger.Info($"{StateOwner.Name} Entering {GetType().Name} state ...");
    }

    public virtual void ExitState()
    {
        Game.Core.Logger.Info($"{StateOwner.Name} Exiting {GetType().Name} state ...");
    }
}