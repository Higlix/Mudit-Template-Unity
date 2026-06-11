using UnityEngine;
using Unity.Services.LevelPlay;
using System.Threading;
using Mudit.Core.StaticHelpers;

namespace Mudit.Core.Services.AdsService.AdControllers
{
	class BannerController
	{
		readonly private LevelPlayBannerAd bannerAd;
		private RetryLoad.RefInt retryAttempts = new RetryLoad.RefInt();
		private CancellationTokenSource disposeToken = new CancellationTokenSource();

		private bool isLoaded = false;
		private bool isVisible = false;

		public BannerController(string BannerID)
		{
			LevelPlayBannerAd.Config config = new LevelPlayBannerAd.Config.Builder()
				.SetDisplayOnLoad(true)                    // Setup: Don't show automatically
				.SetSize(LevelPlayAdSize.BANNER)            // Optional: Default is BANNER
				.SetPosition(LevelPlayBannerPosition.BottomCenter) // Optional: Default is BottomCenter
				.Build();

			retryAttempts.Value = 0;
			bannerAd = new LevelPlayBannerAd(BannerID, config);

			bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
			bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
			bannerAd.OnAdClicked += BannerOnAdClickedEvent;
			bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
			bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
			bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
			bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
			bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

		}
		public void Load()
		{
			bannerAd?.LoadAd();
		}

		public void ToggleVisibility()
		{
			if (isLoaded && isVisible)
			{
				bannerAd?.HideAd();
				isVisible = false;
			}
			else if (isLoaded && !isVisible)
			{
				bannerAd?.ShowAd();
			}
		}

		void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
		{
			retryAttempts.Value = 0;
			Debug.Log($"Received BannerOnAdLoadedEvent With AdInfo: {adInfo}");
			isLoaded = true;
		}

		void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
		{
			Debug.Log($"Received BannerOnAdLoadFailedEvent With Error: {error}");
			RetryLoad.WithBackoff(Load, retryAttempts, disposeToken.Token, "Banner").Forget();
		}

		void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received BannerOnAdClickedEvent With AdInfo: {adInfo}");
		}

		void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received BannerOnAdDisplayedEvent With AdInfo: {adInfo}");
			isVisible = true;
		}

		void BannerOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
		{
			Debug.Log($"Received BannerOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
		}

		void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received BannerOnAdCollapsedEvent With AdInfo: {adInfo}");
		}

		void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received BannerOnAdLeftApplicationEvent With AdInfo: {adInfo}");
		}

		void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received BannerOnAdExpandedEvent With AdInfo: {adInfo}");
		}

		public void Dispose()
		{
			bannerAd.OnAdLoaded -= BannerOnAdLoadedEvent;
			bannerAd.OnAdLoadFailed -= BannerOnAdLoadFailedEvent;
			bannerAd.OnAdClicked -= BannerOnAdClickedEvent;
			bannerAd.OnAdDisplayed -= BannerOnAdDisplayedEvent;
			bannerAd.OnAdDisplayFailed -= BannerOnAdDisplayFailedEvent;
			bannerAd.OnAdCollapsed -= BannerOnAdCollapsedEvent;
			bannerAd.OnAdLeftApplication -= BannerOnAdLeftApplicationEvent;
			bannerAd.OnAdExpanded -= BannerOnAdExpandedEvent;
			bannerAd.Dispose();
		}
	}
}