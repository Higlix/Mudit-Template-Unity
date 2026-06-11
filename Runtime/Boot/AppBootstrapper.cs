using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Mudit.Core.Interfaces.Services;
using Mudit.Core.ScriptableObjects.Data;
using System.Collections.Generic;

namespace Mudit.Core.Boot
{
	public class AppBootstrapper
	{
		readonly IReadOnlyList<IRootService> services;
		readonly ServiceData serviceData;

		[Inject]
		public AppBootstrapper(IReadOnlyList<IRootService> services, ServiceData serviceData)
		{
			this.services = services;
			this.serviceData = serviceData;
		}

		public async UniTask BootAsync()
		{
			Debug.Log("Boot Sequence Started...");

			foreach (var service in services)
			{
				await service.InitializeAsync(serviceData);
			}
		
			Debug.Log("Boot Sequence Completed!");
		}
	}
}