using System;
using ClanData;
using ExploreData;
using Messages;

namespace Player;

public class PlayerInfo
{
	private Clan _clan;

	private PortraitBuilder.Argument _portraitArgument;

	private bool _isValidPortraitArgument;

	public bool Valid { get; private set; }

	public ulong EntityId { get; private set; }

	public int Freq { get; private set; }

	public string Name { get; private set; }

	public int Level { get; private set; }

	public ulong ClanId { get; private set; }

	public string ClanName { get; private set; }

	public double DisconnectedAt { get; private set; }

	public bool Online { get; private set; }

	public ExploreData.Region Region { get; private set; }

	public ExploreData.Region ReturningRegion { get; private set; }

	public PlayerDisplay Display { get; private set; }

	public void Set(ulong id)
	{
		Valid = false;
		EntityId = id;
	}

	public void Set(PlayerInfoJson json)
	{
		Valid = true;
		EntityId = json.entity_id;
		Freq = json.freq;
		Name = json.name;
		Level = json.level;
		ClanId = json.clan.clan_id;
		ClanName = json.clan.clan_name;
		DisconnectedAt = json.disconnected_at;
		Online = json.online;
		if (json.region != null)
		{
			Region = new ExploreData.Region(json.region);
		}
		if (json.returning_region != null)
		{
			ReturningRegion = new ExploreData.Region(json.returning_region);
		}
		Display = json.display;
		_clan = null;
	}

	public void GetClan(Action<Clan> callback)
	{
		if (ClanId == 0L)
		{
			callback(null);
		}
		else if (_clan == null)
		{
			ClanSystem.GetClanInfo(ClanId, delegate(Clan clan)
			{
				_clan = clan;
				callback(clan);
			});
		}
		else
		{
			callback(_clan);
		}
	}

	public PortraitBuilder.Argument GetPortraitArgument()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (_isValidPortraitArgument)
		{
			return _portraitArgument;
		}
		bool male = Display.DefaultBody == null || !Display.DefaultBody.Contains("Female");
		PortraitBuilder.Argument argument = PortraitBuilder.MakeArgument(Display.Portrait, Display.PortraitBg, KUtility.ToColor(Display.PortraitBgColor), male, PortraitEmotion.Normal, KUtility.ToColor(Display.SkinColor), KUtility.ToColor(Display.HairColor), KUtility.ToColor(Display.EyeColor), KUtility.ToColor(Display.LipColor));
		PortraitBuilder.FillEmptyBackground(EntityId, ref argument.Background, ref argument.BgColor);
		_portraitArgument = argument;
		_isValidPortraitArgument = true;
		return argument;
	}
}
