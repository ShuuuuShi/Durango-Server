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

	private ulong _entityId;

	private CharacterBehavior _characterTarget;

	private ImmovableBase _immovableTarget;

	public GameObject Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
			if ((Object)(object)_target == (Object)null)
			{
				_entityId = 0uL;
				ObjectType = Type.None;
				_characterTarget = null;
				_immovableTarget = null;
				return;
			}
			_characterTarget = _target.GetComponent<CharacterBehavior>();
			_immovableTarget = _target.GetComponent<ImmovableBase>();
			if ((Object)(object)_target.GetComponent<TriggerPrologueSelectCharacter>() != (Object)null)
			{
				_entityId = 100uL;
				ObjectType = Type.PrologueSelectCharacter;
				return;
			}
			SelectableObject component = _target.GetComponent<SelectableObject>();
			if ((Object)(object)component != (Object)null)
			{
				_entityId = component.EntityId;
				ObjectType = Type.PropSelectableByClient;
				return;
			}
			PetAI component2 = _target.GetComponent<PetAI>();
			if ((Object)(object)component2 != (Object)null)
			{
				if (component2.IsMyMaster(((Component)PlayerBehavior.LocalPlayer).gameObject))
				{
					ObjectType = Type.Vehicle;
				}
				else
				{
					ObjectType = Type.None;
				}
			}
			else if ((Object)(object)_characterTarget != (Object)null)
			{
				ObjectType = Type.Animal;
			}
			else if ((Object)(object)_immovableTarget != (Object)null)
			{
				ObjectType = Type.Prop;
			}
		}
	}

	public Type ObjectType { get; private set; }

	public Vector2 Tile
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			if (ObjectType == Type.Animal)
			{
				return -Vector2.one;
			}
			if ((_tile.x < 0f || _tile.y < 0f) && (Object)(object)_target != (Object)null)
			{
				_tile = TerrainA6.ClientPositionToTilePosition(_target.transform.position);
			}
			return _tile;
		}
	}

	public ulong EntityId
	{
		get
		{
			return (_entityId != 0L) ? _entityId : ObjectIdentifier.GetEntityId(_target);
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
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_target != (Object)null && _target.activeSelf)
			{
				if ((Object)(object)_characterTarget != (Object)null)
				{
					return _characterTarget.InteractionPosition;
				}
				if ((Object)(object)_immovableTarget != (Object)null)
				{
					return _immovableTarget.InteractionPosition;
				}
				return _target.transform.position;
			}
			return Vector3.zero;
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
		return (!((Object)(object)_target == (Object)null)) ? _target.GetComponent<T>() : ((T)(object)null);
	}

	public bool IsValid()
	{
		if ((Object)(object)_target == (Object)null || !_target.activeSelf)
		{
			return false;
		}
		switch (ObjectType)
		{
		case Type.Prop:
		{
			CharacterBehavior component = _target.GetComponent<CharacterBehavior>();
			if ((Object)(object)component != (Object)null && !component.IsVisible)
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
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PlayerBehavior.LocalPlayer == (Object)null)
		{
			return 0f;
		}
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		Vector3 interactionPosition = KUtility.GetInteractionPosition(obj);
		currentPosition.y = 0f;
		Vector3 val = currentPosition - interactionPosition;
		return ((Vector3)(ref val)).magnitude;
	}
}
