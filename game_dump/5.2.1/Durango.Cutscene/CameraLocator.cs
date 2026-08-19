using UnityEngine;

namespace Durango.Cutscene;

public class CameraLocator : MonoBehaviour
{
	public Transform OriginGameObject { get; set; }

	public Transform TargetGameObject { get; set; }

	private void LateUpdate()
	{
		if (!(OriginGameObject == null) && !(TargetGameObject == null))
		{
			base.transform.position = OriginGameObject.position;
			base.transform.LookAt(TargetGameObject);
		}
	}
}
