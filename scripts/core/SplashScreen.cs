using Godot;
using System;
using System.Threading.Tasks;
using Game.Core;

namespace Game.UI;

public partial class SplashScreen : Control
{
	[Export] public float DisplayTime = 9.0f;
	[Export] public NodePath FadeRectPath;
	[Export] public string SplashMusicPath = "res://assets/audio/music/chargement.mp3";

	private ColorRect _fadeRect;

	public override async void _Ready()
	{
		_fadeRect = GetNode<ColorRect>(FadeRectPath);

		// 1. Lancement de la musique de démarrage via l'Autoload MusicPlayer
		var musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer");
		musicPlayer.PlayMusic(SplashMusicPath, -5.0f);

		// 2. On cache le rideau du SceneManager pour voir notre logo
		if (SceneManager.Instance != null && SceneManager.Instance.FadeRect != null)
		{
			SceneManager.Instance.FadeRect.Visible = false;
		}

		// 3. Configuration initiale du rideau local (Noir opaque)
		_fadeRect.Color = new Color(0, 0, 0, 1);
		_fadeRect.Visible = true;

		await Fade(1.0f, 1.0f);

		// 4. Apparition du logo
		await Fade(0.0f, 5.0f);

		// 5. Temps de pause sur le logo
		await Task.Delay((int)(DisplayTime * 1000));

		// 6. Disparition du logo vers le noir
		await Fade(2.0f, 2.0f);

		// 7. Transition vers le jeu
		// On réactive le rideau du SceneManager pour éviter le flash blanc/gris
		if (SceneManager.Instance != null && SceneManager.Instance.FadeRect != null)
		{
			SceneManager.Instance.FadeRect.Color = new Color(0, 0, 0, 1);
			SceneManager.Instance.FadeRect.Visible = true;
		}

		GD.Print("[Splash] Chargement du GameManager...");
		// On lance le GameManager qui va ensuite piloter le SceneManager
		GetTree().ChangeSceneToFile("res://scenes/core/game_manager.tscn");
	}

	private async Task Fade(float targetAlpha, float duration)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_fadeRect, "color:a", targetAlpha, duration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.InOut);

		await ToSignal(tween, Tween.SignalName.Finished);
	}
}