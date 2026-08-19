using System.Collections.Generic;
using Shared.Battle;
using UnityEngine;

public class BoneLookAtTarget : MonoBehaviour
{
	private struct LookAtCoord
	{
		private float _height;

		private float _yawWorld;

		private float _distance2D;

		private bool _valid;

		public static LookAtCoord FromWorldPos(Vector3 targetPos, Vector3 bodyPos)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			LookAtCoord result = default(LookAtCoord);
			result._height = targetPos.y;
			targetPos.y = 0f;
			bodyPos.y = 0f;
			Vector3 val = targetPos - bodyPos;
			result._distance2D = ((Vector3)(ref val)).magnitude;
			result._yawWorld = KMathUtil.CalcYawWithTarget(targetPos, bodyPos);
			result._valid = true;
			return result;
		}

		public bool IsInvalid()
		{
			return !_valid;
		}

		public Vector3 ToWorldPos(Vector3 bodyPos)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			bodyPos.y = 0f;
			Vector3 val = KMathUtil.CalcDirectionFromYaw(_yawWorld);
			Vector3 result = bodyPos + val * _distance2D;
			result.y = _height;
			return result;
		}

		public static LookAtCoord Lerp(LookAtCoord v1, LookAtCoord v2, float myYawWorld, float t)
		{
			LookAtCoord result = default(LookAtCoord);
			result._height = Mathf.Lerp(v1._height, v2._height, t);
			result._distance2D = Mathf.Lerp(v1._distance2D, v2._distance2D, t);
			result._yawWorld = KMathUtil.NormalizeAngDeg(Mathf.Lerp(KMathUtil.NormalizeAngDeg(v1._yawWorld - myYawWorld), KMathUtil.NormalizeAngDeg(v2._yawWorld - myYawWorld), t) + myYawWorld);
			result._valid = true;
			return result;
		}
	}

	[SerializeField]
	private GameObject _target;

	[SerializeField]
	private float _cullDistance = 1000f;

	[SerializeField]
	private float _maxLookDistance = 1000f;

	[SerializeField]
	private bool _onlyLookAtRunWalkStand = true;

	[SerializeField]
	private bool _autoFindHeadBone = true;

	[SerializeField]
	private Transform _head;

	[SerializeField]
	private float _inheritRatio = 0.5f;

	[SerializeField]
	private int _numInherits = 1;

	[SerializeField]
	private Vector3 _fixBoneRotation = new Vector3(0f, -90f, -90f);

	[SerializeField]
	private float _yawLimitDeg = 120f;

	[SerializeField]
	private float _pitchLimitDeg;

	[SerializeField]
	private float _distanceLimit = 100f;

	[SerializeField]
	private float _headingSpeed = 2f;

	private bool _activated;

	[SerializeField]
	private bool _autoChangeTarget = true;

	[ExposedInEditor(null)]
	private bool _debug;

	private bool _isInitialized;

	private CharacterBehavior _characterOwner;

	private Transform _transform;

	private List<KeyValuePair<Transform, float>> _affectedBones = new List<KeyValuePair<Transform, float>>();

	private List<KeyValuePair<CharacterBehavior, float>> _targetsCache = new List<KeyValuePair<CharacterBehavior, float>>();

	private LookAtCoord _lastLookAtTarget;

	private Vector3 _lastLookTargetPos;

	private float _nextLookTargetChange = -1f;

	private float _globalRatio;

	public bool Activated
	{
		get
		{
			return _activated;
		}
		set
		{
			if (_activated != value)
			{
				_globalRatio = 0f;
				if (!value)
				{
					_target = null;
				}
			}
			_activated = value;
		}
	}

	public bool AutoChangeTarget
	{
		get
		{
			return _autoChangeTarget && Activated;
		}
		set
		{
			_autoChangeTarget = value;
		}
	}

	private void Start()
	{
		_characterOwner = ((Component)this).gameObject.GetComponent<CharacterBehavior>();
		_transform = ((Component)this).gameObject.transform;
		Init();
	}

	public void Init()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (_autoFindHeadBone && (Object)null == (Object)(object)_head)
		{
			CharacterBehavior component = ((Component)this).gameObject.GetComponent<CharacterBehavior>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform bodyPartTransform = component.GetBodyPartTransform(BodyPart.Head, bAllowNull: true);
				if (Object.op_Implicit((Object)(object)bodyPartTransform))
				{
					_head = bodyPartTransform;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)_head))
		{
			RecalcAffectedBonesList();
			Activated = true;
			_isInitialized = true;
		}
		if (!KSingleton<PlayerController>.HasInstance())
		{
			Activated = false;
		}
	}

	private GameObject FindRandomTarget()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (Random.value < 0.3f)
		{
			return null;
		}
		Vector3 position = _transform.position;
		int count;
		Collider[] array = KCollisionUtility.OverlapSphere(position, 1000f, LayerMask.op_Implicit(LayerHelper.DefaultMask), out count);
		_targetsCache.Clear();
		for (int i = 0; i < count; i++)
		{
			Collider val = array[i];
			GameObject gameObject = ((Component)val).gameObject;
			if (!((Object)(object)gameObject == (Object)(object)((Component)this).gameObject) && (gameObject.CompareTag("Enemy") || gameObject.CompareTag("Player")))
			{
				CharacterBehavior component = gameObject.GetComponent<CharacterBehavior>();
				if (!((Object)(object)component == (Object)null))
				{
					Vector3 val2 = ((Component)val).transform.position - position;
					float value = Vector3.Magnitude(val2);
					_targetsCache.Add(new KeyValuePair<CharacterBehavior, float>(component, value));
				}
			}
		}
		if (_targetsCache.Count == 0)
		{
			return null;
		}
		_targetsCache.Sort((KeyValuePair<CharacterBehavior, float> x, KeyValuePair<CharacterBehavior, float> y) => x.Value.CompareTo(y.Value));
		int num = _targetsCache.Count - 1;
		int index = Random.Range(0, Mathf.Min(num, 3));
		return ((Component)_targetsCache[index].Key.GetBodyPartTransform(BodyPart.Head)).gameObject;
	}

	public void SetLookTarget(GameObject target, bool bFindHead = false)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_target != (Object)(object)target)
		{
			if (bFindHead && (Object)(object)target != (Object)null)
			{
				CharacterBehavior component = target.GetComponent<CharacterBehavior>();
				if (Object.op_Implicit((Object)(object)component))
				{
					Transform bodyPartTransform = component.GetBodyPartTransform(BodyPart.Head, bAllowNull: true);
					if (Object.op_Implicit((Object)(object)bodyPartTransform))
					{
						_target = ((Component)bodyPartTransform).gameObject;
					}
				}
			}
			else
			{
				_target = target;
			}
		}
		ResetNextLookTargetChangeTime();
	}

	private void ResetNextLookTargetChangeTime()
	{
		_nextLookTargetChange = Time.time + Random.Range(2f, 10f);
	}

	public void ForaceUpdate()
	{
		LateUpdate();
	}

	private void LateUpdate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_characterOwner == (Object)null || (Object)(object)PlayerBehavior.LocalPlayer == (Object)null || !_characterOwner.IsAlive || !_characterOwner.IsVisible)
		{
			return;
		}
		Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - _characterOwner.CurrentPosition;
		if (((Vector3)(ref val)).sqrMagnitude > _cullDistance * _cullDistance)
		{
			return;
		}
		if (!_isInitialized)
		{
			Init();
		}
		if (_autoChangeTarget && _nextLookTargetChange < Time.time)
		{
			ResetNextLookTargetChangeTime();
			if (Object.op_Implicit((Object)(object)_target))
			{
				Vector3 val2 = _target.transform.position - _head.position;
				if (((Vector3)(ref val2)).magnitude > _maxLookDistance)
				{
					_target = null;
				}
			}
			if ((Object)(object)_target == (Object)null)
			{
				_target = FindRandomTarget();
			}
		}
		ProcessHeadTransformation();
	}

	private void ProcessHeadTransformation()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_head == (Object)null)
		{
			return;
		}
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 targetPos = GetTargetPos(forward);
		Vector3 position = _head.position;
		position.y = 0f;
		if (_lastLookAtTarget.IsInvalid())
		{
			_lastLookAtTarget = LookAtCoord.FromWorldPos(_head.position + forward * 10000f, position);
			return;
		}
		targetPos = LimitTargetPos(forward, targetPos, position);
		LookAtCoord v = LookAtCoord.FromWorldPos(targetPos, position);
		float myYawWorld = KMathUtil.CalcYaw(forward);
		_lastLookAtTarget = LookAtCoord.Lerp(_lastLookAtTarget, v, myYawWorld, _headingSpeed * Time.deltaTime);
		_lastLookTargetPos = _lastLookAtTarget.ToWorldPos(position);
		CalcGlobalRatio();
		if (!(_globalRatio <= 0f))
		{
			int count = _affectedBones.Count;
			for (int num = count - 1; num >= 0; num--)
			{
				Transform key = _affectedBones[num].Key;
				float value = _affectedBones[num].Value;
				Quaternion rotation = key.rotation;
				key.LookAt(_lastLookTargetPos);
				key.Rotate(_fixBoneRotation);
				key.rotation = Quaternion.Lerp(rotation, key.rotation, value * _globalRatio);
			}
			if (_debug)
			{
				Debug.DrawLine(_head.position, _head.position + forward * 300f, Color.white);
				Debug.DrawLine(_head.position, _lastLookTargetPos, Color.red);
			}
		}
	}

	private Vector3 LimitTargetPos(Vector3 forward, Vector3 targetPos, Vector3 bodyPos)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if (_pitchLimitDeg > Mathf.Epsilon)
		{
			targetPos = KMathUtil.LimitPitchWithTarget(targetPos, _head.position, 0f - _pitchLimitDeg, _pitchLimitDeg);
		}
		float num = KMathUtil.CalcYawWithTarget(targetPos, bodyPos);
		float num2 = KMathUtil.CalcYaw(forward);
		float num3 = KMathUtil.NormalizeAngDeg(num - num2);
		if (!(0f - _yawLimitDeg <= num3) || !(num3 <= _yawLimitDeg))
		{
			float y = targetPos.y;
			num3 = ((!(num3 > 0f)) ? (0f - _yawLimitDeg) : _yawLimitDeg);
			num = num2 + num3;
			Vector3 val = KMathUtil.CalcDirectionFromYaw(num);
			Vector3 val2 = targetPos - _head.position;
			val2.y = 0f;
			float num4 = Mathf.Max(((Vector3)(ref val2)).magnitude, _distanceLimit);
			targetPos = bodyPos + val * num4;
			targetPos.y = y;
		}
		return targetPos;
	}

	private void CalcGlobalRatio()
	{
		if (_onlyLookAtRunWalkStand && !_characterOwner.IsLookAtAvailable)
		{
			_globalRatio -= Time.deltaTime;
		}
		else if (Object.op_Implicit((Object)(object)_target))
		{
			_globalRatio += Time.deltaTime;
		}
		else
		{
			_globalRatio -= Time.deltaTime;
		}
		_globalRatio = Mathf.Clamp(_globalRatio, 0f, 1f);
	}

	private Vector3 GetTargetPos(Vector3 forward)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_target))
		{
			return _target.transform.position;
		}
		return _head.position + forward * 10000f;
	}

	private void RecalcAffectedBonesList()
	{
		_affectedBones.Clear();
		float num = 1f;
		Transform val = _head;
		for (int i = 0; i < _numInherits; i++)
		{
			if ((Object)null == (Object)(object)val)
			{
				break;
			}
			_affectedBones.Add(new KeyValuePair<Transform, float>(val, num));
			val = val.parent;
			num *= _inheritRatio;
		}
	}
}
