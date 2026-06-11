using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using Mudit.Core.LifetimeScopes;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.Interfaces.Services;

namespace Mudit.Core.Boot
{
	public sealed class SingleEntryPoint : IStartable
	{
		readonly ISceneLoaderService sceneLoaderService;
		readonly LifetimeScope rootLifetimeScope;
		readonly ScopeRegistryData scopeRegistryData;

		[Inject]
		public SingleEntryPoint(
				ISceneLoaderService sceneLoaderService,
				LifetimeScope rootLifetimeScope,
				ScopeRegistryData scopeRegistryData
			)
		{
			this.scopeRegistryData = scopeRegistryData;
			this.rootLifetimeScope = rootLifetimeScope;
			this.sceneLoaderService = sceneLoaderService;
		}

		void IStartable.Start()
		{
			RunBootSequence().Forget();
		}

		private async UniTaskVoid RunBootSequence()
		{
			LifetimeScope bootLifetimeScope = rootLifetimeScope.CreateChildFromPrefab(scopeRegistryData.Get<BootLifetimeScope>());

			Debug.Log("BootLifetimeScope created");

			// Resolve AppBootstrapper from the CHILD scope (not injected into this class anymore)
			var bootstrapper = bootLifetimeScope.Container.Resolve<AppBootstrapper>();

			await bootstrapper.BootAsync();

			bootLifetimeScope.Dispose();
			Debug.Log("BootLifetimeScope disposed");

			string targetScene = scopeRegistryData.AfterBootSceneName;

	#if UNITY_EDITOR
			if (!string.IsNullOrEmpty(EditorBootStrapper.TargetScene))
			{
				targetScene = EditorBootStrapper.TargetScene;
				Debug.Log($"[SingleEntryPoint] Redirecting to original target: {targetScene}");
			}
	#endif

			await sceneLoaderService.LoadSceneAsync(targetScene);
		}
	}
}
