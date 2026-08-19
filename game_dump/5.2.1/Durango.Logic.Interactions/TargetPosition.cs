using Durango.Terrain;
using UnityEngine;

namespace Durango.Logic.Interactions;

public class TargetPosition
{
	private enum Type
	{
		None,
		Object,
		Movable,
		Immovable,
		Position
	}

	private Type _type;

	private Transform _object;

	private CharacterBehavior _movable;

	private ImmovableBase _immovable;

	private Vector3 _position;

	public void Reset()
	{
		_object = null;
		_movable = null;
		_immovable = null;
		_position = Vector3.zero;
	}

	public void Set(GameObject obj)
	{
		Reset();
		if (obj == null)
		{
			_type = Type.None;
			return;
		}
		_movable = obj.GetComponent<CharacterBehavior>();
		if ((bool)_movable)
		{
			_type = Type.Movable;
			return;
		}
		_immovable = obj.GetComponent<ImmovableBase>();
		if ((bool)_immovable)
		{
			_type = Type.Immovable;
			return;
		}
		_object = obj.transform;
		_type = Type.Object;
	}

	public void Set(Vector3 worldPos)
	{
		Reset();
		_type = Type.Position;
		_position = Util.WorldPositionToClientPosition(worldPos);
	}

	public void Set(Point2 tile)
	{
		Reset();
		_type = Type.Position;
		_position = Util.TilePositionToClientPosition(new Vector2((float)tile.x + 0.5f, (float)tile.y + 0.5f));
	}

	public Vector3 Get()
	{
		TryGet(out var pos);
		return pos;
	}

	public bool TryGet(out Vector3 pos)
	{
		switch (_type)
		{
		case Type.Object:
			if ((bool)_object)
			{
				pos = _object.position;
				return true;
			}
			break;
		case Type.Movable:
			if ((bool)_movable)
			{
				pos = _movable.InteractionPosition;
				return true;
			}
			break;
		case Type.Immovable:
			if ((bool)_immovable)
			{
				pos = _immovable.InteractionPosition;
				return true;
			}
			break;
		case Type.Position:
			pos = _position;
			return true;
		}
		pos = Vector3.zero;
		return false;
	}
}
