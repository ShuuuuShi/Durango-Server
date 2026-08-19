using MsgPack;

namespace Messages;

public struct CraftStartedOnWorkbench
{
	public const uint TypeCode = 66u;

	public ulong EntityId;

	public float Duration;

	public static void Pack(Packer packer, CraftStartedOnWorkbench val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(66u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.Duration);
	}

	public static CraftStartedOnWorkbench Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		CraftStartedOnWorkbench result = default(CraftStartedOnWorkbench);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Duration = ((MessagePackObject)(ref lastReadData2)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<CraftStartedOnWorkbench EntityId={EntityId} Duration={Duration}>";
	}
}
