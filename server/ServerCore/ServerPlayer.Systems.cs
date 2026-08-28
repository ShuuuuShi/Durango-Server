using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Systems — ระบบย่อยทั้งหมดที่เหลือ (S5-S7) แบบ gated
///
/// แต่ละ entry point จะ:
/// - Query → ส่ง empty response (list/dict ว่าง) เมื่อ feature ปิด
/// - Mutation → Info+Abort เมื่อ feature ปิด
///
/// ระบบที่ยังไม่ได้ implement full logic:
/// Market, Pets (Taming/Livestock), Factions, Missions, Attendance, Cargo,
/// Archipelago, Band, AddOns, DyeAndBleach, Estate, Encyclopedia, PrivateConversation
///
/// ข้อจำกัด per discipline: ห้าม mutation, ห้าม UI hang, ต้องมี response เสมอ
/// </summary>
public partial class ServerPlayer
{
    // ── Market ────────────────────────────────────────────────────────

    private void RegisterMarketHandlers()
    {
        _conn.Recv<GetCommodities>(HandleGetCommodities);
        _conn.Recv<SearchProducts>(HandleSearchProducts);
        _conn.Recv<GetRegisteredProducts>(HandleGetRegisteredProducts);
        _conn.Recv<GetSoldProducts>(HandleGetSoldProducts);
        _conn.Recv<GetPurchasedProducts>(HandleGetPurchasedProducts);
        _conn.Recv<GetPersonalProducts>(HandleGetPersonalProducts);
        _conn.Recv<GetExpiredProducts>(HandleGetExpiredProducts);
        _conn.Recv<GetSimilarProducts>(HandleGetSimilarProducts);
        _conn.Recv<GetOffers>(HandleGetOffers);
        _conn.Recv<RegisterProduct>(HandleRegisterProduct);
        _conn.Recv<RegisterMultipleProducts>(HandleRegisterMultipleProducts);
        _conn.Recv<BuyProduct>(HandleBuyProduct);
        _conn.Recv<UnregisterProduct>(HandleUnregisterProduct);
        _conn.Recv<WithdrawProduct>(HandleWithdrawProduct);
        _conn.Recv<AddToFavoriteProducts>(HandleAddToFavoriteProducts);
        _conn.Recv<RemoveFromFavoriteProducts>(HandleRemoveFromFavoriteProducts);
        _conn.Recv<GetFavoriteProducts>(HandleGetFavoriteProducts);
    }

