using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Gameplay;
using Godot;
using Godot.Collections;
using Game.Core;

namespace Game.Core;

public partial class SceneManager : Node
{
	/// <summary>
	/// LE SINGLETON : C'est une astuce pour que tous les autres fichiers 
	/// puissent parler au SceneManager sans le chercher partout.
	/// </summary>
	public static SceneManager Instance { get; private set; }

	/// <summary>
	/// UN VERROU (Boolean) : Vrai ou Faux. 
	/// Permet d'éviter que le joueur change de scène deux fois en même temps.
	/// </summary>
	public static bool IsChanging { get; private set; }

	// [Export] permet de voir et de glisser-déposer ces objets directement dans l'éditeur Godot.
	[ExportCategory("Scene Manager Variables")]
	[Export]
	public ColorRect FadeRect; // Le rectangle qui sert à faire l'écran noir.

	[Export]
	public Level CurrentLevel; // La carte (le niveau) sur laquelle le joueur marche actuellement.

	[Export]
	public Array<Level> AllLevels; // Un sac (liste) qui contient tous les niveaux déjà visités pour les rouvrir plus vite.

	/// <summary>
	/// _Ready se lance une seule fois quand le SceneManager apparaît dans le jeu.
	/// </summary>
	public override void _Ready()
	{
		// On dit : "L'instance officielle, c'est moi !"
		Instance = this;
		// Au début, on ne change pas de scène, donc c'est faux.
		IsChanging = false;

		// Affiche un message dans la console pour dire que tout va bien.
		Logger.Info("Loading scene manager ...");
	}

	/// <summary>
	/// FONCTION DE COMBAT : Appelé quand on touche une haute herbe.
	/// 'static' = on peut l'appeler via SceneManager.StartBattle()
	/// 'async' = la fonction peut faire des pauses (attendre un fondu).
	/// </summary>
	public static async void StartBattle(PokemonResource wildPokemon)
	{
		if (IsChanging) return;
		IsChanging = true;

		Logger.Info($"Starting battle against {wildPokemon.Name}...");

		// On va chercher le joueur pour le "figer"
		var player = GameManager.GetPlayer();
		if (player == null)
		{
			Logger.Error("Player not found! Cannot start battle.");
			IsChanging = false;
			return;
		}

		player.SetProcess(false);
		player.Hide();

		var playerCamera = player.GetNodeOrNull<Camera2D>("Camera2D");
		if (playerCamera != null)
		{
			playerCamera.Set("current", false);
		}

		await Instance.FadeOut();

		var battleScenePrefab = GD.Load<PackedScene>("res://scenes/core/battle_scene.tscn");
		var battleInstance = battleScenePrefab.Instantiate<BattleScene>();

		// On donne au combat les infos du monstre sauvage et de notre Dracaufeu.
		battleInstance.EnemyPokemon = wildPokemon;
		battleInstance.PlayerPokemon = GD.Load<PokemonResource>("res://resources/pokemon/charizard.tres");

		if (Instance.CurrentLevel != null)
		{
			Instance.CurrentLevel.Hide();
		}

		// Ajoute la scène de combat dans le SubViewport du jeu
		GameManager.GetGameViewPort().AddChild(battleInstance);

		// Crée et active une caméra de combat pour avoir le bon zoom
		var battleCamera = new Camera2D
		{
			Zoom = Vector2.One,
			Position = new Vector2(576, 324), // Centre de la résolution 1152x648
		};
		battleInstance.AddChild(battleCamera);
		battleCamera.MakeCurrent();

		var musicPlayer = Instance.GetNode<MusicPlayer>("/root/MusicPlayer");
		musicPlayer.PlayMusic("res://assets/audio/music/battle_wild.mp3", -20.0f);

		await Instance.FadeIn();
		IsChanging = false;
	}

	/// <summary>
	/// CHANGER DE CARTE : Pour passer d'une ville à une route par exemple.
	/// </summary>
	public static async void ChangeLevel(LevelName levelName = LevelName.small_town, int trigger = 0, bool spawn = false)
	{
		// Tant que (while) le verrou est actif, on attend une image (process_frame).
		while (IsChanging)
		{
			await Instance.ToSignal(Instance.GetTree(), "process_frame");
		}

		IsChanging = true;

		// On appelle la fonction pour charger les fichiers de la nouvelle carte.
		await Instance.GetLevel(levelName);

		var musicPlayer = Instance.GetNode<MusicPlayer>("/root/MusicPlayer");

		//selon le nom du niveau, on choisit la musique.
		switch (levelName)
		{
			case LevelName.small_town:
				musicPlayer.PlayMusic("res://assets/audio/music/music1.mp3", -22.0f);
				break;
			case LevelName.small_town_cave:
				musicPlayer.PlayMusic("res://assets/audio/music/music2.mp3", -17.0f);
				break;
			case LevelName.small_town_greens_house:
				musicPlayer.PlayMusic("res://assets/audio/music/music3.mp3", -17.0f);
				break;
			case LevelName.small_town_purples_house:
				musicPlayer.PlayMusic("res://assets/audio/music/music3.mp3", -17.0f);
				break;
			case LevelName.small_town_pokemon_center:
				musicPlayer.PlayMusic("res://assets/audio/music/music4.mp3", -17.0f);
				break;

		}

		// Si c'est le début du jeu (spawn), on crée le joueur.
		if (spawn)
		{
			Instance.Spawn();
		}
		else
		{
			// Sinon on le déplace juste vers la porte (trigger).
			Instance.Switch(trigger);
		}

		// On retire le noir.
		await Instance.FadeIn();
		IsChanging = false;
	}

