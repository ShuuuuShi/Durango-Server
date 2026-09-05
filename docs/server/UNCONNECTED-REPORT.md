# รายงาน: แพ็กเก็ตที่ตัวเกมส่งแต่เซิร์ฟยังไม่ได้ทำของจริง
อัปเดต: 2026-09-04 · ที่มา: `docs/server/protocol-gap.json` (สแกน `client/**/*.cs` หา `Send(new X)` เทียบกับ `Recv<X>` ใน `server/ServerCore`)
**ตั้งแต่ 4 ก.ย. 2026 ไม่มีแพ็กเก็ตไหน "เงียบ" อีกแล้ว** — `ServerPlayer.Fallback.cs` ตอบทุกตัว: ตัวที่ตอบง่ายได้ OK/ลิสต์ว่าง, ที่เหลือได้ `Abort` พร้อมข้อความ "ระบบนี้ยังไม่เปิดในรุ่นนี้ (ชื่อ)" ตาม Seq ที่ client ส่งมา (client เลิกรอ ไม่ค้าง) · เทส `test-client --fallback-check` 9/9
| | จำนวน |
|---|---|
| message ที่ตัวเกมส่งหาเซิร์ฟจริง | 283 |
| เซิร์ฟมี handler ของจริง | 250 |
| ยังไม่มีของจริง (รายการข้างล่าง) | 141 |
| ในนั้น ตอบแบบเบา ๆ แล้ว (OK/ว่าง/เงียบ) | 9 |
| ตอบ Abort "ยังไม่เปิด" | 132 |

ครอบทั้ง **พอร์ตเกม (8191)** และ **พอร์ตแชท radiotower (8192)** — Keepalive / GetLatestChatLog / คำถามแคลน วิ่งทางพอร์ตแชท (เจอตอนเทสเกมจริงในเครื่อง 4 ก.ย.) ⇒ `RadiotowerServer.cs` มีชุดเดียวกัน · เทสเกมจริง (Clean, client 0.1.4, Online) เข้าโลกแล้ว `/admin/status` → `unhandled_messages: []`

หมายเหตุ: ตัวจับท้ายสุดครอบ message ที่สแกนไม่เจอด้วย (เช่นที่ client ส่งผ่าน helper อื่น) — ดูสถิติจริงได้ที่ `/admin/status` (UnhandledCounts) และ log `[fallback]`

## ตาย / ชุบชีวิต — เฟส S1 (4)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `ConfirmResurrection` | 56238474 | Abort ยังไม่เปิด |
| `Resurrect` | 132 | Abort ยังไม่เปิด |
| `ResurrectPet` | 239187 | Abort ยังไม่เปิด |
| `SetResurrectionRewards` | 133 | Abort ยังไม่เปิด |

## คราฟต์ / ไอเทม — เฟส S2 (16)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `AddItemsToWarehouse` | 3690 | Abort ยังไม่เปิด |
| `CancelCrafting` | 2023 | Abort ยังไม่เปิด |
| `DeliverItems` | 3614 | Abort ยังไม่เปิด |
| `GetReceivedItems` | 3809 | Abort ยังไม่เปิด |
| `GetSectionItems` | 3692 | Abort ยังไม่เปิด |
| `MoveItemsInWarehouse` | 3685 | Abort ยังไม่เปิด |
| `PopItemsFromWarehouse` | 3691 | Abort ยังไม่เปิด |
| `PutInItemsIntoPet` | 806 | Abort ยังไม่เปิด |
| `PutItemsForDomestication` | 694352 | Abort ยังไม่เปิด |
| `RequestResetReformSlot` | 59145 | Abort ยังไม่เปิด |
| `SetRecipeLike` | 3800 | OK (ยังไม่เก็บค่า) |
| `SkipEntrustedCraft` | 7498153 | Abort ยังไม่เปิด |
| `SkipPostprocess` | 2450 | Abort ยังไม่เปิด |
| `TakeOutItemsFromPet` | 807 | Abort ยังไม่เปิด |
| `Tool_Collectibles` | 328 | Abort ยังไม่เปิด |
| `UseItemsForPioneerPoint` | 812234572 | Abort ยังไม่เปิด |

## ภารกิจ / สอนเล่น — เฟส S2 (13)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `AcceptSuggestion` | 9138749 | Abort ยังไม่เปิด |
| `CheckSequenceMissionCleared` | 3631 | Abort ยังไม่เปิด |
| `CustomQuestEvent` | 312798 | Abort ยังไม่เปิด |
| `GetRecommendMissionCost` | 3628 | Abort ยังไม่เปิด |
| `GiveUpDistribution` | 451390 | OK |
| `ParticipateTutorialBoat` | 2303 | Abort ยังไม่เปิด |
| `PutMaterialsIntoTutorialBoat` | 2304 | Abort ยังไม่เปิด |
| `ReceiveAdvisorReward` | 3908 | Abort ยังไม่เปิด |
| `RefuseSuggestion` | 9138750 | Abort ยังไม่เปิด |
| `RequestQuestScoreReward` | 237925 | Abort ยังไม่เปิด |
| `RequestReturnerGuideAction` | 3450984 | Abort ยังไม่เปิด |
| `SkipTutorialMission` | 3633 | Abort ยังไม่เปิด |
| `TutorialEvent` | 701 | Abort ยังไม่เปิด |

