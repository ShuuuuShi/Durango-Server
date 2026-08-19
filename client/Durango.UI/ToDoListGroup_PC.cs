using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ToDoListGroup_PC : ToDoListGroupBase
{
	[SerializeField]
	private ToDoListHandleWidget _handle;

	[SerializeField]
	private UIWidget _bottomFrame;

	[SerializeField]
	private UIWidget _verticalBG;

	[SerializeField]
	private TweenerPlayer _verticalAppearTweenerPlayer;

	[SerializeField]
	private TweenerPlayer _notificationTweenerPlayer;

	protected override void Start()
	{
		base.Start();
		ShowVertical(visible: false);
		UIEventListener.Get(_handle.gameObject).onClick = delegate
		{
			ShowVertical(visible: true);
		};
		UIEventListener.Get(_closeBtn).onClick = delegate
		{
			ShowVertical(visible: false);
		};
	}

	protected override void ShowVertical(bool visible)
	{
		if (_scrollView.Nodes.Count == 0)
		{
			_handle.gameObject.SetActive(value: false);
			_vertical.gameObject.SetActive(value: false);
			_detailWidget.gameObject.SetActive(value: false);
			return;
		}
		_handle.gameObject.SetActive(value: true);
		_vertical.gameObject.SetActive(value: true);
		_detailWidget.gameObject.SetActive(value: true);
		UpdateVerticalHeight();
		if (_showVertical != visible)
		{
			_handle.Show(!visible);
			if (visible)
			{
				_verticalAppearTweenerPlayer.ResetToFirst();
				_verticalAppearTweenerPlayer.Play();
			}
			else
			{
				_verticalAppearTweenerPlayer.ResetToLast();
				_verticalAppearTweenerPlayer.Play(forward: false, null);
			}
			ShowIcons(visible);
			_detailWidget.Show(visible);
			base.ShowVertical(visible);
		}
	}

	private void ShowIcons(bool show)
	{
		PlayToDoIconAnimation(_noticeButton, show);
		PlayToDoIconAnimation(_webEventButton, show);
		foreach (GameObject node in _scrollView.Nodes)
		{
			PlayToDoIconAnimation(node, show);
		}
	}

	private void PlayToDoIconAnimation(GameObject toDoIcon, bool showIcon)
	{
		TweenerPlayer component = toDoIcon.GetComponent<TweenerPlayer>();
		if (component != null)
		{
			component.ResetToFirst();
			component.Play(0f, (!showIcon) ? 1 : 0);
		}
	}

	private void UpdateVerticalHeight()
	{
		int num = 0;
		if (_closeBtn.activeSelf)
		{
			UIWidget component = _closeBtn.GetComponent<UIWidget>();
			if (component != null)
			{
				num += component.height;
			}
		}
		if (_noticeButton.activeSelf)
		{
			UIWidget component2 = _noticeButton.GetComponent<UIWidget>();
			if (component2 != null)
			{
				num += component2.height;
			}
		}
		if (_webEventButton.activeSelf)
		{
			UIWidget component3 = _webEventButton.GetComponent<UIWidget>();
			if (component3 != null)
			{
				num += component3.height;
			}
		}
		ListObjectPool nodes = _scrollView.Nodes;
		foreach (GameObject item in nodes)
		{
			UIWidget component4 = item.GetComponent<UIWidget>();
			num += component4.height;
		}
		if (_bottomFrame.gameObject.activeSelf)
		{
			UIWidget component5 = _bottomFrame.GetComponent<UIWidget>();
			if (component5 != null)
			{
				num += component5.height;
			}
		}
		UIWidget parentWidget = _vertical.ParentWidget;
		parentWidget.height = num;
		_vertical.UpdateLayout();
		_scrollView.Panel.UpdateAnchors();
		_verticalBG.UpdateAnchors();
	}

	protected override void UpdateNotificationEffect(bool hasNotification)
	{
		if (hasNotification)
		{
			_notification.SetActive(value: true);
			_notificationTweenerPlayer.ResetToFirst();
			_notificationTweenerPlayer.Play();
		}
		else
		{
			_notification.SetActive(value: false);
		}
	}
}
