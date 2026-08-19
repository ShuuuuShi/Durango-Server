using Durango.Logic.Explore;
using JetBrains.Annotations;
using Messages;

namespace Durango.Player;

public class PlayerInfo
{
	public bool Valid;

	public string EntityId = string.Empty;

	public int Freq;

	public string Name = string.Empty;

	public int Level;

	public string ClanId = string.Empty;

	public string ClanName = string.Empty;

	public string PersonalRegionId = string.Empty;

	public int PioneerGrade;

	[CanBeNull]
	public Durango.Logic.Explore.Region Region;

	[CanBeNull]
	public Durango.Logic.Explore.Region ReturningRegion;

	public PlayerDisplay Display;

	private PortraitBuilder.Argument? _portraitArgument;

	public bool HasClan => !string.IsNullOrEmpty(ClanId);

	public string RegionName
	{
		get
		{
			if (Region != null)
			{
				return Region.Name;
			}
			return string.Empty;
		}
	}

	public string ReturningRegionName
	{
		get
		{
			if (ReturningRegion != null)
			{
				return ReturningRegion.Name;
			}
			return string.Empty;
		}
	}

	public bool IsMale
	{
		get
		{
			if (Display.DefaultBody != null)
			{
				return !Display.DefaultBody.Contains("Female");
			}
			return true;
		}
	}

	public void Set(PlayerInfoJson json)
	{
		Valid = true;
		EntityId = json.EntityId;
		Freq = json.Freq;
		Name = json.Name;
		Level = json.Level;
		ClanId = json.Clan.ClanId;
		ClanName = json.Clan.ClanName;
		PersonalRegionId = json.PersonalRegionId;
		PioneerGrade = json.PioneerGrade;
		if (json.Region != null)
		{
			Region = new Durango.Logic.Explore.Region(json.Region);
		}
		if (json.ReturningRegion != null)
		{
			ReturningRegion = new Durango.Logic.Explore.Region(json.ReturningRegion);
		}
		Display = json.Display;
		_portraitArgument = null;
	}

	public PortraitBuilder.Argument GetPortraitArgument()
	{
		PortraitBuilder.Argument argument;
		if (_portraitArgument.HasValue)
		{
			argument = _portraitArgument.Value;
		}
		else
		{
			argument = Display.GetPortraitArgument(EntityId, IsMale);
			_portraitArgument = argument;
		}
		return argument;
	}

	public string GetFreq(int? freqSize = null)
	{
		return ToFreq(Freq, freqSize);
	}

	public static string ToFreq(int freq, int? freqSize = null)
	{
		if (!freqSize.HasValue)
		{
			return $"#{freq:0000} kHz";
		}
		return $"[size={freqSize}]#{freq:0000}[/size] kHz";
	}

	public string GetNameFreq(int freqSize = 21, string freqHexCode = "")
	{
		if (string.IsNullOrEmpty(freqHexCode))
		{
			return $"{Name}[size={freqSize}] #{Freq:0000} kHz[/size]";
		}
		return $"{Name}[{freqHexCode}][size={freqSize}] #{Freq:0000} kHz[/size][-]";
	}
}
