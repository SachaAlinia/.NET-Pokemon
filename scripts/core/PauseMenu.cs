using Godot;
using System;

namespace Game.UI;

public partial class PauseMenu : CanvasLayer
{
	public override void _Ready()
	{
		// On cache le menu au démarrage
		Visible = false;

		// Connexion des signaux des boutons
		GetNode<Button>("MenuContainer/ResumeButton").Pressed += OnResumePressed;
		GetNode<Button>("MenuContainer/QuitButton").Pressed += OnQuitPressed;
	}

	public override void _Input(InputEvent @event)
	{
		// Si on appuie sur Echap (UI Cancel par défaut dans Godot)
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
	}

	public void TogglePause()
	{
		bool isPaused = !GetTree().Paused;
		GetTree().Paused = isPaused;
		Visible = isPaused;

		if (isPaused)
			GD.Print("Jeu en Pause");
		else
			GD.Print("Reprise du jeu");
	}

	private void OnResumePressed()
	{
		TogglePause();
	}

	private void OnQuitPressed()
	{
		// On quitte proprement en dépausant avant pour éviter des bugs
		GetTree().Paused = false;
		GetTree().Quit();
	}
}