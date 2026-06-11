using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.Serializables.Configurations;
using UnityEngine;
using VContainer;

namespace Mudit.Core.Services.SaveSlotManager
{
	public class SaveSlotManager : ISaveSlotManager
	{
		private readonly ISaveService saveService;
		private string savesRoot;
		private int activeSlot;
		private int maxSlots;

		[Inject]
		public SaveSlotManager(ISaveService saveService)
		{
			this.saveService = saveService;
		}

		public int ActiveSlot => activeSlot;
		public int MaxSlots => maxSlots;

		public async UniTask InitializeAsync(ServiceData settings)
		{
			SaveConfig config = settings.SaveConfig;
			maxSlots = config.GetMaxSlots;
			savesRoot = Path.Combine(Application.persistentDataPath, "saves");
			Directory.CreateDirectory(savesRoot);

			int storedSlot = await ReadSlotIndexAsync();
			await ApplySlotAsync(storedSlot);

			Debug.Log($"[SaveSlotManager] Initialized. Active slot: {activeSlot}, Max slots: {maxSlots}");
		}

		public async UniTask SetActiveSlotAsync(int slotIndex)
		{
			ValidateSlotIndex(slotIndex);
			await ApplySlotAsync(slotIndex);
			await WriteSlotIndexAsync(slotIndex);
		}

		public async UniTask<int[]> GetUsedSlotsAsync()
		{
			await UniTask.SwitchToThreadPool();

			var used = new List<int>();
			for (int i = 0; i < maxSlots; i++)
			{
				string slotDir = GetSlotDirectory(i);
				if (Directory.Exists(slotDir))
					used.Add(i);
			}

			await UniTask.SwitchToMainThread();
			return used.ToArray();
		}

		public async UniTask CreateSlotAsync(int slotIndex)
		{
			ValidateSlotIndex(slotIndex);

			await UniTask.SwitchToThreadPool();
			Directory.CreateDirectory(GetSlotDirectory(slotIndex));
			await UniTask.SwitchToMainThread();
		}

		public async UniTask DeleteSlotAsync(int slotIndex)
		{
			ValidateSlotIndex(slotIndex);

			string slotDir = GetSlotDirectory(slotIndex);

			await UniTask.SwitchToThreadPool();
			if (Directory.Exists(slotDir))
				Directory.Delete(slotDir, true);
			await UniTask.SwitchToMainThread();

			if (activeSlot == slotIndex)
			{
				await ApplySlotAsync(0);
				await WriteSlotIndexAsync(0);
			}
		}

		public async UniTask<bool> SlotExistsAsync(int slotIndex)
		{
			ValidateSlotIndex(slotIndex);

			await UniTask.SwitchToThreadPool();
			bool exists = Directory.Exists(GetSlotDirectory(slotIndex));
			await UniTask.SwitchToMainThread();
			return exists;
		}

		private async UniTask ApplySlotAsync(int slotIndex)
		{
			activeSlot = slotIndex;
			string slotDir = GetSlotDirectory(slotIndex);
			Directory.CreateDirectory(slotDir);
			saveService.SetBasePath(slotDir);
			await UniTask.CompletedTask;
		}

		private string GetSlotDirectory(int slotIndex)
		{
			return Path.Combine(savesRoot, $"slot_{slotIndex}");
		}

		private void ValidateSlotIndex(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= maxSlots)
				throw new ArgumentOutOfRangeException(nameof(slotIndex),
					$"Slot index {slotIndex} is out of range [0, {maxSlots}).");
		}

		// --- Slot index persistence ---

		private string SlotIndexFilePath => Path.Combine(savesRoot, "slot_index.json");

		private async UniTask<int> ReadSlotIndexAsync()
		{
			await UniTask.SwitchToThreadPool();

			string json = null;
			if (File.Exists(SlotIndexFilePath))
			{
				try
				{
					json = File.ReadAllText(SlotIndexFilePath);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"[SaveSlotManager] Failed to read slot index file, defaulting to 0: {e.Message}");
				}
			}

			await UniTask.SwitchToMainThread();

			int result = 0;
			if (json != null)
			{
				try
				{
					var data = JsonUtility.FromJson<SlotIndexData>(json);
					if (data.activeSlot >= 0 && data.activeSlot < maxSlots)
						result = data.activeSlot;
				}
				catch (Exception e)
				{
					Debug.LogWarning($"[SaveSlotManager] Failed to parse slot index, defaulting to 0: {e.Message}");
				}
			}

			return result;
		}

		private async UniTask WriteSlotIndexAsync(int slotIndex)
		{
			var data = new SlotIndexData { activeSlot = slotIndex };
			string json = JsonUtility.ToJson(data, false);

			await UniTask.SwitchToThreadPool();

			string tempPath = SlotIndexFilePath + ".tmp";
			File.WriteAllText(tempPath, json);
			if (File.Exists(SlotIndexFilePath)) File.Delete(SlotIndexFilePath);
			File.Move(tempPath, SlotIndexFilePath);

			await UniTask.SwitchToMainThread();
		}

		[Serializable]
		private class SlotIndexData
		{
			public int activeSlot;
		}
	}
}
