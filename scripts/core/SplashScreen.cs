using Godot;
using System;
using System.Threading.Tasks;
using Game.Core;

namespace Game.UI;

public partial class SplashScreen : Control
{
	[Export] public float DisplayTime = 9.0f; // Temps d'attente sur le logo
	[Export] public NodePath FadeRectPath;
	[Export] public string SplashMusicPath = "res://assets/audio/music/chargement.mp3";

	private ColorRect _fadeRect;

	/// <summary>
	/// Séquence d'introduction asynchrone (Musique -> Fondus -> Transition).
	/// </summary>
	public override async void _Ready()
	{
		_fadeRect = GetNode<ColorRect>(FadeRectPath);

		// 1. Musique via le lecteur global
		var musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer");
		musicPlayer.PlayMusic(SplashMusicPath, -5.0f);

		// 2. On cache temporairement le rideau du SceneManager pour utiliser le nôtre
		if (SceneManager.Instance != null && SceneManager.Instance.FadeRect != null)
		{
			SceneManager.Instance.FadeRect.Visible = false;
		}

		_fadeRect.Color = new Color(0, 0, 0, 1);
		_fadeRect.Visible = true;

		await Fade(1.0f, 1.0f); // Reste noir 1s

		// 4. Apparition du logo (devient transparent)
		await Fade(0.0f, 5.0f);

		// 5. Attente
		await Task.Delay((int)(DisplayTime * 1000));

		// 6. Disparition du logo vers le noir
		await Fade(2.0f, 2.0f);

		// 7. Prépare le rideau du SceneManager pour la suite
		if (SceneManager.Instance != null && SceneManager.Instance.FadeRect != null)
		{
			SceneManager.Instance.FadeRect.Color = new Color(0, 0, 0, 1);
			SceneManager.Instance.FadeRect.Visible = true;
		}

		GD.Print("[Splash] Chargement du GameManager...");
		// Change de scène vers le GameManager qui prend le relais
		GetTree().ChangeSceneToFile("res://scenes/core/game_manager.tscn");
	}

	/// <summary>
	/// Fonction générique pour animer la transparence du rectangle noir.
	/// </summary>
	private async Task Fade(float targetAlpha, float duration)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_fadeRect, "color:a", targetAlpha, duration)
			 .SetTrans(Tween.TransitionType.Quad);

		await ToSignal(tween, Tween.SignalName.Finished);
	}
}