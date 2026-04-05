using Godot;
using System;
using System.Threading.Tasks;
using Game.Core;

namespace Game.UI;

public partial class SplashScreen : Control
{
	[Export] public float DisplayTime = 3.0f;
	[Export] public NodePath FadeRectPath;

	private ColorRect _fadeRect;

	public override async void _Ready()
	{
		// 1. Initialisation des références
		_fadeRect = GetNode<ColorRect>(FadeRectPath);

		// --- SYNCHRONISATION AVEC LE SCENEMANAGER (AUTOLOAD) ---
		// On force le rideau du SceneManager à disparaître pour voir notre Splash
		if (SceneManager.Instance != null && SceneManager.Instance.FadeRect != null)
		{
			SceneManager.Instance.FadeRect.Visible = false;
			GD.Print("[Splash] Rideau du SceneManager masqué.");
		}

		// 2. Préparation du rideau local du SplashScreen
		// On commence en noir total
		_fadeRect.Color = new Color(0, 0, 0, 1);
		_fadeRect.Visible = true;

		// 3. Étape 1 : Apparition du logo (le noir local devient transparent)
		GD.Print("[Splash] Apparition du logo...");
		await Fade(0.0f, 1.5f);

		// 4. Attente (temps d'affichage du logo)
		await Task.Delay((int)(DisplayTime * 1000));

		// 5. Étape 2 : Disparition du logo (le noir local revient)
		GD.Print("[Splash] Disparition du logo...");
		await Fade(1.0f, 1.0f);

		// --- PRÉPARATION DU CHANGEMENT DE SCÈNE ---
		// Avant de quitter, on demande au SceneManager de remettre SON rideau en noir.
		// Comme ça, quand cette scène sera détruite, l'écran restera noir pendant
		// que le GameManager initialise la ville.
		if (SceneManager.Instance != null && SceneManager.Instance.FadeRect != null)
		{
			SceneManager.Instance.FadeRect.Color = new Color(0, 0, 0, 1);
			SceneManager.Instance.FadeRect.Visible = true;
			GD.Print("[Splash] Rideau du SceneManager réactivé pour la transition.");
		}

		// 6. Lancement du jeu
		GD.Print("[Splash] Chargement du GameManager...");
		GetTree().ChangeSceneToFile("res://scenes/core/game_manager.tscn");
	}

	private async Task Fade(float targetAlpha, float duration)
	{
		Tween tween = CreateTween();
		// On utilise "color:a" pour animer uniquement la transparence du ColorRect
		tween.TweenProperty(_fadeRect, "color:a", targetAlpha, duration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.InOut);

		await ToSignal(tween, Tween.SignalName.Finished);
	}
}