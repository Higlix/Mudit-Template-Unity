using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.ScriptableObjects.Database;
using Mudit.Core.LifetimeScopes;
using Mudit.Core.Serializables;
using Mudit.Core.Enums;

namespace Mudit.Core.Services.UIService
{
	public class UIService : IUIService, IDisposable
	{
		private readonly IObjectResolver rootContainer;
		private IObjectResolver activeContainer;
		private readonly CompositeDisposable disposables = new CompositeDisposable();
		
		private GameObject uiRoot;
		private Transform layersRoot;

		private RootGameObject rootGameObject;
		
		private SceneUIDatabase sceneUIDatabase;
		private ViewRegistryData viewRegistryData;
		private Dictionary<Type, UIView> instantiatedViews = new Dictionary<Type, UIView>();
		private Stack<UIView> viewStack = new Stack<UIView>();
		
		private ReactiveProperty<UIView> currentView = new ReactiveProperty<UIView>();
		
		public IObservable<UIView> CurrentView => currentView;

		[Inject]
		public UIService(IObjectResolver container, RootGameObject rootGameObject)
		{
			this.rootContainer = container;
			this.activeContainer = container;
			this.rootGameObject = rootGameObject;
		}

		public void BindSceneContainer(IObjectResolver sceneContainer)
		{
			activeContainer = sceneContainer ?? rootContainer;
		}

		public async UniTask InitializeAsync(ServiceData settings)
		{
			sceneUIDatabase = settings.SceneUIDatabase;
			ResetPrefabs();
			// Create UI Root if not exists
			// In a real project, this might be injected or found in scene
			if (uiRoot == null)
			{
				uiRoot = new GameObject("UIService");
				Object.DontDestroyOnLoad(uiRoot);
				uiRoot.transform.SetParent(rootGameObject.GetTransform(), false);
				uiRoot.transform.SetAsFirstSibling();
				uiRoot.transform.localPosition = Vector3.zero;
				
				// Create a Canvas
				var canvas = uiRoot.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.vertexColorAlwaysGammaSpace = true;

				var rectTransform = uiRoot.GetComponent<RectTransform>();
				rectTransform.anchorMin = Vector2.zero; // (0, 0)
				rectTransform.anchorMax = Vector2.one;  // (1, 1)

				// Reset offsets to zero so it snaps to the anchors
				rectTransform.offsetMin = Vector2.zero; // Left and Bottom offsets
				rectTransform.offsetMax = Vector2.zero; // Right and Top offsets


				// rectTransform.anchorMin = Vector2.zero;
				// rectTransform.anchorMax = Vector2.one;
				// rectTransform.pivot = new Vector2(0.5f, 0.5f);
				// rectTransform.sizeDelta = Vector2.zero;
				// rectTransform.anchoredPosition = Vector2.zero;
				// rectTransform.localScale = Vector3.one;
				// rectTransform.localRotation = Quaternion.identity;
				// rectTransform.localPosition = Vector3.zero;
				// rectTransform.localEulerAngles = Vector3.zero;
				// rectTransform.localScale = Vector3.one;
				// rectTransform.localRotation = Quaternion.identity;
				// rectTransform.localScale = Vector3.one;
				// rectTransform.localRotation = Quaternion.identity;
				// rectTransform.localScale = Vector3.one;

				uiRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();
				
				var scaler = uiRoot.AddComponent<UnityEngine.UI.CanvasScaler>();
				scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
				scaler.referenceResolution = new Vector2(1920, 1080); // Or your target resolution
				scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
				scaler.matchWidthOrHeight = 0.5f; // Average
	
				layersRoot = uiRoot.transform;
			}

			Debug.Log("UIService Initialized.");
			await UniTask.CompletedTask;
		}

		private void ResetPrefabs()
		{
			// Destroy existing instances to clean up the scene
			foreach (var view in instantiatedViews.Values)
			{
				if (view != null && view.gameObject != null)
				{
					Object.Destroy(view.gameObject);
				}
			}

			instantiatedViews.Clear();
			viewStack.Clear();
			currentView.Value = null;
		}

