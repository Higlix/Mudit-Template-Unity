namespace Mudit.Core.Interfaces.Services
{
	public readonly struct SaveResult<T>
	{
		public readonly bool Success;
		public readonly T Data;
		public readonly int Version;

		public SaveResult(bool success, T data, int version)
		{
			Success = success;
			Data = data;
			Version = version;
		}

		public static SaveResult<T> Fail()
		{
			return new SaveResult<T>(false, default, -1);
		}
	}
}
