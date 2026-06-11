using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.Services.UIService;
using Mudit.Samples.LifetimeScopes;

namespace Mudit.Core.UI.Views
{
	public class MainMenuView : UIView
	{
		[SerializeField] private Button goToTestAdsButton;
		[SerializeField] private Button goToTestSoundsButton;
		[SerializeField] private Button switchScenesButton;

		private IUIService uiService;
		private ISceneLoaderService sceneLoaderService;

		private TestSpeaker testSpeaker;

		[Inject]
		public void Construct(IUIService uiService, ISceneLoaderService sceneLoaderService) // TestSpeaker testSpeaker)
		{
			this.uiService = uiService;
			this.sceneLoaderService = sceneLoaderService;
			// this.testSpeaker = testSpeaker;
		}

		public override async UniTask Initialize()
		{
			await base.Initialize();

			Debug.Log("This is a test to see if git works");

			goToTestAdsButton.onClick.AddListener(() =>
			{
				// testSpeaker.SayYourName();
			});

			switchScenesButton.onClick.AddListener(() => {
				sceneLoaderService.LoadSceneAsync("GamePlay").Forget();
			});

			goToTestSoundsButton.onClick.AddListener(() => {
			});
		}
	}
}