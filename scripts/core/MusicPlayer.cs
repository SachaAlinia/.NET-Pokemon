using Godot;
using System;

namespace Game.Core
{
	public partial class MusicPlayer : AudioStreamPlayer
	{
		private string _currentMusicPath; // Mémorise la musique en cours

		/// <summary>
		/// Joue un fichier audio. Ne fait rien si la musique demandée est déjà en lecture.
		/// </summary>
		public void PlayMusic(string path, float volumeDb = 0.0f)
		{
			// Sécurité : si on demande de jouer "musique1" alors qu'elle joue déjà, on ne recommence pas à zéro.
			if (_currentMusicPath == path) return;

			AudioStream stream = GD.Load<AudioStream>(path);

			if (stream != null)
			{
				Stream = stream;
				VolumeDb = volumeDb;
				_currentMusicPath = path;
				Play(); // Démarre la lecture audio
			}
		}
	}
}