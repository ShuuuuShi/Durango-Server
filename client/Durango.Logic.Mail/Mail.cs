using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Network;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Mailing;

namespace Durango.Logic.Mail;

public class Mail : IComparable<Mail>
{
	public string Id;

	public double SentAt;

	public string SenderId;

	public MailType MailType;

	public string Text;

	public Dictionary<Currency, int> Money;

	public ItemData[] AttachedItems;

	public VoucherInfo[] AttachedVouchers;

	public bool Highlighted;

	public string AcceptedEntityId;

	public bool Accepted;

	public bool IsNew;

	public bool IsValid;

	public bool IsRead;

	public double? ExpiresAt;

	public bool IsUserMail;

	public Mail(Messages.Mail msg, bool isUserMail)
	{
		Set(msg, isUserMail);
	}

	public int CompareTo(Mail other)
	{
		if (other == null)
		{
			return 1;
		}
		return Math.Sign(other.SentAt - SentAt);
	}

	public void Set(Messages.Mail msg, bool isUserMail)
	{
		IsUserMail = isUserMail;
		Id = msg.Id;
		SentAt = msg.SentAt;
		SenderId = msg.SenderId;
		Text = msg.Text;
		MailType = msg.MailType;
		Money = msg.Money;
		IsRead = msg.Read;
		ExpiresAt = msg.ExpiresAt;
		Highlighted = msg.Highlighted;
		if (msg.AttachedItems != null && msg.AttachedItems.Length > 0)
		{
			AttachedItems = new ItemData[msg.AttachedItems.Length];
			int i = 0;
			for (int num = AttachedItems.Length; i < num; i++)
			{
				AttachedItems[i] = new ItemData(msg.AttachedItems[i]);
			}
		}
		AttachedVouchers = msg.AttachedVouchers;
		AcceptedEntityId = msg.AcceptedEntityId;
		Accepted = msg.Accepted;
		IsValid = true;
	}

	public void GetText(out string titleText, out string mainText)
	{
		titleText = null;
		mainText = null;
		switch (MailType)
		{
		case MailType.MarketUnregistered:
			titleText = T._("장터에 등록한 아이템이 돌아왔습니다");
			mainText = Text;
			break;
		case MailType.Invitation:
			titleText = T._("부족: 새로운 부족 가입 요청이 도착했습니다");
			mainText = Text;
			break;
		default:
		{
			string text = Text;
			string[] array = text.Split(new char[1] { '\n' }, 2);
			titleText = array[0];
			if (array.Length > 1)
			{
				mainText = array[1];
			}
			break;
		}
		}
		if (Highlighted)
		{
			titleText = $"[icon=icon_stt_alert] {titleText}";
		}
	}

	public SyncString GetExpiresText()
	{
		if (ExpiresAt.HasValue)
		{
			return new SyncString(delegate(out string text, out float period)
			{
				double num = ExpiresAt.Value - Connections.Frontend.GetPredictedServerTime();
				if (num > 0.0)
				{
					text = string.Format("[icon=icon_skill_time] {0}", TimedeltaFormatter.Format(num, 2, (!(num >= 600.0)) ? "sec" : "min"));
					period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
				}
				else
				{
					text = string.Format("[icon=icon_skill_time] {0}", T._("만료"));
					period = 0f;
				}
			});
		}
		return string.Format("[icon=icon_skill_time] {0}", T._("영구"));
	}

	public bool IsGm()
	{
		return MailType == MailType.Gm;
	}

	public bool IsCategory(CategoryType type)
	{
		if (IsUserMail)
		{
			return type switch
			{
				CategoryType.User => true, 
				CategoryType.GM => IsGm(), 
				_ => false, 
			};
		}
		return type switch
		{
			CategoryType.Shop => MailType == MailType.InAppPurchased, 
			CategoryType.System => MailType == MailType.Event || MailType == MailType.CouponGift || MailType == MailType.Periodic, 
			CategoryType.All => true, 
			_ => false, 
		};
	}
}
