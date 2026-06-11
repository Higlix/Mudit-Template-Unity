using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.Services.UIService;

namespace Mudit.Core.UI.Views
{
	public class GamePlayView : UIView
	{
		[SerializeField] private Button backToMainMenuButton;
		
		private IUIService uiService;
		private ISceneLoaderService sceneLoaderService;

		[Inject]
		public void Construct(IUIService uiService, ISceneLoaderService sceneLoaderService)
		{
			this.uiService = uiService;
			this.sceneLoaderService = sceneLoaderService;
		}

		public override async UniTask Initialize()
		{
			await base.Initialize();
			Debug.Log("GamePlayView Initialized");
			backToMainMenuButton.onClick.AddListener(() => 
			{
				Debug.Log("Back to Main Menu");
				sceneLoaderService.LoadSceneAsync("MainMenu").Forget();
			});
		}
	}
}