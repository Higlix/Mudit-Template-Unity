using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.Enums;

namespace Mudit.Core.Services.AudioService
{
	public class AudioService : IAudioService, IDisposable
	{
		private readonly CompositeDisposable disposables = new CompositeDisposable();
		
		// Reactive Properties for Volume
		private readonly ReactiveProperty<float> masterVolume = new ReactiveProperty<float>(1f);
		private readonly ReactiveProperty<float> musicVolume = new ReactiveProperty<float>(1f);
		private readonly ReactiveProperty<float> sfxVolume = new ReactiveProperty<float>(1f);
		private readonly ReactiveProperty<float> uiVolume = new ReactiveProperty<float>(1f);

		// Audio Sources
		private GameObject audioRoot;
		private AudioSource musicSource;
		private AudioSource musicSourceSecondary; // For cross-fading
		private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
		private List<AudioSource> activeSfxSources = new List<AudioSource>();
		
		private AudioData audioData;
		private const int InitialSfxPoolSize = 10;

		public async UniTask InitializeAsync(ServiceData settings)
		{
			audioData = settings.AudioData;
			if (audioData != null)
			{
				audioData.Initialize();
				masterVolume.Value = audioData.DefaultMasterVolume;
				musicVolume.Value = audioData.DefaultMusicVolume;
				sfxVolume.Value = audioData.DefaultSfxVolume;
				uiVolume.Value = audioData.DefaultSfxVolume; // Default UI to SFX volume
			}

			// Create Audio Root
			audioRoot = new GameObject("AudioService_Root");
			Object.DontDestroyOnLoad(audioRoot);

			// Setup Music Sources
			musicSource = CreateSource("MusicSource_Main", true);
			musicSourceSecondary = CreateSource("MusicSource_Sec", true);

			// Setup SFX Pool
			for (int i = 0; i < InitialSfxPoolSize; i++)
			{
				var source = CreateSource($"SfxSource_{i}", false);
				sfxPool.Enqueue(source);
			}

			// Subscribe to volume changes to update playing sources immediately
			masterVolume.Subscribe(_ => UpdateAllVolumes()).AddTo(disposables);
			musicVolume.Subscribe(_ => UpdateAllVolumes()).AddTo(disposables);
			sfxVolume.Subscribe(_ => UpdateAllVolumes()).AddTo(disposables);
			uiVolume.Subscribe(_ => UpdateAllVolumes()).AddTo(disposables);

			Debug.Log("AudioService Initialized.");
			await UniTask.CompletedTask;
		}

		private AudioSource CreateSource(string name, bool loop)
		{
			var go = new GameObject(name);
			go.transform.SetParent(audioRoot.transform);
			var source = go.AddComponent<AudioSource>();
			source.loop = loop;
			source.playOnAwake = false;
			return source;
		}

		public void PlayMusic(string key, float fadeDuration = 0.5f)
		{
			if (audioData == null || !audioData.TryGetClip(key, out var clip, out var volumeScale))
			{
				Debug.LogWarning($"AudioService: Music clip '{key}' not found.");
				return;
			}

			if (musicSource.clip == clip && musicSource.isPlaying) return;

			Debug.Log($"AudioService: Playing music clip '{key}' with volume scale {volumeScale} and fade duration {fadeDuration}");
			PlayMusicWithFade(clip, volumeScale, fadeDuration).Forget();
		}

		public void StopMusic()
		{
			musicSource.Stop();
			musicSource.clip = null;
			musicSourceSecondary.Stop();
			musicSourceSecondary.clip = null;
		}

