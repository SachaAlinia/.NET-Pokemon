using Game.Core;
using Godot;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Gère la lecture des entrées du joueur.
/// </summary>
public partial class PlayerInput : CharacterInput
{
    // Durée minimale pour considérer un appui prolongé.
    [ExportCategory("Player Input")]
    [Export]
    public double HoldThreshold = 0.2f;

    // Temps de maintien actuel de la touche.
    [Export]
    public double HoldTime = 0.0f;

    /// <summary>
    /// Initialise le composant d'entrée du joueur.
    /// </summary>
    public override void _Ready()
    {
        Logger.Info("Loading player input component ...");
    }
}