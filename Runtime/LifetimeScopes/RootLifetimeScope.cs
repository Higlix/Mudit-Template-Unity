using VContainer;
using VContainer.Unity;
using UnityEngine;
using MessagePipe;
using System;
using Mudit.Core.Boot;
using Mudit.Core.ScriptableObjects.Data;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.Services.AdsService;
using Mudit.Core.Services.AnalyticsServices;
using Mudit.Core.Services.AudioService;
using Mudit.Core.Services.LoadingService;
using Mudit.Core.Services.SceneLoaderService;
using Mudit.Core.Services.UIService;
using Mudit.Core.Services.PaymentServices;
using Mudit.Core.Services.SaveService;
using Mudit.Core.Services.SaveSlotManager;

namespace Mudit.Core.LifetimeScopes
{
	public interface IRootGameObject
	{
		public GameObject GetGameObject();
		public Transform GetTransform();
	}

	[Serializable]
	public struct RootGameObject : IRootGameObject
	{
		[SerializeField]
		GameObject root;
		[SerializeField]
		Transform transform;

		public GameObject GetGameObject()
		{
			return root;
		}

		public Transform GetTransform()
		{
			return transform;
		}
	}

	public class RootLifetimeScope : LifetimeScope
	{
		[SerializeField]
		bool isAnalyticsEnabled = true;
		[SerializeField]
		bool isAdsEnabled = true;
		[SerializeField]
		bool isPaymentsEnabled = true;
		[SerializeField]
		bool isAudioEnabled = true;
		[SerializeField]
		bool isUIEnabled = true;
		[SerializeField]
		bool isSaveEnabled = true;

		[SerializeField]
		ScopeRegistryData scopeRegistryData;

		[SerializeField]
		RootGameObject rootGameObject;

		[SerializeField]
		GameObject loadingCanvas;

		protected override void Configure(IContainerBuilder builder)
		{
			var options = builder.RegisterMessagePipe();
			builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));

			builder.RegisterInstance(scopeRegistryData);
			builder.RegisterInstance(rootGameObject);

			builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoaderService>().As<IRootService>();

			if (isSaveEnabled)
			{
				builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>().As<IRootService>();
				builder.Register<SaveSlotManager>(Lifetime.Singleton).As<ISaveSlotManager>().As<IRootService>();
			}

			if (isPaymentsEnabled)
			{
			#if UNITY_ANDROID && !UNITY_EDITOR
				builder.Register<AndroidPaymentService>(Lifetime.Singleton).As<IPaymentService>().As<IRootService>();
			#elif UNITY_IOS && !UNITY_EDITOR
				builder.Register<IOSPaymentService>(Lifetime.Singleton).As<IPaymentService>().As<IRootService>();
			#else
				builder.Register<MockPaymentService>(Lifetime.Singleton).As<IPaymentService>().As<IRootService>();
			#endif
			}

			if (isAnalyticsEnabled)
			{
				builder.Register<FirebaseAnalyticsService>(Lifetime.Singleton).As<IAnalyticsService>().As<IRootService>();
			}

			if (isAdsEnabled)
			{
				builder.Register<AdsService>(Lifetime.Singleton).As<IAdsService>().As<IRootService>();
			}

			if (isAudioEnabled)
			{
				builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>().As<IRootService>();
			}

			if (isUIEnabled)
			{
				builder.Register<UIService>(Lifetime.Singleton).As<IUIService>().As<IRootService>();
			}

			builder.RegisterEntryPoint<SingleEntryPoint>();


			// Register the LoadingService, injecting the RootCanvas into it
			builder.Register<LoadingService>(Lifetime.Singleton).As<ILoadingService>().As<IRootService>()
				.WithParameter(loadingCanvas);
			
	#if DEVELOPMENT_BUILD || UNITY_EDITOR
		// Register the DebugConsoleService
	#endif
			ConfigureLocalRootLifetimeScope(builder);
		}

		public RootGameObject GetRootGameObject() => rootGameObject;
		
		protected virtual void ConfigureLocalRootLifetimeScope(IContainerBuilder builder)
		{

		}
	}
}