using System;
using System.Collections.Generic;
using MailData;
using UnityEngine;

public class MailControl : MonoBehaviour
{
	public Action DragFinishedOnTop;

	public Action DragFinishedOnBottom;

	public Action<Mail, MailAction> MailActionClicked;

	[SerializeField]
	private GameObject _noData;

	private UIWidget _widget;

	private KScrollView _mailView;

	private bool _updatePosition;

	public UIWidget Widget => (!((Object)(object)_widget != (Object)null)) ? (_widget = ((Component)this).GetComponent<UIWidget>()) : _widget;

	public void Init()
	{
		_mailView = ((Component)this).GetComponent<KScrollView>();
		_mailView.Nodes.Init(InitMailNode);
		_mailView.DragFinishedOnFirst = delegate
		{
			if (DragFinishedOnTop != null)
			{
				DragFinishedOnTop();
			}
		};
		_mailView.DragFinishedOnLast = delegate
		{
			if (DragFinishedOnBottom != null)
			{
				DragFinishedOnBottom();
			}
		};
	}

	private void LateUpdate()
	{
		if (_updatePosition)
		{
			LateRepositionMailWidget();
		}
	}

	private void InitMailNode(GameObject obj)
	{
		MailNodeWidget component = obj.GetComponent<MailNodeWidget>();
		component.Init();
		component.HeightChanged = RepositionMailWidget;
		component.ActionClicked = Mail_ActionClicked;
	}

	private void Mail_ActionClicked(Mail mail, MailAction action)
	{
		if (MailActionClicked != null)
		{
			MailActionClicked(mail, action);
		}
	}

	public void RepositionMailWidget()
	{
		_updatePosition = true;
	}

	private void LateRepositionMailWidget()
	{
		_updatePosition = false;
		_mailView.Reposition();
	}

	public void SetMails(IList<Mail> mails)
	{
		int size = KUtility.GetSize(mails);
		_mailView.Nodes.Set(size);
		for (int i = 0; i < size; i++)
		{
			MailNodeWidget component = _mailView.Nodes[i].GetComponent<MailNodeWidget>();
			component.Set(mails[i]);
		}
		_noData.gameObject.SetActive(size == 0);
		RepositionMailWidget();
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
		Widget.alpha = 0f;
		TweenAlpha.Begin(((Component)this).gameObject, 0.5f, 1f);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
