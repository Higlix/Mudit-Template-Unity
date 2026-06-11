using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;

namespace Mudit.Core.Services.PaymentServices
{
	public class AndroidPaymentService : IPaymentService
	{
		public async UniTask InitializeAsync(ServiceData settings)
		{
			Debug.Log("Initializing Android Payments...");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
			Debug.Log("Android Payments Initialized.");
		}
	}

	public class IOSPaymentService : IPaymentService
	{
		public async UniTask InitializeAsync(ServiceData settings)
		{
			Debug.Log("Initializing iOS Payments...");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
			Debug.Log("iOS Payments Initialized.");
		}
	}

	public class MockPaymentService : IPaymentService
	{
		public async UniTask InitializeAsync(ServiceData settings)
		{
			Debug.Log("Initializing Mock Payments...");
			await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
			Debug.Log("Mock Payments Initialized.");
		}
	}
}