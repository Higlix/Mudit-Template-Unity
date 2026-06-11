using VContainer;
using VContainer.Unity;
using UnityEngine;
using Mudit.Core.Boot;
using Mudit.Core.ScriptableObjects.Data;

namespace Mudit.Core.LifetimeScopes
{
	public class BootLifetimeScope : LifetimeScope
	{
		[SerializeField]
		AppData bootSettings;

		[SerializeField]
		ServiceData ServiceData;

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(bootSettings);
			builder.RegisterInstance(ServiceData);
			
			builder.Register<AppBootstrapper>(Lifetime.Singleton);
		}
	}
}
