using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace Mudit.Core.StaticHelpers
{
	public static class RetryLoad
	{
		private const int MAX_RETRY_ATTEMPTS = 8;
		private const float MAX_RETRY_DELAY_SEC = 256f;

		public static async UniTaskVoid WithBackoff(
			Action loadAction,
			RefInt currentRetryCount,
			CancellationToken cancellationToken,
			string tag = "undefined",
			DelayType delayType = DelayType.Exponential)
		{
			// Increment the retry count
			currentRetryCount.Value++;

			if (currentRetryCount.Value > MAX_RETRY_ATTEMPTS)
			{
				Debug.LogWarning($"[{tag}] Max retry attempts reached ({MAX_RETRY_ATTEMPTS}). Stopping retries.");
				return;
			}

			double delay = 0f;

			if (delayType == DelayType.Linear)
			{
				// 1, 2, 3, 4, 5, 6, 7, 8
				delay = currentRetryCount.Value;
			}
			else if (delayType == DelayType.Exponential)
			{
				// 2, 4, 8, 16, 32, 64, 128, 256
				delay = Mathf.Pow(2, currentRetryCount.Value);
			}
			else if (delayType == DelayType.Random)
			{
				// 2 - MAX_RETRY_DELAY_SEC
				delay = UnityEngine.Random.Range(2f, MAX_RETRY_DELAY_SEC);
			}

			Debug.Log($"[{tag}] Retrying load attempt #{currentRetryCount.Value} in {delay} seconds...");

			try
			{
				await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
				loadAction?.Invoke();
			}
			catch (OperationCanceledException)
			{
				// Service was disposed or cancelled, stop retrying
			}
		}

		// Simple wrapper class to pass int by reference to async methods
		public class RefInt
		{
			public int Value;
		}

		// Delay type for the retry load
		public enum DelayType
		{
			Linear,
			Exponential,
			Random
		}
	}
}