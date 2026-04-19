using Godot;
using System.Threading.Tasks;

namespace Game.Gameplay;

/// <summary>
/// Affiche du texte flottant (dégâts, healing, miss, etc.) au-dessus des Pokemon
/// </summary>
public partial class FloatingText : Label
{
    public static FloatingText Create(Node parent, Vector2 position, string text, Color color)
    {
        var floatingText = new FloatingText
        {
            Text = text,
            CustomMinimumSize = new Vector2(100, 40),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = position,
            Modulate = color,
        };

        floatingText.AddThemeColorOverride("font_color", color);
        parent.AddChild(floatingText);
        _ = floatingText.AnimateAndRemove();
        return floatingText;
    }

    private async Task AnimateAndRemove()
    {
        Tween tween = GetTree().CreateTween();

        // Montée + fade
        tween.TweenProperty(this, "position", Position - Vector2.Up * 60, 1.0f);
        tween.TweenProperty(this, "modulate", new Color(Modulate.R, Modulate.G, Modulate.B, 0.0f), 1.0f);

        await ToSignal(tween, "finished");
        QueueFree();
    }
}