## สิ่งปลูกสร้าง / ซ่อม / ที่ดิน — เฟส S3 (13)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `CapsulateArtifact` | 4020 | Abort ยังไม่เปิด |
| `ChangeMannequinDisplay` | 24311 | Abort ยังไม่เปิด |
| `CompleteArtifact` | 2094 | Abort ยังไม่เปิด |
| `ExtendFloor` | 25565 | Abort ยังไม่เปิด |
| `ExtinguishBurnable` | 2097 | Abort ยังไม่เปิด |
| `FireBurnable` | 2096 | Abort ยังไม่เปิด |
| `GetCapsulatingCost` | 4022 | Abort ยังไม่เปิด |
| `GetExpectedCropBooster` | 37123 | Abort ยังไม่เปิด |
| `GrowRapidly` | 3712 | Abort ยังไม่เปิด |
| `RepairArtifact` | 2055 | Abort ยังไม่เปิด |
| `SetArtifactAccess` | 987123450 | Abort ยังไม่เปิด |
| `SetBlueprintLike` | 3801 | OK (ยังไม่เก็บค่า) |
| `Sprinkle` | 37121 | Abort ยังไม่เปิด |

## คลัง / โกดัง — เฟส S3 (6)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `GetWarehouse` | 3683 | Abort ยังไม่เปิด |
| `MakeSection` | 3686 | Abort ยังไม่เปิด |
| `RemoveSection` | 3687 | Abort ยังไม่เปิด |
| `RenameWarehouseSection` | 3696 | Abort ยังไม่เปิด |
| `SetSectionItemOrder` | 3689 | Abort ยังไม่เปิด |
| `SetSectionOrder` | 3688 | Abort ยังไม่เปิด |

## โซเชียล / แชท — เฟส S3/S8 (11)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `AddFavoriteRegionOwners` | 20011 | Abort ยังไม่เปิด |
| `Block` | 4016 | Abort ยังไม่เปิด |
| `GetLatestChatLog` | 25 | ตอบ ChatLogs ว่าง |
| `KickVisitor` | 20424 | Abort ยังไม่เปิด |
| `RadioTalk` | 2601 | Abort ยังไม่เปิด |
| `RemoveFavoriteRegionOwners` | 20012 | Abort ยังไม่เปิด |
| `SetSocialOptions` | 24002 | OK (ยังไม่เก็บค่า) |
| `SetTimelineOption` | 81234528 | OK (ยังไม่เก็บค่า) |
| `ToggleClanNotification` | 4025 | OK (แคลนปิด) |
| `ToggleConversationNotification` | 4011 | OK |
| `Unblock` | 4017 | Abort ยังไม่เปิด |

## วิจัย / สกิลพิเศษ — เฟส S4 (7)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `ChargeEffect` | 820 | Abort ยังไม่เปิด |
| `DrawActiveSkill` | 800101 | Abort ยังไม่เปิด |
| `GetAvailablePersonalResearch` | 5987336 | Abort ยังไม่เปิด |
| `RedrawActiveSkill` | 800102 | Abort ยังไม่เปิด |
| `StartPersonalResearch` | 5987338 | Abort ยังไม่เปิด |
| `TakeEffect` | 821 | Abort ยังไม่เปิด |
| `UsePetActiveSkill` | 800200 | Abort ยังไม่เปิด |

## เกาะ / วาร์ป / เดินทาง — เฟส S6 (18)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `ActivePersonalRegionWarphole` | 3022 | Abort ยังไม่เปิด |
| `GetRegion` | 2120 | Abort ยังไม่เปิด |
| `GetRegionMapInfo` | 205 | Abort ยังไม่เปิด |
| `GetRoutes` | 2030 | Abort ยังไม่เปิด |
| `GetRoutesOfParty` | 20300 | Abort ยังไม่เปิด |
| `OpenMap` | 915 | Abort ยังไม่เปิด |
| `RecommendArchipelago` | 3012 | Abort ยังไม่เปิด |
| `RecommendPersonalRegion` | 3002 | Abort ยังไม่เปิด |
| `RecommendRegion` | 3001 | Abort ยังไม่เปิด |
| `RequestFullCountPOIsReward` | 9031 | Abort ยังไม่เปิด |
| `RequestNearestPOI` | 911 | Abort ยังไม่เปิด |
| `SailingBack` | 3130 | Abort ยังไม่เปิด |
| `SetCargoWarpholeTaxRate` | 3816 | Abort ยังไม่เปิด |
| `TravelByRegionInArchipelago` | 2054 | Abort ยังไม่เปิด |
| `TravelToStableRegion` | 20321235 | Abort ยังไม่เปิด |
| `WarpToPersonalRegion` | 3023 | Abort ยังไม่เปิด |
| `WarpToUrbanRegion` | 3024 | Abort ยังไม่เปิด |
| `Weather` | 331 | รับเงียบ |

