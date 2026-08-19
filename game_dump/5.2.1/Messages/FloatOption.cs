using MsgPack;

namespace Messages;

public struct FloatOption
{
	public const uint TypeCode = 335u;

	public string Key;

	public double Value;

	public static void Pack(Packer packer, FloatOption val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(335u);
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

	public static FloatOption Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FloatOption result = default(FloatOption);
		result.Key = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Value = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<FloatOption Key={Key} Value={Value}>";
	}
}
