using System;
using ItemSystem;
using UnityEngine;

public class ItemIcon2 : MonoBehaviour
{
	public enum Mode
	{
		Enable,
		Disable,
		DisableButSelectable,
		DisableWithSelectionMark
	}

	[Serializable]
	private struct Options
	{
		public Color SelectorColorEnabled;

		public Color SelectorColorDisabled;

		public Color SelectorColorUnsuitabled;

		public Color ContentGaugeColor;

		public Color ReinsHungryColor;

		public Color LevelColorNormal;

		public Color LevelColorWarning;
	}

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UISprite _gauge;

	[SerializeField]
	private UISprite _selector;

	[SerializeField]
	private UISprite _multiSelector;

	[SerializeField]
	private UISprite _selectBg;

	[SerializeField]
	private UISprite _equipMacker;

	[SerializeField]
	private UISprite _summonMacker;

	[SerializeField]
	private UISprite _likeIcon;

	[SerializeField]
	private UISprite _newIcon;

	[SerializeField]
	private UILabel _itemLv;

	[SerializeField]
	private UISprite _durabilityWarning;

	[SerializeField]
	private Options _options;

	private ItemList _parent;

	private UIWidget _widget;

	private bool _selected;

	private Mode _iconMode;

	private bool _isPrevDisable;

	private bool _isShow;

	private Transform _transform;

	private float _refreshGaugeChecktime;

	private float _refreshDurabilityChecktime;

	private TweenPosition _positionTweener;

	private TweenAlpha _alphaTweener;

	public Action<ItemIcon2> OnItemClick;

	public Action<ItemIcon2, bool> OnItemTouch;

	public Action<ItemIcon2, Vector2> OnItemDrag;

	public Action<ItemIcon2> OnItemDragOver;

	public Action<ItemIcon2> OnItemLongTouch;

	private ItemList Parent => (!((Object)(object)_parent == (Object)null)) ? _parent : (_parent = ((Component)this).GetComponentInParent<ItemList>());

