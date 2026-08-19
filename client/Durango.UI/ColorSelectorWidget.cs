using System;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class ColorSelectorWidget : MonoBehaviour
{
	[SerializeField]
	public UIWidget Widget;

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
	private Vector2 _spriteSize;

	private Color[][] _colors;

	private string[] _tabs;

	private Action<int, Color> _onSelectColor;

	private Color[] _selectedColorByTab;

	private int _currentTab;

	private bool _isInit;

	public Color32 CurrentColor => _selectedColorByTab[_currentTab];

	public event Action<Color> ColorChanged;

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
		UISprite component = obj.GetComponent<UISprite>();
		ref Color reference = ref _selectedColorByTab[_currentTab];
		reference = component.color;
		if (_onSelectColor != null)
		{
			_onSelectColor(_currentTab, component.color);
		}
		SelectColor(component.color);
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
			if (_tab[i] == obj)
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
		Set(new Color[1][] { colors }, new Color[1] { currentSelect }, null, 0, onSelectColor);
	}

	public void Set(Color[][] colors, Color[] currentSelect, string[] tabs, int currentTab, Action<int, Color> onSelectColor)
	{
		Init();
		_colors = colors;
		_selectedColorByTab = currentSelect;
		_tabs = tabs;
		_currentTab = currentTab;
		_onSelectColor = onSelectColor;
		Refresh();
	}

	public void SelectColor(Color color)
	{
		_selectedColorByTab[_currentTab] = color;
		if (this.ColorChanged != null)
		{
			this.ColorChanged(color);
		}
	}

	public bool TrySelectColor(Color color)
	{
		if (_colors[_currentTab].Contains(color))
		{
			SelectColor(color);
			return true;
		}
		return false;
	}

	public void Refresh()
	{
		FillData();
		UpdateLayout();
	}

	public void FillData()
	{
		Color[] array = ((_colors != null && _colors.Length > _currentTab) ? _colors[_currentTab] : null);
		_sprite.Set(KUtility.GetSize(array));
		int i = 0;
		for (int count = _sprite.Count; i < count; i++)
		{
			UISprite component = _sprite[i].GetComponent<UISprite>();
			component.color = array[i];
		}
		_tab.Set(KUtility.GetSize(_tabs));
		int num = 0;
		int j = 0;
		for (int count2 = _tab.Count; j < count2; j++)
		{
			ColorSelectorTab component2 = _tab[j].GetComponent<ColorSelectorTab>();
			float num2 = component2.Set(_tabs[j]);
			num = Mathf.Max((int)num2, num);
		}
		_tabWidget.width = num;
		int k = 0;
		for (int count3 = _tab.Count; k < count3; k++)
		{
			ColorSelectorTab component3 = _tab[k].GetComponent<ColorSelectorTab>();
			component3.Select(k == _currentTab);
			component3.Widget.width = num;
		}
		UIUtility.UpdateAnchors(_tabWidget.transform);
	}

	public void UpdateLayout(int height = -1)
	{
		if (height > 0)
		{
			_mainWidget.height = height;
			UIUtility.UpdateAnchors(_mainWidget.transform);
		}
		int num = Mathf.FloorToInt(((float)_paletteWidget.width - _spriteSize.x * 0.5f) / _spriteSize.x);
		Vector3 localPosition = _sprite.BaseObject.transform.localPosition;
		int i = 0;
		for (int count = _sprite.Count; i < count; i++)
		{
			int num2 = i % num;
			int num3 = i / num;
			Vector3 localPosition2 = localPosition + Vector3.right * _spriteSize.x * num2 + Vector3.down * (num2 % 2) * _spriteSize.y + Vector3.down * num3 * _spriteSize.y * 2f;
			_sprite[i].transform.localPosition = localPosition2;
		}
		_paletteScrollView.ResetPosition();
		UIWidget component = _tab.BaseObject.GetComponent<UIWidget>();
		int num4 = component.height + 10;
		Vector3 localPosition3 = _tab.BaseObject.transform.localPosition;
		int j = 0;
		for (int count2 = _tab.Count; j < count2; j++)
		{
			Transform transform = _tab[j].transform;
			transform.localPosition = localPosition3 + Vector3.down * j * num4;
		}
	}
}
