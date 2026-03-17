using Godot;
using System;
using Game.Core; 
using Game.Utilities;

namespace Game.Gameplay
{
    public partial class CharacterMovement : Node
    {
        [Signal] public delegate void AnimationEventHandler(string animationType);

        [ExportCategory("Nodes")]
        [Export] public Node2D Character;
        [Export] public CharacterInput CharacterInput;
        [Export] public CharacterCollisionRayCast CharacterCollisionRayCast;

        [ExportCategory("Movement")]
        [Export] public Vector2 TargetPosition = Vector2.Down;
        [Export] public bool IsWalking = false;
        [Export] public bool CollisionDetected = false;

        public override void _Ready()
        {
            CharacterInput.Walk += StartWalking;
            CharacterInput.Turn += Turn;

            CharacterCollisionRayCast.Collision += (value) => CollisionDetected = value;

            Game.Core.Logger.Info("Loading player movement...");
        }

        public override void _Process(double delta)
        {
            Walk(delta);
        }

        public bool IsMoving()
        {
            return IsWalking;
        }

        public bool IsColliding()
        {
            return CollisionDetected;
        }

        public void StartWalking()
        {
            if (IsMoving()) return;

            // 1. On calcule où on VEUT aller
            Vector2 nextTarget = Character.Position + CharacterInput.Direction * Globals.GRID_SIZE;

            // 2. On vérifie SI ce point est occupé par un mur
            if (!IsTargetOccupied(nextTarget))
            {
                EmitSignal(SignalName.Animation, "walk");
                TargetPosition = nextTarget;
                Game.Core.Logger.Info($"Moving to {TargetPosition}");
                IsWalking = true;
            }
            else 
            {
                Game.Core.Logger.Info("Mouvement bloqué : obstacle détecté.");
            }
        }

        // Ajoute cette fonction de vérification "propre"
        public bool IsTargetOccupied(Vector2 targetPos)
        {
            var spaceState = Character.GetWorld2D().DirectSpaceState;
            // On vérifie le centre de la case cible (+8 si tes cases font 16)
            Vector2 checkPoint = targetPos + new Vector2(8, 8);

            var query = new PhysicsPointQueryParameters2D
            {
                Position = checkPoint,
                CollisionMask = 1, // Layer 1 (tes murs bleus)
                CollideWithAreas = true,
                CollideWithBodies = true
            };

            var results = spaceState.IntersectPoint(query);
            
            foreach (var result in results)
            {
                var collider = (Node)result["collider"];
                // Si c'est un StaticBody2D (tes murs), on bloque
                if (collider is StaticBody2D) return true;
            }

            return false;
        }

		public void Walk(double delta)
        {
            if (IsWalking)
            {
                // Note : Globals.GRID_SIZE causera l'erreur de build
                Character.Position = Character.Position.MoveToward(TargetPosition, (float)delta * Globals.GRID_SIZE * 4);

                if (Character.Position.DistanceTo(TargetPosition) < 1f)
                {
                    StopWalking();
                }
            }
            else
            {
                EmitSignal(SignalName.Animation, "idle");
            }
        }

        public void StopWalking()
        {
            IsWalking = false;
            SnapPositionToGrid();
        }

        public void Turn()
        {
            EmitSignal(SignalName.Animation, "turn");
        }

        public void SnapPositionToGrid()
        {
            Character.Position = new Vector2(
                Mathf.Round(Character.Position.X / Globals.GRID_SIZE) * Globals.GRID_SIZE,
                Mathf.Round(Character.Position.Y / Globals.GRID_SIZE) * Globals.GRID_SIZE
            );
        }
    }
}