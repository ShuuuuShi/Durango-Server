using L10N;

namespace ClanData;

public enum ClanAction
{
	[T.EnumName("가입 수락")]
	Approve,
	[T.EnumName("가입 거절")]
	DropApplier,
	[T.EnumName("추방")]
	Kick,
	[T.EnumName("탈퇴")]
	Leave,
	[T.EnumName("정보")]
	MemberInfo,
	[T.EnumName("등급 관리")]
	EditClanInfo,
	[T.EnumName("등급 변경")]
	PromoteMember
}
