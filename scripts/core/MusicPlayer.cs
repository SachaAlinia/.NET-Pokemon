using Godot;
using System;

namespace Game.Core
{
	// Vérifie bien que "partial" est présent et que le nom est identique à l'AutoLoad
	public partial class MusicPlayer : AudioStreamPlayer
	{
		private string _currentMusicPath;

		public void PlayMusic(string path, float volumeDb = 0.0f)
		{
			if (_currentMusicPath == path) return;

			AudioStream stream = GD.Load<AudioStream>(path);

			if (stream != null)
			{
				Stream = stream;
				VolumeDb = volumeDb; 
				_currentMusicPath = path;
				Play();
			}
		}
	}
}