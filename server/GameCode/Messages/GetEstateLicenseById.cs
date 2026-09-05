using MsgPack;

namespace Messages;

public struct GetEstateLicenseById
{
	public const uint TypeCode = 879534u;

	public string EstateId;

	public static void Pack(Packer packer, GetEstateLicenseById val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(879534u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
	}

	public static GetEstateLicenseById Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetEstateLicenseById result = default(GetEstateLicenseById);
		result.EstateId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetEstateLicenseById EstateId={EstateId}>";
	}
}