	public ItemData Item { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public bool Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			if (_selected)
			{
				Item.NewChecker.IsNew = false;
			}
			RefreshSelector();
		}
	}

	public Mode IconMode
	{
		get
		{
			return _iconMode;
		}
		set
		{
			_iconMode = value;
			RefreshIconMode();
			RefreshSelector();
		}
	}

	public bool LevelWarning
	{
		set
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			_itemLv.color = ((!value) ? _options.LevelColorNormal : _options.LevelColorWarning);
		}
	}

	public Point2 Pos { get; set; }

	public int Width => (Item != null) ? Item.Width : 0;

	public int Height => (Item != null) ? Item.Height : 0;

	public bool Like
	{
		get
		{
			return (Object)(object)_likeIcon != (Object)null && ((Component)_likeIcon).gameObject.activeSelf;
		}
		set
		{
			if ((Object)(object)_likeIcon != (Object)null)
			{
				((Component)_likeIcon).gameObject.SetActive(value);
			}
		}
	}

	public bool New
	{
		set
		{
			if ((Object)(object)_newIcon != (Object)null)
			{
				((Component)_newIcon).gameObject.SetActive(value);
			}
		}
	}

	public bool IsVisible => ((Component)this).gameObject.activeSelf && _isShow;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _transform.localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_transform.localPosition = value;
		}
	}

	private TweenPosition PositionTweener
	{
		get
		{
			if ((Object)(object)_positionTweener == (Object)null)
			{
				_positionTweener = ((Component)this).GetComponent<TweenPosition>();
			}
			return _positionTweener;
		}
	}

	private TweenAlpha AlphaTweener
	{
		get
		{
			if ((Object)(object)_alphaTweener == (Object)null)
			{
				_alphaTweener = ((Component)this).GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		UIWidget widget = Widget;
		widget.onChange = (UIWidget.OnDimensionsChanged)Delegate.Combine(widget.onChange, new UIWidget.OnDimensionsChanged(OnChangeSize));
		Selected = false;
		_transform = ((Component)this).transform;
		_likeIcon.color = UIManager.UIDarkOrange;
	}

	private void Update()
	{
		float time = Time.time;
		if (_refreshGaugeChecktime > 0f && _refreshGaugeChecktime < time)
		{
			RefreshGaugeInfo();
		}
		if (_refreshDurabilityChecktime > 0f && _refreshDurabilityChecktime < time)
		{
			RefreshDurabilityInfo();
		}
	}

	private void OnEnable()
	{
		_isPrevDisable = true;
		_isShow = true;
		_selector.UpdateAnchors();
		Widget.alpha = CalcAlpha();
	}

	public void Show(bool show)
	{
		((Component)this).gameObject.SetActive(show);
		_isPrevDisable = false;
		_isShow = show;
	}

	public void Set(ItemData item)
	{
		Item = item;
		Item.NewChecker.RegisterCallback(OnChangeNew);
		_itemIcon.SetIcon(item);
		if ((Object)(object)_equipMacker != (Object)null)
		{
			((Component)_equipMacker).gameObject.SetActive(Item.IsEquipments);
		}
		if ((Object)(object)_summonMacker != (Object)null)
		{
			((Component)_summonMacker).gameObject.SetActive(Item.Reins != null && KSingleton<AnimalManager>.HasInstance() && Object.op_Implicit((Object)(object)KSingleton<AnimalManager>.Instance().GetAnimal(Item.Id)));
		}
		Like = Item.Like;
		New = Item.NewChecker.IsNew;
		LevelWarning = false;
		_itemLv.text = LocalizeUtil.FormatLevel(Item.Level);
		RefreshDurabilityInfo();
		RefreshGaugeInfo();
	}

	public void OnChangeNew()
	{
		New = Item.NewChecker.IsNew;
	}

	public void ChangeParent(Transform parent)
	{
		((Component)this).transform.parent = parent;
		ParentHasChanged();
	}

	public void SetPosition(Vector3 pos, bool useTween = false)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (useTween)
		{
			if (_isPrevDisable)
			{
				((Behaviour)PositionTweener).enabled = false;
				Position = pos;
				AlphaTweener.from = 0f;
				AlphaTweener.to = CalcAlpha();
				AlphaTweener.tweenFactor = 0f;
				AlphaTweener.PlayForward();
			}
			else
			{
				Widget.alpha = CalcAlpha();
				PositionTweener.from = Position;
				PositionTweener.to = pos;
				PositionTweener.tweenFactor = 0f;
				PositionTweener.PlayForward();
			}
		}
		else
		{
			((Behaviour)PositionTweener).enabled = false;
			((Behaviour)AlphaTweener).enabled = false;
			Position = pos;
			Widget.alpha = CalcAlpha();
		}
		_isPrevDisable = false;
	}

	private void RefreshIconMode()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		switch (IconMode)
		{
		case Mode.Enable:
			_selector.color = _options.SelectorColorEnabled;
			break;
		case Mode.Disable:
			_selector.color = _options.SelectorColorDisabled;
			break;
		case Mode.DisableButSelectable:
			_selector.color = _options.SelectorColorUnsuitabled;
			break;
		case Mode.DisableWithSelectionMark:
			_selector.color = _options.SelectorColorDisabled;
			break;
		}
		Widget.alpha = CalcAlpha();
	}

	private void RefreshSelector()
	{
		if (Selected || IconMode == Mode.DisableWithSelectionMark)
		{
			if (Parent.SelectableCount != 1)
			{
				((Component)_selector).gameObject.SetActive(false);
				((Component)_multiSelector).gameObject.SetActive(_iconMode != Mode.DisableButSelectable);
				((Behaviour)_durabilityWarning).enabled = false;
			}
			else
			{
				if (!((Component)_selector).gameObject.activeSelf)
				{
					UITweener component = ((Component)_selector).GetComponent<UITweener>();
					if ((Object)(object)component != (Object)null)
					{
						component.tweenFactor = 0f;
						component.PlayForward();
					}
				}
				((Component)_selector).gameObject.SetActive(true);
				((Component)_multiSelector).gameObject.SetActive(false);
				((Behaviour)_durabilityWarning).enabled = true;
			}
			((Component)_selectBg).gameObject.SetActive(true);
		}
		else
		{
			((Component)_selector).gameObject.SetActive(false);
			((Component)_selectBg).gameObject.SetActive(false);
			((Component)_multiSelector).gameObject.SetActive(false);
			((Behaviour)_durabilityWarning).enabled = true;
		}
	}

	private float CalcAlpha()
	{
		return (IconMode != 0) ? 0.5f : 1f;
	}

	private void OnChangeSize()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		val.x = (float)Widget.width / 2f;
		val.y = (float)(-Widget.height) / 2f;
		((Component)_itemIcon).transform.localPosition = Vector2.op_Implicit(val);
		int num = Mathf.Min(Widget.width, Widget.height);
		_itemIcon.UITexture.width = num - 37;
		_itemIcon.UITexture.height = num - 37;
		UIUtility.UpdateAnchors(((Component)this).transform);
	}

	private void ParentHasChanged()
	{
		UIWidget[] componentsInChildren = ((Component)this).GetComponentsInChildren<UIWidget>();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			componentsInChildren[i].ParentHasChanged();
		}
	}

	private void OnClick()
	{
		if (OnItemClick != null)
		{
			OnItemClick(this);
		}
	}

	private void OnPress(bool press)
	{
		if (OnItemTouch != null)
		{
			OnItemTouch(this, press);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (OnItemDrag != null)
		{
			OnItemDrag(this, delta);
		}
	}

	private void OnDragOver()
	{
		if (OnItemDragOver != null)
		{
			OnItemDragOver(this);
		}
	}

	private void OnLongPress()
	{
		if (OnItemLongTouch != null)
		{
			OnItemLongTouch(this);
		}
	}

	private void RefreshGaugeInfo()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		float num = -1f;
		Color color = Color.clear;
		float num2 = 0f;
		Gauge gauge = null;
		if (Item.ContentCount > 0)
		{
			float floatAttribute = Item.GetFloatAttribute("capacity");
			if (floatAttribute > 0f)
			{
				num = (float)Item.ContentCount / floatAttribute;
				color = _options.ContentGaugeColor;
			}
		}
		else if (Item.Reins != null)
		{
			gauge = Item.Reins.Hungry;
			color = _options.ReinsHungryColor;
		}
		if (gauge != null)
		{
			float num3 = gauge.Max();
			if (num3 > 0f)
			{
				float num4 = gauge.Min();
				num = gauge.Ratio();
				num2 = (num3 - num4) / Mathf.Abs(gauge.Velocity());
			}
		}
		if ((Object)(object)_gauge != (Object)null)
		{
			if (num < 0f)
			{
				((Component)_gauge).gameObject.SetActive(false);
			}
			else
			{
				((Component)_gauge).gameObject.SetActive(true);
				_gauge.fillAmount = num;
				_gauge.color = color;
			}
		}
		_refreshGaugeChecktime = ((!(num2 > 0f)) ? 0f : (Time.time + num2));
	}

	private void RefreshDurabilityInfo()
	{
		Gauge durability = Item.Durability;
		if (durability == null)
		{
			if ((Object)(object)_durabilityWarning != (Object)null)
			{
				((Component)_durabilityWarning).gameObject.SetActive(false);
			}
			_refreshDurabilityChecktime = 0f;
			return;
		}
		double currentTime = Gauge.CurrentTime;
		float num = durability.RealMax();
		float num2 = durability.Get(currentTime);
		if ((Object)(object)_durabilityWarning != (Object)null)
		{
			((Component)_durabilityWarning).gameObject.SetActive(num2 / num < 0.2f);
		}
		float num3 = (float)(durability.When(num * 0.2f) - currentTime);
		_refreshDurabilityChecktime = Time.time + num3;
	}
}
