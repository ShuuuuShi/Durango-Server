using System;
using UnityEngine;

public class ColorSelectorWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private UIWidget _tabWidget;

	[SerializeField]
	private UIWidget _paletteWidget;

	[SerializeField]
	private UIScrollView _paletteScrollView;

	[SerializeField]
	private ListObjectPool _sprite;

	[SerializeField]
	private ListObjectPool _tab;

	[SerializeField]
	private UISprite _selector;

	[SerializeField]
	private Vector2 _spriteSize;

	private Color[][] _colors;

	private string[] _tabs;

	private Action<int, Color> _onSelectColor;

	private Color[] _currentSelectColor;

	private int _currentTab;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_sprite.Init(InitColorSprite);
			_tab.Init(InitTabObject);
		}
	}

	private void Awake()
	{
		Init();
	}

	private void InitColorSprite(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickColorSprite;
	}

	private void OnClickColorSprite(GameObject obj)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		UISprite component = obj.GetComponent<UISprite>();
		ref Color reference = ref _currentSelectColor[_currentTab];
		reference = component.color;
		((Component)_selector).gameObject.SetActive(true);
		((Component)_selector).transform.localPosition = ((Component)component).transform.localPosition;
		if (_onSelectColor != null)
		{
			_onSelectColor(_currentTab, component.color);
		}
	}

	private void InitTabObject(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickTabObject;
	}

	private void OnClickTabObject(GameObject obj)
	{
		int currentTab = -1;
		int i = 0;
		for (int count = _tab.Count; i < count; i++)
		{
			if ((Object)(object)_tab[i] == (Object)(object)obj)
			{
				currentTab = i;
				break;
			}
		}
		_currentTab = currentTab;
		Refresh();
	}

	public void Set(Color[] colors, Color currentSelect, Action<int, Color> onSelectColor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Set(new Color[1][] { colors }, (Color[])(object)new Color[1] { currentSelect }, null, 0, onSelectColor);
	}

	public void Set(Color[][] colors, Color[] currentSelect, string[] tabs, int currentTab, Action<int, Color> onSelectColor)
	{
		Init();
		_colors = colors;
		_currentSelectColor = currentSelect;
		_tabs = tabs;
		_currentTab = currentTab;
		_onSelectColor = onSelectColor;
		Refresh();
	}

	public void SelectColor(Color color)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_currentSelectColor[_currentTab] = color;
		UpdateLayout();
	}

	public void Refresh()
	{
		FillData();
		UpdateLayout();
	}

	public void FillData()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		_sprite.Set((_colors != null && _colors.Length > _currentTab) ? ((_colors[_currentTab] != null) ? _colors[_currentTab].Length : 0) : 0);
		int i = 0;
		for (int count = _sprite.Count; i < count; i++)
		{
			UISprite component = _sprite[i].GetComponent<UISprite>();
			component.color = _colors[_currentTab][i];
		}
		_tab.Set((_tabs != null) ? _tabs.Length : 0);
		int num = 0;
		int j = 0;
		for (int count2 = _tab.Count; j < count2; j++)
		{
			ColorSelectorTab component2 = _tab[j].GetComponent<ColorSelectorTab>();
			component2.Label.UpdateNGUIText();
			NGUIText.regionWidth = UIManager.ScreenWidth;
			NGUIText.finalSize = component2.Label.fontSize;
			int num2 = (int)NGUIText.CalculatePrintedSize(_tabs[j]).x + 30;
			num = Mathf.Max(num2, num);
		}
		_tabWidget.width = num;
		NGUITools.UpdateWidgetCollider(((Component)_tabWidget).gameObject);
		int k = 0;
		for (int count3 = _tab.Count; k < count3; k++)
		{
			ColorSelectorTab component3 = _tab[k].GetComponent<ColorSelectorTab>();
			component3.Select(k == _currentTab);
			component3.Label.text = _tabs[k];
			component3.Widget.width = num;
			component3.UpdateAnchor();
		}
	}

	public void UpdateLayout()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		((Component)_selector).gameObject.SetActive(false);
		int num = Mathf.FloorToInt(((float)_paletteWidget.width - _spriteSize.x * 0.5f) / _spriteSize.x);
		Vector3 localPosition = _sprite.BaseObject.transform.localPosition;
		int i = 0;
		for (int count = _sprite.Count; i < count; i++)
		{
			int num2 = i % num;
			int num3 = i / num;
			Vector3 localPosition2 = localPosition + Vector3.right * _spriteSize.x * (float)num2 + Vector3.down * (float)(num2 % 2) * _spriteSize.y + Vector3.down * (float)num3 * _spriteSize.y * 2f;
			_sprite[i].transform.localPosition = localPosition2;
			if (_currentSelectColor[_currentTab] == _sprite[i].GetComponent<UISprite>().color)
			{
				((Component)_selector).gameObject.SetActive(true);
				((Component)_selector).transform.localPosition = localPosition2;
			}
		}
		_paletteScrollView.ResetPosition();
		UIWidget component = _tab.BaseObject.GetComponent<UIWidget>();
		int num4 = component.height + 10;
		Vector3 localPosition3 = _tab.BaseObject.transform.localPosition;
		int j = 0;
		for (int count2 = _tab.Count; j < count2; j++)
		{
			Transform transform = _tab[j].transform;
			transform.localPosition = localPosition3 + Vector3.down * (float)j * (float)num4;
		}
	}
}
