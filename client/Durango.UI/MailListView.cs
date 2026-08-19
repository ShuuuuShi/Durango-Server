using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Mail;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MailListView : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private MailBottomBar _bottomBar;

	private bool _isWaiting;

	private IList<Mail> _mails;

	private KInfiniteScrollView.View<Mail, MailNodeWidget> _view;

	public event Action<MailNodeWidget> MailClicked;

	public void Init()
	{
		_bottomBar.Init();
		MailBottomBar bottomBar = _bottomBar;
		bottomBar.AcceptAllClicked = (Action)Delegate.Combine(bottomBar.AcceptAllClicked, new Action(OnAcceptAll));
		_view = _scrollView.Initialize(delegate(MailNodeWidget node, Mail mail)
		{
			node.Set(mail);
		}, delegate(MailNodeWidget node)
		{
			node.Init();
			node.Clicked = (Action<MailNodeWidget>)Delegate.Combine(node.Clicked, new Action<MailNodeWidget>(OnMailClick));
		});
	}

	private void OnDisable()
	{
		_isWaiting = false;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetMails(IList<Mail> mails, bool reset)
	{
		_mails = mails;
		_view.SetList(_mails);
		if (reset)
		{
			_scrollView.ResetPosition();
		}
		else
		{
			_scrollView.Reposition();
		}
		int size = KUtility.GetSize(mails);
		_noData.gameObject.SetActive(size == 0);
	}

	public void Redraw()
	{
		_view.Redraw();
	}

	private void OnAcceptAll()
	{
		if (_isWaiting || _mails.Count == 0)
		{
			return;
		}
		UIManager.MessageBox.Show(T._("정말 우편함의 모든 우편을 받으시겠습니까?"), delegate(bool isOk)
		{
			if (isOk)
			{
				_isWaiting = true;
				GameSystem<MailSystem>.Instance().AcceptMails(_mails.Where((Mail m) => !m.Accepted && (!m.Highlighted || m.IsRead)).ToList(), delegate
				{
					_isWaiting = false;
				});
			}
		});
	}

	private void OnMailClick(MailNodeWidget node)
	{
		if (this.MailClicked != null)
		{
			this.MailClicked(node);
		}
	}
}