	/// <summary>
	/// CHARGEMENT : Va chercher le niveau dans la mémoire ou sur le disque.
	/// </summary>
	public async Task GetLevel(LevelName levelName)
	{
		if (CurrentLevel != null)
		{
			await Instance.FadeOut();
			// On enlève l'ancien niveau de l'affichage.
			GameManager.GetGameViewPort().RemoveChild(CurrentLevel);
		}

		// On regarde dans notre "sac" AllLevels si on a déjà chargé ce niveau avant.
		CurrentLevel = AllLevels.FirstOrDefault(level => level.LevelName == levelName);

		if (CurrentLevel != null)
		{
			// Si oui, on le réaffiche simplement.
			GameManager.GetGameViewPort().AddChild(CurrentLevel);
		}
		else
		{
			// Si non, on charge le fichier .tscn et on l'ajoute à notre sac pour la prochaine fois.
			CurrentLevel = GD.Load<PackedScene>("res://scenes/levels/" + levelName + ".tscn").Instantiate<Level>();
			AllLevels.Add(CurrentLevel);
			GameManager.GetGameViewPort().AddChild(CurrentLevel);
		}
	}

	/// <summary>
	/// APPARITION : Crée le joueur pour la première fois.
	/// </summary>
	public void Spawn()
	{
		// On cherche dans Godot tous les objets qui sont dans le groupe "SPAWNPOINTS".
		var spawnPoints = CurrentLevel.GetTree().GetNodesInGroup(LevelGroup.SPAWNPOINTS.ToString());

		if (spawnPoints.Count <= 0)
			throw new Exception("Missing spawn point(s)!"); // Erreur si on a oublié d'en mettre un dans l'éditeur.

		var spawnPoint = (SpawnPoint)spawnPoints[0]; // On prend le premier point trouvé.
													 // On crée le joueur à partir de son fichier .tscn.
		var player = GD.Load<PackedScene>("res://scenes/characters/player.tscn").Instantiate<Player>();

		GameManager.AddPlayer(player); // On l'enregistre dans le GameManager.
		GameManager.GetPlayer().Position = spawnPoint.Position; // On le pose au bon endroit.
	}

	/// <summary>
	/// TELEPORTATION : Déplace le joueur quand il prend une porte.
	/// </summary>
	public void Switch(int trigger)
	{
		// On cherche les sorties de secours / portes.
		var sceneTriggers = CurrentLevel.GetTree().GetNodesInGroup(LevelGroup.SCENETRIGGERS.ToString());

		if (sceneTriggers.Count <= 0)
			throw new Exception("Missing scene trigger(s)!");

		// On cherche la porte qui a le numéro (trigger) demandé.
		if (sceneTriggers.FirstOrDefault(st => ((SceneTrigger)st).CurrentLevelTrigger == trigger) is not SceneTrigger sceneTrigger)
			throw new Exception($"Missing scene trigger {trigger}");

		// On déplace le joueur à la position de la porte + un petit décalage (GRID_SIZE) pour qu'il ne rentre pas immédiatement.
		GameManager.GetPlayer().Position = sceneTrigger.Position + sceneTrigger.EntryDirection * Globals.GRID_SIZE;
	}

	/// <summary>
	/// FONDU NOIR : Utilise un 'Tween' (un moteur d'animation fluide).
	/// </summary>
	public async Task FadeOut()
	{
		Tween tween = CreateTween();
		// On dit au rectangle noir : "Passe ton opacité (a) à 1.25 (totalement noir) en 0.75 secondes".
		tween.TweenProperty(FadeRect, "color:a", 1.25, 0.75);
		await ToSignal(tween, "finished"); // On attend la fin de l'animation.
	}

	/// <summary>
	/// FONDU TRANSPARENT : Rend le rectangle noir invisible.
	/// </summary>
	public async Task FadeIn()
	{
		Tween tween = CreateTween();
		// On repasse l'opacité à 0.
		tween.TweenProperty(FadeRect, "color:a", 0.0, 1.25);
		await ToSignal(tween, "finished");
	}

	public static Level GetCurrentLevel()
	{
		return Instance.CurrentLevel;
	}
}