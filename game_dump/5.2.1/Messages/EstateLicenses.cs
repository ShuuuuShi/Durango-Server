using MsgPack;

namespace Messages;

public struct EstateLicenses
{
	public const uint TypeCode = 3821u;

	public EstateLicense? UrbanEstate;

	public int LargestUrbanEstateSize;

	public EstateLicense? PersonalEstate;

	public int LargestPersonalEstateSize;

	public EstateLicense? ClanEstate;

	public int LargestClanEstateSize;

	public ClanCargoWarphole? ClanCargoWarphole;

	public double ClanCargoWarpholeVisitAvailableAt;

	public EntityTile? PersonalWarpholeTile;

	public EntityTile? UrbanWarpholeTile;

	public static void Pack(Packer packer, EstateLicenses val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(11);
			packer.Pack(3821u);
		}
		else
		{
			packer.PackArrayHeader(10);
		}
		if (!val.UrbanEstate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EstateLicense.Pack(packer, val.UrbanEstate.Value);
		}
		packer.Pack(val.LargestUrbanEstateSize);
		if (!val.PersonalEstate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EstateLicense.Pack(packer, val.PersonalEstate.Value);
		}
		packer.Pack(val.LargestPersonalEstateSize);
		if (!val.ClanEstate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EstateLicense.Pack(packer, val.ClanEstate.Value);
		}
		packer.Pack(val.LargestClanEstateSize);
		if (!val.ClanCargoWarphole.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.ClanCargoWarphole.Pack(packer, val.ClanCargoWarphole.Value);
		}
		packer.Pack(val.ClanCargoWarpholeVisitAvailableAt);
		if (!val.PersonalWarpholeTile.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EntityTile.Pack(packer, val.PersonalWarpholeTile.Value);
		}
		if (!val.UrbanWarpholeTile.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EntityTile.Pack(packer, val.UrbanWarpholeTile.Value);
		}
	}

	public static EstateLicenses Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EstateLicenses result = default(EstateLicenses);
		if (unpacker.LastReadData.IsNil)
		{
			result.UrbanEstate = null;
		}
		else
		{
			EstateLicense value = EstateLicense.Unpack(unpacker);
			result.UrbanEstate = value;
		}
		unpacker.Read();
		result.LargestUrbanEstateSize = unpacker.LastReadData.AsInt32();
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
		unpacker.Read();
		result.LargestPersonalEstateSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ClanEstate = null;
		}
		else
		{
			EstateLicense value3 = EstateLicense.Unpack(unpacker);
			result.ClanEstate = value3;
		}
		unpacker.Read();
		result.LargestClanEstateSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ClanCargoWarphole = null;
		}
		else
		{
			ClanCargoWarphole value4 = Messages.ClanCargoWarphole.Unpack(unpacker);
			result.ClanCargoWarphole = value4;
		}
		unpacker.Read();
		result.ClanCargoWarpholeVisitAvailableAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PersonalWarpholeTile = null;
		}
		else
		{
			EntityTile value5 = EntityTile.Unpack(unpacker);
			result.PersonalWarpholeTile = value5;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UrbanWarpholeTile = null;
		}
		else
		{
			EntityTile value6 = EntityTile.Unpack(unpacker);
			result.UrbanWarpholeTile = value6;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EstateLicenses UrbanEstate={UrbanEstate} LargestUrbanEstateSize={LargestUrbanEstateSize} PersonalEstate={PersonalEstate} LargestPersonalEstateSize={LargestPersonalEstateSize} ClanEstate={ClanEstate} LargestClanEstateSize={LargestClanEstateSize} ClanCargoWarphole={ClanCargoWarphole} ClanCargoWarpholeVisitAvailableAt={ClanCargoWarpholeVisitAvailableAt} PersonalWarpholeTile={PersonalWarpholeTile} UrbanWarpholeTile={UrbanWarpholeTile}>";
	}
}
