using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuTooltip : TooltipBase
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private ListObjectPool _menuItems;

	[SerializeField]
	private int _minWidth;

	private string _title;

	private IList<string> _menus;

	private Action<int> _callback;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_menuItems.Init(OnInitMenuItem);
		}
	}

	private void OnInitMenuItem(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickMenuItem;
		UIEventListener.Get(obj).onDrag = OnDragMenuItem;
	}

	private void OnClickMenuItem(GameObject obj)
	{
		int num = -1;
		int i = 0;
		for (int count = _menuItems.Count; i < count; i++)
		{
			if ((Object)(object)_menuItems[i] == (Object)(object)obj)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			if (_callback != null)
			{
				_callback(num);
			}
			Hide();
		}
	}

	private void OnDragMenuItem(GameObject obj, Vector2 delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		OnDrag(delta);
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		_callback = null;
	}

	public void Set(string title, IList<string> menus, Action<int> callback)
	{
		Init();
		_title = title;
		_menus = menus;
		_callback = callback;
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_menuItems.Set((_menus != null) ? _menus.Count : 0);
		int i = 0;
		for (int count = _menuItems.Count; i < count; i++)
		{
			Transform transform = _menuItems[i].transform;
			UISpriteLabel component = ((Component)transform.FindChild("Label")).GetComponent<UISpriteLabel>();
			GameObject gameObject = ((Component)transform.FindChild("Line")).gameObject;
			component.text = _menus[i];
			gameObject.SetActive(i < count - 1);
		}
	}

	protected override void UpdateLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(((Component)_titleWidget).transform.localPosition.y);
		float num2 = Mathf.Abs(((Component)_titleLabel).transform.localPosition.y);
		float num3 = _titleLabel.Label.printedSize.x;
		int i = 0;
		for (int count = _menuItems.Count; i < count; i++)
		{
			Transform transform = _menuItems[i].transform;
			UILabel component = ((Component)transform.FindChild("Label")).GetComponent<UILabel>();
			num3 = Mathf.Max(num3, component.printedSize.x);
		}
		num3 += num * 2f + num2 * 2f;
		base.Widget.width = Mathf.Max((int)num3, _minWidth);
		int num4 = (int)((float)base.Widget.width - num * 2f);
		_titleWidget.width = num4;
		Vector3 val = ((Component)_titleWidget).transform.localPosition + Vector3.down * (float)_titleWidget.height;
		float num5 = num * 2f + (float)_titleWidget.height;
		int num6 = 0;
		int j = 0;
		for (int count2 = _menuItems.Count; j < count2; j++)
		{
			UIWidget component2 = _menuItems[j].GetComponent<UIWidget>();
			((Component)component2).transform.localPosition = val + Vector3.down * (float)num6;
			component2.width = num4;
			Transform val2 = ((Component)component2).transform.FindChild("Label");
			Vector3 localPosition = val2.localPosition;
			localPosition.x = (float)num4 * 0.5f - 10f;
			val2.localPosition = localPosition;
			num6 += component2.height;
			NGUITools.UpdateWidgetCollider(((Component)component2).gameObject);
		}
		num5 += (float)num6;
		base.Widget.height = (int)num5;
		NGUITools.UpdateWidgetCollider(((Component)base.Widget).gameObject);
		UIUtility.UpdateAnchors(((Component)this).transform);
	}
}
