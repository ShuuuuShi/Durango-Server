using MsgPack;

namespace Messages;

public struct PersonalRegionInfo
{
	public const uint TypeCode = 20422u;

	public PersonalRegion? PersonalRegion;

	public EstateLicense? PersonalEstate;

	public static void Pack(Packer packer, PersonalRegionInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(20422u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (!val.PersonalRegion.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.PersonalRegion.Pack(packer, val.PersonalRegion.Value);
		}
		if (!val.PersonalEstate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EstateLicense.Pack(packer, val.PersonalEstate.Value);
		}
	}

	public static PersonalRegionInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PersonalRegionInfo result = default(PersonalRegionInfo);
		if (unpacker.LastReadData.IsNil)
		{
			result.PersonalRegion = null;
		}
		else
		{
			PersonalRegion value = Messages.PersonalRegion.Unpack(unpacker);
			result.PersonalRegion = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PersonalEstate = null;
		}
		else
		{
			EstateLicense value2 = EstateLicense.Unpack(unpacker);
			result.PersonalEstate = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PersonalRegionInfo PersonalRegion={PersonalRegion} PersonalEstate={PersonalEstate}>";
	}
}
