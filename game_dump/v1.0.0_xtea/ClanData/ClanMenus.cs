using L10N;

namespace ClanData;

public enum ClanMenus
{
	Invalid = -1,
	[T.EnumName("창설")]
	MakeClan,
	[T.EnumName("정보")]
	Info,
	[T.EnumName("부족원")]
	Members,
	[T.EnumName("레벨")]
	Level,
	[T.EnumName("타임라인")]
	Timeline,
	[T.EnumName("검색")]
	ClanList
}
