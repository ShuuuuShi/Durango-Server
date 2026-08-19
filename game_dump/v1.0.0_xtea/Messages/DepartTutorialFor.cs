using MsgPack;

namespace Messages;

public struct DepartTutorialFor
{
	public const uint TypeCode = 2307u;

	public ulong TargetRegionId;

	public int EntryPointOffset;

	public static void Pack(Packer packer, DepartTutorialFor val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2307u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.TargetRegionId);
		packer.Pack(val.EntryPointOffset);
	}

	public static DepartTutorialFor Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		DepartTutorialFor result = default(DepartTutorialFor);
		result.TargetRegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntryPointOffset = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<DepartTutorialFor TargetRegionId={TargetRegionId} EntryPointOffset={EntryPointOffset}>";
	}
}
