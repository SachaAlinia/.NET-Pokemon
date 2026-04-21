// Fichier : BattlePokemon.cs
// Rôle : Définit les classes utilisées pendant les combats pour représenter un Pokémon
//        avec ses statistiques calculées, ses PV actuels, ses attaques (avec PP), etc.
// Remarque : Je n'ai modifié aucune ligne de code existante, j'ai seulement ajouté des commentaires
//           explicatifs pour que quelqu'un ne connaissant rien au C# comprenne mieux.

using Game.Core;
using Godot.Collections;
using System.Collections.Generic;

namespace Game.Gameplay;

/// <summary>
/// Représente une instance de Pokemon en combat, avec level, exp, PP des attaques, etc.
/// </summary>
public class BattlePokemon
{
    // Référence vers la ressource contenant les données statiques du Pokémon
    public PokemonResource Resource { get; set; }

    // Niveau du Pokémon utilisé pour calculer les stats
    public int Level { get; set; } = 5;

    // PV actuels et PV maximum (valeurs entières)
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }

    // Expérience (non utilisée dans ce fichier, mais présente pour progression)
    public int Experience { get; set; } = 0;

    // Liste des attaques possédant leurs PP (utilise la classe MoveWithPP définie plus bas)
    public List<MoveWithPP> Moves { get; set; } = new();

    // Éventuelle altération d'état (empoisonné, paralysé...) pendant le combat
    public PokemonAilment CurrentAilment { get; set; } = PokemonAilment.None;

    // Stats calculées dynamiquement à partir des "Base" présentes dans la ressource et du level
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpAtk { get; set; }
    public int SpDef { get; set; }
    public int Speed { get; set; }

    // Constructeur : prend la ressource, le niveau et éventuellement une liste d'attaques
    public BattlePokemon(PokemonResource resource, int level = 5, Array<MoveResource> moves = null)
    {
        Resource = resource;
        Level = level;

        // Calcul des stats selon le niveau (méthode ci-dessous)
        CalculateStats();

        // On initialise les PV courants à la valeur maximale calculée
        CurrentHP = MaxHP;

        // Assignation des moves (si fournis) : crée des MoveWithPP pour gérer les PP
        if (moves != null)
        {
            foreach (var move in moves)
            {
                // Si la move a un PP défini (>0) on l'utilise ; sinon on met une valeur par défaut (20)
                Moves.Add(new MoveWithPP(move, move.PP > 0 ? move.PP : 20));
            }
        }
    }

    // Calcul des statistiques en fonction de la ressource (BaseStats) et du niveau
    private void CalculateStats()
    {
        // Formule Pokemon simple : (2 * BaseStat * Level / 100) + Level + 5
        // - MaxHP : formule standard modifiée (ajoute Level + 5) pour donner une valeur entière
        // - Les autres stats : version simple (2*Base*Level/100 + 5)
        // Ces formules sont des approximations adaptées pour un jeu simple.
        MaxHP = (2 * Resource.BaseHp * Level / 100) + Level + 5;
        Attack = (2 * Resource.BaseAttack * Level / 100) + 5;
        Defense = (2 * Resource.BaseDefense * Level / 100) + 5;
        SpAtk = (2 * Resource.BaseSpecialAttack * Level / 100) + 5;
        SpDef = (2 * Resource.BaseSpecialDefense * Level / 100) + 5;
        Speed = (2 * Resource.BaseSpeed * Level / 100) + 5;
    }

    // Applique des dégâts au Pokémon : on décrémente CurrentHP en s'assurant de ne pas passer sous 0
    public void TakeDamage(int damage)
    {
        CurrentHP = System.Math.Max(0, CurrentHP - damage);
    }

    // Soigne le Pokémon : on augmente CurrentHP sans dépasser MaxHP
    public void Heal(int amount)
    {
        CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
    }

    // Propriétés utilitaires pour vérifier l'état du Pokémon
    public bool IsAlive => CurrentHP > 0;         // Vrai si PV > 0
    public bool IsFainted => CurrentHP <= 0;      // Vrai si PV <= 0 (K.O.)
    public float HPPercent => (float)CurrentHP / MaxHP; // Pourcentages de PV (utile pour barres UI)
}

/// <summary>
/// Représente une attaque avec ses PP restants
/// </summary>
public class MoveWithPP
{
    // Référence vers la ressource de l'attaque (nom, puissance, type, etc.)
    public MoveResource Move { get; set; }

    // PP actuels et PP maximum pour cette attaque sur ce Pokémon
    public int CurrentPP { get; set; }
    public int MaxPP { get; set; }

    // Constructeur : on reçoit la ressource MoveResource et le nombre de PP à utiliser
    public MoveWithPP(MoveResource move, int pp)
    {
        Move = move;
        MaxPP = pp;
        CurrentPP = pp;
    }

    // Indique si l'attaque peut être utilisée (PP > 0)
    public bool CanUse => CurrentPP > 0;

    // Consomme 1 PP en s'assurant de ne pas descendre sous 0
    public void UsePP()
    {
        CurrentPP = System.Math.Max(0, CurrentPP - 1);
    }
}