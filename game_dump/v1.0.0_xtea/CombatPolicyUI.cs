using System;
using Shared.Battle;
using UnityEngine;

public class CombatPolicyUI : KSingleton<CombatPolicyUI>
{
	[Serializable]
	private struct TextureStruct
	{
		public Texture Texture;

		public Color Color;
	}

	[SerializeField]
	private GameObject _directionGuide;

	[SerializeField]
	private GameObject _baseWidget;

	[SerializeField]
	private ListObjectPool _arcList;

	[SerializeField]
	private TextureStruct _spriteNormal;

	[SerializeField]
	private TextureStruct _spriteSelected;

	[SerializeField]
	private TweenPosition _tweenPosition;

	[SerializeField]
	private float _deltaDistFactor;

	[SerializeField]
	private TweenRotation _tweenRotation;

	[SerializeField]
	private float _deltaAngle;

	[SerializeField]
	private float _circleRadiusFactor;

	[SerializeField]
	private float _minCircleRadius;

	[SerializeField]
	private float _maxCircleRadius;

	[SerializeField]
	private float _selectedArcFactor;

	private CharacterBehavior _targetCharacter;

	private Transform _directionGuideTransform;

	private CombatDirectionArc[] _arcs;

	private bool resetPositionAndRotation;

	private Vector3 _previousTargetPos;

	private float _previousTargetYaw;

	private float _circleRadius;

	public bool IsActivated => (Object)(object)_targetCharacter != (Object)null;

	public GameObject DirectionGuide => _directionGuide;

	public event Action<DamageDirection> CombatDirectionChanged;

