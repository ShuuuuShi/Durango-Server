using System;
using UnityEngine;

[ResourcePath("ui_sound")]
public class UISound : ResourceSingleton<UISound>
{
	public enum ClickType
	{
		NoSound = 0,
		ButtonDefault = 1,
		ButtonMedium = 2,
		ButtonHighlight = 3,
		InteractionTarget = 4,
		InteractionMenuDefault = 5,
		ActionButtonDefault = 6,
		ShopCommodity = 8,
		CalendarItem = 9,
		AutoFill = 10,
		TechSupport = 11,
		ButtonBattle = 12
	}

	public enum GroupType
	{
		NoSound,
		Default,
		LeftMenu,
		Craft,
		Skill,
		Pet,
		Faction,
		Encyclopedia,
		Equip,
		PopUp,
		WorldMap,
		Inventory,
		Event,
		Shop,
		Quest,
		PunchingRanking,
		RecipeSelect,
		Party
	}

	[Serializable]
	public struct Group
	{
		public SoundEventType Open;

		public SoundEventType Close;
	}

	[EnumList(typeof(GroupType), true, 0, -1)]
	[SerializeField]
	private Group[] _uiGroupSounds;

	[EnumList(typeof(ClickType), true, 0, -1)]
	[SerializeField]
	private SoundEventType[] _uiSounds;

	public static void PlayClick(ClickType type)
	{
		SoundManager.PlayEvent(ResourceSingleton<UISound>.Instance()._uiSounds[(int)type]);
	}

	public static void PlayOpenGroup(GroupType type)
	{
		SoundManager.PlayEvent(ResourceSingleton<UISound>.Instance()._uiGroupSounds[(int)type].Open);
	}

	public static void PlayCloseGroup(GroupType type)
	{
		SoundManager.PlayEvent(ResourceSingleton<UISound>.Instance()._uiGroupSounds[(int)type].Close);
	}
}
