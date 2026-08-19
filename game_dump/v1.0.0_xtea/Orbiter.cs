using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

public class Orbiter : MonoBehaviour
{
	public float TankCollisionOrbitRadius = 1.5f;

	public float TankCollisionRotationSpeed = 1f;

	public Trail Trail;

	private TankController _tankBeingController;

	private Vector3 _pos;

	private void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_pos = Vector3.zero;
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		Ray val = Camera.main.ScreenPointToRay(Input.mousePosition);
		TankController tankController = null;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val, ref val2, 1000f))
		{
			tankController = ((Component)((Component)((RaycastHit)(ref val2)).collider).transform.root).GetComponent<TankController>();
			if ((Object)(object)tankController == (Object)null)
			{
				_pos = ((RaycastHit)(ref val2)).point;
			}
			else
			{
				flag = true;
				_pos = ((Component)tankController).transform.position;
			}
		}
		if (!flag)
		{
			Trail.Emit = false;
			return;
		}
		if ((Object)(object)_tankBeingController != (Object)(object)tankController)
		{
			Trail.Emit = true;
			((Component)this).transform.localScale = Vector3.one * TankCollisionOrbitRadius;
			((Component)this).transform.Rotate(Vector3.up, TankCollisionRotationSpeed * Time.deltaTime);
			((Component)this).transform.position = _pos;
		}
		if (Input.GetMouseButtonDown(0))
		{
			if ((Object)(object)_tankBeingController != (Object)null)
			{
				_tankBeingController.InControl = false;
			}
			tankController.InControl = true;
			_tankBeingController = tankController;
		}
	}
}
