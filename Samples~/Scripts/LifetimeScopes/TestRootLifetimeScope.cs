using VContainer;
using System;
using UnityEngine;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.LifetimeScopes;
using Mudit.Core.ScriptableObjects.Data;
using Cysharp.Threading.Tasks;

namespace Mudit.Samples.LifetimeScopes
{
	public interface ITestService : IRootService
	{
		void Test();
	}

	public class TestService : ITestService, IDisposable
	{
		public async UniTask InitializeAsync(ServiceData settings)
		{
			await UniTask.Delay(1000);
			Test();
		}

		public void Test()
		{
			Debug.Log("Test");
		}

		public void Dispose()
		{
			Debug.Log("TestService disposed");
		}
	}

	public class TestRootLifetimeScope : RootLifetimeScope
	{
		protected override void ConfigureLocalRootLifetimeScope(IContainerBuilder builder)
		{
			builder.Register<TestService>(Lifetime.Singleton).As<ITestService>().As<IRootService>();
		}
	}
}