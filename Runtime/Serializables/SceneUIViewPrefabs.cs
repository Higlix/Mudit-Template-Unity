using UnityEngine;
using Mudit.Core.Enums;

namespace Mudit.Core.Serializables
{
	[System.Serializable]
	public class SceneUIViewPrefabs
	{
		[SerializeField]
		private string sceneName;

		[SerializeField]
		private ViewRegistryData viewRegistryData;

		[SerializeField]
		private InstantiationMode instantiationMode = InstantiationMode.Eager;

		public string GetSceneName()
		{
			return sceneName;
		}
		public ViewRegistryData GetViewRegistryData()
		{
			return viewRegistryData;
		}

		public InstantiationMode InstantiationMode => instantiationMode;
	}
}