using MsgPack;
using Shared.Item;

namespace Messages;

public struct Crafted
{
	public const uint TypeCode = 122u;

	public Result Result;

	public ActionInfo ActionInfo;

	public Item[] Items;

	public static void Pack(Packer packer, Crafted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(122u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Result);
		ActionInfo.Pack(packer, val.ActionInfo);
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Items.Length);
		for (int i = 0; i < val.Items.Length; i++)
		{
			Item.Pack(packer, val.Items[i]);
		}
	}

	public static Crafted Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Crafted result = default(Crafted);
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
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Items = new Item[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Crafted Result={Result} ActionInfo={ActionInfo} Items={Items}>";
	}
}
