using System.Collections.Generic;
using ItemSystem;
using Messages;
using Shared.Economy;
using Shared.Mailing;

namespace MailData;

public class Mail
{
	public ulong Id;

	public double SentAt;

	public ulong SenderId;

	public MailType MailType;

	public string Text;

	public Dictionary<Currency, int> Money;

	public ItemData[] AttachedItems;

	public bool Accepted;

	public bool IsNew;

	public bool IsValid;

	public Mail()
	{
	}

	public Mail(Messages.Mail msg)
	{
		Set(msg);
	}

	public void Set(Messages.Mail msg)
	{
		Id = msg.Id;
		SentAt = msg.SentAt;
		SenderId = msg.SenderId;
		Text = msg.Text;
		MailType = msg.MailType;
		Money = msg.Money;
		if (msg.AttachedItems != null && msg.AttachedItems.Length > 0)
		{
			AttachedItems = new ItemData[msg.AttachedItems.Length];
			int i = 0;
			for (int num = AttachedItems.Length; i < num; i++)
			{
				AttachedItems[i] = new ItemData(msg.AttachedItems[i]);
			}
		}
		Accepted = msg.Accepted;
		IsValid = true;
	}

	public static int Comparison(Mail t1, Mail t2)
	{
		if (t1.SentAt > t2.SentAt)
		{
			return -1;
		}
		if (t1.SentAt < t2.SentAt)
		{
			return 1;
		}
		return 0;
	}
}
