using L10N;

namespace ItemSystem;

public enum UseType
{
	[T.EnumName("사용하기")]
	None,
	[T.EnumName("가방으로 옮기기")]
	TakeOut,
	[T.EnumName("옮기기")]
	PutIn,
	[T.EnumName("먹기")]
	Eat,
	[T.EnumName("마시기")]
	Drink,
	[T.EnumName("소환/해제")]
	ToggleSpawn,
	[T.EnumName("장착하기")]
	Equip,
	[T.EnumName("벗기")]
	UnEquip,
	[T.EnumName("물주기")]
	Water,
	[T.EnumName("수리하기")]
	Repair,
	[T.EnumName("배치하기")]
	Place,
	[T.EnumName("부활 보상 제안")]
	Resurrection_Rewards,
	[T.EnumName("이삿짐 싸기")]
	PackArtifact,
	[T.EnumName("이삿짐 풀기")]
	UnpackArtifact,
	[T.EnumName("복사")]
	CheatCopy
}
