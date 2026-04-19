using System;
using System.Collections.Generic;
using Game.Core;
using Godot;
using Godot.Collections;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

public partial class CharacterMovement : Node
{
    // Signal envoyé à l'Animation pour dire "Je marche" ou "Je m'arrête"
    [Signal] public delegate void AnimationEventHandler(string animationType);

    [ExportCategory("Nodes")]
    [Export] public Node2D Character; // L'objet physique (le corps)
    [Export] public CharacterInput CharacterInput; // Le cerveau qui donne les ordres

    [ExportCategory("Movement")]
    public Vector2 TargetPosition = Vector2.Down; // La case où on veut aller
    public bool IsWalking = false; // Est-ce qu'on est en train de glisser vers la case ?
    public ECharacterMovement ECharacterMovement = ECharacterMovement.WALKING;

    [ExportCategory("Jumping")]
    public Vector2 StartPosition; // Où on était avant de sauter
    public bool IsJumping = false; // Est-ce qu'on est en l'air ?
    public float JumpHeight = 10f; // Hauteur du petit bond 
    public float LerpSpeed = 2f; // Vitesse de la transition
    public float Progress = 0f; // 0 = début du saut, 1 = fin du saut

    private bool isPlayer = true;

    public override void _Ready()
    {
        // BRANCHEMENT : On relie les ordres du cerveau (Input) à nos fonctions.
        CharacterInput.Walk += StartMoving; // Si le cerveau dit "Marche", on lance StartMoving.
        CharacterInput.Turn += Turn;        // Si le cerveau dit "Tourne", on lance Turn.

        if (GetParent().Name != "Player") isPlayer = false;

        Logger.Info("Loading character movement component ...");
    }

    public override void _Process(double delta)
    {
        // À chaque image, on déplace le personnage s'il doit marcher ou sauter.
        Walk(delta);
        Jump(delta);

        // Si on ne bouge pas, on envoie le signal "idle" pour l'animation de repos.
        if (!IsMoving())
        {
            if (isPlayer && Modules.IsActionPressed()) return;
            EmitSignal(SignalName.Animation, "idle");
        }
    }

    public bool IsMoving() => IsWalking || IsJumping;

    /// <summary>
    /// DETECTION : Regarde ce qu'il y a sur la case visée.
    /// </summary>
    public (Vector2, Array<Dictionary>) GetTargetColliders(Vector2 targetPosition)
    {
        var spaceState = GetViewport().GetWorld2D().DirectSpaceState;

        // On vise le milieu de la case (8 pixels car la grille fait 16)
        Vector2 adjustedTargetPosition = targetPosition + new Vector2(8, 8);

        // On crée une "requête" de collision à cet endroit.
        var query = new PhysicsPointQueryParameters2D
        {
            Position = adjustedTargetPosition,
            CollisionMask = 1,
            CollideWithAreas = true,
        };

        return (adjustedTargetPosition, spaceState.IntersectPoint(query));
    }

    /// <summary>
    /// OBSTACLE : Est-ce qu'on a le droit de marcher là ?
    /// </summary>
    public bool IsTargetOccupied(Vector2 targetPosition)
    {
        var (adjustedTargetPosition, result) = GetTargetColliders(targetPosition);

        if (result.Count == 0) return false; // Rien ? Alors la case est libre !

        var collider = (Node)(GodotObject)result[0]["collider"];
        var colliderType = collider.GetType().Name;

        // Selon l'objet, on bloque ou pas :
        return colliderType switch
        {
            "Sign" => true,       // Un panneau bloque le passage
            "TallGrass" => false, // L'herbe ne bloque pas
            "TileMapLayer" => isPlayer ? GetTileMapLayerCollision((TileMapLayer)collider, adjustedTargetPosition) : true,
            "SceneTrigger" => !isPlayer, // Le joueur passe à travers les portes, mais pas les PNJ
            _ => true,
        };
    }

