using UnityEngine;

namespace Mudit.Core.Serializables.Configurations
{
	[System.Serializable]
	public class IronSourceConfig
	{
		[Header("App Keys")]
		[SerializeField]
		private string appKeyAndroid = "85460dcd";
		[SerializeField]
		private string appKeyIOS = "8545d445";
		readonly private string appKeyUnknown = "unexpected_platform";

		public string GetAppKeyUnknown => appKeyUnknown;
		public string GetAppKeyAndroid => appKeyAndroid;
		public string GetAppKeyIOS => appKeyIOS;

		[Header("Ad Units")]
		[Header("Banner")]
		[SerializeField]
		private string bannerAdUnitIdAndroid = "thnfvcsog13bhn08";
		[SerializeField]
		private string bannerAdUnitIdIOS = "iep3rxsyp9na3rw8";
		readonly private string bannerAdUnitIdUnknown = "unexpected_platform";

		public string GetBannerAdUnitIdUnknown => bannerAdUnitIdUnknown;
		public string GetBannerAdUnitIdAndroid => bannerAdUnitIdAndroid;
		public string GetBannerAdUnitIdIOS => bannerAdUnitIdIOS;

		[Header("Interstitial")]
		[SerializeField]
		private string interstitialAdUnitIdAndroid = "aeyqi3vqlv6o8sh9";
		[SerializeField]
		private string interstitialAdUnitIdIOS = "wmgt0712uuux8ju4";
		readonly private string interstitialAdUnitIdUnknown = "unexpected_platform";
		public string GetInterstitialAdUnitIdUnknown => interstitialAdUnitIdUnknown;
		public string GetInterstitialAdUnitIdAndroid => interstitialAdUnitIdAndroid;
		public string GetInterstitialAdUnitIdIOS => interstitialAdUnitIdIOS;

		[Header("Rewarded")]
		[SerializeField]
		private string rewardedAdUnitIdAndroid = "76yy3nay3ceui2a3";
		[SerializeField]
		private string rewardedAdUnitIdIOS = "qwouvdrkuwivay5q";
		readonly private string rewardedAdUnitIdUnknown = "unexpected_platform";
		public string GetRewardedAdUnitIdUnknown => rewardedAdUnitIdUnknown;
		public string GetRewardedAdUnitIdAndroid => rewardedAdUnitIdAndroid;
		public string GetRewardedAdUnitIdIOS => rewardedAdUnitIdIOS;
	}
}