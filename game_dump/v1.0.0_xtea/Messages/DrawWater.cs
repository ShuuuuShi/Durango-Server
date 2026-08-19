using MsgPack;

namespace Messages;

public struct DrawWater
{
	public const uint TypeCode = 3493u;

	public ulong ToolItemId;

	public static void Pack(Packer packer, DrawWater val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3493u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ToolItemId);
	}

	public static DrawWater Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		DrawWater result = default(DrawWater);
		result.ToolItemId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<DrawWater ToolItemId={ToolItemId}>";
	}
}
