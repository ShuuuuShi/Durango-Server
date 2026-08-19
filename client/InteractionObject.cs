using Durango.Prologue;
using Durango.Terrain;
using UnityEngine;

public class InteractionObject
{
	public enum Type
	{
		Animal,
		Prop,
		PrologueSelectCharacter,
		PropSelectableByClient,
		Vehicle,
		None
	}

	private GameObject _target;

	private Vector2 _tile;

	private string _entityId;

	private ImmovableBase _immovableTarget;

	public CharacterBehavior CharacterTarget { get; private set; }

	public GameObject Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
			if (_target == null)
			{
				_entityId = string.Empty;
				ObjectType = Type.None;
				CharacterTarget = null;
				_immovableTarget = null;
				return;
			}
			CharacterTarget = _target.GetComponent<CharacterBehavior>();
			_immovableTarget = _target.GetComponent<ImmovableBase>();
			if (_target.GetComponent<TriggerPrologueSelectCharacter>() != null)
			{
				_entityId = "100";
				ObjectType = Type.PrologueSelectCharacter;
				return;
			}
			SelectableObject component = _target.GetComponent<SelectableObject>();
			if (component != null)
			{
				_entityId = component.EntityId;
				ObjectType = Type.PropSelectableByClient;
				return;
			}
			PetAI component2 = _target.GetComponent<PetAI>();
			if (component2 != null)
			{
				if (component2.IsLocalPlayersPet())
				{
					ObjectType = Type.Vehicle;
				}
				else
				{
					ObjectType = Type.None;
				}
			}
			else if ((bool)_target.GetComponent<VehicleProp>())
			{
				ObjectType = Type.Vehicle;
			}
			else if (_immovableTarget != null)
			{
				ObjectType = Type.Prop;
			}
			else if (CharacterTarget != null)
			{
				ObjectType = Type.Animal;
			}
		}
	}

	public Type ObjectType { get; private set; }

	public Vector2 Tile
	{
		get
		{
			if (ObjectType == Type.Animal)
			{
				return -Vector2.one;
			}
			if (_immovableTarget != null)
			{
				return _immovableTarget.WorldTile.ToVector2();
			}
			if ((_tile.x < 0f || _tile.y < 0f) && _target != null)
			{
				_tile = Util.ClientPositionToTilePosition(_target.transform.position);
			}
			return _tile;
		}
	}

	public string EntityId
	{
		get
		{
			return (!string.IsNullOrEmpty(_entityId)) ? _entityId : ObjectIdentifier.GetEntityId(_target);
		}
		set
		{
			_entityId = value;
		}
	}

	public int EntityType => ObjectIdentifier.GetEntityType(_target);

	public float LimitDistance { get; set; }

	public float Distance => GetDistance(Target);

	public float DistanceRatio
	{
		get
		{
			if (LimitDistance < 0f || ObjectType == Type.PrologueSelectCharacter)
			{
				return 0f;
			}
			return Distance / LimitDistance;
		}
	}

	public Vector3 Position
	{
		get
		{
			if (_target == null)
			{
				return Vector3.zero;
			}
			if (CharacterTarget != null)
			{
				return CharacterTarget.InteractionPosition;
			}
			return (!(_immovableTarget != null)) ? _target.transform.position : _immovableTarget.InteractionPosition;
		}
	}

	public InteractionObject()
	{
		_tile.x = -1f;
		_tile.y = -1f;
		ObjectType = Type.None;
	}

	public InteractionObject(GameObject obj)
		: this()
	{
		Target = obj;
		LimitDistance = 2000f;
	}

	public T GetTargetComponent<T>() where T : Component
	{
		return (!(_target == null)) ? _target.GetComponent<T>() : ((T)null);
	}

	public bool IsValid()
	{
		if (_target == null || !_target.activeSelf)
		{
			return false;
		}
		switch (ObjectType)
		{
		case Type.Prop:
		{
			CharacterBehavior component = _target.GetComponent<CharacterBehavior>();
			if (component != null && !component.WillBeRendered)
			{
				return false;
			}
			return true;
		}
		case Type.Animal:
		case Type.PrologueSelectCharacter:
		case Type.PropSelectableByClient:
		case Type.Vehicle:
			return true;
		case Type.None:
			return false;
		default:
			return false;
		}
	}

	public static float GetDistance(GameObject obj)
	{
		if (PlayerBehavior.LocalPlayer == null)
		{
			return 0f;
		}
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		Vector3 interactionPosition = GetInteractionPosition(obj);
		currentPosition.y = 0f;
		return (currentPosition - interactionPosition).magnitude;
	}

	public static Vector3 GetInteractionPosition(GameObject obj, bool ignoreY = true)
	{
		if (obj == null)
		{
			return Vector3.zero;
		}
		CharacterBehavior component = obj.GetComponent<CharacterBehavior>();
		Vector3 result;
		if ((bool)component)
		{
			result = component.InteractionPosition;
		}
		else
		{
			ImmovableBase component2 = obj.GetComponent<ImmovableBase>();
			result = ((!component2) ? obj.transform.position : component2.InteractionPosition);
		}
		if (ignoreY)
		{
			result.y = 0f;
		}
		return result;
	}

	public float CalcInteractionDistance()
	{
		if (CharacterTarget != null)
		{
			return Mathf.Max(CharacterTarget.XRadius, CharacterTarget.YRadius);
		}
		SelectableObject component = _target.GetComponent<SelectableObject>();
		if (component != null)
		{
			return component.InteractableDistance;
		}
		return (!(_immovableTarget != null)) ? 100f : _immovableTarget.InteractionDistance;
	}
}
