using MsgPack;
using Shared.System;

namespace Messages;

public struct TamingCompletedEffect
{
	public const uint TypeCode = 812u;

	public Shared.System.RewardEffect Type;

	public ulong AnimalEntityId;

	public Rider Rider;

	public static void Pack(Packer packer, TamingCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(812u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		packer.Pack(val.AnimalEntityId);
		Rider.Pack(packer, val.Rider);
	}

	public static TamingCompletedEffect Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		TamingCompletedEffect result = default(TamingCompletedEffect);
		if (num < 0 || 9 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.AnimalEntityId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		result.Rider = Rider.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<TamingCompletedEffect Type={Type} AnimalEntityId={AnimalEntityId} Rider={Rider}>";
	}
}
