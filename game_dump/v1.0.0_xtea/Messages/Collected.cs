using MsgPack;
using Shared.Item;

namespace Messages;

public struct Collected
{
	public const uint TypeCode = 104u;

	public Item Item;

	public Result Result;

	public ActionInfo ActionInfo;

	public bool RanOut;

	public static void Pack(Packer packer, Collected val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(104u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		Item.Pack(packer, val.Item);
		packer.Pack((int)val.Result);
		ActionInfo.Pack(packer, val.ActionInfo);
		packer.Pack(val.RanOut);
	}

	public static Collected Unpack(Unpacker unpacker)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		Collected result = default(Collected);
		result.Item = Item.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Result = Result.Invalid;
		}
		else
		{
			result.Result = (Result)num;
		}
		unpacker.Read();
		result.ActionInfo = ActionInfo.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.RanOut = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Collected Item={Item} Result={Result} ActionInfo={ActionInfo} RanOut={RanOut}>";
	}
}
