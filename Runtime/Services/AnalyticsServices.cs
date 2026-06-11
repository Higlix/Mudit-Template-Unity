using Cysharp.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;

namespace Mudit.Core.Services.AnalyticsServices
{
	public class FirebaseAnalyticsService : IAnalyticsService
	{
		public async UniTask InitializeAsync(ServiceData settings)
		{
			Debug.Log("Initializing Firebase Analytics...");
			DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
			if (dependencyStatus == DependencyStatus.Available)
			{
				FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
				Debug.Log("Firebase Analytics Collection Enabled.");
			}
			else
			{
				Debug.LogError("Firebase Analytics Initialization Failed.");
			}
		}

		public void LogAddNewtork()
		{
			FirebaseAnalytics.LogEvent("add_network");
		}
	}
}