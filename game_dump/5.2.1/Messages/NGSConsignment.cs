using MsgPack;

namespace Messages;

public struct NGSConsignment
{
	public const uint TypeCode = 76917u;

	public byte[] Data;

	public static void Pack(Packer packer, NGSConsignment val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(76917u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Data == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Data);
		}
	}

	public static NGSConsignment Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		NGSConsignment result = default(NGSConsignment);
		result.Data = unpacker.LastReadData.AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<NGSConsignment Data={Data}>";
	}
}
