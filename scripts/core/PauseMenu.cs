using Godot;
using System;

namespace Game.UI;

public partial class PauseMenu : CanvasLayer
{
	/// <summary>
	/// Initialisation du menu de pause et connexion des boutons.
	/// </summary>
	public override void _Ready()
	{
		Hide(); // Menu invisible par défaut

		// CRUCIAL : "Always" permet au menu de continuer à fonctionner même si le reste du jeu est figé (Paused)
		ProcessMode = ProcessModeEnum.Always;

		// Récupération des boutons par leur chemin dans l'arbre de scène
		var resumeBtn = GetNode<Button>("CenterContainer/MenuContainer/ResumeButton");
		var quitBtn = GetNode<Button>("CenterContainer/MenuContainer/QuitButton");

		// Abonnement aux signaux "Pressed" (quand on clique)
		resumeBtn.Pressed += OnResumeButtonPressed;
		quitBtn.Pressed += OnQuitButtonPressed;

		GD.Print("PauseMenu prêt et boutons connectés !");
	}

	/// <summary>
	/// Détecte l'appui sur la touche Echap.
	/// </summary>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel")) // ui_cancel est mappé sur Echap par défaut
		{
			TogglePause();
		}
	}

	/// <summary>
	/// Alterne l'état de pause du moteur Godot.
	/// </summary>
	private void TogglePause()
	{
		// Inverse l'état de pause du moteur (! veut dire "contraire de")
		GetTree().Paused = !GetTree().Paused;

		// Affiche le menu si on est en pause, le cache sinon
		Visible = GetTree().Paused;

		GD.Print(GetTree().Paused ? "Jeu en pause" : "Reprise du jeu");
	}

	/// <summary>
	/// Action liée au bouton "Reprendre".
	/// </summary>
	private void OnResumeButtonPressed()
	{
		GD.Print("Clic sur Resume détecté !");
		TogglePause();
	}

	/// <summary>
	/// Action liée au bouton "Quitter".
	/// </summary>
	private void OnQuitButtonPressed()
	{
		GetTree().Quit(); // Ferme l'application
	}
}