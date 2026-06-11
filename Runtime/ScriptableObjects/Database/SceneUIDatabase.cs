using UnityEngine;
using System.Collections.Generic;
using Mudit.Core.Serializables;

namespace Mudit.Core.ScriptableObjects.Database
{
	[CreateAssetMenu(fileName = "SceneUIDatabase", menuName = "Mudit/Database/SceneUIDatabase")]
	public class SceneUIDatabase : ScriptableObject
	{
		[SerializeField] 
		private List<SceneUIViewPrefabs> sceneUIPrefabs = new List<SceneUIViewPrefabs>();
		
		public ViewRegistryData GetViewRegistryData(string sceneName)
		{
			var settings = GetSceneSettings(sceneName);
			return settings?.GetViewRegistryData();
		}

		public SceneUIViewPrefabs GetSceneSettings(string sceneName)
		{
			foreach (var sceneUIViewPrefabs in sceneUIPrefabs)
			{
				if (sceneUIViewPrefabs.GetSceneName() == sceneName)
				{
					return sceneUIViewPrefabs;
				}
			}
			return null;
		}
	}
}