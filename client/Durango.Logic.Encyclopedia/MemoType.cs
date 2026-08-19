using L10N;

namespace Durango.Logic.Encyclopedia;

public enum MemoType
{
	Invalid = -1,
	[T.EnumName("듀랑고 노트")]
	Fiction = 0,
	[T.EnumName("생존 지침")]
	Tooltip = 1,
	[T.EnumName("생존 메모")]
	Survival = 2,
	[T.EnumName("기타")]
	Collect = 100,
	[T.EnumName("단체")]
	Faction = 101
}
