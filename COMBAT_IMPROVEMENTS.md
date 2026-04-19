# Système de Combat Pokemon - Améliorations

## Nouveautés Ajoutées

### 1. **Classe BattlePokemon** (`BattlePokemon.cs`)
- Gère les données d'une instance Pokemon en combat
- Calcul automatique des stats selon le niveau
- Suivi de l'expérience et des ailments
- Classe `MoveWithPP` pour tracker les PP des attaques

### 2. **Système de Dégâts Réaliste** (`DamageCalculator.cs`)
- Formule officielle Pokemon pour calculer les dégâts
- Tableau complet d'efficacité des types
- Système de coups critiques (5% de base)
- Facteur aléatoire pour plus de variance
- Messages d'efficacité ("C'est super efficace !", etc.)

### 3. **Texte Flottant** (`FloatingText.cs`)
- Affichage des dégâts en animation flottante
- Fade-out progressif
- Positionnement au-dessus des Pokemon

### 4. **Améliorations du Combat** (`BattleScene.cs`)
- Affichage des niveaux des Pokemon
- Affichage des PV en texte (ex: "45/100")
- Animations d'approche/retraite des combattants
- Gestion des PP des attaques
- Boutons d'attaque montrant les PP restants
- Écran de fin de combat avec victoire/défaite
- Gain d'expérience à la victoire

## Structure de l'UI Requise

Assurez-vous que votre scène BattleScene contient ces nœuds :

```
BattleScene
├── BattlePositions
│   ├── PlayerSpawn
│   │   └── PlayerSprite (Sprite2D)
│   └── EnemySpawn
│       └── EnemySprite (Sprite2D)
└── UI
    ├── PlayerHUD
    │   ├── Name (Label)
    │   ├── Level (Label)
    │   ├── HP (TextureProgressBar)
    │   └── HPText (Label) - "45/100"
    ├── EnemyHUD
    │   ├── Name (Label)
    │   ├── Level (Label)
    │   ├── HP (TextureProgressBar)
    │   └── HPText (Label)
    ├── DialogueLabel (RichTextLabel)
    ├── ActionMenu (Control)
    │   └── Attaque (Button)
    ├── MoveMenu (Control)
    │   ├── MovesGrid (GridContainer)
    │   └── Retour (Button)
    └── BattleEndScreen (Control)
        ├── Label (Label) - Texte VICTOIRE/DÉFAITE
        └── ContinueButton (Button)
```

## Utilisation

### Initialiser un combat dans GameManager/SceneManager :

```csharp
var battleScene = GetNode<BattleScene>("Battle");
battleScene.PlayerPokemon = playerPokeResource;
battleScene.EnemyPokemon = enemyPokemonResource;
battleScene.PlayerLevel = 15;
battleScene.EnemyLevel = 12;
battleScene.PlayerMoves = new Array<MoveResource> { move1, move2, move3, move4 };
battleScene.EnemyMoves = new Array<MoveResource> { enemyMove1, enemyMove2 };
```

## Statistiques Calculées Automatiquement

Les stats sont calculées selon la formule officielle Pokemon :
- **HP**: (2 × Base × Niveau / 100) + Niveau + 5
- **Autres**: (2 × Base × Niveau / 100) + 5

## Efficacité des Types

Système complet couvrant :
- Feu, Eau, Électrique, Herbe, Glaçon, Combat
- Poison, Sol, Roche, Insecte, Spectre, Acier
- Psy, Ténèbres, Dragon, Fée

## Prochaines Améliorations Possibles

- [ ] Système d'ailments (poison, paralysie, sommeil, etc.)
- [ ] Capacités Pokemon (Abilities)
- [ ] Items en combat (potions, super potions)
- [ ] Système de switch Pokemon
- [ ] Animations des attacks spécifiques
- [ ] Musique et effets sonores de combat
- [ ] Pokémon shiny en combat
- [ ] Statistiques persistantes après combat
