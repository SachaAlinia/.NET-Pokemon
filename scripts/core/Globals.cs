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

<<<<<<< HEAD
    // CHANGEMENT ICI : On renomme la variable pour éviter le conflit
    private RandomNumberGenerator _rng; 
=======
    private RandomNumberGenerator RandomNumberGenerator;
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd

    public override void _Ready()
    {
        Instance = this;

<<<<<<< HEAD
        _rng = new()
=======
        RandomNumberGenerator = new()
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd
        {
            Seed = Seed
        };

<<<<<<< HEAD
        Game.Core.Logger.Info("Loading Globals ...");
=======
        Logger.Info("Loading Globals ...");
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd
    }

    public static RandomNumberGenerator GetRandomNumberGenerator()
    {
<<<<<<< HEAD
        return Instance._rng; // On utilise le nouveau nom ici aussi
=======
        return Instance.RandomNumberGenerator;
>>>>>>> 682157cd58aeb8651ff30a09b88dc3104c9a62dd
    }
}