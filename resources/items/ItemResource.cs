using Godot;

public enum ItemType { Potion, PokeBall, Utility }

public partial class ItemResource : Resource
{
    [Export] public string Name;
    [Export] public string Description;
    [Export] public Texture2D Icon;
    [Export] public ItemType Type;
    [Export] public int Value; // Puissance du soin ou taux de capture
}