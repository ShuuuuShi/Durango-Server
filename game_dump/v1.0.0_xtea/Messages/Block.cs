using MsgPack;

namespace Messages;

public struct Block
{
	public const uint TypeCode = 4016u;

	public ulong EntityId;

	public static void Pack(Packer packer, Block val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4016u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static Block Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Block result = default(Block);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<Block EntityId={EntityId}>";
	}
}
