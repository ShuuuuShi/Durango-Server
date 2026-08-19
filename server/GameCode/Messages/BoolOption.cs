using MsgPack;

namespace Messages;

public struct BoolOption
{
	public const uint TypeCode = 337u;

	public string Key;

	public bool Value;

	public static void Pack(Packer packer, BoolOption val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(337u);
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

	public static BoolOption Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BoolOption result = default(BoolOption);
		result.Key = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Value = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<BoolOption Key={Key} Value={Value}>";
	}
}
