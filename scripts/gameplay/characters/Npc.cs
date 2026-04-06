using Game.Core;
using Game.UI;
using Game.Utilities;
using Godot;
using Godot.Collections;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Contrôle le comportement et l’apparence d’un PNJ.
/// </summary>
[Tool]
public partial class Npc : CharacterBody2D
{
    // Apparence actuelle du PNJ.
    private NpcAppearance npcAppearance = NpcAppearance.Worker;

    [ExportCategory("Traits")]
    [Export]
    public NpcAppearance NpcAppearance
    {
        get => npcAppearance;
        set
        {
            if (npcAppearance != value)
            {
                npcAppearance = value;
                UpdateAppearance();
            }
        }
    }

    // Sprite animé pour afficher le PNJ.
    private AnimatedSprite2D animatedSprite2D;
    // Composant d'entrée pour le PNJ.
    private NpcInput npcInput;
    // Machine à états du PNJ.
    private StateMachine stateMachine;
    // Composant de mouvement du PNJ.
    private CharacterMovement characterMovement;

    // Dictionnaire des frames de sprite pour chaque apparence.
    private readonly Dictionary<NpcAppearance, SpriteFrames> appearanceFrames = new()
    {
        { NpcAppearance.BugCatcher, GD.Load<SpriteFrames>("res://resources/spriteframes/bug_catcher.tres") },
        { NpcAppearance.Gardener, GD.Load<SpriteFrames>("res://resources/spriteframes/gardener.tres") },
        { NpcAppearance.Worker, GD.Load<SpriteFrames>("res://resources/spriteframes/worker.tres") }
    };

    // Configuration du comportement du PNJ.
    [Export]
    public NpcInputConfig NpcInputConfig;

    /// <summary>
    /// Initialise le PNJ au démarrage.
    /// Configure les composants et l'apparence.
    /// </summary>
    public override void _Ready()
    {
        // En mode éditeur, seulement mettre à jour l'apparence.
        if (Engine.IsEditorHint())
        {
            UpdateAppearance();
            return;
        }

        // Initialiser les composants.
        npcInput ??= GetNode<NpcInput>("Input");
        npcInput.Config = NpcInputConfig;

        stateMachine ??= GetNode<StateMachine>("StateMachine");
        stateMachine.ChangeState("Roam");

        animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        characterMovement ??= GetNode<CharacterMovement>("Movement");
    }

    /// <summary>
    /// Met à jour le ZIndex du PNJ en fonction de la position du joueur.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    public override void _Process(double delta)
    {
        // Ne rien faire en mode éditeur.
        if (Engine.IsEditorHint())
            return;

        var player = GameManager.GetPlayer();

        // Ajuster le ZIndex pour l'ordre de rendu.
        if (player != null)
        {
            ZIndex = (player.Position.Y <= Position.Y) ? 6 : 4;
        }
    }

    /// <summary>
    /// Met à jour l'apparence du PNJ en changeant les frames de sprite.
    /// </summary>
    private void UpdateAppearance()
    {
        // Récupérer le sprite animé s'il n'est pas assigné.
        if (animatedSprite2D == null)
        {
            animatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

            if (animatedSprite2D == null)
            {
                return;
            }
        }

        // Appliquer les frames correspondantes à l'apparence.
        if (appearanceFrames.TryGetValue(npcAppearance, out var spriteFrames))
        {
            if (animatedSprite2D.SpriteFrames != spriteFrames)
            {
                Logger.Info($"Updating appearance for {Name} to {spriteFrames.ResourcePath}");
                animatedSprite2D.SpriteFrames = spriteFrames;
            }
        }
        else
        {
            animatedSprite2D.SpriteFrames = null;
        }
    }

    /// <summary>
    /// Joue un message en faisant tourner le PNJ vers le joueur et changeant d'état.
    /// </summary>
    /// <param name="Direction">Direction depuis laquelle le joueur interagit.</param>
    public void PlayMessage(Vector2 Direction)
    {
        // Ne rien faire en mode éditeur.
        if (Engine.IsEditorHint())
            return;

        // Ne pas jouer si en mouvement.
        if (characterMovement.IsMoving())
            return;

        // Tourner vers le joueur si nécessaire.
        if (npcInput.Direction != Direction * -1)
        {
            npcInput.Direction = Direction * -1;
            npcInput.EmitSignal(CharacterInput.SignalName.Turn);
        }

        // Changer d'état et jouer le message.
        stateMachine.ChangeState("Message");
        MessageManager.PlayText([.. NpcInputConfig.Messages]);
    }
}
