using MsgPack;

namespace Messages;

public struct MemoCollected
{
	public const uint TypeCode = 2441u;

	public int Number;

	public static void Pack(Packer packer, MemoCollected val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2441u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Number);
	}

	public static MemoCollected Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		MemoCollected result = default(MemoCollected);
		result.Number = ((MessagePackObject)(ref lastReadData)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<MemoCollected Number={Number}>";
	}
}
