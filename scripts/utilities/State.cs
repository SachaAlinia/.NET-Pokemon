using Game.Core;
using Godot;

namespace Game.Utilities;

/// <summary>
/// Classe abstraite de base pour les états dans une machine à états.
/// Fournit des méthodes virtuelles pour entrer et sortir d'un état.
/// </summary>
public abstract partial class State : Node
{
    [Export]
    // Le nœud propriétaire de cet état (par exemple, un personnage ou un objet).
    public Node StateOwner;

    [Export]
    // La machine à états qui gère cet état.
    public StateMachine StateMachine;

    /// <summary>
    /// Méthode appelée lorsque l'état est activé.
    /// Log l'entrée dans l'état pour le débogage.
    /// </summary>
    public virtual void EnterState()
    {
        Game.Core.Logger.Info($"{StateOwner.Name} Entering {GetType().Name} state ...");
    }

    /// <summary>
    /// Méthode appelée lorsque l'état est désactivé.
    /// Log la sortie de l'état pour le débogage.
    /// </summary>
    public virtual void ExitState()
    {
        Game.Core.Logger.Info($"{StateOwner.Name} Exiting {GetType().Name} state ...");
    }
}