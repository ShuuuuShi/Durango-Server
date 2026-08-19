using MsgPack;

namespace Messages;

public struct Welcome
{
	public const uint TypeCode = 22u;

	public string UserId;

	public string Name;

	public Region Region;

	public Storage Storage;

	public Options Options;

	public Archipelago? Archipelago;

	public string PersonalRegionId;

	public Seasons Seasons;

	public SocialOptions SocialOptions;

	public bool EngagementRewardSent;

	public static void Pack(Packer packer, Welcome val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(11);
			packer.Pack(22u);
		}
		else
		{
			packer.PackArrayHeader(10);
		}
		if (val.UserId == null)
		{
			packer.PackNull();
		}
		else if (val.UserId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.UserId);
		}
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		Region.Pack(packer, val.Region);
		Storage.Pack(packer, val.Storage);
		Options.Pack(packer, val.Options);
		if (!val.Archipelago.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Archipelago.Pack(packer, val.Archipelago.Value);
		}
		if (val.PersonalRegionId == null)
		{
			packer.PackNull();
		}
		else if (val.PersonalRegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PersonalRegionId);
		}
		Seasons.Pack(packer, val.Seasons);
		SocialOptions.Pack(packer, val.SocialOptions);
		packer.Pack(val.EngagementRewardSent);
	}

	public static Welcome Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Welcome result = default(Welcome);
		if (unpacker.LastReadData.IsNil)
		{
			result.UserId = null;
		}
		else
		{
			string userId = unpacker.LastReadData.AsString();
			result.UserId = userId;
		}
		unpacker.Read();
		result.Name = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		result.Storage = Storage.Unpack(unpacker);
		unpacker.Read();
		result.Options = Options.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Archipelago = null;
		}
		else
		{
			Archipelago value = Messages.Archipelago.Unpack(unpacker);
			result.Archipelago = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PersonalRegionId = null;
		}
		else
		{
			string personalRegionId = unpacker.LastReadData.AsString();
			result.PersonalRegionId = personalRegionId;
		}
		unpacker.Read();
		result.Seasons = Seasons.Unpack(unpacker);
		unpacker.Read();
		result.SocialOptions = SocialOptions.Unpack(unpacker);
		unpacker.Read();
		result.EngagementRewardSent = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Welcome UserId={UserId} Name={Name} Region={Region} Storage={Storage} Options={Options} Archipelago={Archipelago} PersonalRegionId={PersonalRegionId} Seasons={Seasons} SocialOptions={SocialOptions} EngagementRewardSent={EngagementRewardSent}>";
	}
}
