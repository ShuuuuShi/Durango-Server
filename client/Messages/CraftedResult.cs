using MsgPack;

namespace Messages;

public struct CraftedResult
{
	public string Id;

	public Item[] Items;

	public static void Pack(Packer packer, CraftedResult val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
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

	public static CraftedResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CraftedResult result = default(CraftedResult);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
		return $"<CraftedResult Id={Id} Items={Items}>";
	}
}
