using MsgPack;

namespace Messages;

public struct SetClanEstateLicense
{
	public const uint TypeCode = 3698u;

	public License License;

	public static void Pack(Packer packer, SetClanEstateLicense val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3698u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		License.Pack(packer, val.License);
	}

	public static SetClanEstateLicense Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetClanEstateLicense result = default(SetClanEstateLicense);
		result.License = License.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SetClanEstateLicense License={License}>";
	}
}
