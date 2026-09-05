using MsgPack;
using Shared.StatusEffect;

namespace Messages;

public struct EffectDetail
{
	public EffectType Type;

	public string Key;

	public float Value;

	public static void Pack(Packer packer, EffectDetail val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.Type);
		if (val.Key == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Key);
		}
		packer.Pack(val.Value);
	}

	public static EffectDetail Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		EffectDetail result = default(EffectDetail);
		if (num < 0 || 14 < num)
		{
			result.Type = EffectType.Invalid;
		}
		else
		{
			result.Type = (EffectType)num;
		}
		unpacker.Read();
		result.Key = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Value = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<EffectDetail Type={Type} Key={Key} Value={Value}>";
	}
}
