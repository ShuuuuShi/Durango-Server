using MsgPack;
using Shared.Memo;

namespace Messages;

public struct MemoCollected
{
	public const uint TypeCode = 2441u;

	public MemoType MemoType;

	public int Number;

	public static void Pack(Packer packer, MemoCollected val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2441u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.MemoType);
		packer.Pack(val.Number);
	}

	public static MemoCollected Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		MemoCollected result = default(MemoCollected);
		if (num < 0 || 1 < num)
		{
			result.MemoType = MemoType.Invalid;
		}
		else
		{
			result.MemoType = (MemoType)num;
		}
		unpacker.Read();
		result.Number = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<MemoCollected MemoType={MemoType} Number={Number}>";
	}
}
