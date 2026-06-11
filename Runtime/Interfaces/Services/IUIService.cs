using System;
using Cysharp.Threading.Tasks;
using Mudit.Core.Services.UIService;
using VContainer;

namespace Mudit.Core.Interfaces.Services
{
	public interface IUIService : IRootService
	{
		UniTask<T> Show<T>(object args = null) where T : UIView;
		UniTask<T> GetView<T>() where T : UIView;
		UniTask Hide<T>() where T : UIView;
		void Back();

		UniTask SetUIViewPrefabsAsync(string sceneName);

		void BindSceneContainer(IObjectResolver sceneContainer);

		IObservable<UIView> CurrentView { get; }
	}
}
