using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct StatusEffect
{
	public ulong Id;

	public string EffectId;

	public int Level;

	public double Since;

	public double Until;

	public int Stacked;

	public Dictionary<string, float> Effects;

	public static void Pack(Packer packer, StatusEffect val, bool hint = false)
	{
		packer.PackArrayHeader(7);
		packer.Pack(val.Id);
		if (val.EffectId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EffectId);
		}
		packer.Pack(val.Level);
		packer.Pack(val.Since);
		packer.Pack(val.Until);
		packer.Pack(val.Stacked);
		if (val.Effects == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Effects.Count);
		foreach (KeyValuePair<string, float> effect in val.Effects)
		{
			if (effect.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(effect.Key);
			}
			packer.Pack(effect.Value);
		}
	}

	public static StatusEffect Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		StatusEffect result = default(StatusEffect);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EffectId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Since = ((MessagePackObject)(ref lastReadData4)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Until = ((MessagePackObject)(ref lastReadData5)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Stacked = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		result.Effects = new Dictionary<string, float>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData8)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			float value = ((MessagePackObject)(ref lastReadData9)).AsSingle();
			result.Effects.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<StatusEffect Id={Id} EffectId={EffectId} Level={Level} Since={Since} Until={Until} Stacked={Stacked} Effects={Effects}>";
	}
}
