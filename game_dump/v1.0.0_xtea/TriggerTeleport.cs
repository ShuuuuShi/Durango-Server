using UnityEngine;

[ExecuteInEditMode]
public class TriggerTeleport : TriggerOnce
{
	public Vector3 _teleportTo = Vector3.zero;

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (_teleportTo == Vector3.zero)
		{
			_teleportTo = ((Component)this).transform.position;
		}
	}

	protected override bool TriggerEntered(Collider other)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior component = ((Component)other).gameObject.GetComponent<PlayerBehavior>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		if (component.IsLocalPlayer)
		{
			KSingleton<PlayerController>.Instance().Teleport(_teleportTo);
		}
		return true;
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
