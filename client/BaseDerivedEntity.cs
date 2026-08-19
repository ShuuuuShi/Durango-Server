using JetBrains.Annotations;
using UnityEngine;

public abstract class BaseDerivedEntity
{
	private static int _nextInstanceId = 999;

	private readonly int _instanceId;

	public GameObject GameObject { get; private set; }

	public Transform Transform { get; private set; }

	protected BaseDerivedEntity(GameObject gameObject)
	{
		GameObject = gameObject;
		Transform = gameObject.transform;
		_instanceId = _nextInstanceId;
		_nextInstanceId++;
	}

	public override int GetHashCode()
	{
		return _instanceId;
	}

	public override bool Equals(object o)
	{
		return CompareBaseObjects(this, o as BaseDerivedEntity);
	}

	public static implicit operator bool(BaseDerivedEntity exists)
	{
		return !CompareBaseObjects(exists, null);
	}

	public static bool operator ==(BaseDerivedEntity x, BaseDerivedEntity y)
	{
		return CompareBaseObjects(x, y);
	}

	public static bool operator !=(BaseDerivedEntity x, BaseDerivedEntity y)
	{
		return !CompareBaseObjects(x, y);
	}

	private static bool CompareBaseObjects(BaseDerivedEntity lhs, BaseDerivedEntity rhs)
	{
		bool flag = object.ReferenceEquals(lhs, null);
		bool flag2 = object.ReferenceEquals(rhs, null);
		if (flag2 && flag)
		{
			return true;
		}
		if (flag2)
		{
			return !IsNativeObjectAlive(lhs);
		}
		if (flag)
		{
			return !IsNativeObjectAlive(rhs);
		}
		return lhs._instanceId == rhs._instanceId;
	}

	private static bool IsNativeObjectAlive([NotNull] BaseDerivedEntity entity)
	{
		return entity.GameObject;
	}
}
