using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Mudit.Core.Enums;
using Mudit.Core.Interfaces.Services;
using UnityEngine;

namespace Mudit.Core.Services.SaveService
{
	internal static class SaveSerializer
	{
		private const int IvSizeBytes = 16;
		private const int KeySizeBytes = 32;

		public static byte[] Serialize<T>(T data, int version, SaveMode mode, string encryptionKey)
		{
			string json = JsonUtility.ToJson(data, false);

			var envelope = new SaveEnvelope
			{
				version = version,
				typeId = typeof(T).FullName,
				payload = json
			};

			string envelopeJson = JsonUtility.ToJson(envelope, false);
			byte[] raw = Encoding.UTF8.GetBytes(envelopeJson);

			return mode switch
			{
				SaveMode.Plain => raw,
				SaveMode.Compressed => Compress(raw),
				SaveMode.Encrypted => Encrypt(raw, encryptionKey),
				SaveMode.EncryptedCompressed => Encrypt(Compress(raw), encryptionKey),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
			};
		}

		public static SaveResult<T> Deserialize<T>(byte[] bytes, SaveMode mode, string encryptionKey)
		{
			try
			{
				byte[] raw = mode switch
				{
					SaveMode.Plain => bytes,
					SaveMode.Compressed => Decompress(bytes),
					SaveMode.Encrypted => Decrypt(bytes, encryptionKey),
					SaveMode.EncryptedCompressed => Decompress(Decrypt(bytes, encryptionKey)),
					_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
				};

				string envelopeJson = Encoding.UTF8.GetString(raw);
				var envelope = JsonUtility.FromJson<SaveEnvelope>(envelopeJson);

				T data = JsonUtility.FromJson<T>(envelope.payload);
				return new SaveResult<T>(true, data, envelope.version);
			}
			catch (Exception e)
			{
				Debug.LogError($"[SaveSerializer] Failed to deserialize {typeof(T).Name}: {e.Message}");
				return SaveResult<T>.Fail();
			}
		}

		private static byte[] Compress(byte[] data)
		{
			using var output = new MemoryStream();
			using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
			{
				gzip.Write(data, 0, data.Length);
			}
			return output.ToArray();
		}

		private static byte[] Decompress(byte[] data)
		{
			using var input = new MemoryStream(data);
			using var gzip = new GZipStream(input, CompressionMode.Decompress);
			using var output = new MemoryStream();
			gzip.CopyTo(output);
			return output.ToArray();
		}

		private static byte[] Encrypt(byte[] data, string key)
		{
			byte[] keyBytes = DeriveKey(key);

			using var aes = Aes.Create();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.Key = keyBytes;
			aes.GenerateIV();

			using var encryptor = aes.CreateEncryptor();
			byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

			// Prepend IV to ciphertext so decryption can extract it
			byte[] result = new byte[IvSizeBytes + encrypted.Length];
			Buffer.BlockCopy(aes.IV, 0, result, 0, IvSizeBytes);
			Buffer.BlockCopy(encrypted, 0, result, IvSizeBytes, encrypted.Length);

			return result;
		}

		private static byte[] Decrypt(byte[] data, string key)
		{
			byte[] keyBytes = DeriveKey(key);

			byte[] iv = new byte[IvSizeBytes];
			Buffer.BlockCopy(data, 0, iv, 0, IvSizeBytes);

			byte[] ciphertext = new byte[data.Length - IvSizeBytes];
			Buffer.BlockCopy(data, IvSizeBytes, ciphertext, 0, ciphertext.Length);

			using var aes = Aes.Create();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.Key = keyBytes;
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor();
			return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
		}

		private static byte[] DeriveKey(string passphrase)
		{
			byte[] salt = Encoding.UTF8.GetBytes("Mudit.SaveService.Salt");
			using var kdf = new Rfc2898DeriveBytes(passphrase, salt, 10000, HashAlgorithmName.SHA256);
			return kdf.GetBytes(KeySizeBytes);
		}
	}
}