## สัตว์เลี้ยง / เชือกจูง — เฟส S7 (15)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `AcceptPetRank` | 74016 | Abort ยังไม่เปิด |
| `CancelPetTask` | 65103 | Abort ยังไม่เปิด |
| `Feeding` | 805 | Abort ยังไม่เปิด |
| `FinishPetTask` | 65104 | Abort ยังไม่เปิด |
| `GetPetInventory` | 49823 | Abort ยังไม่เปิด |
| `PutInReinsToCage` | 694351 | Abort ยังไม่เปิด |
| `ReinifyPet` | 74013 | Abort ยังไม่เปิด |
| `ReleaseReinFromCage` | 694353 | Abort ยังไม่เปิด |
| `ReturnPet` | 808 | Abort ยังไม่เปิด |
| `RevertPetRank` | 74014 | Abort ยังไม่เปิด |
| `SpawnPet` | 923570 | Abort ยังไม่เปิด |
| `StartPetTask` | 65102 | Abort ยังไม่เปิด |
| `TakeOutFromCage` | 810 | Abort ยังไม่เปิด |
| `TakeOutReinFromCage` | 694359 | Abort ยังไม่เปิด |
| `Wash` | 13497 | Abort ยังไม่เปิด |

## แคลน — เฟส หลัง S8 (4)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `CancelClanJoinRequest` | 1923487521 | Abort ยังไม่เปิด |
| `SetClanEmblem` | 3695 | Abort ยังไม่เปิด |
| `SetClanInfo` | 3699 | Abort ยังไม่เปิด |
| `SetClanMemberRole` | 3662 | Abort ยังไม่เปิด |

## ร้านค้า / เงินสด / คูปอง — เฟส ตัด (10)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `AcceptPurchase` | 5247809 | Abort ยังไม่เปิด |
| `AcceptTENCoupon` | 2345690 | Abort ยังไม่เปิด |
| `GetRechargeShuffleCost` | 3625 | Abort ยังไม่เปิด |
| `GetTechSupportEstimates` | 59138 | Abort ยังไม่เปิด |
| `PickMilestone` | 800012 | Abort ยังไม่เปิด |
| `PickMilestoneAgain` | 800014 | Abort ยังไม่เปิด |
| `PurchaseCommodity` | 856710 | Abort ยังไม่เปิด |
| `RequestTechSupport` | 59144 | Abort ยังไม่เปิด |
| `RequestTechSupportEstimate` | 59141 | Abort ยังไม่เปิด |
| `Withdraw` | 2028 | Abort ยังไม่เปิด |

## คอนเสิร์ต / มินิเกม — เฟส ตัด (12)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `FinishConcert` | 63459101 | Abort ยังไม่เปิด |
| `GetPunchMachineLeaderboard` | 785103 | Abort ยังไม่เปิด |
| `HostConcert` | 63459079 | Abort ยังไม่เปิด |
| `LookAroundMood` | 234789 | Abort ยังไม่เปิด |
| `MiniGameDanceScore` | 4625400 | Abort ยังไม่เปิด |
| `PlayConcert` | 63459081 | Abort ยังไม่เปิด |
| `RegisterConcert` | 63459082 | Abort ยังไม่เปิด |
| `Scribble` | 319 | Abort ยังไม่เปิด |
| `SetConcertMusic` | 63459080 | Abort ยังไม่เปิด |
| `SetSharedConcertMusic` | 63459180 | Abort ยังไม่เปิด |
| `TurnOffMusic` | 3892 | Abort ยังไม่เปิด |
| `TurnOnMusic` | 3891 | Abort ยังไม่เปิด |

## ยานพาหนะ / บอลลูน — เฟส ตัด (4)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `FireProjectileFromVehicle` | 203493 | Abort ยังไม่เปิด |
| `MountAirBalloon` | 123987 | Abort ยังไม่เปิด |
| `MountVehicle` | 327918 | Abort ยังไม่เปิด |
| `UnmountVehicle` | 192834 | Abort ยังไม่เปิด |

## อื่น ๆ — เฟส ดูรายตัว (8)

| message | TypeCode | ตอนนี้เซิร์ฟตอบ |
|---|---|---|
| `ContactReactingProp` | 78452083 | Abort ยังไม่เปิด |
| `DiscoverAnimal` | 5002 | Abort ยังไม่เปิด |
| `FindTargetEntityPosition` | 3950 | Abort ยังไม่เปิด |
| `InteractWithEpicNPC` | 3141593 | Abort ยังไม่เปิด |
| `InvestToCrack` | 3663 | Abort ยังไม่เปิด |
| `RepairImmediate` | 2056 | Abort ยังไม่เปิด |
| `RequestDumpedPersonalIsland` | 381922 | Abort ยังไม่เปิด |
| `SuggestBreak` | 9138748 | Abort ยังไม่เปิด |
