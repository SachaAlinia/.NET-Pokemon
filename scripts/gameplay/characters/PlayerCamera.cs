using Game.Core;
using Godot;

namespace Game.Gameplay;

/// <summary>
/// Caméra qui suit le joueur et limite le champ de vue.
/// </summary>
public partial class PlayerCamera : Camera2D
{
	// Niveau suivi par la caméra.
	[ExportCategory("Camera Vars")]
	[Export]
	public Level CurrentLevel;

	/// <summary>
	/// Initialise la caméra avec le niveau actuel.
	/// </summary>
	public override void _Ready()
	{
		CurrentLevel = SceneManager.Instance.CurrentLevel;
		UpdateCameraLimits();
	}

	/// <summary>
	/// Met à jour les limites de la caméra si le niveau change.
	/// </summary>
	/// <param name="delta">Temps écoulé depuis la dernière frame.</param>
	public override void _Process(double delta)
	{
		if (CurrentLevel != SceneManager.Instance.CurrentLevel)
		{
			CurrentLevel = SceneManager.Instance.CurrentLevel;
			UpdateCameraLimits();
		}
	}

	/// <summary>
	/// Met à jour les limites de la caméra selon les dimensions du niveau.
	/// </summary>
	public void UpdateCameraLimits()
	{
		LimitTop = CurrentLevel.Top;
		LimitBottom = CurrentLevel.Bottom;
		LimitLeft = CurrentLevel.Left;
		LimitRight = CurrentLevel.Right;
	}
}
