using MsgPack;

namespace Messages;

public struct ConfirmResurrection
{
	public const uint TypeCode = 56238474u;

	public string HelperEntityId;

	public static void Pack(Packer packer, ConfirmResurrection val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(56238474u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.HelperEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.HelperEntityId);
		}
	}

	public static ConfirmResurrection Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ConfirmResurrection result = default(ConfirmResurrection);
		result.HelperEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<ConfirmResurrection HelperEntityId=" + HelperEntityId + ">";
	}
}
