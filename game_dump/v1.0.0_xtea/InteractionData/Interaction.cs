using L10N;

namespace InteractionData;

public enum Interaction
{
	[T.EnumName("즉시 완료")]
	SkipPostprocess = 0,
	CraftingItem = 1,
	[T.EnumName("이사용 상자 구입")]
	StartPakingArtifact = 2,
	[T.EnumName("이삿짐 싸기")]
	PackingArtifact = 3,
	[T.EnumName("이삿짐 풀기")]
	UnpackArtifact = 4,
	[T.EnumName("이삿짐 싸기")]
	PackArtifact = 5,
	[T.EnumName("워프홀 탐지")]
	SearchWarphole = 6,
	[T.EnumName("씻기")]
	WashBody = 7,
	[T.EnumName("물 뜨기")]
	DrawWater = 8,
	[T.EnumName("물 마시기")]
	DrinkWater = 9,
	InteractionArtifact = 10,
	DeclareWar = 11,
	[T.EnumName("사유지 권한")]
	ManagerEstateLicense = 12,
	[T.EnumName("사유지 확장")]
	ExtendEstateUnit = 13,
	[T.EnumName("워프")]
	Warp = 14,
	[T.EnumName("역워프")]
	WarpBack = 15,
	VehicleInteractionsBegin = 16,
	[T.EnumName("타기")]
	Mount = 17,
	[T.EnumName("내리기")]
	Unmount = 18,
	[T.EnumName("가방")]
	PetInventory = 19,
	[T.EnumName("먹이 주기")]
	FeedPet = 20,
	[T.EnumName("소환 해제")]
	ReturnPet = 21,
	[T.EnumName("이름 변경")]
	RenamePet = 22,
	VehicleInteractionsEnd = 23,
	[T.EnumName("식물 채집 연구")]
	ResearchPlant = 24,
	[T.EnumName("광물 채집 연구")]
	ResearchMine = 25,
	[T.EnumName("동물 도축 연구")]
	ResearchAnimal = 26,
	[T.EnumName("도구 제작 연구")]
	ResearchTool = 27,
	[T.EnumName("의상 제작 연구")]
	ResearchClothes = 28,
	[T.EnumName("요리 연구")]
	ResearchCook = 29,
	[T.EnumName("건축 연구")]
	ResearchConstruction = 30,
	[T.EnumName("생존 연구")]
	ResearchSurvival = 31,
	[T.EnumName("생태 연구")]
	ResearchEcology = 32,
	[T.EnumName("공격 연구")]
	ResearchAttack = 33,
	[T.EnumName("방어 연구")]
	ResearchDefense = 34,
	[T.EnumName("회복 연구")]
	ResearchRecovery = 35,
	ClientSidePropAction = 10000
}
