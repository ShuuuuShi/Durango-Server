using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using UnityEngine;

namespace Durango.UI;

public class ItemIconWidget : MonoBehaviour
{
	public enum Mode
	{
		Enabled,
		Disabled,
		DisabledWithSelectionMark
	}

	public enum MultiIconMode
	{
		Check,
		Index
	}

	[Serializable]
	private struct Options
	{
		public Color SelectorColorEnabled;

		public Color SelectorColorDisabled;

		public Color LevelColorNormal;

		public Color LevelColorWarning;
	}

	[Serializable]
	private struct MultiSelector
	{
		public GameObject Parent;

		public UISprite CheckSprite;

		public UILabel IndexLabel;

		public UISprite IndexBg;
	}

	public static readonly Color DurabilityWarningColor = PresetColor.UIMoreLightGray;

	public static readonly Color DurabilityDestroyedColor = PresetColor.UILightRed;

	public Action<ItemIconWidget> OnItemClick;

	public Action<ItemIconWidget> OnItemRightClick;

	public Action<ItemIconWidget, bool> OnItemTouch;

	public Action<ItemIconWidget, Vector2> OnItemDrag;

	public Action<ItemIconWidget> OnItemDragOver;

	public Action<ItemIconWidget> OnItemLongTouch;

	public Action<ItemIconWidget, float> OnItemScroll;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private ItemGradeViewer _itemGradeViewer;

	[SerializeField]
	private ItemModifiedCountViewer _itemModifiedCountViewer;

	[SerializeField]
	private UISprite _gauge;

	[SerializeField]
	private UISprite _selector;

	[SerializeField]
	private MultiSelector _multiSelector;

	[SerializeField]
	private UISprite _selectBg;

	[SerializeField]
	private UISprite _disableBg;

	[SerializeField]
	private UISprite _equipMacker;

	[SerializeField]
	private UISprite _lockedIcon;

	[SerializeField]
	private UISprite _newIcon;

	[SerializeField]
	private UILabel _itemLv;

	[SerializeField]
	private UISprite _durabilityWarning;

	[SerializeField]
	private GameObject _warpLockObject;

	[SerializeField]
	private Options _options;

	private ItemList _parent;

	private UIWidget[] _bottomLeftWidgets;

	private UIWidget _widget;

	private bool _selected;

	private Mode _iconMode;

	private MultiIconMode _multiIconMode;

	private bool _isEnabled;

	private float _refreshDurabilityChecktime;

	private TweenPosition _positionTweener;

	private TweenAlpha _alphaTweener;

	private int _selectorUpdateFrame;

	public bool Valid { get; set; }

	private ItemList Parent => (!(_parent == null)) ? _parent : (_parent = GetComponentInParent<ItemList>());

