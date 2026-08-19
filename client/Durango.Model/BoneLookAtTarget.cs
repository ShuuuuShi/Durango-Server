using System.Collections.Generic;
using Durango.Utils;
using Shared.Battle;
using UnityEngine;

namespace Durango.Model;

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
			LookAtCoord result = default(LookAtCoord);
			result._height = targetPos.y;
			targetPos.y = 0f;
			bodyPos.y = 0f;
			result._distance2D = (targetPos - bodyPos).magnitude;
			result._yawWorld = Maths.CalcYawWithTarget(targetPos, bodyPos);
			result._valid = true;
			return result;
		}

		public bool IsInvalid()
		{
			return !_valid;
		}

		public Vector3 ToWorldPos(Vector3 bodyPos)
		{
			bodyPos.y = 0f;
			Vector3 vector = Maths.CalcDirectionFromYaw(_yawWorld);
			Vector3 result = bodyPos + vector * _distance2D;
			result.y = _height;
			return result;
		}

		public static LookAtCoord Lerp(LookAtCoord v1, LookAtCoord v2, float myYawWorld, float t)
		{
			LookAtCoord result = default(LookAtCoord);
			result._height = Mathf.Lerp(v1._height, v2._height, t);
			result._distance2D = Mathf.Lerp(v1._distance2D, v2._distance2D, t);
			result._yawWorld = Maths.NormalizeAngDeg(Mathf.Lerp(Maths.NormalizeAngDeg(v1._yawWorld - myYawWorld), Maths.NormalizeAngDeg(v2._yawWorld - myYawWorld), t) + myYawWorld);
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

	private CharacterBehavior _character;

	private readonly List<KeyValuePair<Transform, float>> _affectedBones = new List<KeyValuePair<Transform, float>>();

	private readonly List<KeyValuePair<CharacterBehavior, float>> _targetsCache = new List<KeyValuePair<CharacterBehavior, float>>();

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
			return _autoChangeTarget;
		}
		set
		{
			_autoChangeTarget = value;
		}
	}

	private void Start()
	{
		_character = base.gameObject.GetComponent<CharacterBehavior>();
		if (!(_character == null))
		{
			if (null == _head)
			{
				_head = _character.GetBodyPartTransform(BodyPart.Head, bAllowNull: true);
			}
			if (_head != null)
			{
				RecalcAffectedBonesList();
				Activated = true;
				_character.BoneMergeable.PreUpdate += Process;
			}
		}
	}

	private GameObject FindRandomTarget()
	{
		if (Random.value < 0.3f)
		{
			return null;
		}
		Vector3 position = base.transform.position;
		int count;
		Collider[] array = Collisions.OverlapSphere(position, 1000f, LayerHelper.DefaultMask, out count);
		_targetsCache.Clear();
		for (int i = 0; i < count; i++)
		{
			Collider collider = array[i];
			GameObject gameObject = collider.gameObject;
			if (!(gameObject == base.gameObject))
			{
				CharacterBehavior component = gameObject.GetComponent<CharacterBehavior>();
				if (!(component == null))
				{
					Vector3 vector = collider.transform.position - position;
					float value = Vector3.Magnitude(vector);
					_targetsCache.Add(new KeyValuePair<CharacterBehavior, float>(component, value));
				}
			}
		}
		if (_targetsCache.Count == 0)
		{
			return null;
		}
		_targetsCache.Sort((KeyValuePair<CharacterBehavior, float> x, KeyValuePair<CharacterBehavior, float> y) => x.Value.CompareTo(y.Value));
		int a = _targetsCache.Count - 1;
		int index = Random.Range(0, Mathf.Min(a, 3));
		return _targetsCache[index].Key.GetBodyPartTransform(BodyPart.Head).gameObject;
	}

	public void SetLookTarget(GameObject target, bool findHead = false)
	{
		if (_target != target)
		{
			if (findHead && target != null)
			{
				CharacterBehavior component = target.GetComponent<CharacterBehavior>();
				if ((bool)component)
				{
					Transform bodyPartTransform = component.GetBodyPartTransform(BodyPart.Head, bAllowNull: true);
					if ((bool)bodyPartTransform)
					{
						_target = bodyPartTransform.gameObject;
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

	private void Process()
	{
		if (!base.enabled || _character == null || _head == null)
		{
			return;
		}
		if (!LookAtNotAllowed() && _autoChangeTarget && _nextLookTargetChange < Time.time)
		{
			ResetNextLookTargetChangeTime();
			if (_target != null && _head != null && (_target.transform.position - _head.position).magnitude > _maxLookDistance)
			{
				_target = null;
			}
			if (_target == null)
			{
				_target = FindRandomTarget();
			}
		}
		ProcessHeadTransformation();
	}

	private bool LookAtNotAllowed()
	{
		return PlayerBehavior.LocalPlayer == null || !_character.IsAlive || !_character.WillBeRendered || (PlayerBehavior.LocalPlayer.CurrentPosition - _character.CurrentPosition).sqrMagnitude > _cullDistance * _cullDistance;
	}

	private void ProcessHeadTransformation()
	{
		if (_head == null)
		{
			return;
		}
		Vector3 forward = base.transform.forward;
		Vector3 position = _head.position;
		position.y = 0f;
		if (_lastLookAtTarget.IsInvalid())
		{
			_lastLookAtTarget = LookAtCoord.FromWorldPos(_head.position + forward * 10000f, position);
			return;
		}
		if (!LookAtNotAllowed())
		{
			Vector3 targetPos = GetTargetPos(forward);
			targetPos = LimitTargetPos(forward, targetPos, position);
			LookAtCoord v = LookAtCoord.FromWorldPos(targetPos, position);
			float myYawWorld = Maths.CalcYaw(forward);
			_lastLookAtTarget = LookAtCoord.Lerp(_lastLookAtTarget, v, myYawWorld, _headingSpeed * Time.deltaTime);
			_lastLookTargetPos = _lastLookAtTarget.ToWorldPos(position);
		}
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
		if (_pitchLimitDeg > Mathf.Epsilon)
		{
			targetPos = Maths.LimitPitchWithTarget(targetPos, _head.position, 0f - _pitchLimitDeg, _pitchLimitDeg);
		}
		float num = Maths.CalcYawWithTarget(targetPos, bodyPos);
		float num2 = Maths.CalcYaw(forward);
		float num3 = Maths.NormalizeAngDeg(num - num2);
		if (!(0f - _yawLimitDeg <= num3) || !(num3 <= _yawLimitDeg))
		{
			float y = targetPos.y;
			num3 = ((!(num3 > 0f)) ? (0f - _yawLimitDeg) : _yawLimitDeg);
			num = num2 + num3;
			Vector3 vector = Maths.CalcDirectionFromYaw(num);
			Vector3 vector2 = targetPos - _head.position;
			vector2.y = 0f;
			float num4 = Mathf.Max(vector2.magnitude, _distanceLimit);
			targetPos = bodyPos + vector * num4;
			targetPos.y = y;
		}
		return targetPos;
	}

	private void CalcGlobalRatio()
	{
		float deltaTime = Time.deltaTime;
		if (LookAtNotAllowed() || (_onlyLookAtRunWalkStand && !_character.IsLookAtMotion))
		{
			_globalRatio -= deltaTime;
		}
		else if ((bool)_target)
		{
			_globalRatio += deltaTime;
		}
		else
		{
			_globalRatio -= deltaTime;
		}
		_globalRatio = Mathf.Clamp(_globalRatio, 0f, 1f);
	}

	private Vector3 GetTargetPos(Vector3 forward)
	{
		if ((bool)_target)
		{
			return _target.transform.position;
		}
		return _head.position + forward * 10000f;
	}

	private void RecalcAffectedBonesList()
	{
		_affectedBones.Clear();
		float num = 1f;
		Transform transform = _head;
		for (int i = 0; i < _numInherits; i++)
		{
			if (null == transform)
			{
				break;
			}
			_affectedBones.Add(new KeyValuePair<Transform, float>(transform, num));
			transform = transform.parent;
			num *= _inheritRatio;
		}
	}
}
