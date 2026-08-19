using System;
using System.Collections.Generic;
using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ConfigTabWidget : MonoBehaviour
{
	public Action<string> TabClicked;

	[SerializeField]
	private KScrollView _scrollView;

	private int _currentIndex = -1;

	public bool IsInit { get; private set; }

	public string CurrentCategory { get; private set; }

	public void Init()
	{
		if (!IsInit)
		{
			IsInit = true;
			CreateTabs();
			SelectTab(0);
		}
	}

	public void Reposition()
	{
		_scrollView.ScrollView.movement = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
		_scrollView.Reposition();
	}

	private void CreateTabs()
	{
		_scrollView.Nodes.Clear();
		foreach (string item in EnumerateSettings())
		{
			ConfigTabItem configTabItem = _scrollView.Nodes.Add<ConfigTabItem>();
			configTabItem.Set(item);
			configTabItem.Clicked = (Action)Delegate.Combine(configTabItem.Clicked, new Action(OnTabClick));
		}
	}

	private static IEnumerable<string> EnumerateSettings()
	{
		foreach (KeyValuePair<string, List<Setting>> setting in ConfigInstance.Settings)
		{
			List<Setting> value = setting.Value;
			if (setting.Key != "default" && setting.Key != "screen")
			{
				continue;
			}
			bool flag = true;
			for (int i = 0; i < value.Count; i++)
			{
				if (!Setting.IsHidden(value[i]))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				yield return setting.Key;
			}
		}
	}

	private void OnTabClick()
	{
		int num = _scrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			SelectTab(num);
		}
	}

	public void SelectTab(string category)
	{
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			ConfigTabItem component = _scrollView.Nodes[i].GetComponent<ConfigTabItem>();
			if (component != null && component.Category == category)
			{
				SelectTab(i);
				break;
			}
		}
	}

	private void SelectTab(int index)
	{
		if (_currentIndex == index)
		{
			return;
		}
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			ConfigTabItem component = _scrollView.Nodes[i].GetComponent<ConfigTabItem>();
			if (!(component == null))
			{
				component.Selected = i == index;
				if (i == index)
				{
					_currentIndex = index;
					CurrentCategory = component.Category;
				}
			}
		}
		if (TabClicked != null)
		{
			TabClicked(CurrentCategory);
		}
	}
}
