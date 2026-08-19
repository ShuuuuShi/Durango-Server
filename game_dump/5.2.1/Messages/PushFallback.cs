using MsgPack;

namespace Messages;

public struct PushFallback
{
	public const uint TypeCode = 1021u;

	public string JsonData;

	public static void Pack(Packer packer, PushFallback val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1021u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.JsonData == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.JsonData);
		}
	}

	public static PushFallback Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PushFallback result = default(PushFallback);
		result.JsonData = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<PushFallback JsonData=" + JsonData + ">";
	}
}
