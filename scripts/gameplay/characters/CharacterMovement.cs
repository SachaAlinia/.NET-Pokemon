using System;
using System.Collections.Generic;
using Game.Core;
using Godot;
using Godot.Collections;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Gère le mouvement du personnage.
/// </summary>
public partial class CharacterMovement : Node
{
    [Signal]
    public delegate void AnimationEventHandler(string animationType);

    [ExportCategory("Nodes")]
    [Export]
    // Référence au nœud représentant le personnage.
    public Node2D Character;

    [Export]
    // Composant qui fournit les commandes de déplacement.
    public CharacterInput CharacterInput;

    [ExportCategory("Movement")]
    [Export]
    // Position cible vers laquelle le personnage se dirige.
    public Vector2 TargetPosition = Vector2.Down;

    [Export]
    // Indique si le personnage est en train de marcher.
    public bool IsWalking = false;

    [Export]
    // Animation de mouvement actuelle du personnage.
    public ECharacterMovement ECharacterMovement = ECharacterMovement.WALKING;

    [ExportCategory("Jumping")]
    [Export]
    // Position de départ du saut.
    public Vector2 StartPosition;

    [Export]
    // Indique si le personnage est en train de sauter.
    public bool IsJumping = false;

    [Export]
    // Hauteur du saut.
    public float JumpHeight = 10f;

    [Export]
    // Vitesse de l'interpolation du saut et du déplacement.
    public float LerpSpeed = 2f;

    [Export]
    // Progression du mouvement ou du saut.
    public float Progress = 0f;

    // Indique si ce composant appartient au joueur.
    private bool isPlayer = true;

    /// <summary>
    /// Initialise le composant de mouvement et lie les événements d'entrée.
    /// </summary>
    public override void _Ready()
    {
        // Connecter les signaux d'entrée.
        CharacterInput.Walk += StartMoving;
        CharacterInput.Turn += Turn;

        // Déterminer si c'est le joueur.
        if (GetParent().Name != "Player")
            isPlayer = false;

        Logger.Info("Loading character movement component ...");
    }

    /// <summary>
    /// Met à jour le mouvement du personnage à chaque frame.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière image.</param>
    public override void _Process(double delta)
    {
        // Mettre à jour la marche et le saut.
        Walk(delta);
        Jump(delta);

        // Si pas en mouvement, jouer l'animation idle si applicable.
        if (!IsMoving())
        {
            if (isPlayer)
            {
                if (Modules.IsActionPressed())
                    return;
            }

            EmitSignal(SignalName.Animation, "idle");
        }
    }

    /// <summary>
    /// Vérifie si le personnage est en mouvement.
    /// </summary>
    /// <returns>True si le personnage marche ou saute.</returns>
    public bool IsMoving()
    {
        return IsWalking || IsJumping;
    }

    /// <summary>
    /// Récupère les colliders présents à la position cible.
    /// </summary>
    /// <param name="targetPosition">Position cible en pixels.</param>
    public (Vector2, Array<Dictionary>) GetTargetColliders(Vector2 targetPosition)
    {
        var spaceState = GetViewport().GetWorld2D().DirectSpaceState;

        // Ajuster la position pour le centre de la cellule.
        Vector2 adjustedTargetPosition = targetPosition;
        adjustedTargetPosition.X += 8;
        adjustedTargetPosition.Y += 8;

        var query = new PhysicsPointQueryParameters2D
        {
            Position = adjustedTargetPosition,
            CollisionMask = 1,
            CollideWithAreas = true,
        };

        return (adjustedTargetPosition, spaceState.IntersectPoint(query));
    }

