using MsgPack;

namespace Messages;

public struct StatusEffect
{
	public string Id;

	public string EffectId;

	public int Level;

	public double Since;

	public double Until;

	public int Stacked;

	public bool DurationHidden;

	public string NameGettext;

	public EffectDetail[] Effects;

	public DailyContents? DailyContents;

	public static void Pack(Packer packer, StatusEffect val, bool hint = false)
	{
		packer.PackArrayHeader(10);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
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
		packer.Pack(val.DurationHidden);
		if (val.NameGettext == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.NameGettext);
		}
		if (val.Effects == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Effects.Length);
			for (int i = 0; i < val.Effects.Length; i++)
			{
				EffectDetail.Pack(packer, val.Effects[i]);
			}
		}
		if (!val.DailyContents.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.DailyContents.Pack(packer, val.DailyContents.Value);
		}
	}

	public static StatusEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		StatusEffect result = default(StatusEffect);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EffectId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Since = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Until = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Stacked = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.DurationHidden = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.NameGettext = null;
		}
		else
		{
			string nameGettext = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.NameGettext = nameGettext;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Effects = new EffectDetail[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref EffectDetail reference = ref result.Effects[i];
			reference = EffectDetail.Unpack(unpacker);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.DailyContents = null;
		}
		else
		{
			DailyContents value = Messages.DailyContents.Unpack(unpacker);
			result.DailyContents = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<StatusEffect Id={Id} EffectId={EffectId} Level={Level} Since={Since} Until={Until} Stacked={Stacked} DurationHidden={DurationHidden} NameGettext={NameGettext} Effects={Effects} DailyContents={DailyContents}>";
	}
}
