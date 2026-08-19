using MsgPack;
using Shared.Inspect;

namespace Messages;

public struct AnimalHealthStatusChanged
{
	public const uint TypeCode = 3605u;

	public ulong EntityId;

	public AnimalHealthStatus Status;

	public static void Pack(Packer packer, AnimalHealthStatusChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3605u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack((int)val.Status);
	}

	public static AnimalHealthStatusChanged Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AnimalHealthStatusChanged result = default(AnimalHealthStatusChanged);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 4 < num)
		{
			result.Status = AnimalHealthStatus.Invalid;
		}
		else
		{
			result.Status = (AnimalHealthStatus)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AnimalHealthStatusChanged EntityId={EntityId} Status={Status}>";
	}
}
