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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetStorageItem result = default(GetStorageItem);
		result.Key = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetStorageItem Key={Key}>";
	}
}
