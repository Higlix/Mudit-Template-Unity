namespace Mudit.Core.Interfaces.Services
{
	public interface ISaveableData
	{
		string SaveKey { get; }
		int SaveVersion { get; }
	}
}
