// Fichier : GameManager.cs
// Rôle : Point d'entrée pour des fonctions globales du jeu (gestionnaire central).
// Ce fichier contient une classe "GameManager" qui est un singleton accessible partout,
// gère le joueur, le viewport de jeu, l'inventaire et le démarrage du niveau.

using Game.Gameplay; // Utilise les types liés au gameplay (ex : Player, ItemResource).
using Game.UI; // Utilise les types liés à l'interface utilisateur.
using Godot; // Utilise l'API Godot (Node, SubViewport, etc.).
using System.Collections.Generic; // Utilise les collections génériques (Dictionary, List...).

namespace Game.Core; // Définit l'espace de noms pour organiser le code.

// Déclaration de la classe GameManager qui hérite de Node (nœud Godot).
public partial class GameManager : Node
{
	// Singleton pour accès rapide
	// Propriété statique qui contiendra l'instance active de GameManager.
	public static GameManager Instance { get; private set; }

	[ExportCategory("Nodes")]
	[Export]
	public SubViewport GameViewPort; // Zone de rendu du monde du jeu
									 // Explication :
									 // - SubViewport est un type Godot qui représente une surface de rendu séparée.
									 // - Cette propriété est exportée pour être assignable depuis l'éditeur Godot.
									 // - Elle contient la scène visible du monde (où les entités comme le joueur sont ajoutées).

	[ExportCategory("Vars")]
	[Export]
	public Player Player; // Référence vers l'objet Joueur
						  // Explication :
						  // - Contient la référence vers l'instance du joueur actuellement utilisée par le jeu.
						  // - Elle est exportée pour faciliter le debugging / la liaison dans l'éditeur.

	/// <summary>
	/// Initialise l'instance et lance le premier niveau.
	/// </summary>
	public override void _Ready()
	{
		Instance = this;
		// Explication : Affecte la propriété statique Instance à cette instance de GameManager.
		// Cela permet d'accéder à GameManager depuis n'importe quelle autre classe via GameManager.Instance.

		Logger.Info("Loading game manager ...");
		// Explication : Log d'information (probablement une méthode utilitaire pour afficher dans la console).

		// Demande au SceneManager de charger le niveau par défaut au démarrage
		SceneManager.ChangeLevel(spawn: true);
		// Explication :
		// - Appelle la méthode ChangeLevel de SceneManager pour charger une scène/niveau.
		// - Le paramètre spawn: true indique probablement de positionner le joueur au point d'apparition.
	}

	/// <summary>
	/// Permet de récupérer le viewport de n'importe où.
	/// </summary>
	public static SubViewport GetGameViewPort()
	{
		return Instance.GameViewPort;
		// Explication :
		// - Méthode utilitaire statique qui retourne le SubViewport stocké dans l'instance singleton.
		// - Permet à d'autres classes d'obtenir le SubViewport sans avoir besoin d'une référence directe au GameManager.
	}

	/// <summary>
	/// Enregistre le joueur dans le système et l'ajoute au monde.
	/// </summary>
	public static Player AddPlayer(Player player)
	{
		Instance.GameViewPort.AddChild(player);
		// Explication :
		// - Ajoute l'objet "player" comme enfant du SubViewport pour qu'il soit rendu et mis à jour.
		// - AddChild est une méthode Godot qui attache un nœud (ici Player) à un autre nœud (ici le viewport).

		Instance.Player = player;
		// Explication :
		// - Stocke la référence du joueur dans la propriété Player du GameManager singleton,
		//   afin qu'on puisse la retrouver facilement via GameManager.GetPlayer().

		return Instance.Player;
		// Explication :
		// - Retourne la référence au joueur stockée après l'ajout.
	}

	/// <summary>
	/// Permet de récupérer l'objet joueur de n'importe où.
	/// </summary>
	public static Player GetPlayer()
	{
		return Instance.Player;
		// Explication :
		// - Méthode utilitaire statique pour obtenir la référence du joueur gérée par le GameManager.
		// - Utile quand on est dans du code qui n'a pas de référence directe au joueur.
	}

	// Dans GameManager.cs
	public static Dictionary<ItemResource, int> Inventory = new();
	// Explication :
	// - Dictionnaire statique (partagé globalement) mappant une ressource d'objet à une quantité entière.
	// - ItemResource est probablement une classe/ressource représentant un type d'objet (potion, pokéball...).
	// - new() initialise un dictionnaire vide ; il est prêt à être utilisé dès le chargement de la classe.

	public static void AddItem(ItemResource item, int amount = 1)
	{
		if (Inventory.ContainsKey(item))
			Inventory[item] += amount;
		else
			Inventory[item] = amount;
		// Explication détaillée :
		// - Méthode statique pour ajouter une quantité d'un item à l'inventaire.
		// - Si l'item existe déjà comme clé dans le dictionnaire, on augmente sa quantité.
		// - Sinon, on crée une nouvelle entrée avec la quantité fournie.
		// - Le paramètre amount a une valeur par défaut 1, donc AddItem(item) ajoute 1 exemplaire.
	}
}