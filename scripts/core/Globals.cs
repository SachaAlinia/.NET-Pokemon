using Godot;

namespace Game.Core;

public partial class Globals : Node
{
    public static Globals Instance { get; private set; }

    public const int GRID_SIZE = 16;       // La taille d'une case de ton jeu (16x16 pixels).
    public const int MOVE_NUMBERS = 165;   // Nombre total d'attaques dans ton jeu.
    public const int POKEMON_NUMBERS = 151; // Nombre total de Pokémon (le Pokédex original !).

    // '[Export]' : Permet de changer la "Seed" (graine) dans l'éditeur Godot.
    [ExportCategory("Gameplay")]
    [Export]
    public ulong Seed = 1337; // La graine pour le hasard (si on garde la même, le hasard sera prévisible).

    // 'RandomNumberGenerator' : C'est la machine à lancer les dés de Godot.
    private RandomNumberGenerator RandomNumberGenerator;

    /// <summary>
    /// Initialise les réglages globaux au lancement du jeu.
    /// </summary>
    public override void _Ready()
    {
        Instance = this; // On enregistre cette version du script comme l'instance officielle.

        // On crée la machine à hasard et on lui donne notre graine (Seed).
        RandomNumberGenerator = new()
        {
            Seed = Seed
        };

        Logger.Info("Loading Globals ...");
    }

    /// <summary>
    /// Permet à n'importe quel autre script de demander un nombre au hasard.
    /// </summary>
    /// <returns>Le lanceur de dés officiel du jeu.</returns>
    public static RandomNumberGenerator GetRandomNumberGenerator()
    {
        return Instance.RandomNumberGenerator;
    }
}