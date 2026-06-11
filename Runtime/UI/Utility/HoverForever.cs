using UnityEngine;

namespace Mudit.Core.UI.Utility
{
	public class HoverForever : MonoBehaviour
	{
		[Header("Hover Settings")]
		[Tooltip("How high/low the object moves from its starting position.")]
		[SerializeField] private float amplitude = 10f;

		[Tooltip("How fast the object hovers up and down.")]
		[SerializeField] private float frequency = 2f;

		private Vector3 startPosition;

		void Start()
		{
			// Store the starting local position to hover around it
			startPosition = transform.localPosition;
		}

		void Update()
		{
			// Calculate the new Y offset using a Sine wave
			// Sin(Time * speed) moves between -1 and 1
			// Multiply by amplitude to control distance
			float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;

			// Apply the new position
			transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
		}
	}
}