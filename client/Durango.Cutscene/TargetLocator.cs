using UnityEngine;

namespace Durango.Cutscene;

public class TargetLocator : MonoBehaviour
{
	public Transform Origin { get; set; }

	private void LateUpdate()
	{
		if (!(Origin == null))
		{
			base.transform.position = Origin.position;
			base.transform.localRotation = Origin.localRotation;
		}
	}
}
