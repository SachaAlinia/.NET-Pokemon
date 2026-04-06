using Godot;

namespace Game.Utilities;

/// <summary>
/// Machine à états qui gère les transitions entre différents états.
/// Permet de changer d'état et de contrôler le traitement des états enfants.
/// </summary>
public partial class StateMachine : Node
{
    [ExportCategory("State Machine Vars")]
    [Export]
    // Le nœud client (propriétaire) des états gérés par cette machine.
    public Node Customer;

    [Export]
    // L'état actuellement actif dans la machine.
    public State CurrentState;

    /// <summary>
    /// Initialise la machine à états au démarrage.
    /// Configure les états enfants avec leur propriétaire et désactive leur traitement.
    /// </summary>
    public override void _Ready()
    {
        // Parcourir tous les enfants pour trouver les états.
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                // Assigner le propriétaire à chaque état.
                state.StateOwner = Customer;
                // Désactiver le traitement par défaut pour tous les états.
                state.SetProcess(false);
            }
        }
    }

    /// <summary>
    /// Retourne le nom de l'état actuellement actif.
    /// </summary>
    /// <returns>Le nom de l'état courant sous forme de chaîne.</returns>
    public string GetCurrentState()
    {
        return CurrentState.Name.ToString();
    }

    /// <summary>
    /// Change l'état actuel vers un nouvel état spécifié.
    /// Sort de l'état actuel, entre dans le nouveau, et ajuste le traitement.
    /// </summary>
    /// <param name="newState">Le nouvel état à activer.</param>
    public void ChangeState(State newState)
    {
        // Sortir de l'état actuel s'il existe.
        CurrentState?.ExitState();
        // Changer vers le nouvel état.
        CurrentState = newState;
        // Entrer dans le nouvel état s'il existe.
        CurrentState?.EnterState();

        // Ajuster le traitement : seul l'état actuel doit traiter.
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                state.SetProcess(child == CurrentState);
            }
        }
    }

    /// <summary>
    /// Change l'état actuel vers un état spécifié par son nom.
    /// Sort de l'état actuel, entre dans le nouveau, et ajuste le traitement.
    /// </summary>
    /// <param name="newState">Le nom du nouvel état à activer.</param>
    public void ChangeState(string newState)
    {
        // Récupérer l'état par son nom.
        var _state = GetNode<State>(newState);

        // Sortir de l'état actuel s'il existe.
        CurrentState?.ExitState();
        // Changer vers le nouvel état.
        CurrentState = _state;
        // Entrer dans le nouvel état s'il existe.
        CurrentState?.EnterState();

        // Ajuster le traitement : seul l'état actuel doit traiter.
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                state.SetProcess(child == CurrentState);
            }
        }
    }
}
