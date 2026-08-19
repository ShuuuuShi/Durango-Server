using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class RecentlyVisit : MonoBehaviour
{
	[SerializeField]
	private GameObject _unfoldingButton;

	[SerializeField]
	private GameObject _title;

	[SerializeField]
	private GameObject _contents;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private UIWidget _selector;

	[SerializeField]
	private GameObject _noRecentlyVisit;

	private RecentlyVisitItem _currentSelected;

	private bool _isFolded;

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_unfoldingButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_isFolded = false;
			Set();
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_title);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_isFolded = true;
			Set();
		});
		_scrollView.Nodes.Init(delegate(GameObject go)
		{
			UIEventListener.Get(go).onClick = delegate(GameObject o)
			{
				UIWidget component = o.GetComponent<UIWidget>();
				RecentlyVisitItem component2 = o.GetComponent<RecentlyVisitItem>();
				if (_currentSelected == component2)
				{
					_currentSelected = null;
					_selector.gameObject.SetActive(value: false);
					UIUtility.FindComponentInParent<WorldRoutesViewer>(base.gameObject).SelectExploreArea(null);
				}
				else
				{
					_currentSelected = component2;
					_selector.gameObject.SetActive(value: true);
					_selector.SetPosition(component.GetPosition(0.5f, 0.5f), 0.5f, 0.5f);
					UIUtility.FindComponentInParent<WorldRoutesViewer>(base.gameObject).SelectExploreArea(component2.Template);
				}
			};
		});
	}

	private void OnEnable()
	{
		_isFolded = true;
		_currentSelected = null;
		_selector.gameObject.SetActive(value: false);
	}

	public void Set()
	{
		_unfoldingButton.SetActive(_isFolded);
		_contents.SetActive(!_isFolded);
		if (!_isFolded)
		{
			SetContents();
		}
	}

	private void SetContents()
	{
		LinkedList<string> recentlyVisits = GameSystem<ExploreSystem>.Instance().RecentlyVisits;
		if (KUtility.GetSize(recentlyVisits) == 0)
		{
			_noRecentlyVisit.SetActive(value: true);
			return;
		}
		_noRecentlyVisit.SetActive(value: false);
		_scrollView.Nodes.BeginLoad();
		foreach (string item in recentlyVisits)
		{
			RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Instance.Get(item);
			if (regionTemplate != null)
			{
				RecentlyVisitItem component = _scrollView.Nodes.GetNext().GetComponent<RecentlyVisitItem>();
				component.Set(regionTemplate);
			}
		}
		_scrollView.Nodes.EndLoad();
		_scrollView.Reposition();
	}
}
