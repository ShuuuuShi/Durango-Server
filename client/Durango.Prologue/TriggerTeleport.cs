using Durango.Utils;
using Shared.Teleport;
using UnityEngine;

namespace Durango.Prologue;

[ExecuteInEditMode]
public class TriggerTeleport : TriggerOnce
{
	public Vector3 _teleportTo = Vector3.zero;

	private void Awake()
	{
		if (_teleportTo == Vector3.zero)
		{
			_teleportTo = base.transform.position;
		}
	}

	protected override bool TriggerEntered(Collider other)
	{
		PlayerBehavior component = other.gameObject.GetComponent<PlayerBehavior>();
		if (component == null)
		{
			return false;
		}
		if (component.IsLocalPlayer)
		{
			Singleton<PlayerController>.Instance().Teleport(_teleportTo, TeleportType.Unknown, instance: true);
		}
		return true;
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
