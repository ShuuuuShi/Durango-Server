using L10N;

namespace Durango.Logic;

public enum MenuType
{
	[T.EnumName("캐릭터")]
	[EnumIcon("icon_mainhud_character")]
	Character,
	[T.EnumName("스킬")]
	[EnumIcon("icon_mainhud_skill")]
	Skill,
	[T.EnumName("가방")]
	[EnumIcon("icon_mainhud_inventory")]
	Inventory,
	[T.EnumName("제작/건설")]
	[EnumIcon("icon_mainhud_craft")]
	Craft,
	[T.EnumName("섬 장터")]
	[EnumIcon("icon_mainhud_market")]
	Market,
	[T.EnumName("친구")]
	[EnumIcon("icon_mainhud_social")]
	Social,
	[T.EnumName("우편")]
	[EnumIcon("icon_mainhud_mail", "icon_mainhud_mail_pc")]
	Mail,
	[T.EnumName("촬영")]
	[EnumIcon("icon_mainhud_camera", "icon_mainhud_camera_pc")]
	Screenshot,
	[T.EnumName("설정")]
	[EnumIcon("icon_mainhud_config")]
	Config,
	[T.EnumName("도감")]
	[EnumIcon("icon_mainhud_encylopedia")]
	Encyclopedia,
	[T.EnumName("부족")]
	[EnumIcon("icon_mainhud_guild")]
	Clan,
	[T.EnumName("지원 단체")]
	[EnumIcon("icon_mainhud_newsletter")]
	Faction,
	[T.EnumName("이력")]
	[EnumIcon("icon_mainhud_ticket")]
	Timeline,
	[T.EnumName("동물")]
	[EnumIcon("icon_mainhud_pet")]
	Pet,
	[T.EnumName("사유지")]
	[EnumIcon("icon_mainhud_estate")]
	Estate,
	[T.EnumName("상점")]
	[EnumIcon("icon_mainhud_shop")]
	Shop,
	[T.EnumName("이벤트")]
	[EnumIcon("icon_mainhud_event")]
	Event,
	[T.EnumName("퀘스트")]
	[EnumIcon("icon_mainhud_quest")]
	Quest,
	[T.EnumName("진로 가이드")]
	[EnumIcon("icon_mainhud_learningguide")]
	LearningGuide,
	[T.EnumName("파티")]
	[EnumIcon("icon_mainhud_party")]
	Party,
	[T.EnumName("공지사항")]
	[EnumIcon("icon_mainhud_notice", "icon_mainhud_notice_pc")]
	Notice,
	[T.EnumName("캐릭터 변경")]
	[EnumIcon("icon_mainhud_change_character", "icon_mainhud_change_character_pc")]
	PlayerSelection,
	[T.EnumName("공식 페이스북")]
	[EnumIcon("icon_mainhud_facebook", "icon_mainhud_facebook_pc")]
	OfficialCommunity,
	[T.EnumName("오퍼월")]
	[EnumIcon("icon_mainhud_offerwall")]
	Offerwall,
	[T.EnumName("난투섬")]
	[EnumIcon("icon_mainhud_pvpisland")]
	PvpIsland,
	[T.EnumName("일지")]
	[EnumIcon("icon_mainhud_story")]
	Story,
	[T.EnumName("지도")]
	[EnumIcon(null, "icon_mainhud_map_pc")]
	WorldMap,
	[T.EnumName("캐릭터")]
	[EnumIcon("icon_mainhud_character")]
	CategoryCharacter,
	[T.EnumName("할 일")]
	[EnumIcon("icon_mainhud_quest")]
	CategoryToDo,
	[T.EnumName("친구")]
	[EnumIcon("icon_mainhud_social", "icon_mainhud_social_pc")]
	CategorySocial,
	[T.EnumName("연주")]
	[EnumIcon("icon_mainhud_music")]
	Music,
	[T.EnumName("친구 섬 방문")]
	[EnumIcon("icon_mainhud_visit_friend")]
	Connect,
	[T.EnumName("캐릭터")]
	[EnumIcon("icon_mainhud_character")]
	CharacterOnMenu,
	[T.EnumName("연주")]
	[EnumIcon("icon_mainhud_music")]
	MusicOnMenu,
	[T.EnumName("일지")]
	[EnumIcon("icon_mainhud_story")]
	StoryOnMenu,
	[T.EnumName("타이틀로 돌아가기")]
	[EnumIcon("icon_mainhud_exit")]
	MoveToTitle,
	[T.EnumName("워프 유적")]
	[EnumIcon("icon_mainhud_warpshop")]
	WarpShop
}
