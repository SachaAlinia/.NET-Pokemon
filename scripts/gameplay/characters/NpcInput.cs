using Godot;

namespace Game.Gameplay;

/// <summary>
/// Composant d’entrée pour le PNJ.
/// </summary>
public partial class NpcInput : CharacterInput
{
// Config.
    [Export]
    public NpcInputConfig Config;
}
