using MsgPack;

namespace Messages;

public struct StorageItem
{
	public const uint TypeCode = 2300u;

	public string Key;

	public byte[] Value;

	public static void Pack(Packer packer, StorageItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2300u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Key == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Key);
		}
		if (val.Value == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Value);
		}
	}

	public static StorageItem Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		StorageItem result = default(StorageItem);
		result.Key = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Value = ((MessagePackObject)(ref lastReadData2)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<StorageItem Key={Key} Value={Value}>";
	}
}