	public ItemData Item { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
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
				Item.IsNew = false;
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

	public MultiIconMode MultiMode
	{
		get
		{
			return _multiIconMode;
		}
		set
		{
			if (_multiIconMode != value)
			{
				_multiIconMode = value;
				RefreshSelector();
			}
		}
	}

	public bool LevelWarning
	{
		set
		{
			_itemLv.color = ((!value) ? _options.LevelColorNormal : _options.LevelColorWarning);
		}
	}

	public Point2 Pos { get; set; }

	public int Width => (Item != null) ? Item.Width : 0;

	public int Height => (Item != null) ? Item.Height : 0;

	public bool Locked
	{
		get
		{
			return _lockedIcon.gameObject.activeSelf;
		}
		set
		{
			bool activeSelf = _lockedIcon.gameObject.activeSelf;
			_lockedIcon.gameObject.SetActive(value);
			if (activeSelf != _lockedIcon.gameObject.activeSelf)
			{
				OnUpdateBottomLeftWidgets();
			}
		}
	}

	public bool IsSelectable
	{
		get
		{
			Mode iconMode = _iconMode;
			if (iconMode == Mode.Disabled || iconMode == Mode.DisabledWithSelectionMark)
			{
				return false;
			}
			return true;
		}
	}

	private TweenPosition PositionTweener
	{
		get
		{
			if (_positionTweener == null)
			{
				_positionTweener = GetComponent<TweenPosition>();
			}
			return _positionTweener;
		}
	}

	private TweenAlpha AlphaTweener
	{
		get
		{
			if (_alphaTweener == null)
			{
				_alphaTweener = GetComponent<TweenAlpha>();
			}
			return _alphaTweener;
		}
	}

	private void Awake()
	{
		Selected = false;
		_bottomLeftWidgets = new UIWidget[2] { _durabilityWarning, _lockedIcon };
		OnUpdateBottomLeftWidgets();
	}

	private void OnEnable()
	{
		_selector.UpdateAnchors();
		Widget.alpha = 1f;
	}

	private void OnDisable()
	{
		_isEnabled = false;
	}

	public void Set(ItemData item)
	{
		Item = item;
		_itemIcon.SetIcon(item);
		_itemGradeViewer.Set(item);
		_itemModifiedCountViewer.Set(item.ModifiedCount);
		if (_equipMacker != null)
		{
			_equipMacker.gameObject.SetActive(Item.IsEquipments);
		}
		Locked = Item.Locked;
		_newIcon.gameObject.SetActive(Item.IsNew);
		LevelWarning = false;
		_itemLv.text = LocalizeUtil.FormatLevel(Item.Level);
		_warpLockObject.gameObject.SetActive(item.Unstable);
		RefreshDurabilityInfo();
		RefreshGaugeInfo();
	}

	public void SetWidgetSize(int width, int height, float scale)
	{
		int width2 = Widget.width;
		int height2 = Widget.height;
		Widget.SetDimensions(width, height);
		Widget.transform.localScale = Vector3.one * scale;
		if (width2 != Widget.width || height2 != Widget.height)
		{
			OnChangeSize();
		}
	}

	public void SetPosition(Vector3 pos, bool instant)
	{
		if (instant)
		{
			PositionTweener.enabled = false;
			AlphaTweener.enabled = false;
			base.transform.localPosition = pos;
			Widget.alpha = 1f;
		}
		else if (_isEnabled)
		{
			Widget.alpha = 1f;
			PositionTweener.from = base.transform.localPosition;
			PositionTweener.to = pos;
			PositionTweener.tweenFactor = 0f;
			PositionTweener.PlayForward();
		}
		else
		{
			PositionTweener.enabled = false;
			base.transform.localPosition = pos;
			AlphaTweener.from = 0f;
			AlphaTweener.to = 1f;
			AlphaTweener.tweenFactor = 0f;
			AlphaTweener.PlayForward();
		}
		_isEnabled = true;
	}

	public void RefreshSelector()
	{
		if (Selected || _iconMode == Mode.DisabledWithSelectionMark)
		{
			if (_iconMode == Mode.Enabled && Parent.SelectableCount == 1)
			{
				_multiSelector.Parent.gameObject.SetActive(value: false);
				bool activeSelf = _selector.gameObject.activeSelf;
				_selector.gameObject.SetActive(value: true);
				if (_selectorUpdateFrame != Time.frameCount && !activeSelf)
				{
					UITweener component = _selector.GetComponent<UITweener>();
					if (component != null)
					{
						component.tweenFactor = 0f;
						component.PlayForward();
					}
				}
			}
			else
			{
				_selector.gameObject.SetActive(value: false);
				bool activeSelf2 = _multiSelector.Parent.activeSelf;
				_multiSelector.Parent.gameObject.SetActive(value: true);
				switch ((_iconMode == Mode.Enabled) ? _multiIconMode : MultiIconMode.Check)
				{
				case MultiIconMode.Check:
					_multiSelector.CheckSprite.gameObject.SetActive(value: true);
					_multiSelector.IndexLabel.gameObject.SetActive(value: false);
					_multiSelector.IndexBg.gameObject.SetActive(value: false);
					break;
				case MultiIconMode.Index:
				{
					_multiSelector.CheckSprite.gameObject.SetActive(value: false);
					_multiSelector.IndexLabel.gameObject.SetActive(value: true);
					_multiSelector.IndexBg.gameObject.SetActive(value: true);
					int num = Parent.SelectedIndexOf(Item);
					_multiSelector.IndexLabel.text = (num + 1).ToString();
					break;
				}
				}
				if (_selectorUpdateFrame != Time.frameCount && !activeSelf2)
				{
					TweenerPlayer component2 = _multiSelector.Parent.GetComponent<TweenerPlayer>();
					if (component2 != null)
					{
						component2.Play();
					}
				}
			}
			_selectBg.gameObject.SetActive(value: true);
		}
		else
		{
			_selector.gameObject.SetActive(value: false);
			_selectBg.gameObject.SetActive(value: false);
			_multiSelector.Parent.SetActive(value: false);
		}
		_selectorUpdateFrame = Time.frameCount;
	}

	public void UpdateTick()
	{
		float time = Time.time;
		if (_refreshDurabilityChecktime > 0f && _refreshDurabilityChecktime < time)
		{
			RefreshDurabilityInfo();
		}
	}

	private void RefreshIconMode()
	{
		switch (_iconMode)
		{
		case Mode.Enabled:
			_disableBg.gameObject.SetActive(value: false);
			_selector.color = _options.SelectorColorEnabled;
			break;
		case Mode.Disabled:
		case Mode.DisabledWithSelectionMark:
			_disableBg.gameObject.SetActive(value: true);
			_selector.color = _options.SelectorColorDisabled;
			break;
		}
	}

	private void OnChangeSize()
	{
		Vector2 vector = Widget.localCenter;
		_itemIcon.transform.localPosition = vector;
		int num = Mathf.Min(Widget.width, Widget.height);
		_itemIcon.width = num - 37;
		_itemIcon.height = num - 37;
		UIUtility.UpdateAnchors(base.transform);
		OnUpdateBottomLeftWidgets();
	}

	private void OnClick()
	{
		if (UICamera.GetKey(KeyCode.LeftControl) || UICamera.GetKey(KeyCode.RightControl))
		{
			if (OnItemLongTouch != null)
			{
				OnItemLongTouch(this);
			}
		}
		else if (OnItemClick != null)
		{
			OnItemClick(this);
		}
	}

	private void OnRightClick()
	{
		if (OnItemRightClick != null)
		{
			OnItemRightClick(this);
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

	private void OnScroll(float delta)
	{
		if (OnItemScroll != null)
		{
			OnItemScroll(this, delta);
		}
	}

	private void OnHover(bool hover)
	{
		ButtonInfoTooltip buttonInfoTooltip = UIManager.Popup.Tooltip<ButtonInfoTooltip>();
		if (!(buttonInfoTooltip == null))
		{
			buttonInfoTooltip.Direction = TooltipBase.TooltipDirection.Vertical;
			buttonInfoTooltip.Sign = -1;
			if (hover)
			{
				buttonInfoTooltip.Set(Item.Name);
				buttonInfoTooltip.Show(base.gameObject, Vector3.zero);
			}
			else
			{
				buttonInfoTooltip.Hide();
			}
		}
	}

	private void RefreshGaugeInfo()
	{
		float num = -1f;
		if (Item.ContentCount > 0)
		{
			float floatAttribute = Item.GetFloatAttribute("capacity");
			if (floatAttribute > 0f)
			{
				num = (float)Item.ContentCount / floatAttribute;
			}
		}
		if (_gauge != null)
		{
			if (num < 0f)
			{
				_gauge.gameObject.SetActive(value: false);
				return;
			}
			_gauge.gameObject.SetActive(value: true);
			_gauge.fillAmount = num;
		}
	}

	private void RefreshDurabilityInfo()
	{
		bool activeSelf = _durabilityWarning.gameObject.activeSelf;
		if (Item.GetDurabilityState(out var state, out var refreshPeriod))
		{
			_durabilityWarning.gameObject.SetActive(state != DurabilityState.Good);
			_durabilityWarning.color = ((state != DurabilityState.Destroyed) ? DurabilityWarningColor : DurabilityDestroyedColor);
			_refreshDurabilityChecktime = Time.time + refreshPeriod;
		}
		else
		{
			_durabilityWarning.gameObject.SetActive(value: false);
			_refreshDurabilityChecktime = 0f;
		}
		if (activeSelf != _durabilityWarning.gameObject.activeSelf)
		{
			OnUpdateBottomLeftWidgets();
		}
	}

	private void OnUpdateBottomLeftWidgets()
	{
		Vector3 pos = Widget.localCorners[0] + new Vector3(5f, 5f);
		int i = 0;
		for (int size = KUtility.GetSize(_bottomLeftWidgets); i < size; i++)
		{
			UIWidget uIWidget = _bottomLeftWidgets[i];
			if (uIWidget.gameObject.activeSelf)
			{
				uIWidget.SetPosition(pos, 0f, 0f);
				pos.y += uIWidget.height + 5;
			}
		}
	}
}
