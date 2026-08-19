using MsgPack;

namespace Messages;

public struct SetStorageItem
{
	public const uint TypeCode = 2302u;

	public string Key;

	public byte[] Value;

	public static void Pack(Packer packer, SetStorageItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2302u);
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

	public static SetStorageItem Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetStorageItem result = default(SetStorageItem);
		result.Key = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Value = ((MessagePackObject)(ref lastReadData2)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<SetStorageItem Key={Key} Value={Value}>";
	}
}
