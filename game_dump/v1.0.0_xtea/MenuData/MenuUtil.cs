using L10N;

namespace MenuData;

public static class MenuUtil
{
	public static string GetIcon(MenuType type)
	{
		return IconMap.Get(GetKey(type));
	}

	public static string GetName(MenuType type)
	{
		return type switch
		{
			MenuType.Character => T._("캐릭터"), 
			MenuType.Equip => T._("장비"), 
			MenuType.Skill => T._("스킬"), 
			MenuType.Inventory => T._("가방"), 
			MenuType.Craft => T._("제작/건설"), 
			MenuType.Market => T._("섬 장터"), 
			MenuType.Social => T._("친구 목록"), 
			MenuType.Mail => T._("우편"), 
			MenuType.Screenshot => T._("촬영"), 
			MenuType.Config => T._("설정"), 
			MenuType.Encyclopedia => T._("도감"), 
			MenuType.AutoGuide => T._("진로 가이드"), 
			MenuType.Music => T._("연주"), 
			MenuType.Clan => T._("부족"), 
			MenuType.Faction => T._("통신"), 
			MenuType.Ticket => T._("베타키"), 
			_ => type.ToString(), 
		};
	}

	public static string GetKey(MenuType type)
	{
		return "#mainhud_" + type.ToString().ToLower();
	}
}
