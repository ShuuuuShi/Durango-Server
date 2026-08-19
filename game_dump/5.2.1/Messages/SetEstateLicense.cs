using MsgPack;

namespace Messages;

public struct SetEstateLicense
{
	public const uint TypeCode = 2420u;

	public string EstateId;

	public AccessRights AccessRights;

	public static void Pack(Packer packer, SetEstateLicense val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2420u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
		AccessRights.Pack(packer, val.AccessRights);
	}

	public static SetEstateLicense Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetEstateLicense result = default(SetEstateLicense);
		result.EstateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.AccessRights = AccessRights.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SetEstateLicense EstateId={EstateId} AccessRights={AccessRights}>";
	}
}
