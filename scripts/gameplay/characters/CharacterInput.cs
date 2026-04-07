using Godot;

namespace Game.Gameplay;

/// <summary>
/// Cette classe est le "Cerveau". Elle définit les commandes de base.
/// 'abstract' signifie qu'on ne peut pas l'utiliser telle quelle, 
/// il faut créer une version "Joueur" ou "IA" qui en hérite.
/// </summary>
public abstract partial class CharacterInput : Node
{
    // --- LES SIGNAUX (Alertes) ---
    // C'est comme une sonnette. Quand le personnage veut marcher, il tire la sonnette 'Walk'.
    [Signal] public delegate void WalkEventHandler();
    [Signal] public delegate void TurnEventHandler();

    // --- LES VARIABLES ---
    [ExportCategory("Common Input")]

    [Export]
    // Stocke la direction : (0, 1) pour le bas, (0, -1) pour le haut, etc.
    public Vector2 Direction = Vector2.Zero;

    [Export]
    // La position précise vers laquelle on veut aller sur la carte.
    public Vector2 TargetPosition = Vector2.Zero;
}