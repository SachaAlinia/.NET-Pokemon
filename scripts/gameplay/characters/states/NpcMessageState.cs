using Game.Core;
using Game.Utilities;

namespace Game.Gameplay;

/// <summary>
/// État du PNJ pendant l’affichage de message.
/// </summary>
public partial class NpcMessageState : State
{
    public override void _Ready()
    {
        Signals.Instance.MessageBoxOpen += (value) =>
        {
            if (!value)
            {
                StateMachine.ChangeState("Roam");
            }
        };
    }
}
