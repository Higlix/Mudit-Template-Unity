using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mudit.Core.Enums;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.Serializables.Configurations;
using UnityEngine;

namespace Mudit.Core.Services.SaveService
{
	public class SaveService : ISaveService
	{
		private string basePath;
		private string encryptionKey;
		private SaveMode defaultMode;
		private bool autoBackup;
		private readonly SemaphoreSlim writeLock = new(1, 1);

		public async UniTask InitializeAsync(ServiceData settings)
		{
			SaveConfig config = settings.SaveConfig;

			encryptionKey = config.GetEncryptionKey;
			defaultMode = config.GetDefaultSaveMode;
			autoBackup = config.GetEnableAutoBackup;

			basePath = Path.Combine(Application.persistentDataPath, "saves");
			Directory.CreateDirectory(basePath);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if (config.IsEncryptionKeyDefault)
			{
				Debug.LogWarning(
					"[SaveService] Encryption key is the default placeholder. " +
					"Change it in ServiceData -> SaveConfig before shipping.");
			}
#endif
			Debug.Log($"[SaveService] Initialized. Base path: {basePath}");
			await UniTask.CompletedTask;
		}

		// --- Key-based API (any serializable T) ---

		public async UniTask SaveAsync<T>(string key, T data, SaveMode mode = SaveMode.Encrypted)
		{
			await WriteAsync(key, data, 0, mode);
		}

		public async UniTask<SaveResult<T>> LoadAsync<T>(string key, SaveMode mode = SaveMode.Encrypted)
		{
			return await ReadAsync<T>(key, mode);
		}

		// --- ISaveableData API (key + version from the contract) ---

		public async UniTask SaveAsync<T>(T data, SaveMode mode = SaveMode.Encrypted) where T : ISaveableData
		{
			await WriteAsync(data.SaveKey, data, data.SaveVersion, mode);
		}

		public async UniTask<SaveResult<T>> LoadAsync<T>(SaveMode mode = SaveMode.Encrypted) where T : ISaveableData, new()
		{
			var probe = new T();
			return await ReadAsync<T>(probe.SaveKey, mode);
		}

		// --- Path management ---

		public void SetBasePath(string path)
		{
			basePath = path;
			Directory.CreateDirectory(basePath);
		}

		public string GetBasePath() => basePath;

		// --- Utility ---

		public async UniTask<bool> ExistsAsync(string key)
		{
			string filePath = GetFilePath(key);
			await UniTask.SwitchToThreadPool();
			bool exists = File.Exists(filePath);
			await UniTask.SwitchToMainThread();
			return exists;
		}

		public async UniTask DeleteAsync(string key)
		{
			string filePath = GetFilePath(key);
			string backupPath = filePath + ".bak";

			await UniTask.SwitchToThreadPool();
			if (File.Exists(filePath)) File.Delete(filePath);
			if (File.Exists(backupPath)) File.Delete(backupPath);
			await UniTask.SwitchToMainThread();
		}

		public async UniTask DeleteAllAsync()
		{
			await UniTask.SwitchToThreadPool();

			if (Directory.Exists(basePath))
			{
				string[] files = Directory.GetFiles(basePath);
				foreach (string file in files)
					File.Delete(file);
			}

			await UniTask.SwitchToMainThread();
		}

		// --- Internal I/O ---

		private async UniTask WriteAsync<T>(string key, T data, int version, SaveMode mode)
		{
			string filePath = GetFilePath(key);
			string tempPath = filePath + ".tmp";

			byte[] bytes = SaveSerializer.Serialize(data, version, mode, encryptionKey);

			await writeLock.WaitAsync();
			try
			{
				await UniTask.SwitchToThreadPool();

				Directory.CreateDirectory(Path.GetDirectoryName(filePath));
				File.WriteAllBytes(tempPath, bytes);
				if (File.Exists(filePath)) File.Delete(filePath);
				File.Move(tempPath, filePath);

				if (autoBackup)
					File.Copy(filePath, filePath + ".bak", true);

				await UniTask.SwitchToMainThread();
			}
			catch (Exception e)
			{
				await UniTask.SwitchToMainThread();
				Debug.LogError($"[SaveService] Write failed for key '{key}': {e.Message}");

				try { if (File.Exists(tempPath)) File.Delete(tempPath); }
				catch { /* best-effort cleanup */ }

				throw;
			}
			finally
			{
				writeLock.Release();
			}
		}

		private async UniTask<SaveResult<T>> ReadAsync<T>(string key, SaveMode mode)
		{
			string filePath = GetFilePath(key);

			await UniTask.SwitchToThreadPool();

			if (!File.Exists(filePath))
			{
				if (autoBackup)
				{
					string backupPath = filePath + ".bak";
					if (File.Exists(backupPath))
					{
						Debug.LogWarning($"[SaveService] Primary save missing for '{key}', restoring from backup.");
						File.Copy(backupPath, filePath);
					}
					else
					{
						await UniTask.SwitchToMainThread();
						return SaveResult<T>.Fail();
					}
				}
				else
				{
					await UniTask.SwitchToMainThread();
					return SaveResult<T>.Fail();
				}
			}

			byte[] bytes = File.ReadAllBytes(filePath);
			await UniTask.SwitchToMainThread();

			return SaveSerializer.Deserialize<T>(bytes, mode, encryptionKey);
		}

		private string GetFilePath(string key)
		{
			return Path.Combine(basePath, key + ".sav");
		}
	}
}
