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
		unpacker.Read();
		SetStorageItem result = default(SetStorageItem);
		result.Key = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Value = unpacker.LastReadData.AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<SetStorageItem Key={Key} Value={Value}>";
	}
}
