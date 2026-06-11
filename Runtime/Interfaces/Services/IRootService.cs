using Cysharp.Threading.Tasks;
using Mudit.Core.ScriptableObjects.Data;

namespace Mudit.Core.Interfaces.Services
{
	public interface IRootService
	{
		UniTask InitializeAsync(ServiceData settings);
	}
}