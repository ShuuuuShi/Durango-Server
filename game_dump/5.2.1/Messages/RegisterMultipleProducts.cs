using MsgPack;

namespace Messages;

public struct RegisterMultipleProducts
{
	public const uint TypeCode = 82309u;

	public string[] ItemIds;

	public long EachPrice;

	public float Duration;

	public static void Pack(Packer packer, RegisterMultipleProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(82309u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ItemIds.Length);
			for (int i = 0; i < val.ItemIds.Length; i++)
			{
				if (val.ItemIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.ItemIds[i]);
				}
			}
		}
		packer.Pack(val.EachPrice);
		packer.Pack(val.Duration);
	}

	public static RegisterMultipleProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RegisterMultipleProducts result = default(RegisterMultipleProducts);
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.EachPrice = unpacker.LastReadData.AsInt64();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RegisterMultipleProducts ItemIds={ItemIds} EachPrice={EachPrice} Duration={Duration}>";
	}
}
