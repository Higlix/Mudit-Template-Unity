using Cysharp.Threading.Tasks;
using Mudit.Core.Enums;

namespace Mudit.Core.Interfaces.Services
{
	public interface ISaveService : IRootService
	{
		UniTask SaveAsync<T>(string key, T data, SaveMode mode = SaveMode.Encrypted);
		UniTask<SaveResult<T>> LoadAsync<T>(string key, SaveMode mode = SaveMode.Encrypted);

		UniTask SaveAsync<T>(T data, SaveMode mode = SaveMode.Encrypted) where T : ISaveableData;
		UniTask<SaveResult<T>> LoadAsync<T>(SaveMode mode = SaveMode.Encrypted) where T : ISaveableData, new();

		void SetBasePath(string path);
		string GetBasePath();

		UniTask<bool> ExistsAsync(string key);
		UniTask DeleteAsync(string key);
		UniTask DeleteAllAsync();
	}
}