    private bool RejectMarketDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Market) return true;
        Send(new Info { Text = "ระบบตลาดยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleGetCommodities(GetCommodities msg, PacketHeader header)
    {
        Send(new Commodities { CommodityInfos = null }, header.Seq);
    }

    private void HandleSearchProducts(SearchProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetRegisteredProducts(GetRegisteredProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetSoldProducts(GetSoldProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetPurchasedProducts(GetPurchasedProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetPersonalProducts(GetPersonalProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetExpiredProducts(GetExpiredProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetSimilarProducts(GetSimilarProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    private void HandleGetOffers(GetOffers msg, PacketHeader header)
    {
        Send(new Offers { _Offers = null }, header.Seq);
    }

    private void HandleRegisterProduct(RegisterProduct msg, PacketHeader header)
    {
        if (!RejectMarketDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleRegisterMultipleProducts(RegisterMultipleProducts msg, PacketHeader header)
    {
        if (!RejectMarketDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleBuyProduct(BuyProduct msg, PacketHeader header)
    {
        if (!RejectMarketDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleUnregisterProduct(UnregisterProduct msg, PacketHeader header)
    {
        if (!RejectMarketDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleWithdrawProduct(WithdrawProduct msg, PacketHeader header)
    {
        if (!RejectMarketDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleAddToFavoriteProducts(AddToFavoriteProducts msg, PacketHeader header)
    {
        Send(default(Abort), header.Seq);
    }

    private void HandleRemoveFromFavoriteProducts(RemoveFromFavoriteProducts msg, PacketHeader header)
    {
        Send(default(Abort), header.Seq);
    }

    private void HandleGetFavoriteProducts(GetFavoriteProducts msg, PacketHeader header)
    {
        Send(new Products { _Products = null }, header.Seq);
    }

    // ── Pets / Taming / Livestock ─────────────────────────────────────

    private void RegisterPetHandlers()
    {
        _conn.Recv<GetPetsInfo>(HandleGetPetsInfo);
        _conn.Recv<GetPreviewPet>(HandleGetPreviewPet);
        _conn.Recv<StartDomestication>(HandleStartDomestication);
        _conn.Recv<FinishDomestication>(HandleFinishDomestication);
        _conn.Recv<CancelDomestication>(HandleCancelDomestication);
        _conn.Recv<UseTamingAction>(HandleUseTamingAction);
        _conn.Recv<PutInCage>(HandlePutInCage);
        _conn.Recv<FeedInCage>(HandleFeedInCage);
        _conn.Recv<RenamePet>(HandleRenamePet);
        _conn.Recv<ReleasePet>(HandleReleasePet);
        _conn.Recv<GrazePets>(HandleGrazePets);
        _conn.Recv<GetAvailableTask>(HandleGetAvailableTask);
        _conn.Recv<GetMilestoneCandidate>(HandleGetMilestoneCandidate);
        _conn.Recv<AcceptMilestone>(HandleAcceptMilestone);
    }

    private void HandleGetPetsInfo(GetPetsInfo msg, PacketHeader header)
    {
        Send(new PetsInfo
        {
            Pets = default,
            GrazedPets = default,
            GrazableCount = 0
        }, header.Seq);
    }

    private void HandleGetPreviewPet(GetPreviewPet msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private bool RejectTamingDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบจับ/เลี้ยงสัตว์ยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleStartDomestication(StartDomestication msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleFinishDomestication(FinishDomestication msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleCancelDomestication(CancelDomestication msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleUseTamingAction(UseTamingAction msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandlePutInCage(PutInCage msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleFeedInCage(FeedInCage msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Livestock) { RejectLivestockDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private bool RejectLivestockDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบปศุสัตว์ยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleRenamePet(RenamePet msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleReleasePet(ReleasePet msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleGrazePets(GrazePets msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleGetAvailableTask(GetAvailableTask msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleGetMilestoneCandidate(GetMilestoneCandidate msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    private void HandleAcceptMilestone(AcceptMilestone msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Taming) { RejectTamingDisabled(header); return; }
        Send(default(Abort), header.Seq);
    }

    // ── Factions ──────────────────────────────────────────────────────

    private void RegisterFactionHandlers()
    {
        _conn.Recv<GetFactions>(HandleGetFactions);
        _conn.Recv<ActivateFaction>(HandleActivateFaction);
        _conn.Recv<ReportFactionProp>(HandleReportFactionProp);
        _conn.Recv<GetFactionDeliveryCondition>(HandleGetFactionDeliveryCondition);
        _conn.Recv<GetSupportRequests>(HandleGetSupportRequests);
        _conn.Recv<SendFactionSupportRequest>(HandleSendFactionSupportRequest);
    }

    private void HandleGetFactions(GetFactions msg, PacketHeader header)
    {
        Send(new Factions { _Factions = null, DailyMissionAvailableAt = 0 }, header.Seq);
    }

    private bool RejectFactionDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Factions) return true;
        Send(new Info { Text = "ระบบ Faction ยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleActivateFaction(ActivateFaction msg, PacketHeader header)
    {
        if (!RejectFactionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleReportFactionProp(ReportFactionProp msg, PacketHeader header)
    {
        if (!RejectFactionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleGetFactionDeliveryCondition(GetFactionDeliveryCondition msg, PacketHeader header)
    {
        if (!RejectFactionDisabled(header)) return;
        Send(default(FactionDeliveryCondition), header.Seq);
    }

    private void HandleGetSupportRequests(GetSupportRequests msg, PacketHeader header)
    {
        Send(new SupportRequests { Requests = new SupportRequest[0] }, header.Seq);
    }

    private void HandleSendFactionSupportRequest(SendFactionSupportRequest msg, PacketHeader header)
    {
        if (!RejectFactionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    // ── Missions ──────────────────────────────────────────────────────

    private void RegisterMissionHandlers()
    {
        _conn.Recv<GetMissions>(HandleGetMissions);
        _conn.Recv<RecommendMissions>(HandleRecommendMissions);
        _conn.Recv<AcceptMission>(HandleAcceptMission);
        _conn.Recv<CancelMission>(HandleCancelMission);
        _conn.Recv<ShuffleMission>(HandleShuffleMission);
        _conn.Recv<RechargeMissionShuffleCount>(HandleRechargeMissionShuffleCount);
        _conn.Recv<RecommendMissionImmediately>(HandleRecommendMissionImmediately);
    }

    private void HandleGetMissions(GetMissions msg, PacketHeader header)
    {
        Send(new MissionInfos
        {
            Missions = null,
            MissionActivatesAt = new Dictionary<Shared.Faction.FactionType, double>(),
            RecommendFailReasons = null,
            ShuffleCount = 0,
            ShuffleAt = null
        }, header.Seq);
    }

    private void HandleRecommendMissions(RecommendMissions msg, PacketHeader header)
    {
        Send(new MissionInfos { Missions = null, MissionActivatesAt = new Dictionary<Shared.Faction.FactionType, double>(), RecommendFailReasons = null, ShuffleCount = 0, ShuffleAt = null }, header.Seq);
    }

    private bool RejectMissionDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Missions) return true;
        Send(new Info { Text = "ระบบภารกิจยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleAcceptMission(AcceptMission msg, PacketHeader header)
    {
        if (!RejectMissionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleCancelMission(CancelMission msg, PacketHeader header)
    {
        if (!RejectMissionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleShuffleMission(ShuffleMission msg, PacketHeader header)
    {
        if (!RejectMissionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleRechargeMissionShuffleCount(RechargeMissionShuffleCount msg, PacketHeader header)
    {
        if (!RejectMissionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleRecommendMissionImmediately(RecommendMissionImmediately msg, PacketHeader header)
    {
        if (!RejectMissionDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    // ── Attendance ────────────────────────────────────────────────────

    private void RegisterAttendanceHandlers()
    {
        _conn.Recv<GetAttendanceRewards>(HandleGetAttendanceRewards);
        _conn.Recv<GiveAttendanceReward>(HandleGiveAttendanceReward);
        _conn.Recv<GiveAttendanceAppendix>(HandleGiveAttendanceAppendix);
    }

    private void HandleGetAttendanceRewards(GetAttendanceRewards msg, PacketHeader header)
    {
        Send(new AttendanceRewards
        {
            Rewards = null,
            Appendices = null
        }, header.Seq);
    }

    private bool RejectAttendanceDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Attendance) return true;
        Send(new Info { Text = "ระบบเข้าร่วมประจำวันยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleGiveAttendanceReward(GiveAttendanceReward msg, PacketHeader header)
    {
        if (!RejectAttendanceDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleGiveAttendanceAppendix(GiveAttendanceAppendix msg, PacketHeader header)
    {
        if (!RejectAttendanceDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    // ── Cargo ─────────────────────────────────────────────────────────

    private void RegisterCargoHandlers()
    {
        _conn.Recv<GetCargoReceivers>(HandleGetCargoReceivers);
        _conn.Recv<ActivateCargoReceiver>(HandleActivateCargoReceiver);
        _conn.Recv<OpenGate>(HandleOpenGate);
        _conn.Recv<CloseGate>(HandleCloseGate);
        _conn.Recv<SendCargo>(HandleSendCargo);
        _conn.Recv<ReceiveCargoImmediately>(HandleReceiveCargoImmediately);
        _conn.Recv<OccupyCargoWarphole>(HandleOccupyCargoWarphole);
        _conn.Recv<GetCargoWarpholeDefenseReward>(HandleGetCargoWarpholeDefenseReward);
    }

    private void HandleGetCargoReceivers(GetCargoReceivers msg, PacketHeader header)
    {
        Send(new CargoReceivers { PrivateReceiver = default, ClanReceiver = default, CostPerSize = 0 }, header.Seq);
    }

    private bool RejectCargoDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Cargo) return true;
        Send(new Info { Text = "ระบบขนส่งยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleActivateCargoReceiver(ActivateCargoReceiver msg, PacketHeader header)
    {
        if (!RejectCargoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleOpenGate(OpenGate msg, PacketHeader header)
    {
        if (!RejectCargoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleCloseGate(CloseGate msg, PacketHeader header)
    {
        if (!RejectCargoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleSendCargo(SendCargo msg, PacketHeader header)
    {
        if (!RejectCargoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleReceiveCargoImmediately(ReceiveCargoImmediately msg, PacketHeader header)
    {
        if (!RejectCargoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleOccupyCargoWarphole(OccupyCargoWarphole msg, PacketHeader header)
    {
        if (!RejectCargoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleGetCargoWarpholeDefenseReward(GetCargoWarpholeDefenseReward msg, PacketHeader header)
    {
        Send(new Info { Text = "รางวัลป้องกันจุดขนส่งยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
    }

    // ── Archipelago ───────────────────────────────────────────────────

    private void RegisterArchipelagoHandlers()
    {
        _conn.Recv<GetArchipelago>(HandleGetArchipelago);
        _conn.Recv<GetRouteOfArchipelago>(HandleGetRouteOfArchipelago);
        _conn.Recv<WarpToNextArchipelagoRegion>(HandleWarpToNextArchipelagoRegion);
        _conn.Recv<ReissueArchipelagoTodos>(HandleReissueArchipelagoTodos);
        _conn.Recv<RequestArchipelagoRegionClear>(HandleRequestArchipelagoRegionClear);
    }

    private void HandleGetArchipelago(GetArchipelago msg, PacketHeader header)
    {
        Send(new Archipelago { Id = null, TemplateId = null, UnstableFactor = 0, Name = null, ExpiresAt = 0, IncludedRegions = null }, header.Seq);
    }

    private bool RejectArchipelagoDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Archipelago) return true;
        Send(new Info { Text = "ระบบหมู่เกาะยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleGetRouteOfArchipelago(GetRouteOfArchipelago msg, PacketHeader header)
    {
        Send(new RoutesOfArchipelago { ArchipelagoRoutes = null }, header.Seq);
    }

    private void HandleWarpToNextArchipelagoRegion(WarpToNextArchipelagoRegion msg, PacketHeader header)
    {
        if (!RejectArchipelagoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleReissueArchipelagoTodos(ReissueArchipelagoTodos msg, PacketHeader header)
    {
        if (!RejectArchipelagoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleRequestArchipelagoRegionClear(RequestArchipelagoRegionClear msg, PacketHeader header)
    {
        if (!RejectArchipelagoDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    // ── Band / Music ──────────────────────────────────────────────────

    private void RegisterBandHandlers()
    {
        _conn.Recv<GetMusics>(HandleGetMusics);
        _conn.Recv<GetMusic>(HandleGetMusic);
        _conn.Recv<PlayMusic>(HandlePlayMusic);
        _conn.Recv<StopMusic>(HandleStopMusic);
        _conn.Recv<PlaySharedMusic>(HandlePlaySharedMusic);
        _conn.Recv<ChangeFollowMusic>(HandleChangeFollowMusic);
        _conn.Recv<SaveMusicToSlot>(HandleSaveMusicToSlot);
        _conn.Recv<RemoveMusicFromSlot>(HandleRemoveMusicFromSlot);
        _conn.Recv<PublishMusic>(HandlePublishMusic);
        _conn.Recv<GetSharedMusic>(HandleGetSharedMusic);
    }

    private void HandleGetMusics(GetMusics msg, PacketHeader header)
    {
        Send(new Musics { _Musics = null, SharedMusics = null }, header.Seq);
    }

    private bool RejectBandDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.Band) return true;
        Send(new Info { Text = "ระบบเพลง/ดนตรียังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleGetMusic(GetMusic msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandlePlayMusic(PlayMusic msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleStopMusic(StopMusic msg, PacketHeader header)
    {
        Send(default(OK), header.Seq);
    }

    private void HandlePlaySharedMusic(PlaySharedMusic msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleChangeFollowMusic(ChangeFollowMusic msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleSaveMusicToSlot(SaveMusicToSlot msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleRemoveMusicFromSlot(RemoveMusicFromSlot msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandlePublishMusic(PublishMusic msg, PacketHeader header)
    {
        if (!RejectBandDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleGetSharedMusic(GetSharedMusic msg, PacketHeader header)
    {
        Send(new Info { Text = "เพลงที่แชร์ยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
    }

    // ── Private Conversation (NPC) ────────────────────────────────────

    private void RegisterConversationHandlers()
    {
        _conn.Recv<InviteToConversation>(HandleInviteToConversation);
        _conn.Recv<ExitConversation>(HandleExitConversation);
        _conn.Recv<GetRecipients>(HandleGetRecipients);
    }

    private bool RejectConversationDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.PrivateConversation) return true;
        Send(new Info { Text = "ระบบแชทส่วนตัวยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleInviteToConversation(InviteToConversation msg, PacketHeader header)
    {
        if (!RejectConversationDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleExitConversation(ExitConversation msg, PacketHeader header)
    {
        Send(default(OK), header.Seq);
    }

    private void HandleGetRecipients(GetRecipients msg, PacketHeader header)
    {
        Send(new Recipients { ConversationId = null, EntityIds = null }, header.Seq);
    }

    // ── AddOns ────────────────────────────────────────────────────────

    private void RegisterAddOnsHandlers()
    {
        _conn.Recv<GetAddOns>(HandleGetAddOns);
        _conn.Recv<GetClaimedDlc>(HandleGetClaimedDlc);
        _conn.Recv<PlaceAddOns>(HandlePlaceAddOns);
        _conn.Recv<PurchaseCommodityWithSteamDlc>(HandlePurchaseCommodityWithSteamDlc);
        _conn.Recv<PurchaseCommodityWithVoucher>(HandlePurchaseCommodityWithVoucher);
    }

    private void HandleGetAddOns(GetAddOns msg, PacketHeader header)
    {
        Send(new AddOns { _AddOns = null }, header.Seq);
    }

    private void HandleGetClaimedDlc(GetClaimedDlc msg, PacketHeader header)
    {
        Send(new ClaimedDlc { DlcIds = null }, header.Seq);
    }

    private bool RejectAddOnsDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.AddOns) return true;
        Send(new Info { Text = "ระบบ AddOns ยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandlePlaceAddOns(PlaceAddOns msg, PacketHeader header)
    {
        if (!RejectAddOnsDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandlePurchaseCommodityWithSteamDlc(PurchaseCommodityWithSteamDlc msg, PacketHeader header)
    {
        if (!RejectAddOnsDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandlePurchaseCommodityWithVoucher(PurchaseCommodityWithVoucher msg, PacketHeader header)
    {
        if (!RejectAddOnsDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    // ── Dye / Bleach ──────────────────────────────────────────────────

    private void RegisterDyeHandlers()
    {
        _conn.Recv<Dye>(HandleDye);
        _conn.Recv<Bleach>(HandleBleach);
        _conn.Recv<EstimateDye>(HandleEstimateDye);
        _conn.Recv<EstimateBleach>(HandleEstimateBleach);
    }

    private bool RejectDyeDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.DyeAndBleach) return true;
        Send(new Info { Text = "ระบบย้อม/ฟอกสียังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleDye(Dye msg, PacketHeader header)
    {
        if (!RejectDyeDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleBleach(Bleach msg, PacketHeader header)
    {
        if (!RejectDyeDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleEstimateDye(EstimateDye msg, PacketHeader header)
    {
        if (!RejectDyeDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleEstimateBleach(EstimateBleach msg, PacketHeader header)
    {
        if (!RejectDyeDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    // ── Estate ────────────────────────────────────────────────────────

    private void RegisterEstateHandlers()
    {
        _conn.Recv<GetEstateLicenses>(HandleGetEstateLicenses);
        _conn.Recv<DeclareEstate>(HandleDeclareEstate);
        _conn.Recv<ExpandEstate>(HandleExpandEstate);
        _conn.Recv<ShrinkEstate>(HandleShrinkEstate);
        _conn.Recv<ExtendEstateActivation>(HandleExtendEstateActivation);
        _conn.Recv<SetEstateLicense>(HandleSetEstateLicense);
        _conn.Recv<RemoveEstate>(HandleRemoveEstate);
        _conn.Recv<ReturnToEstate>(HandleReturnToEstate);
        _conn.Recv<VisitEstate>(HandleVisitEstate);
        _conn.Recv<GetEstateLicenseById>(HandleGetEstateLicenseById);
        _conn.Recv<GetClanEstateLicense>(HandleGetClanEstateLicense);
    }

    private void HandleGetEstateLicenses(GetEstateLicenses msg, PacketHeader header)
    {
        Send(default(EstateLicenses), header.Seq);
    }

    private bool RejectEstateDisabled(PacketHeader header)
    {
        if (ServerConfig.Current.Features.LandPermission) return true;
        Send(new Info { Text = "ระบบสิทธิ์ที่ดินยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void HandleDeclareEstate(DeclareEstate msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleExpandEstate(ExpandEstate msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleShrinkEstate(ShrinkEstate msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleExtendEstateActivation(ExtendEstateActivation msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleSetEstateLicense(SetEstateLicense msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleRemoveEstate(RemoveEstate msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleReturnToEstate(ReturnToEstate msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleVisitEstate(VisitEstate msg, PacketHeader header)
    {
        if (!RejectEstateDisabled(header)) return;
        Send(default(Abort), header.Seq);
    }

    private void HandleGetEstateLicenseById(GetEstateLicenseById msg, PacketHeader header)
    {
        Send(default(EstateLicense), header.Seq);
    }

    private void HandleGetClanEstateLicense(GetClanEstateLicense msg, PacketHeader header)
    {
        Send(default(EstateLicense), header.Seq);
    }

    // ── Encyclopedia ──────────────────────────────────────────────────

    private void RegisterEncyclopediaHandlers()
    {
        _conn.Recv<GetEncyclopedia>(HandleGetEncyclopedia);
        _conn.Recv<ChangeFarmingEncyclopediaMastery>(HandleChangeFarmingEncyclopediaMastery);
    }

    private void HandleGetEncyclopedia(GetEncyclopedia msg, PacketHeader header)
    {
        Send(new FarmingEncyclopedia { Data = new Dictionary<string, FarmingEncyclopediaData>() }, header.Seq);
    }

    private void HandleChangeFarmingEncyclopediaMastery(ChangeFarmingEncyclopediaMastery msg, PacketHeader header)
    {
        Send(default(Abort), header.Seq);
    }

    // ── Engagement (no-op — ไม่มี state) ─────────────────────────────

    private void RegisterEngagementHandlers()
    {
        _conn.Recv<DeleteEngagementData>(HandleDeleteEngagementData);
    }

    private void HandleDeleteEngagementData(DeleteEngagementData msg, PacketHeader header)
    {
        Send(default(OK), header.Seq);
    }
}
