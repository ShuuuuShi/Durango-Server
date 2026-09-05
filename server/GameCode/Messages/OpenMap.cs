using MsgPack;

namespace Messages;

public struct OpenMap
{
	public const uint TypeCode = 915u;

	public string VoucherId;

	public static void Pack(Packer packer, OpenMap val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(915u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.VoucherId == null)
		{
			packer.PackNull();
		}
		else if (val.VoucherId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VoucherId);
		}
	}

	public static OpenMap Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		OpenMap result = default(OpenMap);
		if (unpacker.LastReadData.IsNil)
		{
			result.VoucherId = null;
		}
		else
		{
			string voucherId = unpacker.LastReadData.AsString();
			result.VoucherId = voucherId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<OpenMap VoucherId={VoucherId}>";
	}
}
