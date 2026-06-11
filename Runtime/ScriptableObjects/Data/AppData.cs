using UnityEngine;

namespace Mudit.Core.ScriptableObjects.Data
{
	[CreateAssetMenu(fileName = "AppData", menuName = "Mudit/Data/AppData")]
	public class AppData : ScriptableObject
	{
		[SerializeField]
		public bool Debug = false;

		[SerializeField, Range(30, 144)]
		public int FPS = 120; 

	}
}