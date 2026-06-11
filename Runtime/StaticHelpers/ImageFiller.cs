using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Mudit.Core.StaticHelpers
{
	public static class ProgressBarFiller
	{
		public static async UniTask FillProgressBarAsync(Image progressImage, AsyncOperation asyncOperation)
		{
			if (progressImage != null && asyncOperation != null)
			{
				progressImage.fillAmount = 0f;
				while (!asyncOperation.isDone)
				{
					progressImage.fillAmount = Mathf.Clamp01(asyncOperation.progress + 0.15f);
					await UniTask.Yield();
				}
			}
			else
			{
				Debug.LogError("ProgressBarFiller: progressImage or asyncOperation is null");
			}
		}
	}
}