// Ce fichier calcule les dégâts des attaques en combat, en tenant compte des types,
// de l'exactitude (accuracy), des coups critiques, d'un facteur aléatoire, etc.
// Je n'ai modifié aucune ligne de code : j'ai seulement ajouté des commentaires explicatifs.

using Godot; // API Godot (utile pour GD.Randf(), GD.Randi(), Colors, etc.)
using System.Collections.Generic; // Pour Dictionary<,>
using Game.Core; // Contient des définitions de types du jeu (ex: PokemonType, MoveCategory, etc.)

namespace Game.Gameplay;

/// <summary>
/// Calcule les dégâts en tenant compte des stats, types, critiques, etc.
/// </summary>
public static class DamageCalculator
{
    // Tableau d'efficacité des types (très simplifié)
    // Clé : tuple (type de l'attaque, type de la cible) -> Valeur : multiplicateur d'efficacité
    // Exemples :
    // - 2.0f => super efficace (double dégâts)
    // - 0.5f => peu efficace (moitié dégâts)
    // - 0.0f => aucune effet (immunité)
    private static readonly Dictionary<(PokemonType, PokemonType), float> TypeEffectiveness = new()
    {
        // Feu
        { (PokemonType.Fire, PokemonType.Grass), 2.0f },
        { (PokemonType.Fire, PokemonType.Bug), 2.0f },
        { (PokemonType.Fire, PokemonType.Steel), 2.0f },
        { (PokemonType.Fire, PokemonType.Ice), 2.0f },
        { (PokemonType.Fire, PokemonType.Water), 0.5f },
        { (PokemonType.Fire, PokemonType.Ground), 0.5f },
        { (PokemonType.Fire, PokemonType.Rock), 0.5f },

        // Eau
        { (PokemonType.Water, PokemonType.Fire), 2.0f },
        { (PokemonType.Water, PokemonType.Ground), 2.0f },
        { (PokemonType.Water, PokemonType.Rock), 2.0f },
        { (PokemonType.Water, PokemonType.Grass), 0.5f },
        { (PokemonType.Water, PokemonType.Water), 0.5f },
        { (PokemonType.Water, PokemonType.Dragon), 0.5f },

        // Électrique
        { (PokemonType.Electric, PokemonType.Water), 2.0f },
        { (PokemonType.Electric, PokemonType.Flying), 2.0f },
        { (PokemonType.Electric, PokemonType.Grass), 0.5f },
        { (PokemonType.Electric, PokemonType.Electric), 0.5f },
        { (PokemonType.Electric, PokemonType.Dragon), 0.5f },
        { (PokemonType.Electric, PokemonType.Ground), 0.0f }, // Immunité

        // Herbe
        { (PokemonType.Grass, PokemonType.Water), 2.0f },
        { (PokemonType.Grass, PokemonType.Ground), 2.0f },
        { (PokemonType.Grass, PokemonType.Rock), 2.0f },
        { (PokemonType.Grass, PokemonType.Fire), 0.5f },
        { (PokemonType.Grass, PokemonType.Grass), 0.5f },
        { (PokemonType.Grass, PokemonType.Poison), 0.5f },
        { (PokemonType.Grass, PokemonType.Flying), 0.5f },
        { (PokemonType.Grass, PokemonType.Bug), 0.5f },
        { (PokemonType.Grass, PokemonType.Dragon), 0.5f },

        // Glaçon
        { (PokemonType.Ice, PokemonType.Dragon), 2.0f },
        { (PokemonType.Ice, PokemonType.Flying), 2.0f },
        { (PokemonType.Ice, PokemonType.Grass), 2.0f },
        { (PokemonType.Ice, PokemonType.Ground), 2.0f },
        { (PokemonType.Ice, PokemonType.Fire), 0.5f },
        { (PokemonType.Ice, PokemonType.Water), 0.5f },
        { (PokemonType.Ice, PokemonType.Grass), 0.5f },
        { (PokemonType.Ice, PokemonType.Ice), 0.5f },

        // Combat
        { (PokemonType.Fighting, PokemonType.Normal), 2.0f },
        { (PokemonType.Fighting, PokemonType.Ice), 2.0f },
        { (PokemonType.Fighting, PokemonType.Rock), 2.0f },
        { (PokemonType.Fighting, PokemonType.Dark), 2.0f },
        { (PokemonType.Fighting, PokemonType.Steel), 2.0f },
        { (PokemonType.Fighting, PokemonType.Flying), 0.5f },
        { (PokemonType.Fighting, PokemonType.Poison), 0.5f },
        { (PokemonType.Fighting, PokemonType.Bug), 0.5f },
        { (PokemonType.Fighting, PokemonType.Psychic), 0.5f },
        { (PokemonType.Fighting, PokemonType.Ghost), 0.0f },

        // Poison
        { (PokemonType.Poison, PokemonType.Grass), 2.0f },
        { (PokemonType.Poison, PokemonType.Fairy), 2.0f },
        { (PokemonType.Poison, PokemonType.Poison), 0.5f },
        { (PokemonType.Poison, PokemonType.Ground), 0.5f },
        { (PokemonType.Poison, PokemonType.Rock), 0.5f },
        { (PokemonType.Poison, PokemonType.Ghost), 0.5f },
        { (PokemonType.Poison, PokemonType.Steel), 0.0f },

        // Sol
        { (PokemonType.Ground, PokemonType.Fire), 2.0f },
        { (PokemonType.Ground, PokemonType.Electric), 2.0f },
        { (PokemonType.Ground, PokemonType.Poison), 2.0f },
        { (PokemonType.Ground, PokemonType.Rock), 2.0f },
        { (PokemonType.Ground, PokemonType.Steel), 2.0f },
        { (PokemonType.Ground, PokemonType.Grass), 0.5f },
        { (PokemonType.Ground, PokemonType.Bug), 0.5f },
        { (PokemonType.Ground, PokemonType.Flying), 0.0f },

        // Roche
        { (PokemonType.Rock, PokemonType.Fire), 2.0f },
        { (PokemonType.Rock, PokemonType.Ice), 2.0f },
        { (PokemonType.Rock, PokemonType.Flying), 2.0f },
        { (PokemonType.Rock, PokemonType.Bug), 2.0f },
        { (PokemonType.Rock, PokemonType.Water), 0.5f },
        { (PokemonType.Rock, PokemonType.Grass), 0.5f },
        { (PokemonType.Rock, PokemonType.Fighting), 0.5f },
        { (PokemonType.Rock, PokemonType.Ground), 0.5f },
        { (PokemonType.Rock, PokemonType.Steel), 0.5f },

        // Insecte
        { (PokemonType.Bug, PokemonType.Grass), 2.0f },
        { (PokemonType.Bug, PokemonType.Psychic), 2.0f },
        { (PokemonType.Bug, PokemonType.Dark), 2.0f },
        { (PokemonType.Bug, PokemonType.Fire), 0.5f },
        { (PokemonType.Bug, PokemonType.Fighting), 0.5f },
        { (PokemonType.Bug, PokemonType.Poison), 0.5f },
        { (PokemonType.Bug, PokemonType.Flying), 0.5f },
        { (PokemonType.Bug, PokemonType.Ghost), 0.5f },
        { (PokemonType.Bug, PokemonType.Steel), 0.5f },
        { (PokemonType.Bug, PokemonType.Fairy), 0.5f },

        // Spectre
        { (PokemonType.Ghost, PokemonType.Ghost), 2.0f },
        { (PokemonType.Ghost, PokemonType.Psychic), 2.0f },
        { (PokemonType.Ghost, PokemonType.Normal), 0.0f },
        { (PokemonType.Ghost, PokemonType.Dark), 0.5f },

        // Acier
        { (PokemonType.Steel, PokemonType.Ice), 2.0f },
        { (PokemonType.Steel, PokemonType.Rock), 2.0f },
        { (PokemonType.Steel, PokemonType.Fairy), 2.0f },
        { (PokemonType.Steel, PokemonType.Fire), 0.5f },
        { (PokemonType.Steel, PokemonType.Grass), 0.5f },
        { (PokemonType.Steel, PokemonType.Ice), 0.5f },
        { (PokemonType.Steel, PokemonType.Flying), 0.5f },
        { (PokemonType.Steel, PokemonType.Psychic), 0.5f },
        { (PokemonType.Steel, PokemonType.Bug), 0.5f },
        { (PokemonType.Steel, PokemonType.Rock), 0.5f },
        { (PokemonType.Steel, PokemonType.Steel), 0.5f },
        { (PokemonType.Steel, PokemonType.Fairy), 0.5f },
        { (PokemonType.Steel, PokemonType.Normal), 0.5f },
        { (PokemonType.Steel, PokemonType.Flying), 0.5f },
        { (PokemonType.Steel, PokemonType.Poison), 0.0f },

        // Psy
        { (PokemonType.Psychic, PokemonType.Fighting), 2.0f },
        { (PokemonType.Psychic, PokemonType.Poison), 2.0f },
        { (PokemonType.Psychic, PokemonType.Psychic), 0.5f },
        { (PokemonType.Psychic, PokemonType.Steel), 0.5f },
        { (PokemonType.Psychic, PokemonType.Dark), 0.0f },

        // Ténèbres
        { (PokemonType.Dark, PokemonType.Ghost), 2.0f },
        { (PokemonType.Dark, PokemonType.Psychic), 2.0f },
        { (PokemonType.Dark, PokemonType.Fighting), 0.5f },
        { (PokemonType.Dark, PokemonType.Dark), 0.5f },
        { (PokemonType.Dark, PokemonType.Fairy), 0.5f },

        // Dragon
        { (PokemonType.Dragon, PokemonType.Dragon), 2.0f },
        { (PokemonType.Dragon, PokemonType.Steel), 0.5f },
        { (PokemonType.Dragon, PokemonType.Fairy), 0.0f },

        // Fée
        { (PokemonType.Fairy, PokemonType.Fighting), 2.0f },
        { (PokemonType.Fairy, PokemonType.Dragon), 2.0f },
        { (PokemonType.Fairy, PokemonType.Dark), 2.0f },
        { (PokemonType.Fairy, PokemonType.Poison), 0.5f },
        { (PokemonType.Fairy, PokemonType.Steel), 0.5f },
    };

