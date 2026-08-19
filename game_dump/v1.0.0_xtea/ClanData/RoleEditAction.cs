using L10N;

namespace ClanData;

public enum RoleEditAction
{
	MoveToFront,
	MoveToBack,
	[T.EnumName("등급 삭제")]
	Delete,
	[T.EnumName("등급 이름 변경")]
	ChangeName
}
