using VContainer;
using VContainer.Unity;

namespace Mudit.Samples.LifetimeScopes
{
	public class MainMenuLifetimeScope : LifetimeScope
	{
		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<TestWorker>();
			builder.Register<TestSpeaker>(Lifetime.Singleton);
		}
	}
}