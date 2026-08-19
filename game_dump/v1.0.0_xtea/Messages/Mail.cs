using System.Collections.Generic;
using MsgPack;
using Shared.Economy;
using Shared.Mailing;

namespace Messages;

public struct Mail
{
	public ulong Id;

	public double SentAt;

	public ulong SenderId;

	public MailType MailType;

	public string Text;

	public Dictionary<Currency, int> Money;

	public Item[] AttachedItems;

	public bool Accepted;

	public ulong? ClanId;

	public static void Pack(Packer packer, Mail val, bool hint = false)
	{
		packer.PackArrayHeader(9);
		packer.Pack(val.Id);
		packer.Pack(val.SentAt);
		packer.Pack(val.SenderId);
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
		packer.Pack(val.Accepted);
		if (!val.ClanId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ClanId.Value);
		}
	}

	public static Mail Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Mail result = default(Mail);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SentAt = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.SenderId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		if (num < 0 || 3 < num)
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
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.Money = new Dictionary<Currency, int>(num2, default(CurrencyComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			Currency key = ((num3 >= 0 && 1 >= num3) ? ((Currency)num3) : Currency.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData7)).AsInt32();
			result.Money.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		int num4 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		result.AttachedItems = new Item[num4];
		for (int j = 0; j < num4; j++)
		{
			unpacker.Read();
			ref Item reference = ref result.AttachedItems[j];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		result.Accepted = ((MessagePackObject)(ref lastReadData9)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.ClanId = null;
		}
		else
		{
			MessagePackObject lastReadData11 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData11)).AsUInt64();
			result.ClanId = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Mail Id={Id} SentAt={SentAt} SenderId={SenderId} MailType={MailType} Text={Text} Money={Money} AttachedItems={AttachedItems} Accepted={Accepted} ClanId={ClanId}>";
	}
}
