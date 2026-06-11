using UnityEngine;
using System.Collections.Generic;

namespace Mudit.Core.ScriptableObjects.Data
{
	[CreateAssetMenu(fileName = "AudioData", menuName = "Mudit/Data/AudioData")]
	public class AudioData : ScriptableObject
	{
		[System.Serializable]
		public class AudioEntry
		{
			public string Key;
			public AudioClip Clip;
			[Range(0f, 1f)]
			public float Volume = 1f;
		}

		[Header("Volume Settings")]
		[Range(0f, 1f)] public float DefaultMasterVolume = 1f;
		[Range(0f, 1f)] public float DefaultMusicVolume = 0.8f;
		[Range(0f, 1f)] public float DefaultSfxVolume = 1f;

		[Header("Music")]
		[SerializeField] private List<AudioEntry> musicClips = new List<AudioEntry>();

		[Header("Sound Effects")]
		[SerializeField] private List<AudioEntry> sfxClips = new List<AudioEntry>();

		[Header("UI")]
		[SerializeField] private List<AudioEntry> uiClips = new List<AudioEntry>();
		// ----------------------------------
		private Dictionary<string, AudioEntry> clipDictionary;

		public void Initialize()
		{
			clipDictionary = new Dictionary<string, AudioEntry>();
			void AddClips(List<AudioEntry> list)
			{
				foreach (var entry in list)
				{
					if (!clipDictionary.ContainsKey(entry.Key))
					{
						clipDictionary.Add(entry.Key, entry);
					}
					else
					{
						Debug.LogWarning($"AudioSettings: Duplicate clip key '{entry.Key}' found.");
					}
				}
			}
			AddClips(musicClips);
			AddClips(sfxClips);
			AddClips(uiClips);
		}

		public bool TryGetClip(string key, out AudioClip clip, out float volume)
		{
			if (clipDictionary == null) Initialize();

			if (clipDictionary.TryGetValue(key, out var entry))
			{
				clip = entry.Clip;
				volume = entry.Volume;
				return true;
			}

			clip = null;
			volume = 0f;
			return false;
		}
	}
}