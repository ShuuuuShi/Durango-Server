using System.Collections.Generic;
using K1Network;
using L10N;
using Messages;
using PlayGuide;
using Shared.Faction;

public class FactionRadioDisplay
{
	private const string RadioMotion = "Avatar_Radio";

	private Dictionary<FactionType, string> _colorMap = new Dictionary<FactionType, string>
	{
		{
			FactionType.ChlorophylForum,
			"89D2FF"
		},
		{
			FactionType.ChamberOfPioneer,
			"89D2FF"
		},
		{
			FactionType.TheFirm,
			"89D2FF"
		},
		{
			FactionType.TheCommittee,
			"FF7F27"
		},
		{
			FactionType.Lama,
			"89D2FF"
		}
	};

	private Dictionary<FactionType, string> _speakerNameMap = new Dictionary<FactionType, string>
	{
		{
			FactionType.ChlorophylForum,
			T.N_("대외 담당자")
		},
		{
			FactionType.ChamberOfPioneer,
			T.N_("팀장")
		},
		{
			FactionType.TheFirm,
			T.N_("K")
		},
		{
			FactionType.TheCommittee,
			T.N_("X")
		},
		{
			FactionType.Lama,
			T.N_("닥터 라마")
		}
	};

	public string FlashMessage;

	private string _monologMsg = T.N_("<monolog>(이상한 통신을 수신했다. 회사에 알렸다.)</monolog>");

	public FactionRadioDisplay()
	{
		GameSystem<PlayGuideSystem>.Instance().InstantGuideCompleted += InstantGuideCompleted;
	}

	private GuideEvent CreateGuideEvent(FactionType factionType, string[] messages)
	{
		GuideEventJson guideEventJson = new GuideEventJson();
		guideEventJson.messages = messages;
		GuideEvent guideEvent = GuideEvent.Create("faction_radio", guideEventJson);
		guideEvent.Faction = factionType;
		guideEvent.IsBlur = true;
		guideEvent.ShowPortrait = ShowPortrait.None;
		guideEvent.NPCType = GuideEvent.FactioTypeToNPCType(factionType);
		return guideEvent;
	}

	public void StrangeRadioReceived(StrangeRadio msg, PacketHeader header)
	{
		GuideEvent guideEvent = CreateGuideEvent(FactionType.TheFirm, msg.Messages);
		guideEvent.NameTag = T._("이상한 통신");
		SetInstantEvent(guideEvent);
		GuideEvent guideEvent2 = CreateGuideEvent(FactionType.TheFirm, new string[1] { T._(_monologMsg) });
		guideEvent2.NameTag = " ";
		SetInstantEvent(guideEvent2);
	}

	public void FactionRadioReceived(FactionRadio msg, PacketHeader header)
	{
		ShowFactionRadioMessage(msg.Faction, msg.Messages, msg.ShowPortrait);
	}

	public void ShowFactionRadioMessage(FactionType faction, string[] messages, bool showPortrait = true)
	{
		GuideEvent guideEvent = CreateGuideEvent(faction, messages);
		guideEvent.ShowPortrait = ((!showPortrait) ? ShowPortrait.None : ShowPortrait.Faction);
		string text = _speakerNameMap.Get(faction);
		guideEvent.NameTag = ((text == null) ? null : $"[ffbf00]{T._(text)}[-]");
		if (_colorMap.TryGetValue(faction, out var value) && !string.IsNullOrEmpty(value))
		{
			guideEvent.OverrideColorRGB = value;
		}
		SetInstantEvent(guideEvent);
	}

	private void SetInstantEvent(GuideEvent guideEvent)
	{
		guideEvent.IsInstant = true;
		GameSystem<PlayGuideSystem>.Instance().NotifyEvent(guideEvent);
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (!localPlayer.IsCombatMode && localPlayer.IsCurrentAnimState("Stand"))
		{
			KSingleton<PlayerController>.Instance().Motion("Avatar_Radio");
		}
	}

	private void InstantGuideCompleted()
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (!localPlayer.IsCombatMode && localPlayer.CurrentAnimClipInfo != null && localPlayer.CurrentAnimClipInfo.Clip == "Avatar_Radio")
		{
			KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
		}
		if (!string.IsNullOrEmpty(FlashMessage))
		{
			UIManager.SystemMsg(FlashMessage);
			FlashMessage = null;
		}
	}
}
