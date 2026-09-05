using MsgPack;

namespace Messages;

public struct IntegerOption
{
	public const uint TypeCode = 336u;

	public string Key;

	public long Value;

	public static void Pack(Packer packer, IntegerOption val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(336u);
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
		packer.Pack(val.Value);
	}

	public static IntegerOption Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		IntegerOption result = default(IntegerOption);
		result.Key = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Value = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<IntegerOption Key={Key} Value={Value}>";
	}
}
