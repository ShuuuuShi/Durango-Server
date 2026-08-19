using L10N;

namespace Durango.Logic.Mail;

public enum CategoryType
{
	[T.EnumName("계정 우편")]
	User,
	[T.EnumName("누군가의 편지")]
	GM,
	[T.EnumName("전체")]
	All,
	[T.EnumName("상점")]
	Shop,
	[T.EnumName("시스템")]
	System
}
