using System;
using System.Collections.Generic;
using UnityEngine;

public class TabControl : MonoBehaviour
{
	public Action<int, int> TabSelected;

	[SerializeField]
	private ListObjectPool _tabs;

	[SerializeField]
	private int _tabMargin = 10;

	private bool _isInit;

	public int Index
	{
		get
		{
			int result = -1;
			int i = 0;
			for (int count = _tabs.Count; i < count; i++)
			{
				TabItem component = _tabs[i].GetComponent<TabItem>();
				if (component.IsSelect)
				{
					result = i;
					break;
				}
			}
			return result;
		}
	}

	public int Count => _tabs.Count;

	private void InitTab()
	{
		_isInit = true;
		_tabs.Init(Init_TabObject);
	}

	private void Init_TabObject(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClick_TabObject;
	}

	private void OnClick_TabObject(GameObject go)
	{
		int num = -1;
		int prev = -1;
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			TabItem component = _tabs[i].GetComponent<TabItem>();
			if (component.IsSelect)
			{
				prev = i;
			}
			component.Select((Object)(object)go == (Object)(object)_tabs[i]);
			if (component.IsSelect)
			{
				num = i;
			}
		}
		if (num != -1)
		{
			OnSelectTab(prev, num);
		}
	}

	protected virtual void OnSelectTab(int prev, int index)
	{
		if (TabSelected != null)
		{
			TabSelected(prev, index);
		}
	}

	public void SelectTab(int index, bool sendEvent = true)
	{
		TabItem tabItem = null;
		int arg = -1;
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			TabItem component = _tabs[i].GetComponent<TabItem>();
			if (component.IsSelect)
			{
				arg = i;
			}
			component.Select(i == index);
			if (component.IsSelect)
			{
				tabItem = component;
			}
		}
		if (sendEvent && (Object)(object)tabItem != (Object)null && TabSelected != null)
		{
			TabSelected(arg, index);
		}
	}

	public void SetTabs(IList<string> textKeys, int selectedIndex = 0, IList<string> formats = null)
	{
		if (!_isInit)
		{
			InitTab();
		}
		int num = textKeys?.Count ?? 0;
		_tabs.Set(num);
		for (int i = 0; i < num; i++)
		{
			TabItem component = _tabs[i].GetComponent<TabItem>();
			component.LocalizeKey = textKeys[i];
			component.Select(i == selectedIndex);
		}
		SetFormat(formats);
	}

	public void SetFormat(IList<string> format)
	{
		if (!_isInit)
		{
			InitTab();
		}
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			TabItem component = _tabs[i].GetComponent<TabItem>();
			component.Format = ((format != null) ? ((format.Count <= i) ? null : format[i]) : null);
		}
		OnLocalize();
	}

	private void OnLocalize()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int i = 0;
		for (int count = _tabs.Count; i < count; i++)
		{
			TabItem component = _tabs[i].GetComponent<TabItem>();
			component.Localize();
			num = Mathf.Max(new int[1] { component.CalcMinWidth() });
		}
		Vector3 localPosition = _tabs.BaseObject.transform.localPosition;
		int j = 0;
		for (int count2 = _tabs.Count; j < count2; j++)
		{
			TabItem component2 = _tabs[j].GetComponent<TabItem>();
			((Component)component2).transform.localPosition = localPosition + Vector3.right * (float)(num + _tabMargin) * (float)j;
			component2.Widget.width = num;
			NGUITools.UpdateWidgetCollider(((Component)component2).gameObject);
		}
	}
}
