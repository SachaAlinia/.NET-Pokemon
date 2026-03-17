using Godot;

namespace Game.Core;

public partial class Globals : Node
{
    public static Globals Instance { get; private set; }

    public const int GRID_SIZE = 16;
    public const int MOVE_NUMBERS = 165;
    public const int POKEMON_NUMBERS = 151;

    [ExportCategory("Gameplay")]
    [Export]
    public ulong Seed = 1337;

    // CHANGEMENT ICI : On renomme la variable pour éviter le conflit
    private RandomNumberGenerator _rng; 

    public override void _Ready()
    {
        Instance = this;

        _rng = new()
        {
            Seed = Seed
        };

        Game.Core.Logger.Info("Loading Globals ...");
    }

    public static RandomNumberGenerator GetRandomNumberGenerator()
    {
        return Instance._rng; // On utilise le nouveau nom ici aussi
    }
}