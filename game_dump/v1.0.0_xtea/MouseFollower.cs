using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{
	public Trail Trail;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButton(0))
		{
			Trail.Emit = true;
			((Component)this).transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 0.01f));
		}
		else
		{
			Trail.Emit = false;
		}
	}
}
