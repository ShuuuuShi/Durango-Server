using UnityEngine;

namespace Durango.Render.Camera;

public struct Target
{
	public GameObject Object;

	public Vector3 Position;

	private readonly int _type;

	private Vector3 _lastPostion;

	public Target(GameObject value)
	{
		Position = Vector3.zero;
		if (value == null)
		{
			_type = 0;
			Object = null;
		}
		else
		{
			_type = 1;
			Object = value;
		}
		_lastPostion = Vector3.zero;
		_lastPostion = Get();
	}

	public Target(Vector3 value)
	{
		Object = null;
		Position = value;
		_type = 2;
		_lastPostion = Vector3.zero;
		_lastPostion = Get();
	}

	public Vector3 Get()
	{
		switch (_type)
		{
		case 1:
			_lastPostion = ((!Object) ? _lastPostion : InteractionObject.GetInteractionPosition(Object, ignoreY: false));
			break;
		case 2:
			_lastPostion = Position;
			break;
		default:
			_lastPostion = PlayerBehavior.LocalPlayer.CameraOrigin;
			break;
		}
		return _lastPostion;
	}
}
