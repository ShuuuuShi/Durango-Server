using System;
using System.Collections.Generic;
using K1Network;
using MailData;
using Messages;

public class MailSystem : GameSystem<MailSystem>
{
	private readonly List<MailData.Mail> _mails = new List<MailData.Mail>();

	public List<MailData.Mail> Mails => _mails;

	public event Action MailListUpdated;

	private void Awake()
	{
		Connections.Frontend.On<MailPut>(OnMailPut);
		Connections.Frontend.On<Mails>(OnResponse_MailList);
		KSingleton<GameManager>.Instance().Ready += RequestMails;
	}

	public void RequestMails()
	{
		Connections.Frontend.Send(default(GetMails));
	}

	private void OnResponse_MailList(Mails msg, PacketHeader header)
	{
		for (int i = 0; i < _mails.Count; i++)
		{
			_mails[i].IsValid = false;
		}
		for (int j = 0; j < msg._Mails.Length; j++)
		{
			int num = IndexOf(msg._Mails[j].Id);
			if (num == -1)
			{
				_mails.Add(new MailData.Mail(msg._Mails[j]));
			}
			else
			{
				_mails[num].Set(msg._Mails[j]);
			}
		}
		for (int num2 = _mails.Count - 1; num2 >= 0; num2--)
		{
			if (!_mails[num2].IsValid)
			{
				_mails.RemoveAt(num2);
			}
		}
		_mails.Sort(MailData.Mail.Comparison);
		if (this.MailListUpdated != null)
		{
			this.MailListUpdated();
		}
	}

	private void OnMailPut(MailPut msg, PacketHeader header)
	{
		MailData.Mail mail = new MailData.Mail(msg.Mail);
		mail.IsNew = true;
		int num = IndexOf(mail.Id);
		if (num == -1)
		{
			_mails.Add(mail);
		}
		else
		{
			_mails[num] = mail;
		}
		if (this.MailListUpdated != null)
		{
			this.MailListUpdated();
		}
	}

	public void AcceptMail(MailData.Mail mail)
	{
		Connections.Frontend.Send(new AcceptMail
		{
			MailId = mail.Id
		});
	}

	public void DeleteMail(MailData.Mail mail)
	{
		Connections.Frontend.Send(new DeleteMail
		{
			MailId = mail.Id
		});
	}

	public void SendMail(ulong entityId, string text, ulong item)
	{
		Connections.Frontend.Send(new SendMail
		{
			RecipientId = entityId,
			Text = text
		});
	}

	private int IndexOf(ulong id)
	{
		int i = 0;
		for (int count = Mails.Count; i < count; i++)
		{
			if (_mails[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}
}
