namespace Mudit.Core.Enums
{
	public enum SaveMode
	{
		Encrypted,      // AES encryption
		Plain,          // JSON only
		Compressed,     // GZip compression
		EncryptedCompressed // Both encryption and compression
	}
}
