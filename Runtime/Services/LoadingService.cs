using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using UnityEngine.UI;

namespace Mudit.Core.Services.LoadingService
{
	public class LoadingService : ILoadingService
	{
		private readonly GameObject loadingCanvas;
		private Image progressImage;

		// Inject the specific Loading Canvas GameObject here
		[Inject]
		public LoadingService(GameObject loadingCanvas)
		{
			progressImage = null;
			this.loadingCanvas = loadingCanvas;
		}

		public void Show() => loadingCanvas.SetActive(true);
		public void Hide() => loadingCanvas.SetActive(false);

		public async UniTask ShowAsync() 
		{
			loadingCanvas.SetActive(true);
			// Add fade-in logic here if you have a CanvasGroup
			await UniTask.CompletedTask;
		}

		public async UniTask HideAsync()
		{
			// Add fade-out logic here
			await UniTask.CompletedTask; 
			loadingCanvas.SetActive(false);
		}

		public Image GetLoadingScreenProgressImage()
		{
			return progressImage;
		}

		public async UniTask InitializeAsync(ServiceData settings)
		{
			Image[] images = loadingCanvas.GetComponentsInChildren<Image>();
			Debug.Log("Found " + images.Length + " images");
			foreach (Image image in images)
			{
				Debug.Log("Image name: " + image.gameObject.name);
				if (image.gameObject.name == "ProgressBarTop")
				{
					progressImage = image;
					break ;
				}
			}
			await UniTask.CompletedTask;
		}
	}
}
