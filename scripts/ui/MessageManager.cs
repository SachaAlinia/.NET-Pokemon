using Game.Core;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace Game.UI;

/// <summary>
/// Gère l’affichage et le défilement du texte dans la boîte de dialogue.
/// </summary>
public partial class MessageManager : CanvasLayer
{
    /// <summary>
    /// Instance singleton du gestionnaire de messages.
    /// </summary>
    public static MessageManager Instance { get; private set; }

    [ExportCategory("Components")]
    [Export]
    // Boîte de dialogue principale affichée à l’écran.
    public NinePatchRect Box;

    [Export]
    // Zone de texte où le message est affiché.
    public RichTextLabel Label;

    [ExportCategory("Variables")]
    [Export]
    // Indique si le texte est en train de défiler lettre par lettre.
    public bool IsScrolling = false;

    [Export]
    // Délai en millisecondes entre chaque lettre affichée.
    public int Delay = 15;

    [Export]
    // Liste des messages actuellement en file d’attente.
    public Array<string> Messages;

    /// <summary>
    /// Initialise l’instance singleton au démarrage du nœud.
    /// </summary>
    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Joue un texte dans la boîte de dialogue.
    /// </summary>
    /// <param name="payload">Liste de phrases à afficher.</param>
    public static void PlayText(params string[] payload)
    {
        if (IsReading()) return;
        if (payload.Length == 0) return;

        Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, true);

        Instance.Messages = [.. payload];
        ScrollText();
    }

    /// <summary>
    /// Déroule le texte lettre par lettre dans la boîte de dialogue.
    /// </summary>
    public static async void ScrollText()
    {
        if (!IsReading())
            Instance.Box.Visible = true;

        if (Instance.Messages.Count == 0)
        {
            Instance.Box.Visible = false;
            Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, false);
            return;
        }

        Instance.IsScrolling = true;
        Instance.Label.Text = "";

        foreach (char letter in Instance.Messages[0])
        {
            Instance.Label.Text += letter;
            await Task.Delay(Instance.Delay);
        }

        Instance.Messages.RemoveAt(0);
        Instance.IsScrolling = false;
    }

    /// <summary>
    /// Indique si la boîte de dialogue est actuellement visible.
    /// </summary>
    public static bool IsReading()
    {
        return Instance.Box.Visible;
    }

    /// <summary>
    /// Indique si le texte est en train de défiler.
    /// </summary>
    public static bool Scrolling()
    {
        return Instance.IsScrolling;
    }

    /// <summary>
    /// Retourne la liste des messages actuellement en file d’attente.
    /// </summary>
    public static Array<string> GetMessages()
    {
        return Instance.Messages;
    }
}
