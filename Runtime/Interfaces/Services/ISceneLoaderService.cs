using Cysharp.Threading.Tasks;

namespace Mudit.Core.Interfaces.Services
{
	public interface ISceneLoaderService : IRootService
	{
		UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true);
	}
}
