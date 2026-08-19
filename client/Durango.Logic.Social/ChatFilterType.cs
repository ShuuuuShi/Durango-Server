using L10N;

namespace Durango.Logic.Social;

public enum ChatFilterType
{
	[T.EnumName("전체")]
	All,
	[T.EnumName("지역")]
	Region,
	[T.EnumName("개인섬 연합")]
	PersonalRegions,
	[T.EnumName("부족")]
	Clan,
	[T.EnumName("시스템")]
	System,
	[T.EnumName("부족 전쟁")]
	ClanWar,
	[T.EnumName("파티")]
	Party
}
