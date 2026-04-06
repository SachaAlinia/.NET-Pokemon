using Game.Core;
using Godot;

namespace Game.Gameplay;

/// <summary>
/// Composant de gestion des animations du personnage.
/// </summary>
public partial class CharacterAnimation : AnimatedSprite2D
{
    [ExportCategory("Nodes")]
    [Export]
    // Référence au composant d'entrée du personnage.
    public CharacterInput CharacterInput;

    [Export]
    // Référence au composant de mouvement du personnage.
    public CharacterMovement CharacterMovement;

    [ExportCategory("Animations Vars")]
    [Export]
    // Animation courante du personnage.
    public ECharacterAnimation ECharacterAnimation = ECharacterAnimation.idle_down;

    /// <summary>
    /// Initialise le composant d'animation au démarrage.
    /// </summary>
    public override void _Ready()
    {
        // Connecter le signal d'animation du mouvement.
        CharacterMovement.Animation += PlayAnimation;

        Game.Core.Logger.Info("Loading player animation component ...");
    }

    /// <summary>
    /// Joue l'animation appropriée selon le type et la direction.
    /// </summary>
    /// <param name="animationType">Type d'animation demandé (walk, turn, idle).</param>
    public void PlayAnimation(string animationType)
    {
        ECharacterAnimation previousAnimation = ECharacterAnimation;

        // Ne pas changer d'animation si en mouvement.
        if (CharacterMovement.IsMoving())
            return;

        // Sélectionner l'animation selon le type et la direction.
        switch (animationType)
        {
            case "walk":
                if (CharacterInput.Direction == Vector2.Up)
                {
                    ECharacterAnimation = ECharacterAnimation.walk_up;
                }
                else if (CharacterInput.Direction == Vector2.Down)
                {
                    ECharacterAnimation = ECharacterAnimation.walk_down;
                }
                else if (CharacterInput.Direction == Vector2.Left)
                {
                    ECharacterAnimation = ECharacterAnimation.walk_left;
                }
                else if (CharacterInput.Direction == Vector2.Right)
                {
                    ECharacterAnimation = ECharacterAnimation.walk_right;
                }
                break;
            case "turn":
                if (CharacterInput.Direction == Vector2.Up)
                {
                    ECharacterAnimation = ECharacterAnimation.turn_up;
                }
                else if (CharacterInput.Direction == Vector2.Down)
                {
                    ECharacterAnimation = ECharacterAnimation.turn_down;
                }
                else if (CharacterInput.Direction == Vector2.Left)
                {
                    ECharacterAnimation = ECharacterAnimation.turn_left;
                }
                else if (CharacterInput.Direction == Vector2.Right)
                {
                    ECharacterAnimation = ECharacterAnimation.turn_right;
                }
                break;
            case "idle":
                if (CharacterInput.Direction == Vector2.Up)
                {
                    ECharacterAnimation = ECharacterAnimation.idle_up;
                }
                else if (CharacterInput.Direction == Vector2.Down)
                {
                    ECharacterAnimation = ECharacterAnimation.idle_down;
                }
                else if (CharacterInput.Direction == Vector2.Left)
                {
                    ECharacterAnimation = ECharacterAnimation.idle_left;
                }
                else if (CharacterInput.Direction == Vector2.Right)
                {
                    ECharacterAnimation = ECharacterAnimation.idle_right;
                }
                break;
        }

        // Jouer l'animation si elle a changé.
        if (previousAnimation != ECharacterAnimation)
        {
            Play(ECharacterAnimation.ToString());
        }
    }
}