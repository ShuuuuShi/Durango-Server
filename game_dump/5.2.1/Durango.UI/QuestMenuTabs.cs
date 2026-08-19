using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.Logic.Quest;
using Durango.UI.Control;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class QuestMenuTabs : NestedPrefabLinker<IconTabList>
{
	private bool _isDirty = true;

	private readonly List<Category> _categories = new List<Category>();

	public event Action<string> TabClicked;

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_isDirty = true;
		}
	}

	protected override void OnLinked()
	{
		base.OnLinked();
		if (Application.isPlaying)
		{
			base.Object.Clicked += OnClickTab;
		}
	}

	private void RefreshTabList()
	{
		if (!_isDirty)
		{
			return;
		}
		_isDirty = false;
		base.Object.BeginLoad();
		_categories.Clear();
		foreach (Category visibleCategory in GameSystem<QuestSystem>.Instance().VisibleCategories)
		{
			_categories.Add(visibleCategory);
			base.Object.Add(null, visibleCategory.Name);
		}
		base.Object.EndLoad();
		UpdateNotification();
	}

	public void SelectTab(string category)
	{
		RefreshTabList();
		int index = -1;
		for (int i = 0; i < _categories.Count; i++)
		{
			if (_categories[i].Key == category)
			{
				index = i;
				break;
			}
		}
		base.Object.Select(index);
	}

	public void UpdateNotification()
	{
		for (int i = 0; i < _categories.Count; i++)
		{
			base.Object.SetNotification(i, _categories[i].HasNotification(), Durango.Logic.Notification.Type.Important);
		}
	}

	public Transform GetQuestMenuTab(string category)
	{
		RefreshTabList();
		for (int i = 0; i < _categories.Count; i++)
		{
			if (_categories[i].Key == category)
			{
				IconTabWidget iconTabWidget = base.Object.Get(i);
				if (iconTabWidget == null)
				{
					return null;
				}
				return iconTabWidget.transform;
			}
		}
		return null;
	}

	private void OnClickTab(int index)
	{
		if (!Selectable.Current.Selected && this.TabClicked != null)
		{
			string obj = ((index >= 0 && index < _categories.Count) ? _categories[index].Key : null);
			this.TabClicked(obj);
		}
	}
}