	private void Start()
	{
		GameSystem<CombatSystem>.Instance().TargetChanged += OnChangeTarget;
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().OnPickObject += OnPickObject;
		}
	}

	private void Update()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (IsActivated)
		{
			if (resetPositionAndRotation)
			{
				InitPositionAndRotation();
				resetPositionAndRotation = false;
			}
			else
			{
				UpdateDirectionGuidePosition();
				UpdateDirectionGuideRotation();
			}
			_baseWidget.transform.localScale = Vector3.one * KSingleton<MainCamera>.Instance().ZoomScreenRatio;
		}
	}

	public Vector3 GetArcNGuiPosition(DamageDirection direction)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		CombatDirectionArc arc = GetArc(direction);
		return (!((Object)(object)arc != (Object)null)) ? Vector3.zero : arc.GetNguiPosition();
	}

	public void SetArcSelection(bool selected, DamageDirection? direction = null)
	{
		if (direction.HasValue)
		{
			CombatDirectionArc arc = GetArc(direction.Value);
			if ((Object)(object)arc != (Object)null)
			{
				arc.IsSelected = selected;
				RefreshArcSprite(arc);
			}
		}
		else
		{
			EnumerateArcs(delegate(CombatDirectionArc circle)
			{
				circle.IsSelected = selected;
				RefreshArcSprite(circle);
			});
		}
	}

	protected override void OnAwake()
	{
		_directionGuideTransform = _directionGuide.transform;
		_directionGuide.SetActive(false);
	}

	private void OnPickObject(Ray ray, PlayerController.TouchEvent touchEvent, ref bool result)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!IsActivated)
		{
			return;
		}
		int mask = (1 << OverlayCamera.Layer) | LayerMask.op_Implicit(LayerHelper.DefaultMask);
		if (!KUtility.RayCastContextAction(ray, mask, "Selectable", out var pickingObject))
		{
			return;
		}
		CombatDirectionArc component = pickingObject.GetComponent<CombatDirectionArc>();
		if (!((Object)(object)component == (Object)null))
		{
			if (!touchEvent.IsTouchBegan && this.CombatDirectionChanged != null)
			{
				this.CombatDirectionChanged(component.Direction);
			}
			result = true;
		}
	}

	private void InitArcs()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (_arcs != null)
		{
			return;
		}
		_arcList.Set(4);
		_arcs = new CombatDirectionArc[4];
		Vector3 localEulerAngles = _arcList.BaseObject.transform.localEulerAngles;
		int i = 0;
		for (int num = _arcs.Length; i < num; i++)
		{
			_arcs[i] = _arcList[i].GetComponent<CombatDirectionArc>();
			_arcs[i].Direction = (DamageDirection)i;
			float num2 = localEulerAngles.z;
			switch (_arcs[i].Direction)
			{
			case DamageDirection.Back:
				num2 += 180f;
				break;
			case DamageDirection.Left:
				num2 += 90f;
				break;
			case DamageDirection.Right:
				num2 -= 90f;
				break;
			}
			((Component)_arcs[i]).transform.localEulerAngles = new Vector3(localEulerAngles.x, localEulerAngles.y, num2);
		}
	}

	private CombatDirectionArc GetArc(DamageDirection direction)
	{
		return (direction >= DamageDirection.Front && (int)direction < _arcs.Length) ? _arcs[(int)direction] : null;
	}

	private void RefreshArcSprite(CombatDirectionArc arc)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (arc.IsSelected)
		{
			arc.SetSprite(_spriteSelected.Texture, _spriteSelected.Color);
			arc.SetRadius((int)(_circleRadius * _selectedArcFactor));
		}
		else
		{
			arc.SetSprite(_spriteNormal.Texture, _spriteNormal.Color);
			arc.SetRadius((int)_circleRadius);
		}
	}

	private void InitPositionAndRotation()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		_tweenPosition.ResetToBeginning();
		_tweenRotation.ResetToBeginning();
		((Behaviour)_tweenPosition).enabled = false;
		((Behaviour)_tweenRotation).enabled = false;
		_previousTargetPos = _targetCharacter.CurrentPosition;
		_previousTargetYaw = _targetCharacter.CurrentYaw;
		_directionGuideTransform.localPosition = _previousTargetPos;
		_directionGuideTransform.localRotation = GetDirectionGuideRotation(_previousTargetYaw);
	}

	private void UpdateDirectionGuidePosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(_previousTargetPos, _targetCharacter.CurrentPosition) > _circleRadius * _deltaDistFactor)
		{
			_tweenPosition.from = _directionGuideTransform.localPosition;
			_tweenPosition.to = _targetCharacter.CurrentPosition;
			_tweenPosition.tweenFactor = 0f;
			_tweenPosition.PlayForward();
			_previousTargetPos = _targetCharacter.CurrentPosition;
		}
	}

	private void UpdateDirectionGuideRotation()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(_previousTargetYaw - _targetCharacter.CurrentYaw) > _deltaAngle)
		{
			Quaternion localRotation = _directionGuideTransform.localRotation;
			GetValidRotationAngles(((Quaternion)(ref localRotation)).eulerAngles.y, _targetCharacter.CurrentYaw, out var fromRotation, out var toRotation);
			_tweenRotation.quaternionLerp = true;
			_tweenRotation.from = fromRotation;
			_tweenRotation.to = toRotation;
			_tweenRotation.tweenFactor = 0f;
			_tweenRotation.PlayForward();
			_previousTargetYaw = _targetCharacter.CurrentYaw;
		}
	}

	private void OnChangeTarget()
	{
		GameObject target = GameSystem<CombatSystem>.Instance().Target;
		_targetCharacter = ((!((Object)(object)target != (Object)null)) ? null : target.GetComponent<CharacterBehavior>());
		if (IsActivated)
		{
			_circleRadius = Mathf.Min(_targetCharacter.XRadius, _targetCharacter.YRadius) * _circleRadiusFactor;
			_circleRadius = Mathf.Max(_circleRadius, _minCircleRadius);
			_circleRadius = Mathf.Min(_circleRadius, _maxCircleRadius);
			resetPositionAndRotation = true;
			EnumerateArcs(RefreshArcSprite);
			_directionGuide.SetActive(true);
		}
		else
		{
			_directionGuide.SetActive(false);
		}
	}

	private void EnumerateArcs(Action<CombatDirectionArc> action)
	{
		InitArcs();
		int i = 0;
		for (int num = _arcs.Length; i < num; i++)
		{
			action(_arcs[i]);
		}
	}

	private static void GetValidRotationAngles(float fromYaw, float toYaw, out Vector3 fromRotation, out Vector3 toRotation)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(toYaw - fromYaw) > 180f)
		{
			if (toYaw < fromYaw)
			{
				toYaw += 360f;
			}
			else
			{
				fromYaw += 360f;
			}
		}
		Quaternion directionGuideRotation = GetDirectionGuideRotation(fromYaw);
		fromRotation = ((Quaternion)(ref directionGuideRotation)).eulerAngles;
		Quaternion directionGuideRotation2 = GetDirectionGuideRotation(toYaw);
		toRotation = ((Quaternion)(ref directionGuideRotation2)).eulerAngles;
	}

	private static Quaternion GetDirectionGuideRotation(float yaw)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(90f, yaw, 0f);
	}

	public void SetDirectionEnable(bool useDirection)
	{
		_directionGuide.SetActive(useDirection);
	}
}
