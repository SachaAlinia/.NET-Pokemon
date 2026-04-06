using Game.Core;
using Game.Gameplay;
using Godot;

/// <summary>
/// Outil de débogage pour visualiser la grille du niveau, les points solides, les points de patrouille, etc.
/// </summary>
public partial class LevelDebugger : Node2D
{
    [Export]
    // Active ou désactive le mode débogage.
    public bool DebugOn = false;

    // Référence au niveau parent pour accéder à ses propriétés.
    private Level level;

    /// <summary>
    /// Initialise la référence au niveau parent au démarrage.
    /// </summary>
    public override void _Ready()
    {
        level = GetParent<Level>();
    }

    /// <summary>
    /// Met à jour le débogage chaque frame si activé.
    /// </summary>
    /// <param name="delta">Temps écoulé depuis la dernière frame.</param>
    public override void _Process(double delta)
    {
        if (level != null && DebugOn)
        {
            QueueRedraw();
        }
    }

    /// <summary>
    /// Dessine la visualisation de débogage de la grille et des points.
    /// </summary>
    public override void _Draw()
    {
        // Ne rien dessiner si le débogage est désactivé.
        if (!DebugOn)
        {
            return;
        }

        // Ne rien dessiner si le niveau n'est pas disponible.
        if (level == null)
        {
            return;
        }

        var Grid = level.Grid;

        // Ne rien dessiner si la grille n'existe pas.
        if (Grid == null)
            return;

        // Calculer les dimensions de la carte en cellules.
        var mapHeight = level.Bottom / Globals.GRID_SIZE;
        var mapWidth = level.Right / Globals.GRID_SIZE;

        // Parcourir chaque cellule de la grille pour dessiner les obstacles.
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2I cell = new(x, y);
                Vector2 worldPosition = new(x * Globals.GRID_SIZE, y * Globals.GRID_SIZE);

                // Rouge pour les cellules solides (obstacles), vert pour les cellules libres.
                var color = Grid.IsPointSolid(cell) ? new Color(1, 0, 0, 0.7f) : new Color(0, 1, 0, 0.7f);
                DrawRect(new Rect2(worldPosition, Grid.CellSize), color, filled: true);
            }
        }

        // Dessiner les points de patrouille en bleu semi-transparent.
        foreach (var point in level.CurrentPatrolPoints)
        {
            DrawRect(new Rect2(point, Grid.CellSize), new Color(0, 0, 1, 0.3f), filled: true);
        }

        // Dessiner la position cible en cyan semi-transparent si elle est définie.
        if (level.TargetPosition != Vector2.Zero)
            DrawRect(new Rect2(level.TargetPosition, Grid.CellSize), new Color(0, 1, 1, 0.3f), filled: true);
    }
}
