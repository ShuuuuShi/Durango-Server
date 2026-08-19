using System.Collections.Generic;
using MsgPack;
using Shared.Economy;
using Shared.Mailing;

namespace Messages;

public struct Mail
{
	public string Id;

	public double SentAt;

	public string SenderId;

	public MailType MailType;

	public string Text;

	public Dictionary<Currency, int> Money;

	public Item[] AttachedItems;

	public VoucherInfo[] AttachedVouchers;

	public bool Accepted;

	public bool Read;

	public string ClanId;

	public double? ExpiresAt;

	public string AcceptedEntityId;

	public bool Highlighted;

	public static void Pack(Packer packer, Mail val, bool hint = false)
	{
		packer.PackArrayHeader(14);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.SentAt);
		if (val.SenderId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SenderId);
		}
		packer.Pack((int)val.MailType);
		packer.PackString(val.Text);
		if (val.Money == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Money.Count);
			foreach (KeyValuePair<Currency, int> item in val.Money)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		if (val.AttachedItems == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.AttachedItems.Length);
			for (int i = 0; i < val.AttachedItems.Length; i++)
			{
				Item.Pack(packer, val.AttachedItems[i]);
			}
		}
		if (val.AttachedVouchers == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.AttachedVouchers.Length);
			for (int j = 0; j < val.AttachedVouchers.Length; j++)
			{
				VoucherInfo.Pack(packer, val.AttachedVouchers[j]);
			}
		}
		packer.Pack(val.Accepted);
		packer.Pack(val.Read);
		if (val.ClanId == null)
		{
			packer.PackNull();
		}
		else if (val.ClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanId);
		}
		if (!val.ExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ExpiresAt.Value);
		}
		if (val.AcceptedEntityId == null)
		{
			packer.PackNull();
		}
		else if (val.AcceptedEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AcceptedEntityId);
		}
		packer.Pack(val.Highlighted);
	}

	public static Mail Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Mail result = default(Mail);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SentAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.SenderId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 7 < num)
		{
			result.MailType = MailType.Invalid;
		}
		else
		{
			result.MailType = (MailType)num;
		}
		unpacker.Read();
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Money = new Dictionary<Currency, int>(num2, default(CurrencyComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			Currency key = ((num3 >= 0 && 7 >= num3) ? ((Currency)num3) : Currency.Invalid);
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Money.Add(key, value);
		}
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		result.AttachedItems = new Item[num4];
		for (int j = 0; j < num4; j++)
		{
			unpacker.Read();
			ref Item reference = ref result.AttachedItems[j];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.AttachedVouchers = new VoucherInfo[num5];
		for (int k = 0; k < num5; k++)
		{
			unpacker.Read();
			ref VoucherInfo reference2 = ref result.AttachedVouchers[k];
			reference2 = VoucherInfo.Unpack(unpacker);
		}
		unpacker.Read();
		result.Accepted = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Read = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ClanId = null;
		}
		else
		{
			string clanId = unpacker.LastReadData.AsString();
			result.ClanId = clanId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ExpiresAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.ExpiresAt = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AcceptedEntityId = null;
		}
		else
		{
			string acceptedEntityId = unpacker.LastReadData.AsString();
			result.AcceptedEntityId = acceptedEntityId;
		}
		unpacker.Read();
		result.Highlighted = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Mail Id={Id} SentAt={SentAt} SenderId={SenderId} MailType={MailType} Text={Text} Money={Money} AttachedItems={AttachedItems} AttachedVouchers={AttachedVouchers} Accepted={Accepted} Read={Read} ClanId={ClanId} ExpiresAt={ExpiresAt} AcceptedEntityId={AcceptedEntityId} Highlighted={Highlighted}>";
	}
}