    /// <summary>
    /// LES CORNICHES (Ledges) : Pour sauter comme dans Pokémon.
    /// </summary>
    public bool GetTileMapLayerCollision(TileMapLayer tileMapLayer, Vector2 adjustedTargetPosition)
    {
        Vector2I tileCoordinates = tileMapLayer.LocalToMap(adjustedTargetPosition);
        TileData tileData = tileMapLayer.GetCellTileData(tileCoordinates);

        if (tileData == null) return true;

        // On regarde si la case a une donnée spéciale "LEDGE" (corniche)
        var ledgeDirection = (string)tileData.GetCustomData("LEDGE");

        if (ledgeDirection == null) return true;

        // Si on va dans la direction de la corniche (ex: vers le BAS), on autorise le mouvement mais en mode SAUT.
        if (ledgeDirection == "DOWN" && CharacterInput.Direction == Vector2.Down)
        {
            ECharacterMovement = ECharacterMovement.JUMPING;
            return false; // False = ce n'est pas un obstacle, on peut y aller !
        }
        // ... (pareil pour Gauche/Droite)
        return true;
    }

    /// <summary>
    /// DÉPART : On vérifie si on peut bouger et on lance le mouvement.
    /// </summary>
    public void StartMoving()
    {
        if (SceneManager.IsChanging) return;

        // On calcule la case d'arrivée : Position Actuelle + Direction * 16 pixels.
        TargetPosition = Character.Position + CharacterInput.Direction * Globals.GRID_SIZE;

        // Si on ne bouge pas déjà ET que la case est libre ET qu'on arrive à la réserver (pour éviter que deux PNJ se rentrent dedans).
        if (!IsMoving() && !IsTargetOccupied(TargetPosition) && SceneManager.GetCurrentLevel().ReserveTile(TargetPosition))
        {
            EmitSignal(SignalName.Animation, "walk");

            if (ECharacterMovement == ECharacterMovement.JUMPING)
            {
                Progress = 0f;
                StartPosition = Character.Position;
                // Un saut fait avancer de DEUX cases (32 pixels).
                TargetPosition = Character.Position + CharacterInput.Direction * (Globals.GRID_SIZE * 2);
                IsJumping = true;
            }
            else
            {
                IsWalking = true;
            }
        }
    }

    /// <summary>
    /// MARCHE : On déplace le personnage petit à petit.
    /// </summary>
    public void Walk(double delta)
    {
        if (IsWalking)
        {
            // MoveToward : Déplace A vers B de façon fluide.
            Character.Position = Character.Position.MoveToward(TargetPosition, (float)delta * Globals.GRID_SIZE * 4);

            if (Character.Position.DistanceTo(TargetPosition) < 1f)
            {
                StopMoving();
            }
        }
    }

    /// <summary>
    /// SAUT : On calcule une courbe pour l'effet de bond.
    /// </summary>
    public void Jump(double delta)
    {
        if (IsJumping)
        {
            Progress += LerpSpeed * (float)delta;
            // Lerp : Interpolation linéaire (on glisse de Start vers Target).
            Vector2 position = StartPosition.Lerp(TargetPosition, Progress);

            // MATHS : On ajoute un décalage sur l'axe Y (le haut) pour faire une parabole.
            float parabolicOffset = JumpHeight * (1 - 4 * (Progress - 0.5f) * (Progress - 0.5f));
            position.Y -= parabolicOffset;

            Character.Position = position;

            if (Progress >= 1f) StopMoving();
        }
    }

    public void StopMoving()
    {
        SceneManager.GetCurrentLevel().ReleaseTile(TargetPosition); // On libère la case sur la grille
        IsWalking = false;
        IsJumping = false;
        ECharacterMovement = ECharacterMovement.WALKING;
        SnapPositionToGrid(); // On se recale parfaitement sur les pixels
    }

    public void Turn() => EmitSignal(SignalName.Animation, "turn");

    /// <summary>
    /// RECALAGE : Pour être sûr de ne pas finir entre deux cases à cause des arrondis.
    /// </summary>
    public void SnapPositionToGrid()
    {
        Character.Position = new Vector2(
            Mathf.Round(Character.Position.X / Globals.GRID_SIZE) * Globals.GRID_SIZE,
            Mathf.Round(Character.Position.Y / Globals.GRID_SIZE) * Globals.GRID_SIZE
        );
    }
}