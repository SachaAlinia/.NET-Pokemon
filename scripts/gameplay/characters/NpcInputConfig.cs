using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

/// <summary>
/// Décrit les réglages de comportement des PNJ.
/// </summary>
[GlobalClass]
[Tool]
public partial class NpcInputConfig : Resource
{
// Type de déplacement du PNJ.
    [ExportGroup("Movement")]
    [ExportSubgroup("Common")]
    [Export]
    public NpcMovementType NpcMovementType = NpcMovementType.Static;

// Liste des messages affichés par le panneau.
    [Export]
    public Array<string> Messages;

// Origine du déplacement aléatoire.
    [ExportSubgroup("Wander")]
    [Export]
    public Vector2 WanderOrigin = Vector2.Zero;

// Rayon de déplacement pour l’errance.
    [Export]
    public double WanderRadius = 64f;

// Intervalle entre deux déplacements errants.
    [Export]
    public double WanderMoveInterval = 2f;

// Points de patrouille du PNJ.
    [ExportSubgroup("Patrol")]
    [Export]
    public Array<Vector2> PatrolPoints;

// Intervalle entre deux mouvements de patrouille.
    [Export]
    public double PatrolMoveInterval = 2f;

// Index du point de patrouille actuel.
    [Export]
    public int PatrolIndex = 0;

// Intervalle entre deux regards autour.
    [ExportSubgroup("LookAround")]
    [Export]
    public double LookAroundInterval = 2f;
}