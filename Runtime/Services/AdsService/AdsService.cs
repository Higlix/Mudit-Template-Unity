using Cysharp.Threading.Tasks;
using Unity.Services.LevelPlay;
using UnityEngine;
using System;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.Services.AdsService.AdControllers;

namespace Mudit.Core.Services.AdsService
{
	public class AdsService : IAdsService, IDisposable
	{
		private BannerController bannerController;
		private InterstitialController interstitialController;
		private RewardedController rewardedController;
		
		private UniTaskCompletionSource<bool> initCompletionSource;

		public async UniTask InitializeAsync(ServiceData settings)
		{
			Debug.Log("Initializing Ads...");

			initCompletionSource = new UniTaskCompletionSource<bool>();

			LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
			LevelPlay.OnInitFailed += SdkInitializationFailedEvent;


	#if UNITY_ANDROID
			string appKey = settings.IronSourceConfig.GetAppKeyAndroid;
	#elif UNITY_IOS
			string appKey = settings.IronSourceConfig.GetAppKeyIOS;
	#else
			Debug.Log("Initializing Ads with Unknown App Key");
			string appKey = settings.IronSourceConfig.GetAppKeyUnknown;
	#endif

			LevelPlay.Init(appKey);

			bool success = await initCompletionSource.Task;

			if (success)
			{
				Debug.Log("Ads Initialized Successfully.");
				EnableAds(settings);
			}
			else
			{
				Debug.Log("Ads Initialization Failed.");
			}
		}

		void EnableAds(ServiceData settings)
		{
			LevelPlay.OnImpressionDataReady += ImpressionDataReadyEvent;

	#if UNITY_ANDROID

			rewardedController = new RewardedController(settings.IronSourceConfig.GetRewardedAdUnitIdAndroid);
			rewardedController.Load();

			bannerController = new BannerController(settings.IronSourceConfig.GetBannerAdUnitIdAndroid);
			bannerController.Load();

			interstitialController = new InterstitialController(settings.IronSourceConfig.GetInterstitialAdUnitIdAndroid);
			interstitialController.Load();

	#elif UNITY_IOS

			rewardedController = new RewardedController(settings.IronSourceConfig.GetRewardedAdUnitIdIOS);
			rewardedController.Load();

			bannerController = new BannerController(settings.IronSourceConfig.GetBannerAdUnitIdIOS);
			bannerController.Load();

			interstitialController = new InterstitialController(settings.IronSourceConfig.GetInterstitialAdUnitIdIOS);
			interstitialController.Load();

	#endif
		}

		public void LoadInterstitial()
		{
			interstitialController?.Load();
		}

		public void ShowInterstitial()
		{
			interstitialController?.Show();
		}

		public void LoadRewarded()
		{
			rewardedController?.Load();
		}

		public void ShowRewarded(Action onReward)
		{
			rewardedController?.Show(onReward);
		}

		public void ShowBanner()
		{
			bannerController?.Load();
		}

		public void BannerToggleVisibility()
		{
			bannerController?.ToggleVisibility(); 
		}

		void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
		{
			Debug.Log($"[AdsService] Received SdkInitializationCompletedEvent with Config: {config}");
			CleanupInitEvents();
			initCompletionSource?.TrySetResult(true);
		}

		void SdkInitializationFailedEvent(LevelPlayInitError error)
		{
			Debug.Log($"[AdsService] Received SdkInitializationFailedEvent with Error: {error}");
			CleanupInitEvents();
			initCompletionSource?.TrySetResult(false);
		}

		void CleanupInitEvents()
		{
			LevelPlay.OnInitSuccess -= SdkInitializationCompletedEvent;
			LevelPlay.OnInitFailed -= SdkInitializationFailedEvent;
		}

		void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
		{
			Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent ToString(): {impressionData}");
			Debug.Log($"[LevelPlaySample] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
		}

		public void Dispose()
		{
			CleanupInitEvents();
			initCompletionSource = null;
			bannerController?.Dispose();
			interstitialController?.Dispose();
			rewardedController?.Dispose();
		}
	}
}