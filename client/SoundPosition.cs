using UnityEngine;

public struct SoundPosition
{
	public enum Type
	{
		None,
		Position3D,
		ChaseObject
	}

	public static readonly SoundPosition Empty = new SoundPosition(Type.None, Vector3.zero, null);

	public Type PositionType;

	public Vector3 Position;

	public GameObject Target;

	private SoundPosition(Type type, Vector3 position, GameObject target)
	{
		PositionType = type;
		Position = position;
		Target = target;
	}

	public static SoundPosition Fix(Vector3 position)
	{
		return new SoundPosition(Type.Position3D, position, null);
	}

	public static SoundPosition Chase(GameObject target)
	{
		return new SoundPosition(Type.ChaseObject, Vector3.zero, target);
	}

	public static SoundPosition Chase(GameObject target, Vector3 offset)
	{
		return new SoundPosition(Type.ChaseObject, offset, target);
	}
}
