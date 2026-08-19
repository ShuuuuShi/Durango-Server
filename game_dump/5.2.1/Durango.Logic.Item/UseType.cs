using L10N;

namespace Durango.Logic.Item;

public enum UseType
{
	[T.EnumName("사용하기")]
	None,
	[T.EnumName("수리하기")]
	Repair,
	[T.EnumName("먹기")]
	Eat,
	[T.EnumName("마시기")]
	Drink,
	[T.EnumName("길들이기")]
	Taming,
	[T.EnumName("귀속하기")]
	Imprint,
	[T.EnumName("외형 변경")]
	ChangeDisplay,
	[T.EnumName("장착하기")]
	Equip,
	[T.EnumName("벗기")]
	UnEquip,
	[T.EnumName("배치하기")]
	Place,
	[T.EnumName("부활 보상 제안")]
	ResurrectionRewards,
	[T.EnumName("사용하기")]
	Ticket,
	[T.EnumName("가방으로 옮기기")]
	TakeOut,
	[T.EnumName("옮기기")]
	PutIn,
	[T.EnumName("버리기")]
	Drop,
	[T.EnumName("배우기")]
	GainRecipes,
	[T.EnumName("건설")]
	Build,
	[T.EnumName("개봉하기")]
	OpenBox,
	[T.EnumName("사용하기")]
	Use,
	[T.EnumName("염색하기")]
	Dye,
	[T.EnumName("길들이기")]
	Grazing
}
