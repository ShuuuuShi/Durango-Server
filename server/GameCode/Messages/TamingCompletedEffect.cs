using MsgPack;
using Shared.System;

namespace Messages;

public struct TamingCompletedEffect
{
	public const uint TypeCode = 2060u;

	public Shared.System.RewardEffect Type;

	public string AnimalEntityId;

	public int AnimalEntityType;

	public string ReinsId;

	public static void Pack(Packer packer, TamingCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2060u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack((int)val.Type);
		if (val.AnimalEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AnimalEntityId);
		}
		packer.Pack(val.AnimalEntityType);
		if (val.ReinsId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ReinsId);
		}
	}

	public static TamingCompletedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		TamingCompletedEffect result = default(TamingCompletedEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.AnimalEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.AnimalEntityType = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ReinsId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TamingCompletedEffect Type={Type} AnimalEntityId={AnimalEntityId} AnimalEntityType={AnimalEntityType} ReinsId={ReinsId}>";
	}
}
