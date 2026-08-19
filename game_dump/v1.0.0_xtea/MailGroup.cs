using System;
using System.Collections.Generic;
using L10N;
using MailData;
using Player;
using Shared.Mailing;
using UnityEngine;

public class MailGroup : UIBase, INewCheckerable
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private KWidgetScrollView _mainScrollView;

	[SerializeField]
	private UIWidget _menuWidget;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private MailMenuWidget _mailMenus;

	[SerializeField]
	private MailControl _mailControl;

	[SerializeField]
	private MailWriteControl _mailWriter;

	private DelayedFunction _mailListUpdateFunc;

	private NewCheckerCountableNode _newChecker = new NewCheckerCountableNode();

	public NewChecker NewChecker => _newChecker;

	private void Awake()
	{
		if (Debug.isDebugBuild)
		{
			_mailMenus.SetMenus(typeof(MailMenu));
		}
		else
		{
			_mailMenus.SetMenus(typeof(MailMenu), MailMenu.Write);
		}
		_mailListUpdateFunc = new DelayedFunction(MailListUpdate);
		_mailControl.Init();
		_mailWriter.Init();
		OnClose();
	}

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		_mailMenus.MenuSelected += OnSelectMenu;
		_mailControl.MailActionClicked = Mail_ActionClicked;
		_mailWriter.RequestSendMail = OnSendMail;
		GameSystem<MailSystem>.Instance().MailListUpdated += OnMailListUpdated;
		ShowMailAlarms();
	}

	private void OnPortraitMode(bool isPortrait)
	{
		((Behaviour)_mainScrollView.ScrollView).enabled = isPortrait;
	}

	protected override bool OnOpen()
	{
		UpdateLayout();
		bool result = base.OnOpen();
		ShowMails(GameSystem<MailSystem>.Instance().Mails);
		if (UIManager.IsPortraitMode)
		{
			_mainScrollView.MoveToNode(1, instant: true);
		}
		return result;
	}

	private void UpdateLayout()
	{
		UIWidget component = ((Component)((Component)_mainScrollView).transform.parent).GetComponent<UIWidget>();
		int height = component.height;
		int width = Mathf.Min(1280 - ((Component)_menuWidget).GetComponent<UIWidget>().width, UIManager.ScreenWidth) - (component.leftAnchor.absolute - component.rightAnchor.absolute + _mainScrollView.Margin);
		int i = 0;
		for (int nodeCount = _mainScrollView.GetNodeCount(); i < nodeCount; i++)
		{
			UIWidget node = _mainScrollView.GetNode(i);
			node.height = height;
		}
		_mainWidget.width = width;
		UIUtility.UpdateAnchors(((Component)this).transform);
		_mainScrollView.UpdateLayout();
	}

	private void OnSendMail(ulong entityId, string text, ulong itemId)
	{
		GameSystem<MailSystem>.Instance().SendMail(entityId, text, itemId);
	}

	private void Mail_ActionClicked(Mail mail, MailAction action)
	{
		switch (action)
		{
		case MailAction.ReplyMail:
		{
			ulong senderId = mail.SenderId;
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(senderId, ShowWriteMail);
			break;
		}
		case MailAction.TakeItems:
		case MailAction.ClanInviteAccept:
			GameSystem<MailSystem>.Instance().AcceptMail(mail);
			break;
		case MailAction.Delete:
			UIManager.MessageBox.Show(T._("메일을 삭제하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<MailSystem>.Instance().DeleteMail(mail);
				}
			});
			break;
		case MailAction.ClanInviteReject:
			GameSystem<MailSystem>.Instance().DeleteMail(mail);
			break;
		}
	}

	private void OnSelectMenu(Enum key)
	{
		switch ((MailMenu)(object)key)
		{
		case MailMenu.List:
			ShowMails(GameSystem<MailSystem>.Instance().Mails);
			break;
		case MailMenu.Write:
			ShowWriteMail();
			break;
		}
	}

	public void UpdateMails(IList<Mail> mails)
	{
		_mailControl.SetMails(mails);
	}

	public void ShowMails(IList<Mail> mails)
	{
		UpdateMails(mails);
		_mailControl.Show();
		_mailWriter.Hide();
		_mailMenus.SelectMenu(MailMenu.List);
	}

	public void ShowWriteMail(PlayerInfo receiver = null)
	{
		_mailControl.Hide();
		_mailWriter.Show(receiver);
		_mailMenus.SelectMenu(MailMenu.Write);
	}

	private void OnMailListUpdated()
	{
		_mailListUpdateFunc.Call((MonoBehaviour)(object)this);
	}

	private void MailListUpdate()
	{
		List<Mail> mails = GameSystem<MailSystem>.Instance().Mails;
		if (base.IsOpen)
		{
			UpdateMails(mails);
		}
		int num = 0;
		int i = 0;
		for (int count = mails.Count; i < count; i++)
		{
			if (!mails[i].Accepted)
			{
				num++;
			}
		}
		_newChecker.Count = num;
	}

	private void ShowMailAlarms()
	{
		List<Mail> mails = GameSystem<MailSystem>.Instance().Mails;
		int i = 0;
		for (int count = mails.Count; i < count; i++)
		{
			Mail mail = mails[i];
			if (mail.IsNew)
			{
				mail.IsNew = false;
				ShowMailAlarm(mail);
			}
		}
	}

	private void ShowMailAlarm(Mail mail)
	{
		if (mail.MailType == MailType.Normal)
		{
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(mail.SenderId, delegate(PlayerInfo info)
			{
				string text = T._("[b]{0}[/b] 님 으로 부터 편지가 도착했습니다", info.Valid ? info.Name : T._("알수없음"));
				UIManager.Popup.Alarm.ShowAlarm(text, "alarm_mail", 60f, Open);
			});
		}
		else if (mail.MailType == MailType.MarketUnregistered)
		{
			string key = T._("장터에 등록한 아이템이 돌아왔습니다");
			key = LocalizeSystem.Get(key);
			UIManager.Popup.Alarm.ShowAlarm(key, "alarm_mail", 60f, Open);
		}
		else if (mail.MailType != MailType.Invitation)
		{
		}
	}
}
