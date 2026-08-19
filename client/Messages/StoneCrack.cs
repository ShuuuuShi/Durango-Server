using MsgPack;
using Shared.Season2;

namespace Messages;

public struct StoneCrack
{
	public int ActivePhase;

	public ResourceType ResourceType;

	public static void Pack(Packer packer, StoneCrack val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.ActivePhase);
		packer.Pack((int)val.ResourceType);
	}

	public static StoneCrack Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		StoneCrack result = default(StoneCrack);
		result.ActivePhase = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 2 < num)
		{
			result.ResourceType = ResourceType.Invalid;
		}
		else
		{
			result.ResourceType = (ResourceType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<StoneCrack ActivePhase={ActivePhase} ResourceType={ResourceType}>";
	}
}
