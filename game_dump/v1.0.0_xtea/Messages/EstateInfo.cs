using MsgPack;
using Shared.Estate;

namespace Messages;

public struct EstateInfo
{
	public ulong Id;

	public OwnerType Type;

	public ulong OwnerId;

	public static void Pack(Packer packer, EstateInfo val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.Id);
		packer.Pack((int)val.Type);
		packer.Pack(val.OwnerId);
	}

	public static EstateInfo Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		EstateInfo result = default(EstateInfo);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Type = OwnerType.Invalid;
		}
		else
		{
			result.Type = (OwnerType)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.OwnerId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<EstateInfo Id={Id} Type={Type} OwnerId={OwnerId}>";
	}
}
