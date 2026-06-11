using System.Collections.Generic;
using System;
using UnityEngine;
using Mudit.Core.Services.UIService;

namespace Mudit.Core.Serializables
{
	[Serializable]
	public class ViewRegistryData
	{
		[SerializeField] private List<UIView> prefabs = new List<UIView>();
		public IEnumerable<UIView> AllPrefabs => prefabs;
		
		private Dictionary<Type, UIView> prefabDict;

		public void Initialize()
		{
			prefabDict = new Dictionary<Type, UIView>();
			foreach (var prefab in prefabs)
			{
				if (prefab == null) continue;
				
				var type = prefab.GetType();
				if (!prefabDict.ContainsKey(type))
				{
					prefabDict.Add(type, prefab);
				}
			}
		}

		public T GetPrefab<T>() where T : UIView
		{
			if (prefabDict == null) Initialize();
			
			if (prefabDict.TryGetValue(typeof(T), out var prefab))
			{
				return prefab as T;
			}
			return null;
		}
	}
}

