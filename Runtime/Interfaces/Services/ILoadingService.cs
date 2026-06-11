using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Mudit.Core.Interfaces.Services
{
	public interface ILoadingService : IRootService
	{
		void Show();
		void Hide();
		UniTask ShowAsync(); // Optional, if you want fade-in animations
		UniTask HideAsync(); // Optional, if you want fade-out animations
		Image GetLoadingScreenProgressImage();
	}
}