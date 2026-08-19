using System;
using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ToDoListGroupBase : UIBase
{
	[SerializeField]
	protected GameObject _notification;

	[SerializeField]
	protected RectLayoutComponent _vertical;

	[SerializeField]
	protected KScrollView _scrollView;

	[SerializeField]
	protected GameObject _noticeButton;

	[SerializeField]
	protected GameObject _webEventButton;

	[SerializeField]
	private SoundEventType _todoAddedAudio;

	[SerializeField]
	private SoundEventType _todoRemovedAudio;

	[SerializeField]
	protected ToDoDetailWidget _detailWidget;

	[SerializeField]
	protected GameObject _closeBtn;

	[SerializeField]
	private UISprite _noticeButtonIcon;

	[SerializeField]
	private UILabel _noticeButtonLabel;

	[SerializeField]
	private SpriteData _noticeSprite;

	protected Action<float> _widthRatioChanged;

	private float _addedAudioPlayTime;

	private float _removedAudioPlayTime;

	protected float _lastNodeAlpha = 1f;

	private DelayedFunction _updatedWidthRatioFunc;

	protected bool _showVertical;

	public static int Width { get; private set; }

	public bool IsVerticalVisible => _showVertical && base.Visible;

	protected void Awake()
	{
		Width = _vertical.ParentWidget.width;
		_scrollView.Nodes.Init(delegate(GameObject o)
		{
			ToDoIconNode component = o.GetComponent<ToDoIconNode>();
			component.Clicked = ToDoIconNode_Clicked;
			component.CanClickWhenDisabled = true;
		});
	}

	protected virtual void Start()
	{
		GameSystem<ToDoListSystem>.Instance().Added += ToDoListSystem_Added;
		GameSystem<ToDoListSystem>.Instance().Removed += ToDoListSystem_Removed;
		GameSystem<ToDoListSystem>.Instance().ListUpdated += ToDoListSystem_ListUpdated;
		GameSystem<ToDoListSystem>.Instance().ContextUpdated += ToDoListSystem_ContextUpdated;
		GameSystem<NoticeSystem>.Instance().StateUpdated += RefreshNoticeButton;
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += RefreshNoticeButton;
		GameSystem<WebEventSystem>.Instance().Updated += WebEventSystem_Updated;
		GameSystem<SeasonSystem>.Instance().SeasonUpdated += OnSeasonUpdated;
		SoundManager.PrepareEvent(_todoAddedAudio);
		SoundManager.PrepareEvent(_todoRemovedAudio);
		_updatedWidthRatioFunc = new DelayedFunction(delegate
		{
			if (_widthRatioChanged != null)
			{
				_widthRatioChanged((!IsVerticalVisible) ? 1f : 0f);
			}
		});
		base.VisibleController.Changed += delegate
		{
			if (_widthRatioChanged != null)
			{
				_widthRatioChanged((!IsVerticalVisible) ? 1f : 0f);
			}
		};
		UIEventListener.Get(_noticeButton).onClick = delegate
		{
			GameSystem<NoticeSystem>.Instance().ShowLastest();
		};
		UIEventListener.Get(_webEventButton).onClick = delegate
		{
			GameSystem<WebEventSystem>.Instance().Show();
		};
		_webEventButton.gameObject.SetActive(value: false);
		_vertical.UpdateLayout();
		ToDoListSystem_ListUpdated();
		RefreshNoticeButton();
	}

	protected virtual void LateUpdate()
	{
		UpdateNodeTween();
		UpdateAlpha();
	}

	private void UpdateNodeTween()
	{
		if (!IsVerticalVisible)
		{
			return;
		}
		_lastNodeAlpha = 1f;
		ToDoListSystem toDoListSystem = GameSystem<ToDoListSystem>.Instance();
		int collectionCount = toDoListSystem.CollectionCount;
		ListObjectPool nodes = _scrollView.Nodes;
		bool flag = false;
		for (int i = 0; i < collectionCount; i++)
		{
			ToDoCollection collection = toDoListSystem.GetCollection(i);
			float tweenRatio = collection.TweenRatio;
			ToDoIconNode toDoIconNode = nodes.Get<ToDoIconNode>(i);
			if (tweenRatio < 1f)
			{
				bool flag2 = toDoIconNode.Alpha > 0f;
				toDoIconNode.Alpha = tweenRatio;
				_lastNodeAlpha = tweenRatio;
				if (flag2 != toDoIconNode.Alpha > 0f)
				{
					flag = true;
				}
			}
			else
			{
				toDoIconNode.Alpha = 1f;
			}
		}
		if (flag)
		{
			_scrollView.Reposition();
		}
	}

	private void UpdateAlpha()
	{
		bool flag = IsVerticalVisible || !CanHideVertical();
		float value = base.Rect.alpha + ((!flag) ? (0f - Time.deltaTime) : 1f);
		base.Rect.alpha = Mathf.Clamp01(value);
	}

	protected virtual void ShowVertical(bool visible)
	{
		_showVertical = visible;
	}

	public void AddWidthOnChanged([NotNull] Action<float> func)
	{
		_widthRatioChanged = (Action<float>)Delegate.Combine(_widthRatioChanged, func);
		func((!IsVerticalVisible) ? 1f : 0f);
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_vertical.ParentWidget.bottomAnchor.absolute = ((!base.IsPortrait) ? 18 : 80);
		UIUtility.UpdateAnchors(_vertical.transform);
		_updatedWidthRatioFunc.Call(this);
	}

	private void ToDoListSystem_Added(ToDoCollection collection, bool immediately)
	{
		if (!immediately)
		{
			ShowVertical(visible: true);
			if (collection.IsPlaySound())
			{
				PlayToDoSound(_todoAddedAudio, ref _addedAudioPlayTime);
			}
			if (!collection.IsMessageOnly())
			{
				SelectNodeByCollection(collection);
				UpdateNotification();
			}
		}
	}

	private void ToDoListSystem_Removed(ToDoCollection collection, bool immediately)
	{
		if (collection.IsPlaySound())
		{
			PlayToDoSound(_todoRemovedAudio, ref _removedAudioPlayTime);
		}
		if (!collection.IsMessageOnly() && !immediately)
		{
			ShowVertical(visible: true);
			SelectNodeByCollection(collection);
		}
	}

	private void ToDoListSystem_ListUpdated(int selectIndex = -1)
	{
		ToDoListSystem toDoListSystem = GameSystem<ToDoListSystem>.Instance();
		int collectionCount = toDoListSystem.CollectionCount;
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.Set(collectionCount);
		for (int i = 0; i < collectionCount; i++)
		{
			ToDoCollection collection = toDoListSystem.GetCollection(i);
			ToDoIconNode toDoIconNode = nodes.Get<ToDoIconNode>(i);
			toDoIconNode.Alpha = 1f;
			toDoIconNode.Set(collection);
		}
		SelectNode(selectIndex);
		UpdateNotification();
		_scrollView.ResetPosition();
		TryHideVertical();
	}

	private void ToDoListSystem_ContextUpdated(ToDoCollection collection, ToDoBase todo, bool textOnly)
	{
		if (textOnly)
		{
			if (_detailWidget.Collection == collection)
			{
				_detailWidget.Set(collection);
			}
		}
		else
		{
			ShowVertical(visible: true);
			SelectNodeByCollection(collection);
			_detailWidget.ShowUpdatedFeedBack(todo);
		}
	}

	private void RefreshNoticeButton()
	{
		bool flag = HasNewNotice();
		_noticeButton.gameObject.SetActive(flag);
		_vertical.UpdateLayout();
		if (flag)
		{
			_noticeSprite.Set(_noticeButtonIcon);
			_noticeButtonLabel.text = T._("NOTICE");
		}
		UpdateNotification();
		TryHideVertical();
	}

	private static bool HasNewNotice()
	{
		return GameSystem<NoticeSystem>.Instance().State == NoticeSystem.NoticeState.New;
	}

	private void WebEventSystem_Updated()
	{
		_webEventButton.gameObject.SetActive(GameSystem<WebEventSystem>.Instance().HasEvent);
		_vertical.UpdateLayout();
		TryHideVertical();
	}

	private static void PlayToDoSound(SoundEventType audio, ref float playTime)
	{
		if (Time.time > playTime)
		{
			SoundManager.PlayEvent(audio);
			playTime = Time.time + 1f;
		}
	}

	private void OnSeasonUpdated()
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			ToDoIconNode toDoIconNode = nodes.Get<ToDoIconNode>(i);
			toDoIconNode.RereshSeasonInfo();
		}
	}

	private void TryHideVertical()
	{
		if (CanHideVertical())
		{
			ShowVertical(visible: false);
		}
	}

	private bool CanHideVertical()
	{
		return _scrollView.Nodes.Count == 0 && !_noticeButton.activeSelf && !_webEventButton.activeSelf;
	}

	private void UpdateNotification()
	{
		bool hasNotification = HasNewNotice() || !GameSystem<ToDoListSystem>.Instance().IsAllEmpty();
		UpdateNotificationEffect(hasNotification);
	}

	protected virtual void UpdateNotificationEffect(bool hasNotification)
	{
		_notification.SetActive(hasNotification);
	}

	private void ToDoIconNode_Clicked()
	{
		int index = _scrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		SelectNode(index, toggle: true)?.NotifyClicked();
	}

	private ToDoCollection SelectNode(int index, bool toggle = false)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			ToDoIconNode toDoIconNode = nodes.Get<ToDoIconNode>(i);
			if (toDoIconNode.Selected)
			{
				OnCollectionSelected(toDoIconNode.Collection, selected: false);
				if (toggle && index == i)
				{
					index = -1;
				}
			}
			toDoIconNode.Selected = false;
			toDoIconNode.Disabled = toDoIconNode.Collection != null && toDoIconNode.Collection.IsDisabled;
		}
		if (index < 0 || index >= nodes.Count)
		{
			_detailWidget.Set(null);
			return null;
		}
		ToDoIconNode toDoIconNode2 = nodes.Get<ToDoIconNode>(index);
		toDoIconNode2.Selected = true;
		_detailWidget.Set(toDoIconNode2.Collection);
		OnCollectionSelected(toDoIconNode2.Collection, selected: true);
		return toDoIconNode2.Collection;
	}

	private static void OnCollectionSelected(ToDoCollection collection, bool selected)
	{
		PointTargetController point = UIManager.FindScript<NavigateGroup>().Point;
		string[] navigationKey = collection.GetNavigationKey();
		string[] array = navigationKey;
		foreach (string key in array)
		{
			if (point.Has(key))
			{
				point.Select(key, selected);
			}
		}
	}

	private void SelectNodeByCollection(ToDoCollection collection)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			ToDoIconNode toDoIconNode = nodes.Get<ToDoIconNode>(i);
			if (toDoIconNode.Collection == collection)
			{
				SelectNode(i);
				break;
			}
		}
	}
}
