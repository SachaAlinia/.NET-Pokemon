using Game.Core;
using Game.Utilities;
using Godot;

namespace Game.Gameplay;

/// <summary>
/// État de déplacement libre du joueur.
/// </summary>
public partial class PlayerRoamState : State
{
    // Player input.
    [ExportCategory("State Vars")]
    [Export]
    public PlayerInput PlayerInput;

    // Composant gérant le mouvement du personnage.
    [Export]
    public CharacterMovement CharacterMovement;

    /// <summary>
    /// Initialise l'état de déplacement libre.
    /// </summary>
    public override void _Ready()
    {
        Signals.Instance.MessageBoxOpen += (value) =>
        {
            if (value)
            {
                StateMachine.ChangeState("Message");
            }
        };
    }

    /// <summary>
    /// Met à jour l'état chaque frame.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    public override void _Process(double delta)
    {
        GetInputDirection();
        GetInput(delta);
        GetUseInput();
    }

    /// <summary>
    /// Lit la direction d'entrée du joueur.
    /// </summary>
    public void GetInputDirection()
    {
        if (Input.IsActionJustPressed("ui_up"))
        {
            PlayerInput.Direction = Vector2.Up;
            PlayerInput.TargetPosition = new Vector2(0, -Globals.GRID_SIZE);
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            PlayerInput.Direction = Vector2.Down;
            PlayerInput.TargetPosition = new Vector2(0, Globals.GRID_SIZE);
        }
        else if (Input.IsActionJustPressed("ui_left"))
        {
            PlayerInput.Direction = Vector2.Left;
            PlayerInput.TargetPosition = new Vector2(-Globals.GRID_SIZE, 0);
        }
        else if (Input.IsActionJustPressed("ui_right"))
        {
            PlayerInput.Direction = Vector2.Right;
            PlayerInput.TargetPosition = new Vector2(Globals.GRID_SIZE, 0);
        }
    }

    /// <summary>
    /// Gère l'entrée de mouvement du joueur.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    public void GetInput(double delta)
    {
        // Ne rien faire si en mouvement.
        if (CharacterMovement.IsMoving())
            return;

        if (Modules.IsActionJustReleased())
        {
            // Décider entre marcher ou tourner selon la durée de maintien.
            if (PlayerInput.HoldTime > PlayerInput.HoldThreshold)
            {
                PlayerInput.EmitSignal(CharacterInput.SignalName.Walk);
            }
            else
            {
                PlayerInput.EmitSignal(CharacterInput.SignalName.Turn);
            }

            PlayerInput.HoldTime = 0.0f;
        }

        if (Modules.IsActionPressed())
        {
            PlayerInput.HoldTime += delta;

            if (PlayerInput.HoldTime > PlayerInput.HoldThreshold)
            {
                PlayerInput.EmitSignal(CharacterInput.SignalName.Walk);
            }
        }
    }

    /// <summary>
    /// Gère l'entrée d'interaction (touche use).
    /// </summary>
    public void GetUseInput()
    {
        if (Input.IsActionJustReleased("use"))
        {
            // Vérifier les colliders devant le joueur.
            var (_, result) = CharacterMovement.GetTargetColliders((PlayerInput.Direction * Globals.GRID_SIZE) + ((Player)StateOwner).Position);

            foreach (var collision in result)
            {
                var collider = (Node)(GodotObject)collision["collider"];
                var colliderType = collider.GetType().Name;

                // Interagir selon le type de collider.
                switch (colliderType)
                {
                    case "Sign":
                        ((Sign)collider).PlayMessage();
                        break;
                    case "Npc":
                        ((Npc)collider).PlayMessage(PlayerInput.Direction);
                        break;
                }
            }
        }
    }
}
