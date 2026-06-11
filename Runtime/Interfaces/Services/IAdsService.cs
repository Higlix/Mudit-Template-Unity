using System;

namespace Mudit.Core.Interfaces.Services
{
	public interface IAdsService : IRootService
	{
		void ShowInterstitial();

		void ShowBanner();
		void BannerToggleVisibility();

		void LoadRewarded();
		void ShowRewarded(Action onReward);
	}
}