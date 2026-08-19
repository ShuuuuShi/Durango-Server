using System.Collections.Generic;
using MsgPack;
using UnityEngine;

namespace Messages;

public struct CraftEstimation
{
	public const uint TypeCode = 8u;

	public string PrototypeId;

	public int Level;

	public string Name;

	public Vector2 Durability;

	public Dictionary<string, int> Tags;

	public int UnrevealedRareTagCount;

	public int ModifiableCount;

	public float SuccessRate;

	public float GreatSuccessRate;

	public float RequiredAbilityValue;

	public static void Pack(Packer packer, CraftEstimation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(11);
			packer.Pack(8u);
		}
		else
		{
			packer.PackArrayHeader(10);
		}
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.Level);
		packer.PackString(val.Name);
		packer.PackArrayHeader(2);
		packer.Pack(val.Durability.x);
		packer.Pack(val.Durability.y);
		if (val.Tags == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Tags.Count);
			foreach (KeyValuePair<string, int> tag in val.Tags)
			{
				if (tag.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(tag.Key);
				}
				packer.Pack(tag.Value);
			}
		}
		packer.Pack(val.UnrevealedRareTagCount);
		packer.Pack(val.ModifiableCount);
		packer.Pack(val.SuccessRate);
		packer.Pack(val.GreatSuccessRate);
		packer.Pack(val.RequiredAbilityValue);
	}

	public static CraftEstimation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CraftEstimation result = default(CraftEstimation);
		result.PrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		unpacker.ReadSingle(out result.Durability.x);
		unpacker.ReadSingle(out result.Durability.y);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Tags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Tags.Add(key, value);
		}
		unpacker.Read();
		result.UnrevealedRareTagCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ModifiableCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SuccessRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.GreatSuccessRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.RequiredAbilityValue = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<CraftEstimation PrototypeId={PrototypeId} Level={Level} Name={Name} Durability={Durability} Tags={Tags} UnrevealedRareTagCount={UnrevealedRareTagCount} ModifiableCount={ModifiableCount} SuccessRate={SuccessRate} GreatSuccessRate={GreatSuccessRate} RequiredAbilityValue={RequiredAbilityValue}>";
	}
}
