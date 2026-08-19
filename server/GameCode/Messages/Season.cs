using MsgPack;

namespace Messages;

public struct Season
{
	public string Id;

	public string Name;

	public double Since;

	public double Until;

	public string IconSmall;

	public string IconLarge;

	public string BannerImg;

	public string BannerText;

	public static void Pack(Packer packer, Season val, bool hint = false)
	{
		packer.PackArrayHeader(8);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.PackString(val.Name);
		packer.Pack(val.Since);
		packer.Pack(val.Until);
		if (val.IconSmall == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.IconSmall);
		}
		if (val.IconLarge == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.IconLarge);
		}
		if (val.BannerImg == null)
		{
			packer.PackNull();
		}
		else if (val.BannerImg == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BannerImg);
		}
		if (val.BannerText == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.BannerText);
		}
	}

	public static Season Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Season result = default(Season);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Since = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Until = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.IconSmall = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.IconLarge = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BannerImg = null;
		}
		else
		{
			string bannerImg = unpacker.LastReadData.AsString();
			result.BannerImg = bannerImg;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BannerText = null;
		}
		else
		{
			string bannerText = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.BannerText = bannerText;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Season Id={Id} Name={Name} Since={Since} Until={Until} IconSmall={IconSmall} IconLarge={IconLarge} BannerImg={BannerImg} BannerText={BannerText}>";
	}
}
