using System;
using Durango.Logic.Notification;
using UnityEngine;

namespace Durango.UI.Control;

public class IconTabList : UIWidget
{
	[SerializeField]
	private KScrollView _scrollView;

	private Vector2? _baseNodeSize;

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
				IconTabWidget component = obj.GetComponent<IconTabWidget>();
				component.SetClickSound(UISound.ClickType.ButtonMedium);
				component.NotifiactionOn(on: false, Durango.Logic.Notification.Type.Normal);
				component.SetDirection(ScrollView.ScrollView.movement);
				Point2 nodeSize = GetNodeSize();
				component.Widget.SetDimensions(nodeSize.x, nodeSize.y);
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnTabClicked));
				component.CanClickWhenDisabled = true;
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

	protected override void OnSizeChanged()
	{
		base.OnSizeChanged();
		UIUtility.UpdateAnchors(ScrollView.transform);
		UIScrollView.Movement movement = ((base.width <= base.height) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
		KScrollView scrollView = ScrollView;
		scrollView.ScrollView.movement = movement;
		Point2 nodeSize = GetNodeSize();
		ListObjectPool nodes = scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			IconTabWidget component = nodes[i].GetComponent<IconTabWidget>();
			component.SetDirection(scrollView.ScrollView.movement);
			component.Widget.SetDimensions(nodeSize.x, nodeSize.y);
		}
		scrollView.UpdateLayout();
	}

	private Point2 GetNodeSize()
	{
		KScrollView scrollView = ScrollView;
		Vector2 viewSize = scrollView.ViewSize;
		Vector2? baseNodeSize = _baseNodeSize;
		if (!baseNodeSize.HasValue)
		{
			_baseNodeSize = scrollView.Nodes.BaseObject.GetComponent<UIWidget>().localSize;
		}
		Point2 result = new Point2(_baseNodeSize.Value);
		switch (scrollView.ScrollView.movement)
		{
		case UIScrollView.Movement.Horizontal:
			result.y = (int)viewSize.y;
			break;
		case UIScrollView.Movement.Vertical:
			result.x = (int)viewSize.x;
			break;
		}
		return result;
	}

	public void BeginLoad()
	{
		ScrollView.Nodes.BeginLoad();
	}

	public IconTabWidget Add(string icon, SyncString text)
	{
		IconTabWidget component = ScrollView.Nodes.GetNext().GetComponent<IconTabWidget>();
		component.Set(icon, text);
		return component;
	}

	public void EndLoad()
	{
		KScrollView scrollView = ScrollView;
		scrollView.Nodes.EndLoad();
		if (_resetPosition)
		{
			scrollView.ResetPosition();
		}
		else
		{
			scrollView.Reposition();
		}
		_resetPosition = false;
	}

	public void Select(int index)
	{
		KScrollView scrollView = ScrollView;
		ListObjectPool nodes = scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].GetComponent<IconTabWidget>().Selected = i == index;
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
			nodes[i].GetComponent<IconTabWidget>().Selected = false;
		}
	}

	public IconTabWidget Get(int index)
	{
		ListObjectPool nodes = ScrollView.Nodes;
		if (index < 0 || index >= nodes.Count)
		{
			return null;
		}
		return nodes[index].GetComponent<IconTabWidget>();
	}

	public void SetNotification(int index, bool on, Durango.Logic.Notification.Type type)
	{
		ListObjectPool nodes = ScrollView.Nodes;
		if (index >= 0 && index < nodes.Count)
		{
			nodes[index].GetComponent<IconTabWidget>().NotifiactionOn(on, type);
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
