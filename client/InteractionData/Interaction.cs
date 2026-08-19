using L10N;

namespace InteractionData;

public enum Interaction
{
	[Menu(0, MenuType.Small)]
	Invalid = -1,
	[Menu(100, MenuType.Small)]
	None = 0,
	[T.EnumName("공격!")]
	[Menu(200)]
	Attack = 1,
	[T.EnumName("건설")]
	[Menu(300)]
	BuildArtifact = 101,
	[T.EnumName("제거")]
	[Menu(5, MenuType.Small)]
	DestructArtifact = 103,
	[T.EnumName("수리")]
	[Menu(50, MenuType.Small)]
	RepairArtifact = 108,
	[T.EnumName("완성")]
	[Menu(310)]
	CompleteArtifact = 110,
	[T.EnumName("내부 꾸미기")]
	[Menu(250)]
	AddOnManage = 111,
	[T.EnumName("마무리 도와주기")]
	[Menu(310)]
	HelpPostprocess = 112,
	[T.EnumName("이력 보기")]
	[Menu(20, MenuType.Small)]
	GetTimeline = 113,
	[T.EnumName("포장")]
	[Menu(10, MenuType.Small)]
	Capsulate = 114,
	[T.EnumName("즉시 완료")]
	[Menu(100, MenuType.Small)]
	RepairArtifactImmediately = 115,
	[T.EnumName("개조하기")]
	[Menu(260)]
	RemodelArtifact = 116,
	[T.EnumName("권한 조정")]
	[Menu(200)]
	EstateLicense = 201,
	[T.EnumName("사유지 확장")]
	[Menu(50, MenuType.Small)]
	ExtendEstate = 202,
	[Menu(100, MenuType.Small)]
	SailingRegisterRoute = 301,
	[T.EnumName("출항")]
	[Menu(200)]
	SailingExplore = 302,
	[T.EnumName("배 타기")]
	[Menu(210)]
	SailingRoutes = 303,
	[T.EnumName("여행")]
	[Menu(50, MenuType.Small)]
	SailingTravel = 304,
	[T.EnumName("마지막 안정섬으로 항해하기")]
	[Menu(300)]
	SailingWithdraw = 305,
	[T.EnumName("개인 섬 항해")]
	[Menu(200, MenuType.Small)]
	SailingPersonalRegion = 306,
	[T.EnumName("가까운 섬으로 항해하기")]
	[Menu(201)]
	SailingArchipelagoRegion = 307,
	[T.EnumName("파티리더의 항로 이용하기")]
	[Menu(205)]
	SailingRoutesOfParty = 308,
	[T.EnumName("모험 복귀")]
	[Menu(205)]
	SailingBack = 309,
	[T.EnumName("무작위 항해")]
	[Menu(199, MenuType.Small)]
	SailingRandomPersonalRegion = 310,
	[T.EnumName("닫기")]
	[Menu(310)]
	CloseGate = 402,
	[T.EnumName("열기")]
	[Menu(200)]
	OpenGate = 403,
	[T.EnumName("내용물 보기")]
	[Menu(200)]
	Inventory = 404,
	[T.EnumName("함정 열기")]
	[Menu(200)]
	OpenTrap = 405,
	[Menu(200)]
	OpenWorkbench = 406,
	[T.EnumName("휴식")]
	[Menu(300)]
	Rest = 407,
	[T.EnumName("메시지 적기")]
	[Menu(90, MenuType.Small)]
	ScribbleText = 408,
	[T.EnumName("그리기")]
	[Menu(90, MenuType.Small)]
	ScribbleDrawing = 409,
	[T.EnumName("귀환 지점으로 지정")]
	[Menu(90, MenuType.Small)]
	SetAsHome = 410,
	[T.EnumName("거점으로 지정")]
	[Menu(50, MenuType.Small)]
	SetAsBase = 411,
	[Menu(200)]
	[EnumIcon("act_take")]
	[T.EnumName("꺼내기")]
	Take = 412,
	[T.EnumName("가판대")]
	[Menu(200)]
	UseKiosk = 413,
	[T.EnumName("창고 열기")]
	[Menu(400)]
	UseWarehouse = 415,
	[Menu(200)]
	ClanResearch = 416,
	[T.EnumName("내용물 꺼내기")]
	[Menu(200)]
	BrokenInventory = 417,
	[T.EnumName("씻기")]
	[Menu(200)]
	Wash = 418,
	[T.EnumName("물뿌리기")]
	[Menu(200)]
	Sprinkle = 419,
	[T.EnumName("비료 넣기")]
	[Menu(200)]
	PutInLiquidFertilizer = 420,
	[T.EnumName("기술지원")]
	[Menu(260)]
	OpenTechSupport = 421,
	[T.EnumName("개인섬 개척도")]
	[EnumIcon("act_pioneer_grade")]
	[Menu(260)]
	ManagePioneerGrade = 422,
	[Menu(200)]
	[T.EnumName("연구하기")]
	[EnumIcon("act_clan_research")]
	PersonalResearch = 423,
	[T.EnumName("제작")]
	[Menu(200)]
	Craft = 501,
	[T.EnumName("심폐소생술")]
	[Menu(150)]
	Resurrect = 503,
	[Menu(200)]
	Revive = 504,
	[T.EnumName("구조 요청")]
	[Menu(210)]
	SetReviveReward = 505,
	[Menu(150)]
	Collect = 506,
	[T.EnumName("부족으로 초대")]
	[Menu(50, MenuType.Small)]
	InviteToClan = 507,
	[T.EnumName("씨앗 심기")]
	[Menu(200)]
	Plant = 508,
	[T.EnumName("비료 주기")]
	[Menu(100, MenuType.Small)]
	Fertilize = 509,
	[T.EnumName("물 주기")]
	[Menu(150)]
	Watering = 510,
	[T.EnumName("농작물 제거")]
	[Menu(50, MenuType.Small)]
	Uproot = 511,
	[T.EnumName("불 붙이기")]
	[Menu(150)]
	Fire = 512,
	[T.EnumName("불 끄기")]
	[Menu(80, MenuType.Small)]
	Extinguish = 513,
	[T.EnumName("동물 관리")]
	[Menu(200)]
	Cage = 514,
	[T.EnumName("워프")]
	[Menu(300)]
	Warp = 515,
	[Menu(90, MenuType.Small)]
	WarpBack = 516,
	[T.EnumName("파티로 초대")]
	[Menu(10)]
	InviteIntoParty = 517,
	[Menu(190)]
	CreateClan = 518,
	[T.EnumName("염색하기")]
	[Menu(200)]
	Dye = 519,
	[T.EnumName("빠른 성장")]
	[Menu(200)]
	GrowRapidly = 520,
	[T.EnumName("게시판 읽기")]
	[Menu(0, MenuType.Small)]
	ReadPinboard = 521,
	[T.EnumName("티스톤 넣기")]
	[Menu(0, MenuType.Small)]
	OnePunch = 524,
	[T.EnumName("순위표")]
	[Menu(0, MenuType.Small)]
	ViewPunchRanking = 525,
	[T.EnumName("열기구 타기")]
	[Menu(0, MenuType.Small)]
	RideBalloon = 526,
	[T.EnumName("탈색하기")]
	[Menu(190)]
	Bleach = 527,
	[T.EnumName("이름 짓기")]
	[Menu(0, MenuType.Small)]
	NameArtifact = 528,
	[T.EnumName("이름 바꾸기")]
	[Menu(0, MenuType.Small)]
	RenameArtifact = 529,
	[T.EnumName("향 태우기")]
	ChargeEffect = 530,
	[T.EnumName("향 사용하기")]
	TakeEffect = 531,
	[T.EnumName("즉시 부활")]
	[Menu(200)]
	ReviveImmediately = 532,
	[T.EnumName("동물 관리")]
	OpenDomesticCage = 533,
	[T.EnumName("음악 켜기")]
	TurnOnMusic = 534,
	[T.EnumName("음악 끄기")]
	TurnOffMusic = 535,
	[T.EnumName("댄스 타임")]
	[Menu(200)]
	MiniGameDance = 537,
	[T.EnumName("모자 씌우기")]
	[Menu(200)]
	ChangeMannequinHead = 550,
	[T.EnumName("옷 입히기")]
	[Menu(200)]
	ChangeMannequinBody = 551,
	[T.EnumName("합주 시작")]
	[EnumIcon("icon_hormony")]
	[Menu(200)]
	HostConcert = 552,
	[T.EnumName("합주 참여")]
	[EnumIcon("icon_hormony")]
	[Menu(200)]
	RegisterConcert = 553,
	[T.EnumName("임무 받기")]
	[Menu(400)]
	AcceptMission = 601,
	[T.EnumName("임무 중단")]
	[Menu(380)]
	CancelMission = 602,
	[T.EnumName("임무 모두 중단")]
	[Menu(150)]
	CancelAllMissions = 603,
	[T.EnumName("워프홀 부활")]
	[Menu(350)]
	ReviveAtWarphole = 605,
	[T.EnumName("자원유도석 묻기")]
	[Menu(0, MenuType.Small)]
	Invest = 608,
	[T.EnumName("연락 확인")]
	[Menu(0, MenuType.Small)]
	ReportFactionProp = 611,
	[T.EnumName("엽록포럼")]
	[Menu(300)]
	DeliveryChlorophylForum = 612,
	[T.EnumName("개척회의")]
	[Menu(300)]
	DeliveryChamberOfPioneer = 613,
	[T.EnumName("회사")]
	[Menu(300)]
	DeliveryTheFirm = 614,
	[T.EnumName("위원회")]
	[Menu(300)]
	DeliveryTheCommittee = 615,
	[T.EnumName("라마")]
	[Menu(0, MenuType.Small)]
	DeliveryLama = 616,
	[T.EnumName("구조자TF")]
	[Menu(300)]
	DeliveryRescueTf = 617,
	[T.EnumName("활성화")]
	[EnumIcon("act_activation")]
	ActivatePersonalRegionWarphole = 630,
	[T.EnumName("워프")]
	[EnumIcon("act_warphole")]
	WarpToPersonalRegion = 631,
	[T.EnumName("워프")]
	[EnumIcon("act_warphole")]
	WarpToUrbanRegion = 632,
	[T.EnumName("사유지로 전송")]
	[Menu(0, MenuType.Small)]
	WarpCargoToPrivate = 650,
	[T.EnumName("부족으로 전송")]
	[Menu(0, MenuType.Small)]
	WarpCargoToClan = 651,
	[T.EnumName("활성화")]
	[Menu(0, MenuType.Small)]
	ActivateCargoReceiver = 652,
	[Menu(0, MenuType.Small)]
	TaxToClanFund = 653,
	[Menu(0, MenuType.Small)]
	SetTaxPercentage = 654,
	[T.EnumName("꺼내기")]
	[Menu(0, MenuType.Small)]
	GetCargoItems = 655,
	[T.EnumName("점령하기")]
	[Menu(0, MenuType.Small)]
	StartToOccupyCargoWarphole = 656,
	[T.EnumName("워프 가속기 설치 요청")]
	[EnumIcon("act_warp_acc")]
	[Menu(200)]
	Accelerate = 670,
	[T.EnumName("내용물 확인")]
	[EnumIcon("act_take")]
	[Menu(200)]
	ReceiveAccelerationRewards = 671,
	[T.EnumName("워프 가속 지원")]
	[EnumIcon("act_warp_acc")]
	[Menu(200)]
	ParticipateAcceleration = 672,
	[T.EnumName("프로필 보기")]
	[Menu(400)]
	GetProfile = 701,
	[T.EnumName("1:1 대화하기")]
	[Menu(390)]
	Whisper = 702,
	KickFromPersonalRegion = 703,
	[T.EnumName("신고")]
	[Menu(30, MenuType.Small)]
	SendReport = 801,
	[T.EnumName("타기")]
	[Menu(250)]
	Mount = 901,
	[T.EnumName("내리기")]
	[Menu(300)]
	Dismount = 902,
	[T.EnumName("가방")]
	[Menu(300)]
	OpenPetInven = 903,
	[T.EnumName("먹이 주기")]
	[Menu(300)]
	Feeding = 904,
	[T.EnumName("소환 해제")]
	[Menu(50, MenuType.Small)]
	ReturnPet = 905,
	[T.EnumName("이름 변경")]
	[Menu(20, MenuType.Small)]
	RenamePet = 906,
	[T.EnumName("살리기")]
	[Menu(300)]
	ResurrectPet = 907,
	[T.EnumName("투척기 타기")]
	[Menu(0, MenuType.Small)]
	MountVehicle = 910,
	[Menu(0, MenuType.Small)]
	DismountVehicle = 911,
	[T.EnumName("투척물 넣기")]
	[Menu(0, MenuType.Small)]
	AddProjectileToVehicle = 912,
	[T.EnumName("재료 넣기")]
	[Menu(300)]
	BuildTutorialBoat = 10101,
	[T.EnumName("참여하기")]
	[Menu(310)]
	ParticipateTutorialBoat = 10102,
	[T.EnumName("떠나기")]
	[Menu(350)]
	DepartTutorial = 10103,
	[T.EnumName("떠나기")]
	[Menu(0, MenuType.Small)]
	DepartTutorialAirballoon = 10104,
	[T.EnumName("생존식량 먹이기")]
	[Menu(0)]
	RescueWithFood = 10201,
	[T.EnumName("물 주기")]
	[Menu(0)]
	RescueWithWater = 10202,
	[T.EnumName("약 먹이기")]
	[Menu(0)]
	RescueWithMedicine = 10203,
	[T.EnumName("응급처치")]
	[Menu(0)]
	RescueWithCpr = 10204,
	[T.EnumName("신원 확인")]
	[Menu(0)]
	ConfirmIdentity = 10205,
	[T.EnumName("고장내기")]
	[Menu(0)]
	BreakDownRadio = 10206,
	[T.EnumName("서류 건네주기")]
	[Menu(0)]
	GiveRequestedPapers = 10208,
	[T.EnumName("지갑 건네주기")]
	[Menu(0)]
	GiveRequestedWallet = 10209,
	[T.EnumName("노트 건네주기")]
	[Menu(0)]
	GiveRequestedNote = 10210,
	[T.EnumName("물 건네주기")]
	[Menu(0)]
	GiveWater = 10211,
	[T.EnumName("음식 건네주기")]
	[Menu(0)]
	GiveFood = 10212,
	[T.EnumName("입을 것 건네주기")]
	[Menu(0)]
	GiveArmor = 10213,
	[T.EnumName("50레벨 이상 의상 수리키트 전달")]
	[Menu(0)]
	GiveClothesRepairKit50 = 10214,
	[T.EnumName("50레벨 이상 과일 주스 전달")]
	[Menu(0)]
	GiveJuiceFruit50 = 10215,
	[T.EnumName("50레벨 이상 작업 도끼 전달")]
	[Menu(0)]
	GiveAxeTool50 = 10216,
	[T.EnumName("50레벨 이상 작업 망치 전달")]
	[Menu(0)]
	GiveHammerTool50 = 10217,
	[T.EnumName("50레벨 이상 새끼줄 전달")]
	[Menu(0)]
	GiveRope50 = 10218,
	[T.EnumName("재가동")]
	[Menu(0)]
	RebootSystem = 10219,
	[T.EnumName("좌표 입수")]
	[Menu(0)]
	GetCoordinates = 10220,
	[T.EnumName("전원 차단")]
	[Menu(0)]
	PowerDown = 10221,
	[T.EnumName("전파 수신")]
	[Menu(0)]
	GetSignal = 10222,
	[T.EnumName("노이즈 수신")]
	[Menu(0)]
	GetNoise = 10223,
	[T.EnumName("건물 이용하기")]
	[Menu(0)]
	GetStatusEffect = 10224,
	[T.EnumName("독 주입하기")]
	[Menu(0)]
	ArchipelagoGivePoisonsac = 10225,
	[T.EnumName("연구거점 알아내기")]
	[Menu(0)]
	ArchipelagoDiscoverSite = 10226,
	[T.EnumName("용암 붓기!")]
	[Menu(0)]
	ArchipelagoGiveLava = 10227,
	[T.EnumName("무기 납품하기")]
	[Menu(0)]
	ArchipelagoWeaponStone = 10228,
	[T.EnumName("무기 납품하기")]
	[Menu(0)]
	ArchipelagoWeaponBone = 10229,
	[T.EnumName("무기 납품하기")]
	[Menu(0)]
	ArchipelagoWeaponMetal = 10230,
	[T.EnumName("문서 넣어두기")]
	[Menu(0)]
	ArchipelagoGivePaper = 10231,
	[T.EnumName("유품 넣어두기")]
	[Menu(0)]
	ArchipelagoGiveKeepsake = 10232,
	[T.EnumName("도청장치 제거")]
	[Menu(0)]
	ArchipelagoRemoveBug = 10233,
	[T.EnumName("조사하기")]
	[Menu(0, MenuType.Small)]
	EpicFindSecretDocument = 10234,
	[T.EnumName("연락하기")]
	[Menu(0, MenuType.Small)]
	EpicContactPioneercouncil = 10235,
	[T.EnumName("연락하기")]
	[Menu(0, MenuType.Normal)]
	EpicContactChlorophylfourm = 10236,
	[T.EnumName("수리하기")]
	[Menu(0, MenuType.Normal)]
	EpicWarehouseFix = 10237,
	[T.EnumName("기밀문서 전달하기")]
	[Menu(0)]
	EpicWarehouseSecretDocument = 10238,
	[T.EnumName("확인하기")]
	[Menu(0)]
	EpicLighthouse = 10239,
	[T.EnumName("확인하기")]
	[Menu(0)]
	EpicSilo = 10240,
	[T.EnumName("확인하기")]
	[Menu(0)]
	EpicTrap = 10241,
	[T.EnumName("불 밝히기")]
	[Menu(0)]
	EpicLighthouseLightUp = 10242,
	[T.EnumName("폭탄 부착하기")]
	[Menu(0)]
	EpicSiloAttachBomb = 10243,
	[T.EnumName("확인하기")]
	[Menu(0)]
	EpicLastStaion = 10244,
	OpenStorageCage = 10245,
	[T.EnumName("먹이 보관")]
	[Menu(350)]
	Trough = 10246,
	[T.EnumName("권한 설정")]
	[Menu(200)]
	SetAccess = 10247,
	[T.EnumName("대시")]
	[Menu(100, MenuType.Small)]
	Dash = 10248,
	[T.EnumName("선전포고")]
	[Menu(0, MenuType.Small)]
	DeclareWar = 10249,
	[Menu(200)]
	ClientSidePropAction = 10250,
	[T.EnumName("즉시 완료")]
	[Menu(200)]
	SkipPostprocess = 10251,
	[T.EnumName("이삿짐 싸기")]
	[Menu(300)]
	PackArtifact = 10252,
	[T.EnumName("워프홀 탐지")]
	[Menu(300)]
	SearchWarphole = 10253,
	[T.EnumName("항구로 워프")]
	[Menu(490)]
	WarpToPort = 10254,
	[T.EnumName("씻기")]
	[Menu(250)]
	WashBody = 10255,
	[T.EnumName("물 마시기")]
	[Menu(150)]
	DrinkWater = 10256,
	[T.EnumName("건물 상호작용")]
	[Menu(200)]
	InteractionArtifact = 10257,
	[T.EnumName("사유지 메뉴")]
	[Menu(470)]
	EstateMenu = 10258,
	[T.EnumName("뜨기")]
	[Menu(200)]
	SelectDrawContainer = 10259,
	[Menu(200)]
	DrawWater = 10260,
	[Menu(200)]
	DrawLava = 10261,
	[T.EnumName("열기구 타기")]
	[Menu(200)]
	MountAirBalloon = 10262,
	[T.EnumName("열기구 내리기")]
	[Menu(200)]
	DismountAirBalloon = 10263,
	[T.EnumName("스크린샷 캡쳐")]
	[Menu(200)]
	CaptureScreenShot = 10264,
	[T.EnumName("동물 메뉴")]
	[Menu(0, MenuType.Small)]
	OpenPetMenu = 10265,
	[T.EnumName("전리품 포기")]
	[Menu(0, MenuType.Small)]
	GiveUpDistribution = 10266,
	[EnumIcon("skill_dynamic_visual_acuity_1")]
	[T.EnumName("외형 변경")]
	[Menu(0, MenuType.Normal)]
	ChangeDecoration = 10267,
	[EnumIcon("act_blowup")]
	[T.EnumName("제거")]
	[Menu(0, MenuType.Small)]
	RemoveNatural = 10268,
	[T.EnumName("동물 특수 행동")]
	[Menu(0, MenuType.Normal)]
	PetActiveSkill = 10269,
	[EnumIcon("act_modular_vertical_extension")]
	[T.EnumName("증축하기")]
	[Menu(200)]
	ExtendFloor = 10270,
	[EnumIcon("act_modular_vertical_extension")]
	[T.EnumName("지붕과 함께")]
	[Menu(200)]
	ExtendFloorWithRoof = 10271,
	[EnumIcon("act_modular_vertical_extension_rooftop")]
	[T.EnumName("지붕 없이")]
	[Menu(200)]
	ExtendFloorWithoutRoof = 10272,
	[T.EnumName("둘러보기")]
	[Menu(200)]
	LookAroundArtifact = 10273,
	TakeOffMannequinHead = 10274,
	TakeOffMannequinBody = 10275,
	[EnumIcon("act_up")]
	[T.EnumName("위층으로")]
	[Menu(200)]
	ToUpstair = 10276,
	[EnumIcon("act_down")]
	[T.EnumName("아래층으로")]
	[Menu(200)]
	ToDownstair = 10277,
	[EnumIcon("act_blowup")]
	[T.EnumName("제거")]
	[Menu(0, MenuType.Small)]
	RemoveGrazingPet = 10278,
	[EnumIcon("act_blowup")]
	[T.EnumName("제거")]
	[Menu(0, MenuType.Small)]
	RemoveAppearAnimal = 10279,
	[EnumIcon("act_clan_research")]
	[T.EnumName("테스트")]
	TestInteraction = 10280,
	[EnumIcon("act_clan_research")]
	[T.EnumName("건축가 추가")]
	AddArchitect = 10281
}
