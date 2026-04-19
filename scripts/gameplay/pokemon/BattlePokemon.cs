using Game.Core;
using Godot.Collections;
using System.Collections.Generic;

namespace Game.Gameplay;

/// <summary>
/// Représente une instance de Pokemon en combat, avec level, exp, PP des attaques, etc.
/// </summary>
public class BattlePokemon
{
    public PokemonResource Resource { get; set; }
    public int Level { get; set; } = 5;
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int Experience { get; set; } = 0;
    public List<MoveWithPP> Moves { get; set; } = new();
    public PokemonAilment CurrentAilment { get; set; } = PokemonAilment.None;

    // Stats calculées selon le niveau
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpAtk { get; set; }
    public int SpDef { get; set; }
    public int Speed { get; set; }

    public BattlePokemon(PokemonResource resource, int level = 5, Array<MoveResource> moves = null)
    {
        Resource = resource;
        Level = level;

        // Calcul des stats selon le niveau
        CalculateStats();

        CurrentHP = MaxHP;

        // Assignation des moves
        if (moves != null)
        {
            foreach (var move in moves)
            {
                Moves.Add(new MoveWithPP(move, move.PP > 0 ? move.PP : 20));
            }
        }
    }

    private void CalculateStats()
    {
        // Formule Pokemon simple : (2 * BaseStat * Level / 100) + Level + 5
        MaxHP = (2 * Resource.BaseHp * Level / 100) + Level + 5;
        Attack = (2 * Resource.BaseAttack * Level / 100) + 5;
        Defense = (2 * Resource.BaseDefense * Level / 100) + 5;
        SpAtk = (2 * Resource.BaseSpecialAttack * Level / 100) + 5;
        SpDef = (2 * Resource.BaseSpecialDefense * Level / 100) + 5;
        Speed = (2 * Resource.BaseSpeed * Level / 100) + 5;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = System.Math.Max(0, CurrentHP - damage);
    }

    public void Heal(int amount)
    {
        CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
    }

    public bool IsAlive => CurrentHP > 0;
    public bool IsFainted => CurrentHP <= 0;
    public float HPPercent => (float)CurrentHP / MaxHP;
}

/// <summary>
/// Représente une attaque avec ses PP restants
/// </summary>
public class MoveWithPP
{
    public MoveResource Move { get; set; }
    public int CurrentPP { get; set; }
    public int MaxPP { get; set; }

    public MoveWithPP(MoveResource move, int pp)
    {
        Move = move;
        MaxPP = pp;
        CurrentPP = pp;
    }

    public bool CanUse => CurrentPP > 0;

    public void UsePP()
    {
        CurrentPP = System.Math.Max(0, CurrentPP - 1);
    }
}