		public async UniTask SetUIViewPrefabsAsync(string sceneName)
		{
			ResetPrefabs();
			
			var sceneSettings = sceneUIDatabase.GetSceneSettings(sceneName);
			if (sceneSettings == null) return;

			viewRegistryData = sceneSettings.GetViewRegistryData();
			
			if (viewRegistryData != null)
			{
				viewRegistryData.Initialize();

				bool isFirst = true;
				foreach (var prefab in viewRegistryData.AllPrefabs)
				{
					if (prefab == null) continue;

					// Always instantiate the first one (Landing View)
					if (isFirst)
					{
						var instance = await InstantiateView(prefab);
						await ShowInstance(instance);
						isFirst = false;
						
						// If Lazy, stop after the first one
						if (sceneSettings.InstantiationMode == InstantiationMode.Lazy)
						{
							break; 
						}
					}
					else
					{
						// For subsequent views, only instantiate if Eager
						if (sceneSettings.InstantiationMode == InstantiationMode.Eager)
						{
							await InstantiateView(prefab);
							await UniTask.Yield(); // Spread instantiation over frames
						}
					}
				}
			}
		}

		private async UniTask<UIView> InstantiateView(UIView prefab)
		{
			var type = prefab.GetType();
			if (instantiatedViews.ContainsKey(type)) return instantiatedViews[type];

			var instance = activeContainer.Instantiate(prefab);
			instance.transform.SetParent(layersRoot, false);
			await instance.Initialize();
			instance.gameObject.SetActive(false);
			instantiatedViews[type] = instance;
			return instance;
		}

		private async UniTask ShowInstance(UIView view, object args = null)
		{
			if (view == null) return;

			// Handle Stack
			if (viewStack.Count > 0)
			{
				var top = viewStack.Peek();
				if (top == view) 
				{
					Debug.LogWarning("UIService: View is already on top.");
					return;
				}

				if (!view.IsOverlay)
				{
					await top.OnHide();
				}
			}

			viewStack.Push(view);
			currentView.Value = view;
			
			view.transform.SetAsLastSibling(); // Bring to front
			await view.OnShow(args);
		}

		public async UniTask<T> Show<T>(object args = null) where T : UIView
		{
			var view = await GetView<T>();
			if (view == null)
			{
				Debug.LogError($"UIService: Could not find view prefab for type {typeof(T)}");
				return null;
			}
			
			await ShowInstance(view, args);
			return view;
		}

		public async UniTask Hide<T>() where T : UIView
		{
			if (viewStack.Count == 0) return;

			var top = viewStack.Peek();
			if (top is T)
			{
				Back();
			}
			else
			{
				Debug.LogWarning("UIService: Cannot Hide view that is not on top of the stack directly. Use Back().");
			}
			await UniTask.CompletedTask;
		}

		public void Back()
		{
			if (viewStack.Count <= 0) return;

			BackAsync().Forget();
		}

		private async UniTaskVoid BackAsync()
		{
			var current = viewStack.Pop();
			bool isOverlay = current.IsOverlay;
			await current.OnHide();

			if (viewStack.Count > 0)
			{
				var previous = viewStack.Peek();
				currentView.Value = previous;
				if (isOverlay == false)
				{
					await previous.OnShow(null); // Re-show previous
				}
			}
			else
			{
				currentView.Value = null;
			}
		}

		public async UniTask<T> GetView<T>() where T : UIView
		{
			if (instantiatedViews.TryGetValue(typeof(T), out var view))
			{
				return view as T;
			}

			var prefab = viewRegistryData?.GetPrefab<T>();
			if (prefab == null) return null;

			return (await InstantiateView(prefab)) as T;
		}

		public void Dispose()
		{
			disposables.Dispose();
			if (uiRoot != null)
			{
				Object.Destroy(uiRoot);
			}
		}
	}
}