		private async UniTaskVoid PlayMusicWithFade(AudioClip newClip, float volumeScale, float duration)
		{
			// Simple crossfade: Fade out current, swap, fade in. 
			// Improved: Use secondary source to crossfade.
			
			var targetSource = musicSource.isPlaying ? musicSourceSecondary : musicSource;
			var currentSource = musicSource.isPlaying ? musicSource : musicSourceSecondary;

			targetSource.clip = newClip;
			targetSource.volume = 0f;
			targetSource.Play();

			float timer = 0f;
			float startVol = currentSource.volume;
			
			// Crossfade
			while (timer < duration)
			{
				timer += Time.deltaTime;
				float t = timer / duration;

				currentSource.volume = Mathf.Lerp(startVol, 0f, t);
				targetSource.volume = Mathf.Lerp(0f, GetChannelVolume(AudioChannel.Music) * GetChannelVolume(AudioChannel.Master) * volumeScale, t);

				await UniTask.Yield();
			}

			currentSource.Stop();
			currentSource.clip = null;
			musicSource = targetSource; // Swap references so musicSource is always the active one
			musicSourceSecondary = currentSource;
		}

		public void PlaySfx(string key, float pitch = 1f)
		{
			if (audioData == null || !audioData.TryGetClip(key, out var clip, out var volumeScale))
			{
				Debug.LogWarning($"AudioService: SFX clip '{key}' not found.");
				return;
			}

			var source = GetSfxSource();
			source.pitch = pitch;
			source.volume = GetChannelVolume(AudioChannel.Sfx) * GetChannelVolume(AudioChannel.Master) * volumeScale;
			
			// Track active source
			activeSfxSources.Add(source);
			
			source.PlayOneShot(clip);
			
			ReturnSourceAfterPlay(source, clip.length).Forget();
		}
		
		private async UniTaskVoid ReturnSourceAfterPlay(AudioSource source, float delay)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delay));
			if (source != null) // Check if destroyed
			{
				activeSfxSources.Remove(source);
				sfxPool.Enqueue(source);
			}
		}

		private AudioSource GetSfxSource()
		{
			if (sfxPool.Count > 0)
			{
				return sfxPool.Dequeue();
			}
			
			// Expand pool
			var source = CreateSource($"SfxSource_{InitialSfxPoolSize + 1}", false);
			return source;
		}

		public void SetVolume(AudioChannel channel, float volume)
		{
			volume = Mathf.Clamp01(volume);
			switch (channel)
			{
				case AudioChannel.Master: masterVolume.Value = volume; break;
				case AudioChannel.Music: musicVolume.Value = volume; break;
				case AudioChannel.Sfx: sfxVolume.Value = volume; break;
				case AudioChannel.UI: uiVolume.Value = volume; break;
			}
		}

		public IObservable<float> OnVolumeChanged(AudioChannel channel)
		{
			switch (channel)
			{
				case AudioChannel.Master: return masterVolume;
				case AudioChannel.Music: return musicVolume;
				case AudioChannel.Sfx: return sfxVolume;
				case AudioChannel.UI: return uiVolume;
				default: return Observable.Empty<float>();
			}
		}

		public float GetVolume(AudioChannel channel)
		{
			switch (channel)
			{
				case AudioChannel.Master: return masterVolume.Value;
				case AudioChannel.Music: return musicVolume.Value;
				case AudioChannel.Sfx: return sfxVolume.Value;
				case AudioChannel.UI: return uiVolume.Value;
				default: return 0f;
			}
		}

		private float GetChannelVolume(AudioChannel channel)
		{
			return GetVolume(channel);
		}

		private void UpdateAllVolumes()
		{
			// Update Music
			if (musicSource != null) 
				musicSource.volume = GetChannelVolume(AudioChannel.Music) * GetChannelVolume(AudioChannel.Master); 
				
			// Update Active SFX
			foreach(var source in activeSfxSources)
			{
				if (source != null)
					source.volume = GetChannelVolume(AudioChannel.Sfx) * GetChannelVolume(AudioChannel.Master);
			}
			
			// Update Idle SFX (so they start with correct volume next time, though PlaySfx sets it anyway)
			foreach(var source in sfxPool)
			{
				if (source != null)
					source.volume = GetChannelVolume(AudioChannel.Sfx) * GetChannelVolume(AudioChannel.Master);
			}
		}

		public void Dispose()
		{
			disposables.Dispose();
			if (audioRoot != null)
			{
				Object.Destroy(audioRoot);
			}
		}
	}
}