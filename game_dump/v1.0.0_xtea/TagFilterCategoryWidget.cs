using System;
using System.Collections.Generic;
using UnityEngine;

public class TagFilterCategoryWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _categoryLabel;

	[SerializeField]
	private GridContainer _grid;

	[SerializeField]
	private GameObject _selectCheckBox;

	[SerializeField]
	private GameObject _selectCheckBoxUpper;

	[SerializeField]
	private GameObject _openerBox;

	[SerializeField]
	private GameObject _openerArrow;

	private bool _isSelectAll;

	private TagFilterSelectorWidget.ItemType _type;

	private bool _isShowChild;

	private Dictionary<TagFilterSelectorWidget.ItemType, bool> _isShowChildFlags;

	private bool _isInit;

	public ListObjectPool Nodes => _grid.Nodes;

	private bool IsSelectAll
	{
		get
		{
			return _isSelectAll;
		}
		set
		{
			_isSelectAll = value;
			_selectCheckBoxUpper.gameObject.SetActive(value);
		}
	}

	public event Action HeightChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_isShowChildFlags = new Dictionary<TagFilterSelectorWidget.ItemType, bool>();
			UIEventListener uIEventListener = UIEventListener.Get(_selectCheckBox);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickSelectAll));
			UIEventListener uIEventListener2 = UIEventListener.Get(_openerBox);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickOpener));
		}
	}

	private void OnClickSelectAll(GameObject obj)
	{
		IsSelectAll = !IsSelectAll;
		bool isSelectAll = IsSelectAll;
		ListObjectPool nodes = Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			TagSearchCard component = nodes[i].GetComponent<TagSearchCard>();
			component.Select = isSelectAll;
		}
	}

	private void OnClickOpener(GameObject obj)
	{
		ShowChild(!_isShowChild, sendEvent: true, saveState: true);
	}

	public void Set(KeyValuePair<string, List<TagFilterSelectorWidget.ItemStruct>> items, TagFilterSelectorWidget.ItemType itemType)
	{
		Init();
		_type = itemType;
		bool hideIcon = itemType == TagFilterSelectorWidget.ItemType.Tag;
		_categoryLabel.text = items.Key;
		ListObjectPool nodes = Nodes;
		nodes.Init(OnInitTagItems);
		nodes.Set(items.Value.Count);
		nodes.BaseObject.GetComponent<TagSearchCard>().HideIcon = hideIcon;
		for (int i = 0; i < nodes.Count; i++)
		{
			TagSearchCard component = nodes[i].GetComponent<TagSearchCard>();
			component.HideIcon = hideIcon;
			component.Set(items.Key, items.Value[i]);
		}
		_grid.Refresh();
		bool show = _isShowChildFlags.Get(itemType, defaultValue: false);
		ShowChild(show, sendEvent: false, saveState: false);
	}

	private void ShowChild(bool show, bool sendEvent, bool saveState)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		_isShowChild = show;
		if (saveState)
		{
			_isShowChildFlags[_type] = show;
		}
		((Component)_grid).gameObject.SetActive(show);
		UpdateWidgetHeight();
		_openerArrow.transform.localEulerAngles = ((!show) ? (Vector3.forward * 180f) : Vector3.zero);
		if (sendEvent && this.HeightChanged != null)
		{
			this.HeightChanged();
		}
	}

	private void UpdateWidgetHeight()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		int num = (int)Mathf.Abs(_categoryLabel.GetPosition(0f, 0f).y - _grid.Widget.GetPosition(0f, 1f).y);
		if (_isShowChild)
		{
			component.height = _categoryLabel.height + _grid.Widget.height + num;
		}
		else
		{
			component.height = _categoryLabel.height + num;
		}
	}

	private void OnInitTagItems(GameObject obj)
	{
		TagSearchCard component = obj.GetComponent<TagSearchCard>();
		component.Clicked = (Action<TagSearchCard>)Delegate.Combine(component.Clicked, new Action<TagSearchCard>(OnClickTag));
	}

	private void OnClickTag(TagSearchCard card)
	{
		card.Select = !card.Select;
		UpdateSelectAllState();
	}

	private void UpdateSelectAllState()
	{
		ListObjectPool nodes = Nodes;
		bool isSelectAll = true;
		for (int i = 0; i < nodes.Count; i++)
		{
			TagSearchCard component = nodes[i].GetComponent<TagSearchCard>();
			if (!component.Select)
			{
				isSelectAll = false;
				break;
			}
		}
		IsSelectAll = isSelectAll;
	}

	public void Reset()
	{
		for (int i = 0; i < Nodes.Count; i++)
		{
			TagSearchCard component = Nodes[i].GetComponent<TagSearchCard>();
			((Component)component).gameObject.SetActive(true);
			component.Widget.alpha = 1f;
		}
		_grid.Refresh();
		UpdateSelectAllState();
	}

	public void FilterByName(string text)
	{
		ListObjectPool nodes = Nodes;
		int num = 0;
		bool flag = string.IsNullOrEmpty(text);
		for (int i = 0; i < nodes.Count; i++)
		{
			TagSearchCard component = nodes[i].GetComponent<TagSearchCard>();
			if (flag || component.Name.Contains(text))
			{
				((Component)component).gameObject.SetActive(true);
				num++;
			}
			else
			{
				((Component)component).gameObject.SetActive(false);
			}
		}
		UIWidget component2 = ((Component)this).GetComponent<UIWidget>();
		if (num > 0)
		{
			((Component)component2).gameObject.SetActive(true);
			_grid.Refresh();
			ShowChild(!flag || _isShowChildFlags.Get(_type, defaultValue: false), sendEvent: false, saveState: false);
		}
		else
		{
			((Component)component2).gameObject.SetActive(false);
		}
	}
}