    /// <summary>
    /// Détermine si la position cible est occupée par un obstacle.
    /// </summary>
    /// <param name="targetPosition">Position cible en pixels.</param>
    public bool IsTargetOccupied(Vector2 targetPosition)
    {
        var (adjustedTargetPosition, result) = GetTargetColliders(targetPosition);

        if (result.Count == 0)
        {
            return false;
        }
        else if (result.Count == 1)
        {
            var collider = (Node)(GodotObject)result[0]["collider"];
            var colliderType = collider.GetType().Name;

            // Logique spécifique selon le type de collider.
            return colliderType switch
            {
                "Sign" => true,
                "TallGrass" => false,
                "TileMapLayer" => isPlayer ? GetTileMapLayerCollision((TileMapLayer)collider, adjustedTargetPosition) : true,
                "SceneTrigger" => !isPlayer,
                _ => true,
            };
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Vérifie la collision sur une couche de tiles et gère les sauts sur corniche.
    /// </summary>
    /// <param name="tileMapLayer">Couche de tiles à tester.</param>
    /// <param name="adjustedTargetPosition">Position ajustée pour le test.</param>
    public bool GetTileMapLayerCollision(TileMapLayer tileMapLayer, Vector2 adjustedTargetPosition)
    {
        Vector2I tileCoordinates = tileMapLayer.LocalToMap(adjustedTargetPosition);
        TileData tileData = tileMapLayer.GetCellTileData(tileCoordinates);

        if (tileData == null)
            return true;

        var ledgeDirection = (string)tileData.GetCustomData("LEDGE");

        if (ledgeDirection == null)
            return true;

        Logger.Info(ledgeDirection);

        // Gérer les sauts selon la direction de la corniche.
        switch (ledgeDirection)
        {
            case "DOWN":
                if (CharacterInput.Direction == Vector2.Down)
                {
                    ECharacterMovement = ECharacterMovement.JUMPING;
                    return false;
                }
                break;
            case "LEFT":
                if (CharacterInput.Direction == Vector2.Left)
                {
                    ECharacterMovement = ECharacterMovement.JUMPING;
                    return false;
                }
                break;
            case "RIGHT":
                if (CharacterInput.Direction == Vector2.Right)
                {
                    ECharacterMovement = ECharacterMovement.JUMPING;
                    return false;
                }
                break;
        }

        return true;
    }

    /// <summary>
    /// Lance le déplacement si la case cible est libre.
    /// </summary>
    public void StartMoving()
    {
        // Ne pas bouger si changement de niveau en cours.
        if (SceneManager.IsChanging)
            return;

        TargetPosition = Character.Position + CharacterInput.Direction * Globals.GRID_SIZE;

        // Vérifier si on peut bouger.
        if (!IsMoving() && !IsTargetOccupied(TargetPosition) && SceneManager.GetCurrentLevel().ReserveTile(TargetPosition))
        {
            EmitSignal(SignalName.Animation, "walk");
            Logger.Info($"{GetParent().Name} moving from {Character.Position} to {TargetPosition}");

            // Gérer le saut si nécessaire.
            if (ECharacterMovement == ECharacterMovement.JUMPING)
            {
                Progress = 0f;
                StartPosition = Character.Position;
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
    /// Déplace progressivement le personnage vers la position cible.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière image.</param>
    public void Walk(double delta)
    {
        if (IsWalking)
        {
            Character.Position = Character.Position.MoveToward(TargetPosition, (float)delta * Globals.GRID_SIZE * 4);

            // Arrêter si proche de la cible.
            if (Character.Position.DistanceTo(TargetPosition) < 1f)
            {
                StopMoving();
            }
        }
    }

    /// <summary>
    /// Anime le saut du personnage en suivant une trajectoire parabolique.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière image.</param>
    public void Jump(double delta)
    {
        if (IsJumping)
        {
            Progress += LerpSpeed * (float)delta;

            Vector2 position = StartPosition.Lerp(TargetPosition, Progress);

            // Calculer l'offset parabolique pour l'effet de saut.
            float parabolicOffset = JumpHeight * (1 - 4 * (Progress - 0.5f) * (Progress - 0.5f));

            position.Y -= parabolicOffset;

            Character.Position = position;

            // Arrêter le saut à la fin.
            if (Progress >= 1f)
            {
                StopMoving();
            }
        }
    }

    /// <summary>
    /// Arrête le déplacement et relâche la case réservée.
    /// </summary>
    public void StopMoving()
    {
        SceneManager.GetCurrentLevel().ReleaseTile(TargetPosition);
        IsWalking = false;
        IsJumping = false;
        ECharacterMovement = ECharacterMovement.WALKING;
        SnapPositionToGrid();
    }

    /// <summary>
    /// Déclenche l'animation de rotation du personnage.
    /// </summary>
    public void Turn()
    {
        EmitSignal(SignalName.Animation, "turn");
    }

    /// <summary>
    /// Aligne la position du personnage sur la grille du jeu.
    /// </summary>
    public void SnapPositionToGrid()
    {
        Character.Position = new Vector2(
            Mathf.Round(Character.Position.X / Globals.GRID_SIZE) * Globals.GRID_SIZE,
            Mathf.Round(Character.Position.Y / Globals.GRID_SIZE) * Globals.GRID_SIZE
        );
    }
}