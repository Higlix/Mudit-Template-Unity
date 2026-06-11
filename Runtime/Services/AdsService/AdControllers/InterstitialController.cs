using Unity.Services.LevelPlay;
using UnityEngine;
using System.Threading;
using Mudit.Core.StaticHelpers;

namespace Mudit.Core.Services.AdsService.AdControllers
{
	class InterstitialController 
	{
		readonly private LevelPlayInterstitialAd interstitialAd;
		private RetryLoad.RefInt retryAttempts = new RetryLoad.RefInt();
		private CancellationTokenSource disposeToken = new CancellationTokenSource();

		public InterstitialController(string InterstitialID)
		{
			LevelPlayInterstitialAd.Config config = new LevelPlayInterstitialAd.Config.Builder()
				.Build();

			interstitialAd = new LevelPlayInterstitialAd(InterstitialID, config);

			interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
			interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
			interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
			interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
			interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
			interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
			interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

		}

		public void Load()
		{
			interstitialAd.LoadAd();
		}

		public void Show()
		{
			if (interstitialAd.IsAdReady())
			{
				interstitialAd.ShowAd();
			}
			else
			{
				Debug.LogWarning("Interstitial Ad is not ready.");
				disposeToken.Cancel();
				Load();
			}
		}

		void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
		{
			retryAttempts.Value = 0;
			Debug.Log($"Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
		}

		void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
		{
			Debug.Log($"Received InterstitialOnAdLoadFailedEvent With Error: {error}");
			RetryLoad.WithBackoff(Load, retryAttempts, disposeToken.Token, "Interstitial").Forget();
		}

		void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
		}

		void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
		{
			Debug.Log($"Received InterstitialOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
		}

		void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
		}

		void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received InterstitialOnAdClosedEvent With AdInfo: {adInfo}");
			Load();
		}

		void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
		}

		public void Dispose()
		{
			interstitialAd.OnAdLoaded -= InterstitialOnAdLoadedEvent;
			interstitialAd.OnAdLoadFailed -= InterstitialOnAdLoadFailedEvent;
			interstitialAd.OnAdDisplayed -= InterstitialOnAdDisplayedEvent;
			interstitialAd.OnAdDisplayFailed -= InterstitialOnAdDisplayFailedEvent;
			interstitialAd.OnAdClicked -= InterstitialOnAdClickedEvent;
			interstitialAd.OnAdClosed -= InterstitialOnAdClosedEvent;
			interstitialAd.OnAdInfoChanged -= InterstitialOnAdInfoChangedEvent;

			interstitialAd.Dispose();
		}
	}
}