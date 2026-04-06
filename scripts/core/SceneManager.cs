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
	public static SceneManager Instance { get; private set; }
	public static bool IsChanging { get; private set; }

	[ExportCategory("Scene Manager Variables")]
	[Export]
	public ColorRect FadeRect;

	[Export]
	public Level CurrentLevel;

	[Export]
	public Array<Level> AllLevels;

	public override void _Ready()
	{
		Instance = this;
		IsChanging = false;

		Logger.Info("Loading scene manager ...");
	}

	// --- NOUVELLE MÉTHODE : LANCER LE COMBAT ---
	public static async void StartBattle(PokemonResource wildPokemon)
	{
		if (IsChanging) return;
		IsChanging = true;

		Logger.Info($"Starting battle against {wildPokemon.Name}...");

		// 1. On fige et on CACHE le joueur
		var player = GameManager.GetPlayer();
		if (player != null)
		{
			player.SetProcess(false);
			player.Hide(); // TRÈS IMPORTANT : pour que le sprite disparaisse
		}

		// 2. Fondu au noir
		await Instance.FadeOut();

		// 3. Charger la scène de combat
		var battleScenePrefab = GD.Load<PackedScene>("res://scenes/core/battle_scene.tscn");
		var battleInstance = battleScenePrefab.Instantiate<BattleScene>();

		// 4. Injecter les Pokémon AVANT de l'ajouter à l'arbre
		battleInstance.EnemyPokemon = wildPokemon;
		battleInstance.PlayerPokemon = GD.Load<PokemonResource>("res://Resources/Pokemon/charizard.tres");

		// 5. Nettoyage de l'overworld
		if (Instance.CurrentLevel != null)
		{
			// On le cache plutôt que de le supprimer pour revenir plus facilement après
			Instance.CurrentLevel.Hide();
			// Ou si tu veux vraiment le supprimer :
			// GameManager.GetGameViewPort().RemoveChild(Instance.CurrentLevel);
		}

		// 6. Ajouter la scène de combat
		GameManager.GetGameViewPort().AddChild(battleInstance);

		// 7. Musique et Fin du fondu
		var musicPlayer = Instance.GetNode<MusicPlayer>("/root/MusicPlayer");
		musicPlayer.PlayMusic("res://assets/audio/music/battle_wild.mp3", -20.0f);

		await Instance.FadeIn();
		IsChanging = false;
	}

	public static async void ChangeLevel(LevelName levelName = LevelName.small_town, int trigger = 0, bool spawn = false)
	{
		while (IsChanging)
		{
			await Instance.ToSignal(Instance.GetTree(), "process_frame");
		}

		IsChanging = true;

		await Instance.GetLevel(levelName);

		var musicPlayer = Instance.GetNode<MusicPlayer>("/root/MusicPlayer");

		switch (levelName)
		{
			case LevelName.small_town:
				musicPlayer.PlayMusic("res://assets/audio/music/music1.mp3", -22.0f);
				break;
			case LevelName.small_town_cave:
				musicPlayer.PlayMusic("res://assets/audio/music/music2.mp3", -17.0f);
				break;
			case LevelName.small_town_greens_house:
			case LevelName.small_town_purples_house:
				musicPlayer.PlayMusic("res://assets/audio/music/music3.mp3", -20.0f);
				break;
			case LevelName.small_town_pokemon_center:
				musicPlayer.PlayMusic("res://assets/audio/music/music4.mp3", -28.0f);
				break;
		}

		if (spawn)
		{
			Instance.Spawn();
		}
		else
		{
			Instance.Switch(trigger);
		}

		await Instance.FadeIn();
		IsChanging = false;
	}

	public async Task GetLevel(LevelName levelName)
	{
		if (CurrentLevel != null)
		{
			await Instance.FadeOut();
			GameManager.GetGameViewPort().RemoveChild(CurrentLevel);
		}

		CurrentLevel = AllLevels.FirstOrDefault(level => level.LevelName == levelName);

		if (CurrentLevel != null)
		{
			GameManager.GetGameViewPort().AddChild(CurrentLevel);
		}
		else
		{
			CurrentLevel = GD.Load<PackedScene>("res://scenes/levels/" + levelName + ".tscn").Instantiate<Level>();
			AllLevels.Add(CurrentLevel);
			GameManager.GetGameViewPort().AddChild(CurrentLevel);
		}
	}

	public void Spawn()
	{
		var spawnPoints = CurrentLevel.GetTree().GetNodesInGroup(LevelGroup.SPAWNPOINTS.ToString());

		if (spawnPoints.Count <= 0)
			throw new Exception("Missing spawn point(s)!");

		var spawnPoint = (SpawnPoint)spawnPoints[0];
		var player = GD.Load<PackedScene>("res://scenes/characters/player.tscn").Instantiate<Player>();

		GameManager.AddPlayer(player);
		GameManager.GetPlayer().Position = spawnPoint.Position;
	}

	public void Switch(int trigger)
	{
		var sceneTriggers = CurrentLevel.GetTree().GetNodesInGroup(LevelGroup.SCENETRIGGERS.ToString());

		if (sceneTriggers.Count <= 0)
			throw new Exception("Missing scene trigger(s)!");

		if (sceneTriggers.FirstOrDefault(st => ((SceneTrigger)st).CurrentLevelTrigger == trigger) is not SceneTrigger sceneTrigger)
			throw new Exception($"Missing scene trigger {trigger}");

		GameManager.GetPlayer().Position = sceneTrigger.Position + sceneTrigger.EntryDirection * Globals.GRID_SIZE;
	}

	public async Task FadeOut()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(FadeRect, "color:a", 1.25, 0.75);
		await ToSignal(tween, "finished");
	}

	public async Task FadeIn()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(FadeRect, "color:a", 0.0, 1.25);
		await ToSignal(tween, "finished");
	}

	public static Level GetCurrentLevel()
	{
		return Instance.CurrentLevel;
	}
}