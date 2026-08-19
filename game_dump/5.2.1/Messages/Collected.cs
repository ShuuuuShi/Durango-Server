using MsgPack;
using Shared.Item;

namespace Messages;

public struct Collected
{
	public const uint TypeCode = 104u;

	public Item[] Items;

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
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				Item.Pack(packer, val.Items[i]);
			}
		}
		packer.Pack((int)val.Result);
		ActionInfo.Pack(packer, val.ActionInfo);
		packer.Pack(val.RanOut);
	}

	public static Collected Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Collected result = default(Collected);
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 3 < num2)
		{
			result.Result = Result.Invalid;
		}
		else
		{
			result.Result = (Result)num2;
		}
		unpacker.Read();
		result.ActionInfo = ActionInfo.Unpack(unpacker);
		unpacker.Read();
		result.RanOut = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Collected Items={Items} Result={Result} ActionInfo={ActionInfo} RanOut={RanOut}>";
	}
}
