using System;
using Unity.Services.LevelPlay;
using UnityEngine;
using System.Threading;
using Mudit.Core.StaticHelpers;

namespace Mudit.Core.Services.AdsService.AdControllers
{
	public class RewardedController
	{
		private LevelPlayRewardedAd rewardedAd;
		private Action onRewardCallback;
		private bool isRewarded = false;

		private RetryLoad.RefInt retryAttempts = new RetryLoad.RefInt();
		private CancellationTokenSource disposeToken = new CancellationTokenSource();


		public RewardedController(string RewardedID)
		{
			LevelPlayRewardedAd.Config config = new LevelPlayRewardedAd.Config.Builder()
				.Build();

			retryAttempts.Value = 0;
			rewardedAd = new LevelPlayRewardedAd(RewardedID, config);

			rewardedAd.OnAdLoaded += RewardedVideoOnLoadedEvent;
			rewardedAd.OnAdLoadFailed += RewardedVideoOnAdLoadFailedEvent;
			rewardedAd.OnAdDisplayed += RewardedVideoOnAdDisplayedEvent;
			rewardedAd.OnAdDisplayFailed += RewardedVideoOnAdDisplayedFailedEvent;
			rewardedAd.OnAdRewarded += RewardedVideoOnAdRewardedEvent;
			rewardedAd.OnAdClicked += RewardedVideoOnAdClickedEvent;
			rewardedAd.OnAdClosed += RewardedVideoOnAdClosedEvent;
			rewardedAd.OnAdInfoChanged += RewardedVideoOnAdInfoChangedEvent;

			isRewarded = false; 
		}

		public void Load()
		{
			if (disposeToken.IsCancellationRequested)
			{
				return;
			}
			rewardedAd.LoadAd();
		}

		public void Show(Action onReward)
		{
			if (rewardedAd.IsAdReady())
			{
				isRewarded = false;
				onRewardCallback = onReward;
				rewardedAd.ShowAd();
			}
			else
			{
				Debug.LogWarning("Rewarded Ad is not ready.");
				disposeToken.Cancel();
				Load();
			}
		}

		public void Dispose()
		{
			disposeToken.Cancel(); // Stops any pending retry tasks
			disposeToken.Dispose();

			rewardedAd.OnAdLoaded -= RewardedVideoOnLoadedEvent;
			rewardedAd.OnAdLoadFailed -= RewardedVideoOnAdLoadFailedEvent;
			rewardedAd.OnAdDisplayed -= RewardedVideoOnAdDisplayedEvent;
			rewardedAd.OnAdDisplayFailed -= RewardedVideoOnAdDisplayedFailedEvent;
			rewardedAd.OnAdRewarded -= RewardedVideoOnAdRewardedEvent;
			rewardedAd.OnAdClicked -= RewardedVideoOnAdClickedEvent;
			rewardedAd.OnAdClosed -= RewardedVideoOnAdClosedEvent;
			rewardedAd.OnAdInfoChanged -= RewardedVideoOnAdInfoChangedEvent;

			rewardedAd.Dispose();
		}

		public bool IsReady()
		{
			return rewardedAd.IsAdReady();
		}

		// Event Handlers

		void RewardedVideoOnLoadedEvent(LevelPlayAdInfo adInfo)
		{
			retryAttempts.Value = 0;
			Debug.Log($"Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
		}

		void RewardedVideoOnAdLoadFailedEvent(LevelPlayAdError error)
		{
			Debug.Log($"Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");

			// Retry load with backoff
			RetryLoad.WithBackoff(
				Load,
				retryAttempts,
				disposeToken.Token,
				"Rewarded",
				RetryLoad.DelayType.Exponential
			).Forget();
		}

		void RewardedVideoOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
		}

		void RewardedVideoOnAdDisplayedFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
		{
			Debug.Log($"Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
			// Clear callback on failure to prevent stale callbacks? 
			// Or keep it if retry is possible? Usually clear on failure to show.
			onRewardCallback = null;
			Load();
		}

		void RewardedVideoOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
		{
			Debug.Log($"Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
			isRewarded = true;
		}

		void RewardedVideoOnAdClickedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
		}

		void RewardedVideoOnAdClosedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
			if (isRewarded)
			{
				onRewardCallback?.Invoke();
			}
			else
			{
				Debug.LogWarning("Rewarded Ad was not rewarded.");
			}
			onRewardCallback = null;
			isRewarded = false;
			Load();
		}

		void RewardedVideoOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
		{
			Debug.Log($"Received RewardedVideoOnAdInfoChangedEvent With AdInfo: {adInfo}");
		}
	}
}