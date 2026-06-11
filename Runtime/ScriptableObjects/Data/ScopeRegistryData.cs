using UnityEngine;
using System.Collections.Generic;
using VContainer.Unity;
using System;

namespace Mudit.Core.ScriptableObjects.Data
{
    [CreateAssetMenu(fileName = "ScopeRegistryData", menuName = "Mudit/Data/ScopeRegistryData")]
    public class ScopeRegistryData : ScriptableObject
    {
        [SerializeField]
        private List<LifetimeScope> scopePrefabs;
        private Dictionary<Type, LifetimeScope> scopeRegistry;
		[SerializeField]
		private string afterBootSceneName = "MainMenu";

		public string AfterBootSceneName => afterBootSceneName;
        public void Initialize()
        {
            scopeRegistry = new Dictionary<Type, LifetimeScope>();
            foreach (var scope in scopePrefabs)
            {
                if (scope == null) continue;
                var type = scope.GetType();
                if (!scopeRegistry.ContainsKey(type))
                {
                    scopeRegistry.Add(type, scope);
                }
            }
        }
        public T Get<T>() where T : LifetimeScope
        {
            if (scopeRegistry == null) Initialize();
            if (scopeRegistry.TryGetValue(typeof(T), out var scope))
            {
                return scope as T;
            }
            return null;
        }
	}
}