using Game.Core;
using Godot;

namespace Game.Gameplay;

/// <summary>
/// Ce composant gère les images. Il hérite de 'AnimatedSprite2D', 
/// donc il possède déjà toutes les fonctions pour jouer des animations.
/// </summary>
public partial class CharacterAnimation : AnimatedSprite2D
{
    // --- CONNEXIONS ---
    [ExportCategory("Nodes")]
    [Export] public CharacterInput CharacterInput; // Pour savoir dans quelle direction on regarde
    [Export] public CharacterMovement CharacterMovement; // Pour savoir si on est en train de bouger

    [ExportCategory("Animations Vars")]
    [Export]
    // Stocke l'état actuel (ex: idle_down). Utilise l'Enum qu'on a vu plus tôt.
    public ECharacterAnimation ECharacterAnimation = ECharacterAnimation.idle_down;

    /// <summary>
    /// Se lance au démarrage.
    /// </summary>
    public override void _Ready()
    {
        // ON ÉCOUTE LE MOUVEMENT : 
        // Dès que le script de mouvement dit "Hé, je change d'animation", 
        // il appelle notre fonction 'PlayAnimation'.
        CharacterMovement.Animation += PlayAnimation;
    }

    /// <summary>
    /// Choisit la bonne image à afficher.
    /// </summary>
    /// <param name="animationType">Le mot envoyé : "walk", "turn" ou "idle".</param>
    public void PlayAnimation(string animationType)
    {
        // On mémorise l'ancienne animation pour voir si elle change.
        ECharacterAnimation previousAnimation = ECharacterAnimation;

        // SÉCURITÉ : Si le personnage est déjà en train de glisser vers une case, 
        // on ne change pas son image au milieu du chemin.
        if (CharacterMovement.IsMoving())
            return;

        // LE TRIEUR (Switch) : Selon le mot reçu, on regarde la direction.
        switch (animationType)
        {
            case "walk": // Si on marche
                if (CharacterInput.Direction == Vector2.Up) ECharacterAnimation = ECharacterAnimation.walk_up;
                else if (CharacterInput.Direction == Vector2.Down) ECharacterAnimation = ECharacterAnimation.walk_down;
                else if (CharacterInput.Direction == Vector2.Left) ECharacterAnimation = ECharacterAnimation.walk_left;
                else if (CharacterInput.Direction == Vector2.Right) ECharacterAnimation = ECharacterAnimation.walk_right;
                break;

            case "turn": // Si on pivote sur place
                if (CharacterInput.Direction == Vector2.Up) ECharacterAnimation = ECharacterAnimation.turn_up;
                else if (CharacterInput.Direction == Vector2.Down) ECharacterAnimation = ECharacterAnimation.turn_down;
                else if (CharacterInput.Direction == Vector2.Left) ECharacterAnimation = ECharacterAnimation.turn_left;
                else if (CharacterInput.Direction == Vector2.Right) ECharacterAnimation = ECharacterAnimation.turn_right;
                break;

            case "idle": // Si on ne fait rien
                if (CharacterInput.Direction == Vector2.Up) ECharacterAnimation = ECharacterAnimation.idle_up;
                else if (CharacterInput.Direction == Vector2.Down) ECharacterAnimation = ECharacterAnimation.idle_down;
                else if (CharacterInput.Direction == Vector2.Left) ECharacterAnimation = ECharacterAnimation.idle_left;
                else if (CharacterInput.Direction == Vector2.Right) ECharacterAnimation = ECharacterAnimation.idle_right;
                break;
        }

        // Si l'animation est différente de la précédente, on demande à Godot de la jouer.
        if (previousAnimation != ECharacterAnimation)
        {
            // .ToString() transforme l'Enum (walk_up) en texte ("walk_up") pour Godot.
            Play(ECharacterAnimation.ToString());
        }
    }
}