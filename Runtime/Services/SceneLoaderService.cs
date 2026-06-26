using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using Mudit.Core.Interfaces.Services;
using VContainer;
using VContainer.Unity;
using Mudit.Core.ScriptableObjects.Data;
using UnityEngine.UI;
using Mudit.Core.StaticHelpers;

namespace Mudit.Core.Services.SceneLoaderService
{
	public class SceneLoaderService : ISceneLoaderService
	{
		private readonly ILoadingService loadingService;
		private readonly IUIService uiService;

		[Inject]
		public SceneLoaderService(ILoadingService loadingService, IUIService uiService)
		{
			this.loadingService = loadingService;
			this.uiService = uiService;
		}

		public async UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
		{
			// TODO: Implement Loading Screen logic here
			if (showLoadingScreen)
			{
				await loadingService.ShowAsync();
			}

			Image progressImage = loadingService.GetLoadingScreenProgressImage();
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
			await ProgressBarFiller.FillProgressBarAsync(progressImage, asyncOperation);

			while (!asyncOperation.isDone)
			{
				await UniTask.Yield();
			}
			await UniTask.Yield();

			var sceneScope = FindSceneLifetimeScope(sceneName);
			uiService.BindSceneContainer(sceneScope?.Container);

			await uiService.SetUIViewPrefabsAsync(sceneName);
			Debug.Log($"[SceneLoader] Loaded '{sceneName}'.");
			await loadingService.HideAsync();
		}

		public async UniTask InitializeAsync(ServiceData settings)
		{
			await UniTask.CompletedTask;
		}

		private static LifetimeScope FindSceneLifetimeScope(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName)) return null;

			var allScopes = Object.FindObjectsByType<LifetimeScope>(
				FindObjectsInactive.Include);

			foreach (var scope in allScopes)
			{
				Debug.Log("Scope: " + scope.gameObject.name);
				if (scope.gameObject.name.Contains(sceneName)) return scope;
			}
			return null;
		}
	}
}