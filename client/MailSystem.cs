using System;
using System.Collections.Generic;
using Durango.Logic.Mail;
using Durango.Network;
using JetBrains.Annotations;
using Messages;

public class MailSystem : GameSystem<MailSystem>
{
	private readonly List<Durango.Logic.Mail.Mail> _mails = new List<Durango.Logic.Mail.Mail>();

	private bool _isDirtyMails;

	[NotNull]
	public List<Durango.Logic.Mail.Mail> Mails
	{
		get
		{
			if (_isDirtyMails)
			{
				_isDirtyMails = false;
				_mails.Sort();
			}
			return _mails;
		}
	}

	public event Action MailListUpdated;

	public event Action<Durango.Logic.Mail.Mail> MailReceived;

	private void Awake()
	{
		Connections.Frontend.On<MailPut>(OnMailPut);
		Connections.Frontend.On<UserMailPut>(OnUserMailPut);
		Connections.Frontend.On<Mails>(OnMails);
	}

	private void OnMails(Mails msg, PacketHeader header)
	{
		for (int i = 0; i < _mails.Count; i++)
		{
			_mails[i].IsValid = false;
		}
		AddMails(msg._Mails, isUserMail: false);
		AddMails(msg.UserMails, isUserMail: true);
		for (int num = _mails.Count - 1; num >= 0; num--)
		{
			if (!_mails[num].IsValid)
			{
				_isDirtyMails = true;
				_mails.RemoveAt(num);
			}
		}
		if (this.MailListUpdated != null)
		{
			this.MailListUpdated();
		}
	}

	private void AddMails(IList<Messages.Mail> mails, bool isUserMail)
	{
		int i = 0;
		for (int size = KUtility.GetSize(mails); i < size; i++)
		{
			int num = IndexOf(_mails, mails[i].Id);
			if (num == -1)
			{
				_isDirtyMails = true;
				_mails.Add(new Durango.Logic.Mail.Mail(mails[i], isUserMail));
			}
			else
			{
				_mails[num].Set(mails[i], isUserMail);
			}
		}
	}

	private void OnMailPut(MailPut msg, PacketHeader header)
	{
		MailPut(msg.Mail, isUserMail: false);
	}

	private void OnUserMailPut(UserMailPut msg, PacketHeader header)
	{
		MailPut(msg.Mail, isUserMail: true);
	}

	private void MailPut(Messages.Mail msg, bool isUserMail)
	{
		int num = IndexOf(_mails, msg.Id);
		Durango.Logic.Mail.Mail mail;
		if (num == -1)
		{
			_isDirtyMails = true;
			mail = new Durango.Logic.Mail.Mail(msg, isUserMail);
			_mails.Add(mail);
		}
		else
		{
			mail = _mails[num];
			mail.Set(msg, isUserMail);
			_mails[num] = mail;
		}
		mail.IsNew = true;
		if (this.MailListUpdated != null)
		{
			this.MailListUpdated();
		}
		if (this.MailReceived != null)
		{
			this.MailReceived(mail);
		}
	}

	public void AcceptMails(List<Durango.Logic.Mail.Mail> mails, Action<bool> onResult)
	{
		if (KUtility.GetSize(mails) == 0)
		{
			return;
		}
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = ((!mails[0].IsUserMail) ? Connections.Frontend.Send(new AcceptMails
		{
			MailIds = ExtractMailIds(mails)
		}) : Connections.Frontend.Send(new AcceptUserMails
		{
			MailIds = ExtractMailIds(mails)
		}));
		if (onResult != null)
		{
			replyMessageHandlerRegistrar.All(delegate(Packet packet)
			{
				onResult(Packet.IsSuccess(packet));
			});
		}
	}

	public void AcceptUserMails(List<Durango.Logic.Mail.Mail> mails, Action<bool> onResult)
	{
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new AcceptUserMails
		{
			MailIds = ExtractMailIds(mails)
		});
		if (onResult != null)
		{
			replyMessageHandlerRegistrar.All(delegate(Packet packet)
			{
				onResult(Packet.IsSuccess(packet));
			});
		}
	}

	public void DeleteMails(List<Durango.Logic.Mail.Mail> mails, Action<bool> onResult)
	{
		if (KUtility.GetSize(mails) == 0)
		{
			return;
		}
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = ((!mails[0].IsUserMail) ? Connections.Frontend.Send(new DeleteMails
		{
			MailIds = ExtractMailIds(mails)
		}) : Connections.Frontend.Send(new DeleteUserMails
		{
			MailIds = ExtractMailIds(mails)
		}));
		if (onResult != null)
		{
			replyMessageHandlerRegistrar.All(delegate(Packet packet)
			{
				onResult(Packet.IsSuccess(packet));
			});
		}
	}

	public void MarkMailsAsRead(Durango.Logic.Mail.Mail mail)
	{
		int num = IndexOf(_mails, mail.Id);
		if (num != -1 && !_mails[num].IsRead)
		{
			_mails[num].IsRead = true;
			if (mail.IsUserMail)
			{
				Connections.Frontend.Send(new MarkUserMailsAsRead
				{
					MailIds = new string[1] { mail.Id }
				});
			}
			else
			{
				Connections.Frontend.Send(new MarkMailsAsRead
				{
					MailIds = new string[1] { mail.Id }
				});
			}
		}
	}

	public void SendMail(string entityId, string text)
	{
		Connections.Frontend.Send(new SendMail
		{
			RecipientId = entityId,
			Text = text
		});
	}

	private int IndexOf(IList<Durango.Logic.Mail.Mail> mails, string id)
	{
		int i = 0;
		for (int size = KUtility.GetSize(mails); i < size; i++)
		{
			if (mails[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private string[] ExtractMailIds(List<Durango.Logic.Mail.Mail> mails)
	{
		string[] array = new string[mails.Count];
		int i = 0;
		for (int count = mails.Count; i < count; i++)
		{
			array[i] = mails[i].Id;
		}
		return array;
	}
}
