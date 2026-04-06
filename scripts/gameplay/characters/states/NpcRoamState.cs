using Game.Core;
using Game.Utilities;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

/// <summary>
/// État de déplacement libre du PNJ.
/// </summary>
public partial class NpcRoamState : State
{
    // Composant d’entrée du PNJ.
    [ExportCategory("State Vars")]
    [Export]
    public NpcInput NpcInput;

    // Composant gérant le mouvement du personnage.
    [Export]
    public CharacterMovement CharacterMovement;

    // Valeur de timer.
    private double timer = 2f;
    // Liste interne des points de patrouille en cours.
    private Array<Vector2> currentPatrolPoints = [];

    /// <summary>
    /// Met à jour l'état de déplacement libre du PNJ chaque frame.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    public override void _Process(double delta)
    {
        // Ne rien faire si en mouvement.
        if (CharacterMovement.IsMoving())
            return;

        // Gérer selon le type de mouvement.
        switch (NpcInput.Config.NpcMovementType)
        {
            case NpcMovementType.Wander:
                HandleWander(delta, NpcInput.Config.WanderMoveInterval);
                break;
            case NpcMovementType.LookAround:
                HandleLookAround(delta, NpcInput.Config.LookAroundInterval);
                break;
            case NpcMovementType.Patrol:
                HandlePatrol(delta, NpcInput.Config.PatrolMoveInterval);
                break;
        }
    }

    /// <summary>
    /// Gère le mouvement d'errance du PNJ.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    /// <param name="interval">Intervalle entre les mouvements.</param>
    private void HandlePatrol(double delta, double interval)
    {
        // Ne rien faire si pas de points de patrouille.
        if (NpcInput.Config.PatrolPoints.Count == 0)
            return;

        timer -= delta;

        if (timer > 0)
            return;

        Vector2 currentPosition = ((Npc)StateOwner).Position;
        var level = SceneManager.GetCurrentLevel();

        // Calculer le chemin si pas de points courants.
        if (currentPatrolPoints.Count == 0)
        {
            var patrolPoint = NpcInput.Config.PatrolPoints[NpcInput.Config.PatrolIndex];
            NpcInput.Config.PatrolIndex = (NpcInput.Config.PatrolIndex + 1) % NpcInput.Config.PatrolPoints.Count;

            var pathing = level.Grid.GetIdPath(Modules.ConvertVector2ToVector2I(currentPosition), Modules.ConvertVector2ToVector2I(patrolPoint));

            for (int i = 1; i < pathing.Count; i++)
            {
                var point = pathing[i];
                currentPatrolPoints.Add(Modules.ConvertVector2IToVector2(point));
            }

            level.CurrentPatrolPoints = currentPatrolPoints;

            if (currentPatrolPoints.Count == 0)
                return;
        }

        // Avancer vers le prochain point.
        if (((Npc)StateOwner).Position.DistanceTo(currentPatrolPoints[0]) < 1f)
        {
            currentPatrolPoints.RemoveAt(0);
            return;
        }

        NpcInput.TargetPosition = currentPatrolPoints[0];
        level.TargetPosition = NpcInput.TargetPosition;

        // Déterminer la direction.
        Vector2 difference = NpcInput.TargetPosition - currentPosition;

        if (Mathf.Abs(difference.X) > Mathf.Abs(difference.Y))
        {
            NpcInput.Direction = difference.X > 0 ? Vector2.Right : Vector2.Left;
        }
        else
        {
            NpcInput.Direction = difference.Y > 0 ? Vector2.Down : Vector2.Up;
        }

        NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
        timer = interval;
    }

    /// <summary>
    /// Gère le mouvement d'errance du PNJ.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    /// <param name="interval">Intervalle entre les mouvements.</param>
    private void HandleWander(double delta, double interval)
    {
        timer -= delta;

        if (timer > 0)
            return;

        var (direction, targetPosition) = GetNewDirections();

        NpcInput.Direction = direction;
        NpcInput.TargetPosition = targetPosition;

        NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
        timer = interval;
    }

    /// <summary>
    /// Gère le mouvement de regard autour du PNJ.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    /// <param name="interval">Intervalle entre les mouvements.</param>
    private void HandleLookAround(double delta, double interval)
    {
        timer -= delta;

        if (timer > 0)
            return;

        var (direction, targetPosition) = GetNewDirections();

        // Ne tourner que si direction différente.
        if (direction == NpcInput.Direction)
        {
            timer = interval;
            return;
        }

        NpcInput.Direction = direction;
        NpcInput.TargetPosition = targetPosition;

        NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
        timer = interval;
    }

    /// <summary>
    /// Génère une nouvelle direction et position cible pour le PNJ.
    /// </summary>
    /// <returns>Un tuple avec la direction et la position cible.</returns>
    private (Vector2, Vector2) GetNewDirections()
    {
        Vector2[] directions = [Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right];
        Vector2 chosenDirection;

        int tries = 0;

        do
        {
            chosenDirection = directions[Globals.GetRandomNumberGenerator().RandiRange(0, directions.Length - 1)];
            Vector2 nextPosition = CharacterMovement.Character.Position + chosenDirection * Globals.GRID_SIZE;

            // Pour l'errance, vérifier le rayon.
            if (NpcInput.Config.NpcMovementType == NpcMovementType.Wander)
            {
                float distanceFromOrigin = nextPosition.DistanceTo(NpcInput.Config.WanderOrigin);
                if (distanceFromOrigin <= NpcInput.Config.WanderRadius)
                    break;
            }
            else
            {
                break;
            }

            tries++;
        } while (tries < 10);

        return (chosenDirection, chosenDirection * Globals.GRID_SIZE);
    }

}
