using Godot;
using System;

namespace Game.UI;

public partial class PauseMenu : CanvasLayer
{
	// On cache le menu au démarrage
	public override void _Ready()
	{
		Hide();

		// On s'assure que le menu peut fonctionner même quand le jeu est figé
		ProcessMode = ProcessModeEnum.Always;

		// --- CONNEXION MANUELLE DES BOUTONS ---
		// Vérifie bien que les noms "ResumeButton" et "QuitButton" sont EXACTEMENT 
		// les mêmes que dans ton arbre de scène (la hiérarchie à gauche).
		// Si tes boutons sont dans un Panel, le chemin est "Panel/ResumeButton"

		var resumeBtn = GetNode<Button>("CenterContainer/MenuContainer/ResumeButton");
		var quitBtn = GetNode<Button>("CenterContainer/MenuContainer/QuitButton");

		resumeBtn.Pressed += OnResumeButtonPressed;
		quitBtn.Pressed += OnQuitButtonPressed;

		GD.Print("PauseMenu prêt et boutons connectés !");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel")) // Touche Echap
		{
			TogglePause();
		}
	}

	private void TogglePause()
	{
		// Inverse l'état de pause de l'arbre de scènes (le jeu s'arrête ou reprend)
		GetTree().Paused = !GetTree().Paused;

		// Affiche ou cache le menu
		Visible = GetTree().Paused;

		GD.Print(GetTree().Paused ? "Jeu en pause" : "Reprise du jeu");
	}

	// Cette méthode sera appelée par le signal du bouton Resume
	private void OnResumeButtonPressed()
	{
		GD.Print("Clic sur Resume détecté !");
		TogglePause(); // Relance le jeu et cache le menu
	}

	// Cette méthode sera appelée par le signal du bouton Quit
	private void OnQuitButtonPressed()
	{
		GD.Print("Clic sur Quit détecté ! Au revoir !");
		GetTree().Quit(); // Ferme le jeu
	}
}