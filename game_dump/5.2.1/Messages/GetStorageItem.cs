using MsgPack;

namespace Messages;

public struct GetStorageItem
{
	public const uint TypeCode = 2301u;

	public string Key;

	public static void Pack(Packer packer, GetStorageItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2301u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Key == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Key);
		}
	}

	public static GetStorageItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetStorageItem result = default(GetStorageItem);
		result.Key = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<GetStorageItem Key=" + Key + ">";
	}
}
