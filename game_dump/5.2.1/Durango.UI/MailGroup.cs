using System.Collections.Generic;
using Durango.Logic.Mail;
using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("Mail")]
public class MailGroup : UIBase, INotificationable
{
	private enum State
	{
		ListView,
		ContentsView
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private MailMenuTabs _mailTabs;

	[SerializeField]
	private MailListView _mailListView;

	[SerializeField]
	private MailContentsView _mailContentsView;

	private readonly List<Mail> _currentMails = new List<Mail>();

	private readonly List<Mail> _filteredMails = new List<Mail>();

	private State _state;

	private readonly Countable _notification = new Countable(Type.Important, ViewType.Count);

	public Notification Notification => _notification;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		_titleWidget.Object.SetTitle(T._("우편"));
		_mailTabs.Selected += SelectMailCategory;
		GameSystem<MailSystem>.Instance().MailListUpdated += OnMailListUpdated;
		GameSystem<MailSystem>.Instance().MailReceived += ShowMailAlarm;
		base.OnOpenSucceed += MailGroup_OnOpenSucceed;
		_mailListView.MailClicked += OnMailNodeClick;
		ShowMailAlarms();
		SetChildrenActive(activated: false);
	}

	protected override bool TryClose()
	{
		if (_state == State.ContentsView)
		{
			SetState(State.ListView);
			return false;
		}
		return base.TryClose();
	}

	public void Open(CategoryType category)
	{
		base.Open();
		SelectMailCategory(category);
	}

	private void SetState(State state)
	{
		_state = state;
		switch (state)
		{
		case State.ListView:
			_mailListView.Show();
			_mailContentsView.Hide();
			_mailListView.Redraw();
			break;
		case State.ContentsView:
			_mailListView.Hide();
			_mailContentsView.Show();
			break;
		}
	}

	private void MailGroup_OnOpenSucceed()
	{
		SetState(State.ListView);
		UpdateMails(GameSystem<MailSystem>.Instance().Mails, reset: true);
	}

	private void UpdateMails(List<Mail> mails, bool reset)
	{
		_currentMails.Clear();
		if (mails != null)
		{
			_currentMails.AddRange(mails);
		}
		_mailTabs.UpdateMailCount();
		UpdateMailView(_mailTabs.SelectedCategory, reset);
		if (_state != State.ContentsView)
		{
			return;
		}
		Mail mail = _mailContentsView.Mail;
		int num = -1;
		if (mail != null)
		{
			for (int i = 0; i < _currentMails.Count; i++)
			{
				if (_currentMails[i].Id == mail.Id)
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			SetState(State.ListView);
		}
		else
		{
			_mailContentsView.Set(_currentMails[num]);
		}
	}

	private void OnMailListUpdated()
	{
		List<Mail> mails = GameSystem<MailSystem>.Instance().Mails;
		int num = 0;
		int i = 0;
		for (int count = mails.Count; i < count; i++)
		{
			if (!mails[i].IsRead)
			{
				num++;
			}
		}
		_notification.Count = num;
		if (base.IsOpened)
		{
			UpdateMails(mails, reset: false);
		}
	}

	private void ShowMailAlarms()
	{
		List<Mail> mails = GameSystem<MailSystem>.Instance().Mails;
		int num = 0;
		Mail mail = null;
		int i = 0;
		for (int count = mails.Count; i < count; i++)
		{
			Mail mail2 = mails[i];
			if (mail2.IsNew)
			{
				num++;
				mail = mail2;
			}
		}
		if (num == 0)
		{
			return;
		}
		if (num > 1)
		{
			UIManager.Alarm.ShowNotify(T._("메일이 {0}개 왔습니다", num), "alarm_mail", major: true, 1.8f, delegate
			{
				if (mail.IsUserMail)
				{
					Open(CategoryType.User);
				}
				else
				{
					Open(CategoryType.All);
				}
			});
		}
		else
		{
			ShowMailAlarm(mail);
		}
	}

	private void ShowMailAlarm(Mail mail)
	{
		mail.IsNew = false;
		mail.GetText(out var titleText, out var _);
		UIManager.Alarm.ShowNotify(titleText, "alarm_mail", major: true, 1.8f, delegate
		{
			Open((!mail.IsUserMail) ? CategoryType.All : CategoryType.User);
		});
	}

	private void UpdateMailView(CategoryType category, bool reset)
	{
		_filteredMails.Clear();
		for (int i = 0; i < _currentMails.Count; i++)
		{
			if (_currentMails[i].IsCategory(category))
			{
				_filteredMails.Add(_currentMails[i]);
			}
		}
		_mailListView.SetMails(_filteredMails, reset);
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_mailTabs.UpdatePortraitMode(base.IsPortrait);
	}

	private void SelectMailCategory(CategoryType category)
	{
		_mailTabs.SelectTab((int)category);
		SetState(State.ListView);
		UpdateMailView(category, reset: true);
	}

	private void OnMailNodeClick(MailNodeWidget node)
	{
		if (node.Data.IsGm())
		{
			SunsetMailPopup sunsetMailPopup = UIManager.Popup.Tooltip<SunsetMailPopup>();
			sunsetMailPopup.Set(node.Data);
			sunsetMailPopup.Show();
		}
		else
		{
			SetState(State.ContentsView);
			_mailContentsView.Set(node.Data);
		}
	}
}
