using UnityEngine;

namespace Mudit.Core.UI.Utility
{
	public class RotateForever : MonoBehaviour
	{
		[Header("Rotation Speeds")]
		[SerializeField] private float horizontalRotationSpeed = 10f;

		[SerializeField] private float verticalRotationSpeed = 10f;

		[Header("Rotation Directions")]
		[SerializeField] private bool moveHorizontal;
		[SerializeField] private bool reverseHorizontal;

		[SerializeField] private bool moveVertical;
		[SerializeField] private bool reverseVertical;

		private Vector3 horizontalDirection;
		private Vector3 verticalDirection;

		void Start()
		{
			int horizontalIden = reverseHorizontal ? 1 : -1;
			int verticalIden = reverseVertical ? 1 : -1;		

			horizontalDirection = Vector3.forward * horizontalIden;
			verticalDirection = Vector3.right * verticalIden;

			if (!moveHorizontal)
				horizontalRotationSpeed = 0f;
			if (!moveVertical)
				verticalRotationSpeed = 0f;
		}

		void Update()
		{
			transform.Rotate(horizontalDirection, horizontalRotationSpeed * Time.deltaTime);
			transform.Rotate(verticalDirection, verticalRotationSpeed * Time.deltaTime);
		}
	}
}