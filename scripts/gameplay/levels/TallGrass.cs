using Game.Core;
using Godot;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

public partial class TallGrass : Area2D
{
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
        // On récupère le taux de rencontre défini dans le script du Level actuel
        int rate = SceneManager.GetCurrentLevel().EncounterRate;
        int chance = Globals.GetRandomNumberGenerator().RandiRange(0, 100);

        if (chance <= rate)
        {
            Logger.Info($"Pokemon encountered! -> {chance} <= {rate}");

            // 1. On charge le Pokémon sauvage (Onix pour le test)
            // Assure-toi que le chemin vers le .tres est correct
            var wildPokemon = GD.Load<PokemonResource>("res://resources/pokemon/onix.tres");

            if (wildPokemon != null)
            {
                // 2. On lance la transition de combat via le SceneManager
                SceneManager.StartBattle(wildPokemon);
            }
            else
            {
                Logger.Error("TallGrass: Impossible de charger la ressource du Pokémon sauvage !");
            }
        }
    }
}