using System;
using System.Collections.Generic;
using Durango.Logic.Mail;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class MailMenuTabs : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KScrollView _tabsScroll;

	private bool _isInit;

	public CategoryType SelectedCategory { get; private set; }

	public event Action<CategoryType> Selected;

	void IUIInitializable.Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			ListObjectPool nodes = _tabsScroll.Nodes;
			nodes.Init(delegate(GameObject obj)
			{
				Selectable component = obj.GetComponent<Selectable>();
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTab));
			});
			CategoryType[] array = Enums<CategoryType>.All();
			nodes.Set(array.Length);
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].GetComponent<MailMenuTab>().Init(array[i]);
			}
			SelectTab(2);
		}
	}

	public void UpdateMailCount()
	{
		ListObjectPool nodes = _tabsScroll.Nodes;
		CategoryType[] array = Enums<CategoryType>.All();
		List<Mail> mails = GameSystem<MailSystem>.Instance().Mails;
		for (int i = 0; i < nodes.Count; i++)
		{
			MailMenuTab component = nodes[i].GetComponent<MailMenuTab>();
			int num = 0;
			for (int j = 0; j < mails.Count; j++)
			{
				if (mails[j].IsCategory(array[i]))
				{
					num++;
				}
			}
			component.SetCount(num);
		}
	}

	public void SelectTab(int index)
	{
		int count = _tabsScroll.Nodes.Count;
		if (index < 0 || index >= count)
		{
			return;
		}
		for (int i = 0; i < _tabsScroll.Nodes.Count; i++)
		{
			Selectable component = _tabsScroll.Nodes[i].GetComponent<Selectable>();
			if (!(component == null))
			{
				component.Selected = i == index;
				if (i == index)
				{
					SelectedCategory = (CategoryType)index;
				}
			}
		}
	}

	public void UpdatePortraitMode(bool isPortraitMode)
	{
		_tabsScroll.ScrollView.movement = ((!isPortraitMode) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
		for (int i = 0; i < _tabsScroll.Nodes.Count; i++)
		{
			MailMenuTab component = _tabsScroll.Nodes[i].GetComponent<MailMenuTab>();
			if (!(component == null))
			{
				component.UpdateLayout();
			}
		}
		_tabsScroll.UpdateLayout();
	}

	private void OnClickTab()
	{
		int num = _tabsScroll.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1 && this.Selected != null)
		{
			this.Selected((CategoryType)num);
		}
	}
}
