using Game.Core;
using Game.UI;
using Game.Utilities;
using Godot;
using System;

namespace Game.Gameplay;

/// <summary>
/// État du joueur pendant l’affichage de message.
/// </summary>
public partial class PlayerMessageState : State
{
    /// <summary>
    /// Initialise l'état de message.
    /// </summary>
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

    /// <summary>
    /// Met à jour l'état de message chaque frame.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    public override void _Process(double delta)
    {
        // Avancer le texte si pas en défilement et touche use pressée.
        if (!MessageManager.Scrolling() && Input.IsActionJustReleased("use"))
        {
            MessageManager.ScrollText();
        }
    }
}
