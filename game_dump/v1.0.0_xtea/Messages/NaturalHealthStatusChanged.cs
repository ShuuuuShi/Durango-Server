using MsgPack;
using Shared.Inspect;

namespace Messages;

public struct NaturalHealthStatusChanged
{
	public const uint TypeCode = 3607u;

	public ulong EntityId;

	public NaturalHealthStatus Status;

	public static void Pack(Packer packer, NaturalHealthStatusChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3607u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack((int)val.Status);
	}

	public static NaturalHealthStatusChanged Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		NaturalHealthStatusChanged result = default(NaturalHealthStatusChanged);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 6 < num)
		{
			result.Status = NaturalHealthStatus.Invalid;
		}
		else
		{
			result.Status = (NaturalHealthStatus)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<NaturalHealthStatusChanged EntityId={EntityId} Status={Status}>";
	}
}
