using System;
using Durango.Logic.Notification;
using UnityEngine;

namespace Durango.UI.Control;

public class HorizontalTabList : UIWidget
{
	public enum FitStyle
	{
		FitOnWidget,
		FitOnTab,
		FixedSize
	}

	[SerializeField]
	private KScrollView _scrollView;

	private bool _resetPosition = true;

	private bool _isInit;

	public KScrollView ScrollView
	{
		get
		{
			Init();
			return _scrollView;
		}
	}

	public event Action<int> Clicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_scrollView.Nodes.Init(delegate(GameObject obj)
			{
				HorizontalTabWidget component = obj.GetComponent<HorizontalTabWidget>();
				component.NotificationOn(on: false, Durango.Logic.Notification.Type.Normal);
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnTabClicked));
			});
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			ScrollView.ScrollView.DisableSpring();
			_resetPosition = true;
		}
	}

	public void BeginLoad()
	{
		ScrollView.Nodes.BeginLoad();
	}

	public HorizontalTabWidget AddIcon(string icon)
	{
		HorizontalTabWidget component = ScrollView.Nodes.GetNext().GetComponent<HorizontalTabWidget>();
		component.SetIcon(icon);
		return component;
	}

	public HorizontalTabWidget AddText(SyncString text)
	{
		HorizontalTabWidget component = ScrollView.Nodes.GetNext().GetComponent<HorizontalTabWidget>();
		component.SetText(text);
		return component;
	}

	public HorizontalTabWidget AddText(SyncString key, SyncString value)
	{
		HorizontalTabWidget component = ScrollView.Nodes.GetNext().GetComponent<HorizontalTabWidget>();
		component.SetText(key, value);
		return component;
	}

	public void EndLoadByFitOnWidget()
	{
		ScrollView.Nodes.EndLoad();
		UpdateLayout(FitStyle.FitOnWidget);
	}

	public void EndLoadByFit()
	{
		ScrollView.Nodes.EndLoad();
		UpdateLayout(FitStyle.FitOnTab);
	}

	public void EndLoadByFixedSize(int minSize = 0)
	{
		ScrollView.Nodes.EndLoad();
		UpdateLayout(FitStyle.FixedSize, minSize);
	}

	public void UpdateLayout(FitStyle fitStyle, int minFixedSize = 0)
	{
		int num = 0;
		switch (fitStyle)
		{
		case FitStyle.FitOnWidget:
		{
			int count = ScrollView.Nodes.Count;
			num = ((count > 0) ? ((base.width - (count - 1) * ScrollView.Margin) / ScrollView.Nodes.Count) : 0);
			break;
		}
		case FitStyle.FixedSize:
			num = minFixedSize;
			foreach (GameObject node in ScrollView.Nodes)
			{
				HorizontalTabWidget component = node.GetComponent<HorizontalTabWidget>();
				num = Mathf.Max(num, component.GetPreferredSize());
			}
			break;
		}
		foreach (GameObject node2 in ScrollView.Nodes)
		{
			HorizontalTabWidget component2 = node2.GetComponent<HorizontalTabWidget>();
			component2.UpdateLayout(num);
		}
		KScrollView scrollView = ScrollView;
		if (_resetPosition)
		{
			scrollView.ResetPosition();
		}
		else
		{
			scrollView.Reposition();
		}
		_resetPosition = false;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Select(int index)
	{
		KScrollView scrollView = ScrollView;
		ListObjectPool nodes = scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			HorizontalTabWidget component = nodes[i].GetComponent<HorizontalTabWidget>();
			component.Selected = i == index;
		}
		if (index >= 0)
		{
			scrollView.MoveToVisibleArea(index, instant: false);
		}
	}

	public void ClearSelection()
	{
		ListObjectPool nodes = ScrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			HorizontalTabWidget component = nodes[i].GetComponent<HorizontalTabWidget>();
			component.Selected = false;
		}
	}

	public HorizontalTabWidget Get(int index)
	{
		ListObjectPool nodes = ScrollView.Nodes;
		if (index < 0 || index >= nodes.Count)
		{
			return null;
		}
		return nodes[index].GetComponent<HorizontalTabWidget>();
	}

	public void SetNotification(int index, bool on, Durango.Logic.Notification.Type type)
	{
		ListObjectPool nodes = ScrollView.Nodes;
		if (index >= 0 && index < nodes.Count)
		{
			HorizontalTabWidget component = nodes[index].GetComponent<HorizontalTabWidget>();
			component.NotificationOn(on, type);
		}
	}

	private void OnTabClicked()
	{
		GameObject obj = Selectable.Current.gameObject;
		int num = ScrollView.Nodes.IndexOf(obj);
		if (num != -1 && this.Clicked != null)
		{
			this.Clicked(num);
		}
	}
}
