using MsgPack;

namespace Messages;

public struct GetArchipelago
{
	public const uint TypeCode = 2121u;

	public string ArchipelagoId;

	public static void Pack(Packer packer, GetArchipelago val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2121u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ArchipelagoId == null)
		{
			packer.PackNull();
		}
		else if (val.ArchipelagoId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ArchipelagoId);
		}
	}

	public static GetArchipelago Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetArchipelago result = default(GetArchipelago);
		if (unpacker.LastReadData.IsNil)
		{
			result.ArchipelagoId = null;
		}
		else
		{
			string archipelagoId = unpacker.LastReadData.AsString();
			result.ArchipelagoId = archipelagoId;
		}
		return result;
	}

	public override string ToString()
	{
		return "<GetArchipelago ArchipelagoId=" + ArchipelagoId + ">";
	}
}
