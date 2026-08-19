using System;
using UnityEngine;

public class CombatTargetWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private CombatTargetPortraits _portraits;

	[SerializeField]
	private HyperGaugeViewer _gaugeHp;

	[SerializeField]
	private UIWidget _normalMode;

	[SerializeField]
	private GameObject _iconArrowForNormal;

	[SerializeField]
	private UILabel _textDistanceForNormal;

	[SerializeField]
	private GameObject _selectionForNormal;

	[SerializeField]
	private UIWidget _extendedMode;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private GameObject _iconArrowForExtended;

	[SerializeField]
	private UILabel _textDistanceForExtended;

	[SerializeField]
	private GameObject _selectionForExtended;

	[SerializeField]
	private CombatTargetWidgetSelectButton _buttonSelect;

	[SerializeField]
	private UIWidget _buttonCancel;

	private bool _initialized;

	private DamageableEntity _entity;

	public Action<CombatTargetWidget> SelectButtonClicked;

	public Action<CombatTargetWidget> CancelButtonClicked;

	public bool IsSelected
	{
		get
		{
			return _selectionForNormal.activeSelf;
		}
		set
		{
			_selectionForNormal.SetActive(value);
			_selectionForExtended.SetActive(value);
			((Component)_buttonSelect).gameObject.SetActive(!IsSelected);
			((Component)_buttonCancel).gameObject.SetActive(!IsSelected);
			_buttonSelect.IsSelected = false;
		}
	}

	public bool IsExtended
	{
		get
		{
			return ((Component)_extendedMode).gameObject.activeSelf;
		}
		set
		{
			((Component)_extendedMode).gameObject.SetActive(value);
			((Component)_normalMode).gameObject.SetActive(!value);
			_buttonSelect.IsSelected = false;
			((Component)this).GetComponent<UIWidget>().width = ((!value) ? _normalMode.width : _extendedMode.width);
		}
	}

	public DamageableEntity Entity => _entity;

	public void Init()
	{
		if (!_initialized)
		{
			UIEventListener.Get(((Component)_buttonSelect).gameObject).onClick = OnButtonSelectClick;
			UIEventListener.Get(((Component)_buttonCancel).gameObject).onClick = OnButtonCancelClick;
			_initialized = true;
		}
	}

	public void SetEntity(DamageableEntity entity, bool combatMode)
	{
		_entity = entity;
		if (_entity != null)
		{
			_textName.text = _entity.GetName();
			_textLevel.text = LocalizeUtil.FormatLevel(_entity.GetLevel());
			_gaugeHp.Set(_entity.GetLife());
		}
		else
		{
			_textName.text = string.Empty;
			_textLevel.text = string.Empty;
			_gaugeHp.Set(null);
		}
		_portraits.SetPortrait(_entity);
		RefreshSelectButton(combatMode);
	}

	public void UpdateWidget(Vector3 posPlayer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (_entity != null)
		{
			Vector3 currentPosition = _entity.GetCurrentPosition();
			if (IsExtended)
			{
				UpdateDistance(posPlayer, currentPosition, _iconArrowForExtended, _textDistanceForExtended);
			}
			else
			{
				UpdateDistance(posPlayer, currentPosition, _iconArrowForNormal, _textDistanceForNormal);
			}
			if (_entity.GaugeChanged)
			{
				_gaugeHp.Set(_entity.GetLife());
				_entity.GaugeChanged = false;
			}
		}
	}

	public void RefreshSelectButton(bool combatMode)
	{
		_buttonSelect.Refresh(combatMode);
	}

	private static void UpdateDistance(Vector3 posPlayer, Vector3 posTarget, GameObject iconArrow, UILabel textDistance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)(Vector3.Distance(posTarget, posPlayer) / 100f);
		float num2 = KMathUtil.CalcYawWithTarget(posTarget, posPlayer);
		iconArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - (num2 + 45f));
		textDistance.text = $"{num}m";
	}

	private void OnButtonSelectClick(GameObject obj)
	{
		if (SelectButtonClicked != null)
		{
			SelectButtonClicked(this);
		}
		_buttonSelect.IsSelected = true;
	}

	private void OnButtonCancelClick(GameObject obj)
	{
		if (CancelButtonClicked != null)
		{
			CancelButtonClicked(this);
		}
	}
}
