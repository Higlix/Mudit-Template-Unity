using UnityEngine;
using Mudit.Core.Enums;

namespace Mudit.Core.Serializables.Configurations
{
	[System.Serializable]
	public class SaveConfig
	{
		[SerializeField]
		private string encryptionKey= "default-change-me";

		[SerializeField]
		private SaveMode defaultSaveMode = SaveMode.Encrypted;

		[SerializeField]
		private ushort maxSlots = 3;

		[SerializeField]
		private bool enableAutoBackup = false;


		private const string DefaultKeyPlaceholder = "default-change-me";

		public string GetEncryptionKey => encryptionKey;
		public SaveMode GetDefaultSaveMode => defaultSaveMode;
		public ushort GetMaxSlots => maxSlots;
		public bool GetEnableAutoBackup => enableAutoBackup;
		public bool IsEncryptionKeyDefault => encryptionKey == DefaultKeyPlaceholder;
	}
}