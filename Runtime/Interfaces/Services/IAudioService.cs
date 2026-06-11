using System;
using Mudit.Core.Enums;

namespace Mudit.Core.Interfaces.Services
{
	public interface IAudioService : IRootService
	{
		void PlayMusic(string key, float fadeDuration = 0.5f);
		void PlaySfx(string key, float pitch = 1f);
		void StopMusic();
		void SetVolume(AudioChannel channel, float volume);
		IObservable<float> OnVolumeChanged(AudioChannel channel);
		float GetVolume(AudioChannel channel);
	}
}
