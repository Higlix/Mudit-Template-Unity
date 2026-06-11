using UnityEngine;
using Mudit.Core.ScriptableObjects.Database;
using Mudit.Core.Serializables.Configurations;

namespace Mudit.Core.ScriptableObjects.Data
{
	[CreateAssetMenu(fileName = "ServiceData", menuName = "Mudit/Data/ServiceData")]
	public class ServiceData : ScriptableObject
	{
		[Header("IronSource")]
		[SerializeField]
		private IronSourceConfig ironSourceConfig;
		public IronSourceConfig IronSourceConfig => ironSourceConfig;

		[Header("Save")]
		[SerializeField]
		private SaveConfig saveConfig;
		public SaveConfig SaveConfig => saveConfig;

		[Header("General")]
		[SerializeField]
		private string ID_IOS;
		public string GetIOSID => ID_IOS;

		[SerializeField]
		private string ID_Android;
		public string GetAndroidID => ID_Android;
		[SerializeField]
		private string Name;
		public string GameName => Name;


		[Header("Analytics")]
		[SerializeField]
		private string AnalyticsAPIKey;
		public string GetAnalyticsAPIKey => AnalyticsAPIKey;

		[Header("Ads")]
		[SerializeField]
		private string AdsAPIKey;
		public string GetAdsAPIKey => AdsAPIKey;

		[Header("Payments")]
		[SerializeField]
		private string PaymentsAPIKey;
		public string GetPaymentsAPIKey => PaymentsAPIKey;

		[Header("Audio")]
		[SerializeField]
		private AudioData audioData;
		public AudioData AudioData => audioData;

		[Header("UI")]
		[SerializeField]
		private SceneUIDatabase sceneUIDatabase;
		public SceneUIDatabase SceneUIDatabase => sceneUIDatabase;
	}
}