using UnityEngine;

public class MoveArrowGroup : UIBase
{
	[SerializeField]
	private UIWidget _moveArrow;

	[SerializeField]
	private UISprite _cursor;

	[SerializeField]
	private Transform _tail;

	[SerializeField]
	private float _hideDistance = 100f;

	private Vector3 _combatMoveTo;

	private float _hideDelay;

	private bool _isShow;

	private Transform _arrowTransfrom;

	private PlayerController _controller;

	[ExposedInEditor(null)]
	private float _angOffset;

	public Transform ArrowTransform
	{
		get
		{
			if ((Object)(object)_arrowTransfrom == (Object)null)
			{
				_arrowTransfrom = ((Component)_moveArrow).transform;
			}
			return _arrowTransfrom;
		}
	}

	public Vector3 CombatMoveToPos
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (!_isShow)
			{
				return KMathUtil.InvalidVector;
			}
			return _combatMoveTo;
		}
	}

	private void Start()
	{
		_controller = KSingleton<PlayerController>.Instance();
		HideMoveArrow();
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().OnRequestCombatMoveTo += CombatMoveTo;
		}
	}

	private void OnEnable()
	{
		GameSystem<CombatSystem>.Instance().OnRequestMoveTo += CombatMoveTo;
	}

	private void OnDisable()
	{
		GameSystem<CombatSystem>.Instance().OnRequestMoveTo -= CombatMoveTo;
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().OnRequestCombatMoveTo -= CombatMoveTo;
		}
	}

	private void Update()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_controller == (Object)null)
		{
			return;
		}
		if (_hideDelay > 0f)
		{
			_hideDelay -= Time.deltaTime;
			if (_hideDelay <= 0f)
			{
				HideMoveArrow();
			}
		}
		if (_isShow)
		{
			Vector3 val = ((!_controller.IsInServerSideBattle) ? _controller.MoveTarget.TargetPos : _combatMoveTo);
			Vector3 localPosition = MainCamera.WorldToNGUIPos(val);
			ArrowTransform.localPosition = localPosition;
			Vector3 val2 = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition);
			((Component)_cursor).transform.localScale = new Vector3((float)((localPosition.x - val2.x > 0f) ? 1 : (-1)), 1f, 1f);
			_tail.localRotation = Quaternion.Euler(0f, 0f, _angOffset + Mathf.Atan2(localPosition.y - val2.y, localPosition.x - val2.x) * 57.29578f);
			Vector3 val3 = PlayerBehavior.LocalPlayer.CurrentPosition - val;
			float magnitude = ((Vector3)(ref val3)).magnitude;
			if (magnitude < 200f || !GameSystem<CombatSystem>.Instance().CombatMode)
			{
				HideMoveArrow();
			}
			_moveArrow.alpha = Mathf.Clamp01(magnitude / _hideDistance);
		}
	}

	private void CombatMoveTo(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_combatMoveTo = pos;
		Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - _combatMoveTo;
		float magnitude = ((Vector3)(ref val)).magnitude;
		ShowMoveArrow(magnitude / _controller.MoveSpeed + Connections.Frontend.SeverDelayTime + 3f);
		Update();
	}

	private void HideMoveArrow(float delay = 0f)
	{
		if (delay > 0f)
		{
			_hideDelay = delay;
			return;
		}
		_isShow = false;
		((Component)_moveArrow).gameObject.SetActive(false);
	}

	private void ShowMoveArrow(float duration = 0f)
	{
		_isShow = true;
		_hideDelay = duration;
		((Component)_moveArrow).gameObject.SetActive(true);
	}
}
