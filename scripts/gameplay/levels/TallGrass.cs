using Game.Core;
using Godot;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Herbe haute pouvant déclencher un combat sauvage.
/// </summary>
public partial class TallGrass : Area2D
{
// Sprite animé utilisé pour l’objet.
    [Export]
    public AnimatedSprite2D AnimatedSprite2D;

    public override void _Ready()
    {
        AnimatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void OnBodyEntered(Node2D node2D)
    {
        // On vérifie si c'est le joueur qui entre dans l'herbe
        if (node2D is Player)
        {
            CalculateEncounterChance();
        }

        AnimatedSprite2D.Play("down");
    }

    public void OnBodyExited(Node2D node2D)
    {
        AnimatedSprite2D.Play("up");
    }

    public void CalculateEncounterChance()
    {
        // 1. On récupère le niveau actuel
        var currentLevel = SceneManager.GetCurrentLevel();
        int rate = currentLevel.EncounterRate;
        int chance = Globals.GetRandomNumberGenerator().RandiRange(0, 100);

        if (chance <= rate)
        {
            Logger.Info($"Pokemon encountered! -> {chance} <= {rate}");

            // 2. RÉCUPÉRATION ALÉATOIRE :
            // On vérifie si la liste de Pokémon du niveau n'est pas vide
            if (currentLevel.WildPokemons != null && currentLevel.WildPokemons.Count > 0)
            {
                // On choisit un index au hasard entre 0 et le nombre de Pokémon dans la liste
                int randomIndex = Globals.GetRandomNumberGenerator().RandiRange(0, currentLevel.WildPokemons.Count - 1);

                // On récupère le Pokémon correspondant
                var wildPokemon = currentLevel.WildPokemons[randomIndex];

                if (wildPokemon != null)
                {
                    // 3. On lance le combat avec CE Pokémon spécifique
                    SceneManager.StartBattle(wildPokemon);
                }
            }
            else
            {
                Logger.Error("TallGrass: La liste WildPokemons du Level est vide ou nulle !");

                // Secours au cas où la liste est vide pour ne pas bloquer le jeu
                var backupPokemon = GD.Load<PokemonResource>("res://resources/pokemon/onix.tres");
                SceneManager.StartBattle(backupPokemon);
            }
        }
    }
}