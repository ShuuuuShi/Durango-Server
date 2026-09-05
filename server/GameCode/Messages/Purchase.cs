using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Purchase
{
	public const uint TypeCode = 510399u;

	public string Id;

	public string CommodityId;

	public double PurchasedAt;

	public double? AcceptedAt;

	public double ExpiresAt;

	public object Content;

	public Dictionary<string, double> SubAcceptedAt;

	public static void Pack(Packer packer, Purchase val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(510399u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (val.CommodityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CommodityId);
		}
		packer.Pack(val.PurchasedAt);
		if (!val.AcceptedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.AcceptedAt.Value);
		}
		packer.Pack(val.ExpiresAt);
		if (val.Content == null)
		{
			packer.PackNull();
		}
		else if (val.Content is ItemPurchaseContent)
		{
			ItemPurchaseContent.Pack(packer, (ItemPurchaseContent)val.Content, hint: true);
		}
		else if (val.Content is EmotionPurchaseContent)
		{
			EmotionPurchaseContent.Pack(packer, (EmotionPurchaseContent)val.Content, hint: true);
		}
		if (val.SubAcceptedAt == null)
		{
			packer.PackNull();
			return;
		}
		if (val.SubAcceptedAt == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.SubAcceptedAt.Count);
		foreach (KeyValuePair<string, double> item in val.SubAcceptedAt)
		{
			if (item.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(item.Key);
			}
			packer.Pack(item.Value);
		}
	}

	public static Purchase Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Purchase result = default(Purchase);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.CommodityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.PurchasedAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AcceptedAt = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.AcceptedAt = value;
		}
		unpacker.Read();
		result.ExpiresAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Content = null;
		}
		else
		{
			object content = null;
			if (unpacker.ReadUInt32(out var result2))
			{
				switch (result2)
				{
				case 71294574u:
					content = ItemPurchaseContent.Unpack(unpacker);
					break;
				case 71294575u:
					content = EmotionPurchaseContent.Unpack(unpacker);
					break;
				default:
					Debug.LogError("Unexpected type code: " + result2);
					break;
				}
			}
			result.Content = content;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SubAcceptedAt = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Dictionary<string, double> dictionary = new Dictionary<string, double>(num);
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				string key = unpacker.LastReadData.AsString();
				unpacker.Read();
				double value2 = unpacker.LastReadData.AsDouble();
				dictionary.Add(key, value2);
			}
			result.SubAcceptedAt = dictionary;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Purchase Id={Id} CommodityId={CommodityId} PurchasedAt={PurchasedAt} AcceptedAt={AcceptedAt} ExpiresAt={ExpiresAt} Content={Content} SubAcceptedAt={SubAcceptedAt}>";
	}
}
