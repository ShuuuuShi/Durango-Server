using MsgPack;

namespace Messages;

public struct ReactingPropRewarded
{
	public const uint TypeCode = 78452084u;

	public Item[] Items;

	public static void Pack(Packer packer, ReactingPropRewarded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(78452084u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
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

	public static ReactingPropRewarded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ReactingPropRewarded result = default(ReactingPropRewarded);
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ReactingPropRewarded Items={Items}>";
	}
}