    // Résultat détaillé du calcul des dégâts
    public class DamageResult
    {
        // Montant de dégâts à appliquer (int)
        public int Damage { get; set; }
        // Multiplicateur d'efficacité des types (ex: 2.0f, 0.5f)
        public float Effectiveness { get; set; } = 1.0f;
        // Indique si c'était un coup critique
        public bool IsCritical { get; set; }
        // Indique si l'attaque a raté (miss)
        public bool IsMiss { get; set; }
    }

    /// <summary>
    /// Calcule les dégâts avec tous les modificateurs
    /// </summary>
    public static DamageResult CalculateDamage(BattlePokemon attacker, BattlePokemon target, MoveResource move)
    {
        var result = new DamageResult();

        // Vérification du miss
        // Si move.Accuracy > 0 et la valeur aléatoire * 100 dépasse l'accuracy -> échec
        if (move.Accuracy > 0 && GD.Randf() * 100 > move.Accuracy)
        {
            result.IsMiss = true;
            result.Damage = 0;
            return result; // Sortie rapide si l'attaque a raté
        }

        // Move sans dégâts (status, boost, etc.)
        // Si la puissance est 0, on ne fait pas de dégâts physiques
        if (move.Power == 0)
        {
            result.Damage = 0;
            return result;
        }

        // Calcul des stats à utiliser (attaque physique ou spéciale selon la catégorie du move)
        int atk = (move.Category == Game.Core.MoveCategory.Physical)
            ? attacker.Attack
            : attacker.SpAtk;

        int def = (move.Category == Game.Core.MoveCategory.Physical)
            ? target.Defense
            : target.SpDef;

        // Formule de base Pokemon (version simplifiée)
        // baseDamage ≈ (((2 * level / 5 + 2) * power * atk / def) / 50) + 2
        float baseDamage = (((2.0f * attacker.Level / 5.0f + 2.0f) * move.Power * atk / def) / 50.0f) + 2.0f;

        // Multiplicateur de type : calculé pour le type principal, puis multiplié si la cible a un second type.
        float typeEffectiveness = GetTypeEffectiveness(move.PokemonType, target.Resource.TypeOne);
        if (target.Resource.TypeTwo != PokemonType.None)
        {
            typeEffectiveness *= GetTypeEffectiveness(move.PokemonType, target.Resource.TypeTwo);
        }
        result.Effectiveness = typeEffectiveness;

        // Critique : probabilité de critique de base 5% + modificateur du move (move.CritRate exprimé en pourcentage)
        float critChance = 0.05f + (move.CritRate / 100.0f);
        bool isCritical = GD.Randf() < critChance;
        result.IsCritical = isCritical;

        // Facteur aléatoire entre 0.85 et 1.0 (pour varier légèrement les dégâts)
        float randomFactor = GD.Randf() * 0.15f + 0.85f;

        // Calcul final : base * types * (crit ? 1.5 : 1) * random
        float finalDamage = baseDamage * typeEffectiveness * (isCritical ? 1.5f : 1.0f) * randomFactor;

        // On convertit en int pour appliquer les PV ; la conversion tronque les décimales.
        result.Damage = (int)finalDamage;
        return result;
    }

    // Récupère le multiplicateur d'efficacité entre le type du move et le type cible
    private static float GetTypeEffectiveness(PokemonType moveType, PokemonType targetType)
    {
        // Si un des types est "None" on retourne 1 (neutre)
        if (moveType == PokemonType.None || targetType == PokemonType.None)
            return 1.0f;

        // Essaye de trouver la valeur dans le dictionnaire ; sinon neutre (1.0f)
        if (TypeEffectiveness.TryGetValue((moveType, targetType), out var effectiveness))
            return effectiveness;

        return 1.0f;
    }

    // Retourne un message utilisateur selon le multiplicateur d'efficacité
    public static string GetEffectivenessMessage(float effectiveness)
    {
        return effectiveness switch
        {
            0f => "Ça n'a aucun effet...",           // Immunité
            < 1f => "Ce n'est pas très efficace...", // Moins efficace
            > 1f => "C'est super efficace !",         // Super efficace
            _ => ""                                   // exact 1.0f -> pas de message
        };
    }
}