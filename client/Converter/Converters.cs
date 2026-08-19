using System;
using System.Collections.Generic;
using Durango.Utils.Converter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Ability;
using Shared.Animal;
using Shared.Battle;
using Shared.Building;
using Shared.Display;
using Shared.Economy;
using Shared.Encyclopedia;
using Shared.Estate;
using Shared.Faction;
using Shared.Item;
using Shared.Laboratory;
using Shared.Memo;
using Shared.Pet;
using Shared.Player;
using Shared.Purchaser;
using Shared.Push;
using Shared.Quest;
using Shared.Rank;
using Shared.Region;
using Shared.Season2;
using Shared.Skill;
using Shared.StatusEffect;
using Shared.Survival;
using Shared.System;
using Shared.Voucher;
using Yaml;

namespace Converter;

public class Converters : JsonConverter
{
	private static ColorConverter _colorConverter;

	private static GaugeConverter _gaugeConverter;

	private static GettextConverter _gettextConverter;

	private static PairConverter _pairConverter;

	private static JsonSerializerSettings _setting;

	private readonly Dictionary<Type, ReadDelegate> _dictionary = new Dictionary<Type, ReadDelegate>
	{
		{
			typeof(Accessories),
			ReadYaml_Accessories
		},
		{
			typeof(Accessory),
			ReadYaml_Accessory
		},
		{
			typeof(ActionActiveCondition),
			ReadYaml_ActionActiveCondition
		},
		{
			typeof(Advice),
			ReadYaml_Advice
		},
		{
			typeof(AdviceCategories),
			ReadYaml_AdviceCategories
		},
		{
			typeof(AdviceCategoriesYaml),
			ReadYaml_AdviceCategoriesYaml
		},
		{
			typeof(AdviceCategory),
			ReadYaml_AdviceCategory
		},
		{
			typeof(AdviceSubCategory),
			ReadYaml_AdviceSubCategory
		},
		{
			typeof(AdviceYaml),
			ReadYaml_AdviceYaml
		},
		{
			typeof(Ally),
			ReadYaml_Ally
		},
		{
			typeof(Animal),
			ReadYaml_Animal
		},
		{
			typeof(AnimalYaml),
			ReadYaml_AnimalYaml
		},
		{
			typeof(ArchipelagoMission),
			ReadYaml_ArchipelagoMission
		},
		{
			typeof(ArchipelagoMissionDict),
			ReadYaml_ArchipelagoMissionDict
		},
		{
			typeof(ArchipelagoTemplate),
			ReadYaml_ArchipelagoTemplate
		},
		{
			typeof(ArchipelagoTemplateDict),
			ReadYaml_ArchipelagoTemplateDict
		},
		{
			typeof(ArtifactEffect),
			ReadYaml_ArtifactEffect
		},
		{
			typeof(ArtifactEffectDict),
			ReadYaml_ArtifactEffectDict
		},
		{
			typeof(ArtifactFloor),
			ReadYaml_ArtifactFloor
		},
		{
			typeof(ArtifactInteriorMood),
			ReadYaml_ArtifactInteriorMood
		},
		{
			typeof(ArtifactInteriorSet),
			ReadYaml_ArtifactInteriorSet
		},
		{
			typeof(ArtifactLook),
			ReadYaml_ArtifactLook
		},
		{
			typeof(ArtifactModel),
			ReadYaml_ArtifactModel
		},
		{
			typeof(ArtifactModelDict),
			ReadYaml_ArtifactModelDict
		},
		{
			typeof(ArtifactPrototype),
			ReadYaml_ArtifactPrototype
		},
		{
			typeof(ArtifactPrototypeDict),
			ReadYaml_ArtifactPrototypeDict
		},
		{
			typeof(ArtifactSetEffectsYaml),
			ReadYaml_ArtifactSetEffectsYaml
		},
		{
			typeof(Attendance),
			ReadYaml_Attendance
		},
		{
			typeof(Barehands),
			ReadYaml_Barehands
		},
		{
			typeof(Battle),
			ReadYaml_Battle
		},
		{
			typeof(Blueprint),
			ReadYaml_Blueprint
		},
		{
			typeof(BlueprintDict),
			ReadYaml_BlueprintDict
		},
		{
			typeof(BlueprintRemodelingsDict),
			ReadYaml_BlueprintRemodelingsDict
		},
		{
			typeof(BlueprintSlot),
			ReadYaml_BlueprintSlot
		},
		{
			typeof(BodyParts),
			ReadYaml_BodyParts
		},
		{
			typeof(BonusPrototype),
			ReadYaml_BonusPrototype
		},
		{
			typeof(BonusPrototypes),
			ReadYaml_BonusPrototypes
		},
		{
			typeof(BonusPrototypeYaml),
			ReadYaml_BonusPrototypeYaml
		},
		{
			typeof(Build),
			ReadYaml_Build
		},
		{
			typeof(CargoCost),
			ReadYaml_CargoCost
		},
		{
			typeof(CashYaml),
			ReadYaml_CashYaml
		},
		{
			typeof(Chapter),
			ReadYaml_Chapter
		},
		{
			typeof(Chapters),
			ReadYaml_Chapters
		},
		{
			typeof(ClanLevelReward),
			ReadYaml_ClanLevelReward
		},
		{
			typeof(ClanResearch),
			ReadYaml_ClanResearch
		},
		{
			typeof(ClanResearchs),
			ReadYaml_ClanResearchs
		},
		{
			typeof(ClanYaml),
			ReadYaml_ClanYaml
		},
		{
			typeof(Clear),
			ReadYaml_Clear
		},
		{
			typeof(ClearTime),
			ReadYaml_ClearTime
		},
		{
			typeof(CollectibleYaml),
			ReadYaml_CollectibleYaml
		},
		{
			typeof(Commodities),
			ReadYaml_Commodities
		},
		{
			typeof(Commodity),
			ReadYaml_Commodity
		},
		{
			typeof(CommodityCondition),
			ReadYaml_CommodityCondition
		},
		{
			typeof(CommodityContent),
			ReadYaml_CommodityContent
		},
		{
			typeof(ConstantPet),
			ReadYaml_ConstantPet
		},
		{
			typeof(Constants),
			ReadYaml_Constants
		},
		{
			typeof(ConstantsItem),
			ReadYaml_ConstantsItem
		},
		{
			typeof(ContentDescription),
			ReadYaml_ContentDescription
		},
		{
			typeof(Cost),
			ReadYaml_Cost
		},
		{
			typeof(CostsYaml),
			ReadYaml_CostsYaml
		},
		{
			typeof(Crack),
			ReadYaml_Crack
		},
		{
			typeof(CropData),
			ReadYaml_CropData
		},
		{
			typeof(CropInfo),
			ReadYaml_CropInfo
		},
		{
			typeof(WarpRushReward.CurrencyInfo),
			ReadYaml_WarpRushReward_CurrencyInfo
		},
		{
			typeof(Dash),
			ReadYaml_Dash
		},
		{
			typeof(DateTimeDict),
			ReadYaml_DateTimeDict
		},
		{
			typeof(DateTimeYaml),
			ReadYaml_DateTimeYaml
		},
		{
			typeof(DerivedReward),
			ReadYaml_DerivedReward
		},
		{
			typeof(DerivedRewardData),
			ReadYaml_DerivedRewardData
		},
		{
			typeof(DerivedRewardDatas),
			ReadYaml_DerivedRewardDatas
		},
		{
			typeof(DerivedRewards),
			ReadYaml_DerivedRewards
		},
		{
			typeof(Dialogue),
			ReadYaml_Dialogue
		},
		{
			typeof(Durability),
			ReadYaml_Durability
		},
		{
			typeof(DurabilityResult),
			ReadYaml_DurabilityResult
		},
		{
			typeof(EffectDetail),
			ReadYaml_EffectDetail
		},
		{
			typeof(Emoticon),
			ReadYaml_Emoticon
		},
		{
			typeof(Emotions),
			ReadYaml_Emotions
		},
		{
			typeof(EncyclopediaCategories),
			ReadYaml_EncyclopediaCategories
		},
		{
			typeof(EncyclopediaCategory),
			ReadYaml_EncyclopediaCategory
		},
		{
			typeof(EncyclopediaItem),
			ReadYaml_EncyclopediaItem
		},
		{
			typeof(EncyclopediaItems),
			ReadYaml_EncyclopediaItems
		},
		{
			typeof(EncyclopediaModifiers),
			ReadYaml_EncyclopediaModifiers
		},
		{
			typeof(EncyclopediaModifiersYaml),
			ReadYaml_EncyclopediaModifiersYaml
		},
		{
			typeof(Estate),
			ReadYaml_Estate
		},
		{
			typeof(EstateCost),
			ReadYaml_EstateCost
		},
		{
			typeof(Explorer),
			ReadYaml_Explorer
		},
		{
			typeof(Faction),
			ReadYaml_Faction
		},
		{
			typeof(FactionInfo),
			ReadYaml_FactionInfo
		},
		{
			typeof(FactionReward),
			ReadYaml_FactionReward
		},
		{
			typeof(Factions),
			ReadYaml_Factions
		},
		{
			typeof(FactionSupport),
			ReadYaml_FactionSupport
		},
		{
			typeof(Yaml.FatigueCategory),
			ReadYaml_FatigueCategory
		},
		{
			typeof(FatigueCategoryYaml),
			ReadYaml_FatigueCategoryYaml
		},
		{
			typeof(GeneratorData),
			ReadYaml_GeneratorData
		},
		{
			typeof(GeneratorYaml),
			ReadYaml_GeneratorYaml
		},
		{
			typeof(IndicatorData),
			ReadYaml_IndicatorData
		},
		{
			typeof(ItemContent),
			ReadYaml_ItemContent
		},
		{
			typeof(RankingReward.ItemInfo),
			ReadYaml_RankingReward_ItemInfo
		},
		{
			typeof(WarpRushReward.ItemInfo),
			ReadYaml_WarpRushReward_ItemInfo
		},
		{
			typeof(ItemTextCondition),
			ReadYaml_ItemTextCondition
		},
		{
			typeof(Yaml.Job),
			ReadYaml_Job
		},
		{
			typeof(JobsYaml),
			ReadYaml_JobsYaml
		},
		{
			typeof(Chapter.Kind),
			ReadYaml_Chapter_Kind
		},
		{
			typeof(Market),
			ReadYaml_Market
		},
		{
			typeof(MaxLevels),
			ReadYaml_MaxLevels
		},
		{
			typeof(MemoGroupDictionary),
			ReadYaml_MemoGroupDictionary
		},
		{
			typeof(MemoInfo),
			ReadYaml_MemoInfo
		},
		{
			typeof(MemosYaml),
			ReadYaml_MemosYaml
		},
		{
			typeof(Yaml.Messenger),
			ReadYaml_Messenger
		},
		{
			typeof(MessengersYaml),
			ReadYaml_MessengersYaml
		},
		{
			typeof(FactionInfo.MissionData),
			ReadYaml_FactionInfo_MissionData
		},
		{
			typeof(MissionTalk),
			ReadYaml_MissionTalk
		},
		{
			typeof(ModularArtifactContent),
			ReadYaml_ModularArtifactContent
		},
		{
			typeof(MoneyContent),
			ReadYaml_MoneyContent
		},
		{
			typeof(Motion),
			ReadYaml_Motion
		},
		{
			typeof(Musician),
			ReadYaml_Musician
		},
		{
			typeof(Natural),
			ReadYaml_Natural
		},
		{
			typeof(NaturalComponentInfo),
			ReadYaml_NaturalComponentInfo
		},
		{
			typeof(OpenLimit),
			ReadYaml_OpenLimit
		},
		{
			typeof(OpenMapCost),
			ReadYaml_OpenMapCost
		},
		{
			typeof(PerformanceVisibleInfo),
			ReadYaml_PerformanceVisibleInfo
		},
		{
			typeof(PerformanceVisibleInfoDict),
			ReadYaml_PerformanceVisibleInfoDict
		},
		{
			typeof(PeriodicCountsLimit),
			ReadYaml_PeriodicCountsLimit
		},
		{
			typeof(PeriodicLimit),
			ReadYaml_PeriodicLimit
		},
		{
			typeof(PersonalRegion),
			ReadYaml_PersonalRegion
		},
		{
			typeof(PersonalResearch),
			ReadYaml_PersonalResearch
		},
		{
			typeof(PersonalResearchs),
			ReadYaml_PersonalResearchs
		},
		{
			typeof(Pet),
			ReadYaml_Pet
		},
		{
			typeof(PetActiveSkill),
			ReadYaml_PetActiveSkill
		},
		{
			typeof(PetActiveSkillCondition),
			ReadYaml_PetActiveSkillCondition
		},
		{
			typeof(PetActiveSkillConditionDict),
			ReadYaml_PetActiveSkillConditionDict
		},
		{
			typeof(PetActiveSkillConditions),
			ReadYaml_PetActiveSkillConditions
		},
		{
			typeof(PetActiveSkillRankDict),
			ReadYaml_PetActiveSkillRankDict
		},
		{
			typeof(PetActiveSkills),
			ReadYaml_PetActiveSkills
		},
		{
			typeof(PetExp),
			ReadYaml_PetExp
		},
		{
			typeof(PetExpTable),
			ReadYaml_PetExpTable
		},
		{
			typeof(Pets),
			ReadYaml_Pets
		},
		{
			typeof(PetTask),
			ReadYaml_PetTask
		},
		{
			typeof(PetTasks),
			ReadYaml_PetTasks
		},
		{
			typeof(Pioneer),
			ReadYaml_Pioneer
		},
		{
			typeof(PioneerCostExchangeRate),
			ReadYaml_PioneerCostExchangeRate
		},
		{
			typeof(PioneerGradeReward),
			ReadYaml_PioneerGradeReward
		},
		{
			typeof(PioneerGradeRewards),
			ReadYaml_PioneerGradeRewards
		},
		{
			typeof(PioneerGradeRewardText),
			ReadYaml_PioneerGradeRewardText
		},
		{
			typeof(PioneerRate),
			ReadYaml_PioneerRate
		},
		{
			typeof(PlayerAction),
			ReadYaml_PlayerAction
		},
		{
			typeof(PlayerActionAttackInfo),
			ReadYaml_PlayerActionAttackInfo
		},
		{
			typeof(PlayerActionMeta),
			ReadYaml_PlayerActionMeta
		},
		{
			typeof(PlayerActions),
			ReadYaml_PlayerActions
		},
		{
			typeof(PlayerActionSlot),
			ReadYaml_PlayerActionSlot
		},
		{
			typeof(PlayerEntities),
			ReadYaml_PlayerEntities
		},
		{
			typeof(PlayerEntity),
			ReadYaml_PlayerEntity
		},
		{
			typeof(PlayerStatistics),
			ReadYaml_PlayerStatistics
		},
		{
			typeof(PromotionLink),
			ReadYaml_PromotionLink
		},
		{
			typeof(Prototype),
			ReadYaml_Prototype
		},
		{
			typeof(PrototypePreset),
			ReadYaml_PrototypePreset
		},
		{
			typeof(PrototypePresetPerformance),
			ReadYaml_PrototypePresetPerformance
		},
		{
			typeof(PrototypePresetRepair),
			ReadYaml_PrototypePresetRepair
		},
		{
			typeof(PrototypePresetTag),
			ReadYaml_PrototypePresetTag
		},
		{
			typeof(PrototypeYaml),
			ReadYaml_PrototypeYaml
		},
		{
			typeof(PurchasableTime),
			ReadYaml_PurchasableTime
		},
		{
			typeof(PurchaseLimit),
			ReadYaml_PurchaseLimit
		},
		{
			typeof(PurchaseRandomPiece),
			ReadYaml_PurchaseRandomPiece
		},
		{
			typeof(PushCategory),
			ReadYaml_PushCategory
		},
		{
			typeof(PushCategoryYml),
			ReadYaml_PushCategoryYml
		},
		{
			typeof(PushPolicy),
			ReadYaml_PushPolicy
		},
		{
			typeof(PutInContainerInfo),
			ReadYaml_PutInContainerInfo
		},
		{
			typeof(Season2.Quantity),
			ReadYaml_Season2_Quantity
		},
		{
			typeof(QuestMessages),
			ReadYaml_QuestMessages
		},
		{
			typeof(QuestsYml),
			ReadYaml_QuestsYml
		},
		{
			typeof(QuestYml),
			ReadYaml_QuestYml
		},
		{
			typeof(Ranking),
			ReadYaml_Ranking
		},
		{
			typeof(RankingReward),
			ReadYaml_RankingReward
		},
		{
			typeof(RankingRewards),
			ReadYaml_RankingRewards
		},
		{
			typeof(Rankings),
			ReadYaml_Rankings
		},
		{
			typeof(Recipe),
			ReadYaml_Recipe
		},
		{
			typeof(RecipeDict),
			ReadYaml_RecipeDict
		},
		{
			typeof(RecipeSlot),
			ReadYaml_RecipeSlot
		},
		{
			typeof(Recommends),
			ReadYaml_Recommends
		},
		{
			typeof(ReformTechSupport),
			ReadYaml_ReformTechSupport
		},
		{
			typeof(ReformTechSupportDict),
			ReadYaml_ReformTechSupportDict
		},
		{
			typeof(ReformTechSupportTag),
			ReadYaml_ReformTechSupportTag
		},
		{
			typeof(RegionCoOp),
			ReadYaml_RegionCoOp
		},
		{
			typeof(RegionCoOpDict),
			ReadYaml_RegionCoOpDict
		},
		{
			typeof(RegionTemplate),
			ReadYaml_RegionTemplate
		},
		{
			typeof(RegionTemplateDict),
			ReadYaml_RegionTemplateDict
		},
		{
			typeof(RemodelingBlueprint),
			ReadYaml_RemodelingBlueprint
		},
		{
			typeof(Repair),
			ReadYaml_Repair
		},
		{
			typeof(RepairItem),
			ReadYaml_RepairItem
		},
		{
			typeof(RequiredSkill),
			ReadYaml_RequiredSkill
		},
		{
			typeof(Research),
			ReadYaml_Research
		},
		{
			typeof(ResearchEffect),
			ReadYaml_ResearchEffect
		},
		{
			typeof(Resistance),
			ReadYaml_Resistance
		},
		{
			typeof(RestoreCost),
			ReadYaml_RestoreCost
		},
		{
			typeof(Revision),
			ReadYaml_Revision
		},
		{
			typeof(ReviveImmediatelyCost),
			ReadYaml_ReviveImmediatelyCost
		},
		{
			typeof(Reward),
			ReadYaml_Reward
		},
		{
			typeof(RewardItem),
			ReadYaml_RewardItem
		},
		{
			typeof(RewardYaml),
			ReadYaml_RewardYaml
		},
		{
			typeof(Sailing),
			ReadYaml_Sailing
		},
		{
			typeof(SalesFeeRates),
			ReadYaml_SalesFeeRates
		},
		{
			typeof(ScribbleCanvasStruct),
			ReadYaml_ScribbleCanvasStruct
		},
		{
			typeof(ScribbleType),
			ReadYaml_ScribbleType
		},
		{
			typeof(Season2),
			ReadYaml_Season2
		},
		{
			typeof(Season2Voucher),
			ReadYaml_Season2Voucher
		},
		{
			typeof(ShopCategories),
			ReadYaml_ShopCategories
		},
		{
			typeof(ShopCategory),
			ReadYaml_ShopCategory
		},
		{
			typeof(ShopCategoryCondition),
			ReadYaml_ShopCategoryCondition
		},
		{
			typeof(ShopContents),
			ReadYaml_ShopContents
		},
		{
			typeof(ShopUIOption),
			ReadYaml_ShopUIOption
		},
		{
			typeof(FactionInfo.MissionData.ShuffleData),
			ReadYaml_FactionInfo_MissionData_ShuffleData
		},
		{
			typeof(Skill),
			ReadYaml_Skill
		},
		{
			typeof(SkillAdvice),
			ReadYaml_SkillAdvice
		},
		{
			typeof(SkillCategory),
			ReadYaml_SkillCategory
		},
		{
			typeof(SkillCategoryYaml),
			ReadYaml_SkillCategoryYaml
		},
		{
			typeof(SkillConstants),
			ReadYaml_SkillConstants
		},
		{
			typeof(SkillModifier),
			ReadYaml_SkillModifier
		},
		{
			typeof(SkillModifierYaml),
			ReadYaml_SkillModifierYaml
		},
		{
			typeof(SkillUntrain),
			ReadYaml_SkillUntrain
		},
		{
			typeof(SkillUntrainCost),
			ReadYaml_SkillUntrainCost
		},
		{
			typeof(SkillUntrainInfo),
			ReadYaml_SkillUntrainInfo
		},
		{
			typeof(SkillYaml),
			ReadYaml_SkillYaml
		},
		{
			typeof(SkipTutorialMissions),
			ReadYaml_SkipTutorialMissions
		},
		{
			typeof(SlotSourceInfo),
			ReadYaml_SlotSourceInfo
		},
		{
			typeof(SpecialDealBanner),
			ReadYaml_SpecialDealBanner
		},
		{
			typeof(SpecialDealBannersDict),
			ReadYaml_SpecialDealBannersDict
		},
		{
			typeof(Sprinkler),
			ReadYaml_Sprinkler
		},
		{
			typeof(SprinkleWater),
			ReadYaml_SprinkleWater
		},
		{
			typeof(StatusEffectsContent),
			ReadYaml_StatusEffectsContent
		},
		{
			typeof(StatusEffectTemplate),
			ReadYaml_StatusEffectTemplate
		},
		{
			typeof(StatusEffectTemplateYaml),
			ReadYaml_StatusEffectTemplateYaml
		},
		{
			typeof(StoryYaml),
			ReadYaml_StoryYaml
		},
		{
			typeof(SubCommodityAcceptLimit),
			ReadYaml_SubCommodityAcceptLimit
		},
		{
			typeof(SupplyLevel),
			ReadYaml_SupplyLevel
		},
		{
			typeof(Survive),
			ReadYaml_Survive
		},
		{
			typeof(RankingReward.Tag),
			ReadYaml_RankingReward_Tag
		},
		{
			typeof(Tag),
			ReadYaml_Tag
		},
		{
			typeof(TagAllowAction),
			ReadYaml_TagAllowAction
		},
		{
			typeof(TagAllowActions),
			ReadYaml_TagAllowActions
		},
		{
			typeof(TagYaml),
			ReadYaml_TagYaml
		},
		{
			typeof(Talk),
			ReadYaml_Talk
		},
		{
			typeof(Talks),
			ReadYaml_Talks
		},
		{
			typeof(TalksYaml),
			ReadYaml_TalksYaml
		},
		{
			typeof(Taming),
			ReadYaml_Taming
		},
		{
			typeof(TimelineCategory),
			ReadYaml_TimelineCategory
		},
		{
			typeof(TimelineMessage),
			ReadYaml_TimelineMessage
		},
		{
			typeof(TimelineMessagesYaml),
			ReadYaml_TimelineMessagesYaml
		},
		{
			typeof(Title),
			ReadYaml_Title
		},
		{
			typeof(TitleYaml),
			ReadYaml_TitleYaml
		},
		{
			typeof(ToDoContents),
			ReadYaml_ToDoContents
		},
		{
			typeof(CommodityCondition.Type),
			ReadYaml_CommodityCondition_Type
		},
		{
			typeof(UnstableFactorDict),
			ReadYaml_UnstableFactorDict
		},
		{
			typeof(Voucher),
			ReadYaml_Voucher
		},
		{
			typeof(VoucherContent),
			ReadYaml_VoucherContent
		},
		{
			typeof(WarpRushReward.VoucherInfo),
			ReadYaml_WarpRushReward_VoucherInfo
		},
		{
			typeof(Vouchers),
			ReadYaml_Vouchers
		},
		{
			typeof(VoucherWithCommodity),
			ReadYaml_VoucherWithCommodity
		},
		{
			typeof(War),
			ReadYaml_War
		},
		{
			typeof(Warehouse),
			ReadYaml_Warehouse
		},
		{
			typeof(Yaml.WarpAccelerator),
			ReadYaml_WarpAccelerator
		},
		{
			typeof(WarpRushReward),
			ReadYaml_WarpRushReward
		},
		{
			typeof(WarpRushRewards),
			ReadYaml_WarpRushRewards
		},
		{
			typeof(Season2.WeatherInfo),
			ReadYaml_Season2_WeatherInfo
		},
		{
			typeof(WeightedItemContent),
			ReadYaml_WeightedItemContent
		}
	};

	public static JsonSerializerSettings Setting
	{
		get
		{
			if (_setting == null)
			{
				_setting = new JsonSerializerSettings();
				_setting.Converters.Add(new Converters());
				_colorConverter = new ColorConverter();
				_setting.Converters.Add(_colorConverter);
				_gaugeConverter = new GaugeConverter();
				_setting.Converters.Add(_gaugeConverter);
				_gettextConverter = new GettextConverter();
				_setting.Converters.Add(_gettextConverter);
				_pairConverter = new PairConverter();
				_setting.Converters.Add(_pairConverter);
			}
			return _setting;
		}
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return _dictionary.Get(objectType)?.Invoke(reader, objectType, existingValue, serializer);
	}

	public override bool CanConvert(Type objectType)
	{
		return _dictionary.ContainsKey(objectType);
	}

	private static object ReadYaml_Accessories(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Accessories accessories = new Accessories();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Accessory value = ((reader.TokenType != JsonToken.Null) ? ((Accessory)ReadYaml_Accessory(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				accessories.Add(text, value);
			}
		}
		return accessories;
	}

	private static object ReadYaml_Accessory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Accessory accessory = new Accessory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "id":
					accessory.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "model":
					accessory.Model = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "name":
					accessory.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "description":
					accessory.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "type":
					accessory.Type = (AccessoryType)Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return accessory;
	}

	private static object ReadYaml_ActionActiveCondition(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ActionActiveCondition actionActiveCondition = new ActionActiveCondition();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "active_type":
					actionActiveCondition.ActiveType = (ActiveType)Convert.ToInt32(reader.Value);
					break;
				case "value":
					actionActiveCondition.Value = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "duration":
					actionActiveCondition.Duration = Convert.ToSingle(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return actionActiveCondition;
	}

	private static object ReadYaml_Advice(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Advice advice = new Advice();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				advice.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				advice.description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "category_levels":
			{
				Dictionary<Shared.Skill.Category, int> dictionary = new Dictionary<Shared.Skill.Category, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					Shared.Skill.Category key = (Shared.Skill.Category)Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				advice.category_levels = dictionary;
				break;
			}
			case "skills":
			{
				List<SkillAdvice> list2 = new List<SkillAdvice>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					SkillAdvice item2 = (SkillAdvice)ReadYaml_SkillAdvice(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				advice.skills = list2.ToArray();
				break;
			}
			case "difficulty":
				advice.difficulty = Convert.ToInt32(reader.Value);
				break;
			case "cooperation":
				advice.cooperation = Convert.ToInt32(reader.Value);
				break;
			case "recommended":
				advice.recommended = Convert.ToBoolean(reader.Value);
				break;
			case "category":
				advice.category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "subcategory":
				advice.subcategory = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "reward_title_id":
				advice.reward_title_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "reward_items_name":
				advice.reward_items_name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "reward_items":
			{
				List<RewardItem> list3 = new List<RewardItem>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					RewardItem item3 = (RewardItem)ReadYaml_RewardItem(reader, objectType, existingValue, serializer);
					list3.Add(item3);
				}
				advice.reward_items = list3.ToArray();
				break;
			}
			case "required_skill":
				advice.required_skill = (RequiredSkill)ReadYaml_RequiredSkill(reader, objectType, existingValue, serializer);
				break;
			case "hints":
			{
				List<Gettext> list = new List<Gettext>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Gettext item = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					list.Add(item);
				}
				advice.hints = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return advice;
	}

	private static object ReadYaml_AdviceCategories(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AdviceCategories adviceCategories = new AdviceCategories();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "categories")
			{
				List<AdviceCategory> list = new List<AdviceCategory>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					AdviceCategory item = (AdviceCategory)ReadYaml_AdviceCategory(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				adviceCategories.categories = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return adviceCategories;
	}

	private static object ReadYaml_AdviceCategoriesYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AdviceCategoriesYaml adviceCategoriesYaml = new AdviceCategoriesYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "categories")
			{
				List<AdviceCategory> list = new List<AdviceCategory>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					AdviceCategory item = (AdviceCategory)ReadYaml_AdviceCategory(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				adviceCategoriesYaml.categories = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return adviceCategoriesYaml;
	}

	private static object ReadYaml_AdviceCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AdviceCategory adviceCategory = new AdviceCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "id":
				adviceCategory.id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "name":
				adviceCategory.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				adviceCategory.icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "subcategories":
			{
				List<AdviceSubCategory> list = new List<AdviceSubCategory>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					AdviceSubCategory item = (AdviceSubCategory)ReadYaml_AdviceSubCategory(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				adviceCategory.subcategories = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return adviceCategory;
	}

	private static object ReadYaml_AdviceSubCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AdviceSubCategory adviceSubCategory = new AdviceSubCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "id":
					adviceSubCategory.id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "name":
					adviceSubCategory.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return adviceSubCategory;
	}

	private static object ReadYaml_AdviceYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AdviceYaml adviceYaml = new AdviceYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Advice value = ((reader.TokenType != JsonToken.Null) ? ((Advice)ReadYaml_Advice(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				adviceYaml.Add(text, value);
			}
		}
		return adviceYaml;
	}

	private static object ReadYaml_Ally(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Ally ally = default(Ally);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "slot_opens_at":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				ally.SlotOpensAt = list.ToArray();
				flag = true;
				break;
			}
			case "default_slot_count":
				ally.DefaultSlotCount = Convert.ToInt32(reader.Value);
				flag2 = true;
				break;
			case "max_slot_count":
				ally.MaxSlotCount = Convert.ToInt32(reader.Value);
				flag3 = true;
				break;
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'slot_opens_at' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'default_slot_count' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'max_slot_count' not found in JSON.");
		}
		return ally;
	}

	private static object ReadYaml_Animal(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Animal animal = new Animal();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				animal.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "bound_radius":
				animal.BoundRadius = Convert.ToSingle(reader.Value);
				break;
			case "portrait":
				animal.Portrait = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "model_path":
				animal.ModelPath = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "tamable":
				animal.Tamable = Convert.ToBoolean(reader.Value);
				break;
			case "life_gauge_ratio":
			{
				List<float> list = new List<float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					float item = Convert.ToSingle(reader.Value);
					list.Add(item);
				}
				animal.LifeGaugeRatio = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return animal;
	}

	private static object ReadYaml_AnimalYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		AnimalYaml animalYaml = new AnimalYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			int key = Convert.ToInt32(reader.Value);
			reader.Read();
			Animal value = ((reader.TokenType != JsonToken.Null) ? ((Animal)ReadYaml_Animal(reader, objectType, existingValue, serializer)) : null);
			animalYaml.Add(key, value);
		}
		return animalYaml;
	}

	private static object ReadYaml_ArchipelagoMission(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArchipelagoMission archipelagoMission = new ArchipelagoMission();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "clear_point":
				archipelagoMission.ClearPoint = Convert.ToInt32(reader.Value);
				break;
			case "description":
				archipelagoMission.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "intro":
				archipelagoMission.Intro = (Dialogue)ReadYaml_Dialogue(reader, objectType, existingValue, serializer);
				break;
			case "outro":
				archipelagoMission.Outro = (Dialogue)ReadYaml_Dialogue(reader, objectType, existingValue, serializer);
				break;
			case "title":
				archipelagoMission.Title = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "todo_list":
			{
				Dictionary<string, ToDoContents> dictionary = new Dictionary<string, ToDoContents>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					ToDoContents value = ((reader.TokenType != JsonToken.Null) ? ((ToDoContents)ReadYaml_ToDoContents(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				archipelagoMission.ToDos = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return archipelagoMission;
	}

	private static object ReadYaml_ArchipelagoMissionDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArchipelagoMissionDict archipelagoMissionDict = new ArchipelagoMissionDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Dictionary<string, ArchipelagoMission> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, ArchipelagoMission> dictionary = new Dictionary<string, ArchipelagoMission>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					ArchipelagoMission value2 = ((reader.TokenType != JsonToken.Null) ? ((ArchipelagoMission)ReadYaml_ArchipelagoMission(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value2);
					}
				}
				value = dictionary;
			}
			if (text != null)
			{
				archipelagoMissionDict.Add(text, value);
			}
		}
		return archipelagoMissionDict;
	}

	private static object ReadYaml_ArchipelagoTemplate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArchipelagoTemplate archipelagoTemplate = new ArchipelagoTemplate();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "active":
					archipelagoTemplate.Active = Convert.ToBoolean(reader.Value);
					break;
				case "level":
					archipelagoTemplate.Level = Convert.ToInt32(reader.Value);
					break;
				case "biome":
					archipelagoTemplate.Biome = (Biome)Convert.ToInt32(reader.Value);
					break;
				case "start_region_template_id":
					archipelagoTemplate.FirstRegionTemplateId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return archipelagoTemplate;
	}

	private static object ReadYaml_ArchipelagoTemplateDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArchipelagoTemplateDict archipelagoTemplateDict = new ArchipelagoTemplateDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			ArchipelagoTemplate value = ((reader.TokenType != JsonToken.Null) ? ((ArchipelagoTemplate)ReadYaml_ArchipelagoTemplate(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				archipelagoTemplateDict.Add(text, value);
			}
		}
		return archipelagoTemplateDict;
	}

	private static object ReadYaml_ArtifactEffect(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactEffect artifactEffect = new ArtifactEffect();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "path":
					artifactEffect.path = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "file_name":
					artifactEffect.file_name = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return artifactEffect;
	}

	private static object ReadYaml_ArtifactEffectDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactEffectDict artifactEffectDict = new ArtifactEffectDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			ArtifactEffect value = ((reader.TokenType != JsonToken.Null) ? ((ArtifactEffect)ReadYaml_ArtifactEffect(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				artifactEffectDict.Add(text, value);
			}
		}
		return artifactEffectDict;
	}

	private static object ReadYaml_ArtifactFloor(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactFloor artifactFloor = new ArtifactFloor();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "max_stories":
				artifactFloor.MaxStories = Convert.ToInt32(reader.Value);
				break;
			case "floorable_types":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				artifactFloor.FloorableTypes = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return artifactFloor;
	}

	private static object ReadYaml_ArtifactInteriorMood(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactInteriorMood artifactInteriorMood = new ArtifactInteriorMood();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				artifactInteriorMood.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "desc":
				artifactInteriorMood.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "summary_desc":
				artifactInteriorMood.SummaryDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "total_level":
				artifactInteriorMood.TotalLevel = Convert.ToInt32(reader.Value);
				break;
			case "required_stat_factor":
				artifactInteriorMood.RequiredStatFactor = Convert.ToInt32(reader.Value);
				break;
			case "season":
				artifactInteriorMood.Season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "target_prototypes":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				artifactInteriorMood.TargetPrototypes = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return artifactInteriorMood;
	}

	private static object ReadYaml_ArtifactInteriorSet(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactInteriorSet artifactInteriorSet = new ArtifactInteriorSet();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				artifactInteriorSet.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "desc":
				artifactInteriorSet.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "summary_desc":
				artifactInteriorSet.SummaryDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "tag_slots":
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				artifactInteriorSet.TagSlots = dictionary2;
				break;
			}
			case "tag_names":
			{
				Dictionary<string, Gettext> dictionary = new Dictionary<string, Gettext>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Gettext value = ((reader.TokenType != JsonToken.Null) ? ((Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer)) : default(Gettext));
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				artifactInteriorSet.TagNames = dictionary;
				break;
			}
			case "required_stat_factor":
				artifactInteriorSet.RequiredStatFactor = Convert.ToInt32(reader.Value);
				break;
			case "season":
				artifactInteriorSet.Season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "target_prototypes":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				artifactInteriorSet.TargetPrototypes = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return artifactInteriorSet;
	}

	private static object ReadYaml_ArtifactLook(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactLook artifactLook = new ArtifactLook();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "name":
					artifactLook.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "model_key":
					artifactLook.model_key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return artifactLook;
	}

	private static object ReadYaml_ArtifactModel(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactModel artifactModel = new ArtifactModel();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "path":
				artifactModel.path = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "file_names":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				artifactModel.file_names = list.ToArray();
				break;
			}
			case "prototype_id":
				artifactModel.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return artifactModel;
	}

	private static object ReadYaml_ArtifactModelDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactModelDict artifactModelDict = new ArtifactModelDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			ArtifactModel value = ((reader.TokenType != JsonToken.Null) ? ((ArtifactModel)ReadYaml_ArtifactModel(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				artifactModelDict.Add(text, value);
			}
		}
		return artifactModelDict;
	}

	private static object ReadYaml_ArtifactPrototype(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactPrototype artifactPrototype = new ArtifactPrototype();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "__name__":
				artifactPrototype.__name__ = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon":
				artifactPrototype.icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "permanent":
				artifactPrototype.permanent = Convert.ToBoolean(reader.Value);
				break;
			case "rotatable_directions":
				artifactPrototype.rotatable_directions = Convert.ToInt32(reader.Value);
				break;
			case "size":
			{
				List<int> list2 = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item2 = Convert.ToInt32(reader.Value);
					list2.Add(item2);
				}
				artifactPrototype.size = list2.ToArray();
				break;
			}
			case "height":
				artifactPrototype.height = Convert.ToInt32(reader.Value);
				break;
			case "interior_set_effect":
				artifactPrototype.interior_set_effect = Convert.ToBoolean(reader.Value);
				break;
			case "is_size_variable":
				artifactPrototype.is_size_variable = Convert.ToBoolean(reader.Value);
				break;
			case "biomes":
			{
				List<Biome> list7 = new List<Biome>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Biome item7 = (Biome)Convert.ToInt32(reader.Value);
					list7.Add(item7);
				}
				artifactPrototype.biomes = list7.ToArray();
				break;
			}
			case "depth_min":
				artifactPrototype.depth_min = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					artifactPrototype.depth_min = Convert.ToSingle(reader.Value);
				}
				break;
			case "depth_max":
				artifactPrototype.depth_max = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					artifactPrototype.depth_max = Convert.ToSingle(reader.Value);
				}
				break;
			case "components":
			{
				List<string> list9 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item9 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list9.Add(item9);
				}
				artifactPrototype.components = list9.ToArray();
				break;
			}
			case "client_only_components":
			{
				List<string> list8 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item8 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list8.Add(item8);
				}
				artifactPrototype.client_only_components = list8.ToArray();
				break;
			}
			case "indicator":
				artifactPrototype.indicator = (IndicatorData)ReadYaml_IndicatorData(reader, objectType, existingValue, serializer);
				break;
			case "exclusive":
				artifactPrototype.exclusive = (Exclusive)Convert.ToInt32(reader.Value);
				break;
			case "exterior":
				artifactPrototype.exterior = Convert.ToBoolean(reader.Value);
				break;
			case "interior":
				artifactPrototype.interior = Convert.ToBoolean(reader.Value);
				break;
			case "transparent_site":
				artifactPrototype.transparent_site = Convert.ToBoolean(reader.Value);
				break;
			case "scribble":
				artifactPrototype.scribble = (ScribbleType)ReadYaml_ScribbleType(reader, objectType, existingValue, serializer);
				break;
			case "repair_requirement":
				artifactPrototype.repair_requirement = Convert.ToInt32(reader.Value);
				break;
			case "unoccupiable_tiles":
			{
				List<int[]> list5 = new List<int[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					List<int> list6 = new List<int>();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						int item5 = Convert.ToInt32(reader.Value);
						list6.Add(item5);
					}
					int[] item6 = list6.ToArray();
					list5.Add(item6);
				}
				artifactPrototype.unoccupiable_tiles = list5.ToArray();
				break;
			}
			case "effect_tiles":
			{
				List<int[]> list3 = new List<int[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					List<int> list4 = new List<int>();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						int item3 = Convert.ToInt32(reader.Value);
						list4.Add(item3);
					}
					int[] item4 = list4.ToArray();
					list3.Add(item4);
				}
				artifactPrototype.effect_tiles = list3.ToArray();
				break;
			}
			case "time_limited":
				artifactPrototype.time_limited = Convert.ToBoolean(reader.Value);
				break;
			case "is_craft":
				artifactPrototype.is_craft = Convert.ToBoolean(reader.Value);
				break;
			case "musics":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				artifactPrototype.musics = list.ToArray();
				break;
			}
			case "gender":
				artifactPrototype.gender = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "bound_radius":
				artifactPrototype.bound_radius = Convert.ToSingle(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return artifactPrototype;
	}

	private static object ReadYaml_ArtifactPrototypeDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactPrototypeDict artifactPrototypeDict = new ArtifactPrototypeDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			int key = Convert.ToInt32(reader.Value);
			reader.Read();
			ArtifactPrototype value = ((reader.TokenType != JsonToken.Null) ? ((ArtifactPrototype)ReadYaml_ArtifactPrototype(reader, objectType, existingValue, serializer)) : null);
			artifactPrototypeDict.Add(key, value);
		}
		return artifactPrototypeDict;
	}

	private static object ReadYaml_ArtifactSetEffectsYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ArtifactSetEffectsYaml artifactSetEffectsYaml = new ArtifactSetEffectsYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "mood":
			{
				Dictionary<string, ArtifactInteriorMood> dictionary2 = new Dictionary<string, ArtifactInteriorMood>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					ArtifactInteriorMood value2 = ((reader.TokenType != JsonToken.Null) ? ((ArtifactInteriorMood)ReadYaml_ArtifactInteriorMood(reader, objectType, existingValue, serializer)) : null);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				artifactSetEffectsYaml.InteriorMood = dictionary2;
				break;
			}
			case "set":
			{
				Dictionary<string, ArtifactInteriorSet> dictionary = new Dictionary<string, ArtifactInteriorSet>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					ArtifactInteriorSet value = ((reader.TokenType != JsonToken.Null) ? ((ArtifactInteriorSet)ReadYaml_ArtifactInteriorSet(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				artifactSetEffectsYaml.InteriorSet = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return artifactSetEffectsYaml;
	}

	private static object ReadYaml_Attendance(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Attendance attendance = default(Attendance);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "restore_cost")
				{
					attendance.RestoreCost = (RestoreCost)ReadYaml_RestoreCost(reader, objectType, existingValue, serializer);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'restore_cost' not found in JSON.");
		}
		return attendance;
	}

	private static object ReadYaml_Barehands(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Barehands barehands = new Barehands();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "attack_type":
					barehands.attack_type = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "weapon_framework":
					barehands.weapon_framework = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "battle_speed":
					barehands.battle_speed = Convert.ToSingle(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return barehands;
	}

	private static object ReadYaml_Battle(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Battle battle = default(Battle);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "hungry_ratio_enter_battle")
				{
					battle.HungryRatioEnterBattle = Convert.ToSingle(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'hungry_ratio_enter_battle' not found in JSON.");
		}
		return battle;
	}

	private static object ReadYaml_Blueprint(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Blueprint blueprint = new Blueprint();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "category":
				blueprint.category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "subcategory":
				blueprint.subcategory = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "name":
				blueprint.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				blueprint.description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "min_level":
				blueprint.min_level = Convert.ToInt32(reader.Value);
				break;
			case "max_level":
				blueprint.max_level = Convert.ToInt32(reader.Value);
				break;
			case "icon":
				blueprint.icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "default_look":
				blueprint.default_look = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "postprocess_time":
				blueprint.postprocess_time = Convert.ToInt32(reader.Value);
				break;
			case "preview":
				blueprint.preview = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "tool_tags":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				blueprint.tool_tags = dictionary;
				break;
			}
			case "slots":
			{
				List<BlueprintSlot> list = new List<BlueprintSlot>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					BlueprintSlot item = (BlueprintSlot)ReadYaml_BlueprintSlot(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				blueprint.slots = list.ToArray();
				break;
			}
			case "season":
				blueprint.season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "required_ability":
				blueprint.required_ability = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					blueprint.required_ability = (Derived)Convert.ToInt32(reader.Value);
				}
				break;
			case "required_blueprint":
				blueprint.required_blueprint = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return blueprint;
	}

	private static object ReadYaml_BlueprintDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BlueprintDict blueprintDict = new BlueprintDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Blueprint value = ((reader.TokenType != JsonToken.Null) ? ((Blueprint)ReadYaml_Blueprint(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				blueprintDict.Add(text, value);
			}
		}
		return blueprintDict;
	}

	private static object ReadYaml_BlueprintRemodelingsDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BlueprintRemodelingsDict blueprintRemodelingsDict = new BlueprintRemodelingsDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Dictionary<string, RemodelingBlueprint> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, RemodelingBlueprint> dictionary = new Dictionary<string, RemodelingBlueprint>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					RemodelingBlueprint value2 = ((reader.TokenType != JsonToken.Null) ? ((RemodelingBlueprint)ReadYaml_RemodelingBlueprint(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value2);
					}
				}
				value = dictionary;
			}
			if (text != null)
			{
				blueprintRemodelingsDict.Add(text, value);
			}
		}
		return blueprintRemodelingsDict;
	}

	private static object ReadYaml_BlueprintSlot(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BlueprintSlot blueprintSlot = new BlueprintSlot();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "slot_id":
				blueprintSlot.slot_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "slot_name":
				blueprintSlot.slot_name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "size_factor":
				blueprintSlot.size_factor = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "count":
				blueprintSlot.count = Convert.ToInt32(reader.Value);
				break;
			case "required_tags":
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				blueprintSlot.required_tags = dictionary2;
				break;
			}
			case "required_materials":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				blueprintSlot.required_materials = dictionary;
				break;
			}
			case "looks":
			{
				Dictionary<string, ArtifactLook> dictionary3 = new Dictionary<string, ArtifactLook>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text4 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					ArtifactLook value3 = ((reader.TokenType != JsonToken.Null) ? ((ArtifactLook)ReadYaml_ArtifactLook(reader, objectType, existingValue, serializer)) : null);
					if (text4 != null)
					{
						dictionary3.Add(text4, value3);
					}
				}
				blueprintSlot.looks = dictionary3;
				break;
			}
			case "source_info":
			{
				List<SlotSourceInfo> list = new List<SlotSourceInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					SlotSourceInfo item = (SlotSourceInfo)ReadYaml_SlotSourceInfo(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				blueprintSlot.source_info = list.ToArray();
				break;
			}
			case "default_look_tag":
				blueprintSlot.default_look_tag = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return blueprintSlot;
	}

	private static object ReadYaml_BodyParts(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BodyParts bodyParts = new BodyParts();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "dodge_ratio":
				bodyParts.dodge_ratio = Convert.ToSingle(reader.Value);
				break;
			case "defense_ratio":
			{
				Dictionary<string, float> dictionary = new Dictionary<string, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float value = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				bodyParts.defense_ratio = dictionary;
				break;
			}
			case "max_hp":
				bodyParts.max_hp = Convert.ToSingle(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return bodyParts;
	}

	private static object ReadYaml_BonusPrototype(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BonusPrototype bonusPrototype = new BonusPrototype();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "prototype_id":
					bonusPrototype.PrototypeId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "count":
					bonusPrototype.Count = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return bonusPrototype;
	}

	private static object ReadYaml_BonusPrototypes(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BonusPrototypes bonusPrototypes = new BonusPrototypes();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "rate":
				bonusPrototypes.Rate = Convert.ToSingle(reader.Value);
				break;
			case "prototypes":
			{
				List<BonusPrototype> list = new List<BonusPrototype>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					BonusPrototype item = (BonusPrototype)ReadYaml_BonusPrototype(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				bonusPrototypes.Prototypes = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return bonusPrototypes;
	}

	private static object ReadYaml_BonusPrototypeYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		BonusPrototypeYaml bonusPrototypeYaml = new BonusPrototypeYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			BonusPrototypes[] value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				List<BonusPrototypes> list = new List<BonusPrototypes>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					BonusPrototypes item = (BonusPrototypes)ReadYaml_BonusPrototypes(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				value = list.ToArray();
			}
			if (text != null)
			{
				bonusPrototypeYaml.Add(text, value);
			}
		}
		return bonusPrototypeYaml;
	}

	private static object ReadYaml_Build(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Build build = default(Build);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "condition_scales")
			{
				Dictionary<int, float> dictionary = new Dictionary<int, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					float value = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					dictionary.Add(key, value);
				}
				build.ConditionoScales = dictionary;
				flag = true;
			}
			else
			{
				reader.Skip();
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'condition_scales' not found in JSON.");
		}
		return build;
	}

	private static object ReadYaml_CargoCost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CargoCost cargoCost = default(CargoCost);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "immediate_receiving_cost")
				{
					cargoCost.ImmediateReceivingCost = ((reader.Value != null) ? reader.Value.ToString() : null);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return cargoCost;
	}

	private static object ReadYaml_CashYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CashYaml cashYaml = new CashYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "instant_construction")
			{
				List<int[]> list = new List<int[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					List<int> list2 = new List<int>();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						int item = Convert.ToInt32(reader.Value);
						list2.Add(item);
					}
					int[] item2 = list2.ToArray();
					list.Add(item2);
				}
				cashYaml.instant_construction = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return cashYaml;
	}

	private static object ReadYaml_Chapter(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Chapter chapter = new Chapter();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "chapter":
				chapter.ChapterNum = Convert.ToInt32(reader.Value);
				break;
			case "title":
				chapter.Title = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				chapter.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "image":
				chapter.Image = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "movie":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string value = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				chapter.Movie = dictionary;
				break;
			}
			case "quests":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				chapter.Quests = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return chapter;
	}

	private static object ReadYaml_Chapters(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Chapters chapters = new Chapters();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "chapters")
			{
				List<Chapter> list = new List<Chapter>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Chapter item = (Chapter)ReadYaml_Chapter(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				chapters.ChapterList = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return chapters;
	}

	private static object ReadYaml_ClanLevelReward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ClanLevelReward clanLevelReward = default(ClanLevelReward);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "description")
				{
					clanLevelReward.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return clanLevelReward;
	}

	private static object ReadYaml_ClanResearch(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ClanResearch clanResearch = new ClanResearch();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "duration":
					clanResearch.Duration = Convert.ToDouble(reader.Value);
					break;
				case "category":
					clanResearch.Category = (ResearchCategory)Convert.ToInt32(reader.Value);
					break;
				case "name":
					clanResearch.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "effect":
					clanResearch.Effect = (ResearchEffect)ReadYaml_ResearchEffect(reader, objectType, existingValue, serializer);
					break;
				case "currency":
					clanResearch.Currency = (Currency)Convert.ToInt32(reader.Value);
					break;
				case "amount":
					clanResearch.Amount = Convert.ToInt32(reader.Value);
					break;
				case "icon":
					clanResearch.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "tier":
					clanResearch.Tier = (LaboratoryTier)Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return clanResearch;
	}

	private static object ReadYaml_ClanResearchs(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ClanResearchs clanResearchs = new ClanResearchs();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			ClanResearch value = ((reader.TokenType != JsonToken.Null) ? ((ClanResearch)ReadYaml_ClanResearch(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				clanResearchs.Add(text, value);
			}
		}
		return clanResearchs;
	}

	private static object ReadYaml_ClanYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ClanYaml clanYaml = new ClanYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "level_thresholds":
			{
				List<long> list = new List<long>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					long item = Convert.ToInt64(reader.Value);
					list.Add(item);
				}
				clanYaml.LevelThresholds = list.ToArray();
				break;
			}
			case "level_rewards":
			{
				Dictionary<int, ClanLevelReward> dictionary = new Dictionary<int, ClanLevelReward>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					ClanLevelReward value = ((reader.TokenType != JsonToken.Null) ? ((ClanLevelReward)ReadYaml_ClanLevelReward(reader, objectType, existingValue, serializer)) : default(ClanLevelReward));
					dictionary.Add(key, value);
				}
				clanYaml.LevelRewards = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return clanYaml;
	}

	private static object ReadYaml_Clear(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Clear clear = default(Clear);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "days_after":
					clear.DaysAfter = Convert.ToInt32(reader.Value);
					break;
				case "reward_amount":
					clear.RewardAmount = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return clear;
	}

	private static object ReadYaml_ClearTime(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ClearTime clearTime = default(ClearTime);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "days":
					clearTime.Days = Convert.ToInt32(reader.Value);
					break;
				case "reward_amount":
					clearTime.RewardAmount = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return clearTime;
	}

	private static object ReadYaml_CollectibleYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CollectibleYaml collectibleYaml = new CollectibleYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Gettext value = ((reader.TokenType != JsonToken.Null) ? ((Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer)) : default(Gettext));
			if (text != null)
			{
				collectibleYaml.Add(text, value);
			}
		}
		return collectibleYaml;
	}

	private static object ReadYaml_Commodities(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Commodities commodities = new Commodities();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "promotion_links":
			{
				List<PromotionLink> list = new List<PromotionLink>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PromotionLink item = (PromotionLink)ReadYaml_PromotionLink(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				commodities.PromotionLinks = list.ToArray();
				break;
			}
			case "posted_commodities":
			{
				Dictionary<string, Commodity> dictionary2 = new Dictionary<string, Commodity>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Commodity value2 = ((reader.TokenType != JsonToken.Null) ? ((Commodity)ReadYaml_Commodity(reader, objectType, existingValue, serializer)) : null);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				commodities.PostedCommodities = dictionary2;
				break;
			}
			case "test_commodities":
			{
				Dictionary<string, Commodity> dictionary = new Dictionary<string, Commodity>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Commodity value = ((reader.TokenType != JsonToken.Null) ? ((Commodity)ReadYaml_Commodity(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				commodities.TestCommodities = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return commodities;
	}

	private static object ReadYaml_Commodity(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Commodity commodity = new Commodity();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "type":
				commodity.Type = (CommodityType)Convert.ToInt32(reader.Value);
				break;
			case "name":
				commodity.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				commodity.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "large_icon":
				commodity.LargeIcon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon_colors":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				commodity.IconColors = list.ToArray();
				break;
			}
			case "description":
				commodity.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description_first_purchase":
				commodity.FirstPurchaseDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "contents":
				commodity.Contents = (ShopContents)ReadYaml_ShopContents(reader, objectType, existingValue, serializer);
				break;
			case "daily_contents":
				commodity.DailyContents = (ShopContents)ReadYaml_ShopContents(reader, objectType, existingValue, serializer);
				break;
			case "source_commodity_id":
				commodity.SourceCommodityId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "warning":
				commodity.Warning = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "ui_category":
				commodity.Category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "tags":
			{
				List<Tags> list3 = new List<Tags>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Tags item3 = (Tags)Convert.ToInt32(reader.Value);
					list3.Add(item3);
				}
				commodity.Tags = list3.ToArray();
				break;
			}
			case "iap_product_id":
				commodity.IapProductId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "price_currency":
				commodity.PriceCurrency = (Currency)Convert.ToInt32(reader.Value);
				break;
			case "price_amount":
				commodity.PriceAmount = Convert.ToInt64(reader.Value);
				break;
			case "original_price_amount":
				commodity.OriginalPriceAmount = Convert.ToInt64(reader.Value);
				break;
			case "gem_amount":
				commodity.GemAmount = Convert.ToInt64(reader.Value);
				break;
			case "coin_amount":
				commodity.CoinAmount = Convert.ToInt64(reader.Value);
				break;
			case "coin_bonus":
				commodity.CoinBonus = Convert.ToInt64(reader.Value);
				break;
			case "coin_first_purchase_bonus":
				commodity.CoinFirstPurchaseBonus = Convert.ToInt64(reader.Value);
				break;
			case "count":
				commodity.Count = Convert.ToInt32(reader.Value);
				break;
			case "bonus_mileage":
				commodity.BonusMileage = Convert.ToInt32(reader.Value);
				break;
			case "slot_type":
				commodity.SlotType = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					commodity.SlotType = (EquipSlotType)Convert.ToInt32(reader.Value);
				}
				break;
			case "order":
				commodity.Order = Convert.ToInt32(reader.Value);
				break;
			case "content_descriptions":
			{
				List<ContentDescription> list2 = new List<ContentDescription>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ContentDescription item2 = (ContentDescription)ReadYaml_ContentDescription(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				commodity.ContentDescriptions = list2.ToArray();
				break;
			}
			case "purchase_limit":
				commodity.PurchaseLimit = (PurchaseLimit)ReadYaml_PurchaseLimit(reader, objectType, existingValue, serializer);
				break;
			case "purchase_condition":
				commodity.PurchaseCondition = (CommodityCondition)ReadYaml_CommodityCondition(reader, objectType, existingValue, serializer);
				break;
			case "voucher_amount":
				commodity.VoucherAmount = Convert.ToInt32(reader.Value);
				break;
			case "voucher_id":
				commodity.VoucherId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "steam_dlc_app_id":
				commodity.SteamDlcAppId = Convert.ToUInt32(reader.Value);
				break;
			case "sub_commodities":
			{
				Dictionary<string, Commodity> dictionary = new Dictionary<string, Commodity>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Commodity value = ((reader.TokenType != JsonToken.Null) ? ((Commodity)ReadYaml_Commodity(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				commodity.SubCommodities = dictionary;
				break;
			}
			case "accept_condition":
				commodity.AcceptCondition = (CommodityCondition)ReadYaml_CommodityCondition(reader, objectType, existingValue, serializer);
				break;
			case "sub_commodity_accept_limit":
				commodity.SubCommodityAcceptLimit = (SubCommodityAcceptLimit)ReadYaml_SubCommodityAcceptLimit(reader, objectType, existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return commodity;
	}

	private static object ReadYaml_CommodityCondition(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CommodityCondition commodityCondition = new CommodityCondition();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "min_level":
				commodityCondition.MinLevel = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					commodityCondition.MinLevel = Convert.ToInt32(reader.Value);
				}
				break;
			case "max_level":
				commodityCondition.MaxLevel = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					commodityCondition.MaxLevel = Convert.ToInt32(reader.Value);
				}
				break;
			case "season2_resource_type":
				commodityCondition.ResourceType = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					commodityCondition.ResourceType = (ResourceType)Convert.ToInt32(reader.Value);
				}
				break;
			case "season2_supply_level":
				commodityCondition.WarpRushSupplyLevel = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					commodityCondition.WarpRushSupplyLevel = Convert.ToInt32(reader.Value);
				}
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return commodityCondition;
	}

	private static object ReadYaml_CommodityContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CommodityContent commodityContent = new CommodityContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "key":
					commodityContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_in_shop":
					commodityContent.hide_in_shop = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return commodityContent;
	}

	private static object ReadYaml_ConstantPet(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ConstantPet constantPet = default(ConstantPet);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "domesticate_probability":
				constantPet.DomesticateProbability = ((reader.Value != null) ? reader.Value.ToString() : null);
				flag = true;
				break;
			case "domesticate_time":
				constantPet.DomesticateTime = ((reader.Value != null) ? reader.Value.ToString() : null);
				flag2 = true;
				break;
			case "task_time":
				constantPet.TaskTime = ((reader.Value != null) ? reader.Value.ToString() : null);
				flag3 = true;
				break;
			case "battle":
				constantPet.Battle = (Battle)ReadYaml_Battle(reader, objectType, existingValue, serializer);
				flag4 = true;
				break;
			case "domesticate_decrease_limit":
				constantPet.DomesticateDecreaseLimit = Convert.ToSingle(reader.Value);
				flag5 = true;
				break;
			case "performance_reference":
			{
				Dictionary<string, string[]> dictionary2 = new Dictionary<string, string[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string[] value2;
					if (reader.TokenType == JsonToken.Null)
					{
						value2 = null;
					}
					else
					{
						List<string> list4 = new List<string>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							string item4 = ((reader.Value != null) ? reader.Value.ToString() : null);
							list4.Add(item4);
						}
						value2 = list4.ToArray();
					}
					if (text2 != null)
					{
						dictionary2.Add(text2, value2);
					}
				}
				constantPet.PerformanceReference = dictionary2;
				flag6 = true;
				break;
			}
			case "reinify_tags":
			{
				List<string> list3 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list3.Add(item3);
				}
				constantPet.ReinifyTags = list3.ToArray();
				flag7 = true;
				break;
			}
			case "milestone_level":
			{
				Dictionary<int, int[][]> dictionary = new Dictionary<int, int[][]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					int[][] value;
					if (reader.TokenType == JsonToken.Null)
					{
						value = null;
					}
					else
					{
						List<int[]> list = new List<int[]>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							List<int> list2 = new List<int>();
							while (reader.Read() && reader.TokenType != JsonToken.EndArray)
							{
								int item = Convert.ToInt32(reader.Value);
								list2.Add(item);
							}
							int[] item2 = list2.ToArray();
							list.Add(item2);
						}
						value = list.ToArray();
					}
					dictionary.Add(key, value);
				}
				constantPet.MilestoneLevel = dictionary;
				flag8 = true;
				break;
			}
			case "default_feed_energy":
				constantPet.DefaultFeedEnergy = Convert.ToSingle(reader.Value);
				flag9 = true;
				break;
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'domesticate_probability' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'domesticate_time' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'task_time' not found in JSON.");
		}
		if (!flag4)
		{
			throw new JsonSerializationException("Required property 'battle' not found in JSON.");
		}
		if (!flag5)
		{
			throw new JsonSerializationException("Required property 'domesticate_decrease_limit' not found in JSON.");
		}
		if (!flag6)
		{
			throw new JsonSerializationException("Required property 'performance_reference' not found in JSON.");
		}
		if (!flag7)
		{
			throw new JsonSerializationException("Required property 'reinify_tags' not found in JSON.");
		}
		if (!flag8)
		{
			throw new JsonSerializationException("Required property 'milestone_level' not found in JSON.");
		}
		if (!flag9)
		{
			throw new JsonSerializationException("Required property 'default_feed_energy' not found in JSON.");
		}
		return constantPet;
	}

	private static object ReadYaml_Constants(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Constants constants = new Constants();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		bool flag11 = false;
		bool flag12 = false;
		bool flag13 = false;
		bool flag14 = false;
		bool flag15 = false;
		bool flag16 = false;
		bool flag17 = false;
		bool flag18 = false;
		bool flag19 = false;
		bool flag20 = false;
		bool flag21 = false;
		bool flag22 = false;
		bool flag23 = false;
		bool flag24 = false;
		bool flag25 = false;
		bool flag26 = false;
		bool flag27 = false;
		bool flag28 = false;
		bool flag29 = false;
		bool flag30 = false;
		bool flag31 = false;
		bool flag32 = false;
		bool flag33 = false;
		bool flag34 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "newbie_level":
				constants.NewbieLevel = Convert.ToInt32(reader.Value);
				flag = true;
				break;
			case "dash":
				constants.Dash = (Dash)ReadYaml_Dash(reader, objectType, existingValue, serializer);
				flag2 = true;
				break;
			case "explorer":
				constants.Explorer = (Explorer)ReadYaml_Explorer(reader, objectType, existingValue, serializer);
				flag3 = true;
				break;
			case "item":
				constants.Item = (ConstantsItem)ReadYaml_ConstantsItem(reader, objectType, existingValue, serializer);
				flag4 = true;
				break;
			case "market":
				constants.Market = (Market)ReadYaml_Market(reader, objectType, existingValue, serializer);
				flag5 = true;
				break;
			case "max_levels":
				constants.MaxLevels = (MaxLevels)ReadYaml_MaxLevels(reader, objectType, existingValue, serializer);
				flag6 = true;
				break;
			case "natural_components":
			{
				Dictionary<int, NaturalComponentInfo> dictionary2 = new Dictionary<int, NaturalComponentInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					NaturalComponentInfo value2 = ((reader.TokenType != JsonToken.Null) ? ((NaturalComponentInfo)ReadYaml_NaturalComponentInfo(reader, objectType, existingValue, serializer)) : null);
					dictionary2.Add(key, value2);
				}
				constants.NaturalComponents = dictionary2;
				flag7 = true;
				break;
			}
			case "repair":
				constants.Repair = (Repair)ReadYaml_Repair(reader, objectType, existingValue, serializer);
				flag8 = true;
				break;
			case "faction":
				constants.Faction = (FactionInfo)ReadYaml_FactionInfo(reader, objectType, existingValue, serializer);
				flag9 = true;
				break;
			case "estate":
				constants.Estate = (Estate)ReadYaml_Estate(reader, objectType, existingValue, serializer);
				flag10 = true;
				break;
			case "ally":
				constants.Ally = (Ally)ReadYaml_Ally(reader, objectType, existingValue, serializer);
				flag11 = true;
				break;
			case "war":
				constants.War = (War)ReadYaml_War(reader, objectType, existingValue, serializer);
				flag12 = true;
				break;
			case "build":
				constants.Build = (Build)ReadYaml_Build(reader, objectType, existingValue, serializer);
				flag13 = true;
				break;
			case "warehouse":
				constants.Warehouse = (Warehouse)ReadYaml_Warehouse(reader, objectType, existingValue, serializer);
				flag14 = true;
				break;
			case "sailing":
				constants.Sailing = (Sailing)ReadYaml_Sailing(reader, objectType, existingValue, serializer);
				flag15 = true;
				break;
			case "season2":
				constants.Season2 = (Season2)ReadYaml_Season2(reader, objectType, existingValue, serializer);
				flag16 = true;
				break;
			case "resistance":
				constants.Resistance = (Resistance)ReadYaml_Resistance(reader, objectType, existingValue, serializer);
				flag17 = true;
				break;
			case "skill_untrain":
				constants.SkillUntrain = (SkillUntrain)ReadYaml_SkillUntrain(reader, objectType, existingValue, serializer);
				flag18 = true;
				break;
			case "skill":
				constants.Skill = (SkillConstants)ReadYaml_SkillConstants(reader, objectType, existingValue, serializer);
				flag19 = true;
				break;
			case "pet":
				constants.Pet = (ConstantPet)ReadYaml_ConstantPet(reader, objectType, existingValue, serializer);
				flag20 = true;
				break;
			case "attendance":
				constants.Attendance = (Attendance)ReadYaml_Attendance(reader, objectType, existingValue, serializer);
				flag21 = true;
				break;
			case "faction_support":
				constants.FactionSupport = (FactionSupport)ReadYaml_FactionSupport(reader, objectType, existingValue, serializer);
				flag22 = true;
				break;
			case "sprinkler":
				constants.Sprinkler = (Sprinkler)ReadYaml_Sprinkler(reader, objectType, existingValue, serializer);
				flag23 = true;
				break;
			case "crack":
				constants.Crack = (Crack)ReadYaml_Crack(reader, objectType, existingValue, serializer);
				flag24 = true;
				break;
			case "personal_region":
				constants.PersonalRegion = (PersonalRegion)ReadYaml_PersonalRegion(reader, objectType, existingValue, serializer);
				flag25 = true;
				break;
			case "represent_powers":
			{
				Dictionary<RepresentType, Dictionary<Derived, float>> dictionary5 = new Dictionary<RepresentType, Dictionary<Derived, float>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					RepresentType key2 = (RepresentType)Convert.ToInt32(reader.Value);
					reader.Read();
					Dictionary<Derived, float> value5;
					if (reader.TokenType == JsonToken.Null)
					{
						value5 = null;
					}
					else
					{
						Dictionary<Derived, float> dictionary6 = new Dictionary<Derived, float>();
						while (reader.Read() && reader.TokenType != JsonToken.EndObject)
						{
							Derived key3 = (Derived)Convert.ToInt32(reader.Value);
							reader.Read();
							float value6 = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
							dictionary6.Add(key3, value6);
						}
						value5 = dictionary6;
					}
					dictionary5.Add(key2, value5);
				}
				constants.RepresentAbilities = dictionary5;
				flag26 = true;
				break;
			}
			case "skip_tutorial_missions":
			{
				Dictionary<string, SkipTutorialMissions> dictionary4 = new Dictionary<string, SkipTutorialMissions>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text4 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					SkipTutorialMissions value4 = ((reader.TokenType != JsonToken.Null) ? ((SkipTutorialMissions)ReadYaml_SkipTutorialMissions(reader, objectType, existingValue, serializer)) : null);
					if (text4 != null)
					{
						dictionary4.Add(text4, value4);
					}
				}
				constants.SkipTutorialMissionses = dictionary4;
				flag27 = true;
				break;
			}
			case "durability":
				constants.Durability = (Durability)ReadYaml_Durability(reader, objectType, existingValue, serializer);
				flag28 = true;
				break;
			case "taming":
				constants.Taming = (Taming)ReadYaml_Taming(reader, objectType, existingValue, serializer);
				flag29 = true;
				break;
			case "mini_game_dance":
			{
				Dictionary<string, float> dictionary3 = new Dictionary<string, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float value3 = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					if (text3 != null)
					{
						dictionary3.Add(text3, value3);
					}
				}
				constants.MiniGameDance = dictionary3;
				flag30 = true;
				break;
			}
			case "warp_accelerator":
				constants.WarpAccelerator = (Yaml.WarpAccelerator)ReadYaml_WarpAccelerator(reader, objectType, existingValue, serializer);
				flag31 = true;
				break;
			case "put_in_container_infos":
			{
				Dictionary<string, PutInContainerInfo> dictionary = new Dictionary<string, PutInContainerInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					PutInContainerInfo value = ((reader.TokenType != JsonToken.Null) ? ((PutInContainerInfo)ReadYaml_PutInContainerInfo(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				constants.PutInContainerInfos = dictionary;
				flag32 = true;
				break;
			}
			case "musician":
				constants.Musician = (Musician)ReadYaml_Musician(reader, objectType, existingValue, serializer);
				flag33 = true;
				break;
			case "artifact_floor":
				constants.ArtifactFloor = (ArtifactFloor)ReadYaml_ArtifactFloor(reader, objectType, existingValue, serializer);
				flag34 = true;
				break;
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'newbie_level' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'dash' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'explorer' not found in JSON.");
		}
		if (!flag4)
		{
			throw new JsonSerializationException("Required property 'item' not found in JSON.");
		}
		if (!flag5)
		{
			throw new JsonSerializationException("Required property 'market' not found in JSON.");
		}
		if (!flag6)
		{
			throw new JsonSerializationException("Required property 'max_levels' not found in JSON.");
		}
		if (!flag7)
		{
			throw new JsonSerializationException("Required property 'natural_components' not found in JSON.");
		}
		if (!flag8)
		{
			throw new JsonSerializationException("Required property 'repair' not found in JSON.");
		}
		if (!flag9)
		{
			throw new JsonSerializationException("Required property 'faction' not found in JSON.");
		}
		if (!flag10)
		{
			throw new JsonSerializationException("Required property 'estate' not found in JSON.");
		}
		if (!flag11)
		{
			throw new JsonSerializationException("Required property 'ally' not found in JSON.");
		}
		if (!flag12)
		{
			throw new JsonSerializationException("Required property 'war' not found in JSON.");
		}
		if (!flag13)
		{
			throw new JsonSerializationException("Required property 'build' not found in JSON.");
		}
		if (!flag14)
		{
			throw new JsonSerializationException("Required property 'warehouse' not found in JSON.");
		}
		if (!flag15)
		{
			throw new JsonSerializationException("Required property 'sailing' not found in JSON.");
		}
		if (!flag16)
		{
			throw new JsonSerializationException("Required property 'season2' not found in JSON.");
		}
		if (!flag17)
		{
			throw new JsonSerializationException("Required property 'resistance' not found in JSON.");
		}
		if (!flag18)
		{
			throw new JsonSerializationException("Required property 'skill_untrain' not found in JSON.");
		}
		if (!flag19)
		{
			throw new JsonSerializationException("Required property 'skill' not found in JSON.");
		}
		if (!flag20)
		{
			throw new JsonSerializationException("Required property 'pet' not found in JSON.");
		}
		if (!flag21)
		{
			throw new JsonSerializationException("Required property 'attendance' not found in JSON.");
		}
		if (!flag22)
		{
			throw new JsonSerializationException("Required property 'faction_support' not found in JSON.");
		}
		if (!flag23)
		{
			throw new JsonSerializationException("Required property 'sprinkler' not found in JSON.");
		}
		if (!flag24)
		{
			throw new JsonSerializationException("Required property 'crack' not found in JSON.");
		}
		if (!flag25)
		{
			throw new JsonSerializationException("Required property 'personal_region' not found in JSON.");
		}
		if (!flag26)
		{
			throw new JsonSerializationException("Required property 'represent_powers' not found in JSON.");
		}
		if (!flag27)
		{
			throw new JsonSerializationException("Required property 'skip_tutorial_missions' not found in JSON.");
		}
		if (!flag28)
		{
			throw new JsonSerializationException("Required property 'durability' not found in JSON.");
		}
		if (!flag29)
		{
			throw new JsonSerializationException("Required property 'taming' not found in JSON.");
		}
		if (!flag30)
		{
			throw new JsonSerializationException("Required property 'mini_game_dance' not found in JSON.");
		}
		if (!flag31)
		{
			throw new JsonSerializationException("Required property 'warp_accelerator' not found in JSON.");
		}
		if (!flag32)
		{
			throw new JsonSerializationException("Required property 'put_in_container_infos' not found in JSON.");
		}
		if (!flag33)
		{
			throw new JsonSerializationException("Required property 'musician' not found in JSON.");
		}
		if (!flag34)
		{
			throw new JsonSerializationException("Required property 'artifact_floor' not found in JSON.");
		}
		return constants;
	}

	private static object ReadYaml_ConstantsItem(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ConstantsItem constantsItem = default(ConstantsItem);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "dye_recipe":
			{
				Dictionary<ColorChannel, string> dictionary2 = new Dictionary<ColorChannel, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					ColorChannel key2 = (ColorChannel)Convert.ToInt32(reader.Value);
					reader.Read();
					string value2 = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					dictionary2.Add(key2, value2);
				}
				constantsItem.DyeRecipe = dictionary2;
				flag = true;
				break;
			}
			case "bleach_recipe":
			{
				Dictionary<ColorChannel, string> dictionary = new Dictionary<ColorChannel, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					ColorChannel key = (ColorChannel)Convert.ToInt32(reader.Value);
					reader.Read();
					string value = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					dictionary.Add(key, value);
				}
				constantsItem.BleachRecipe = dictionary;
				flag2 = true;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'dye_recipe' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'bleach_recipe' not found in JSON.");
		}
		return constantsItem;
	}

	private static object ReadYaml_ContentDescription(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ContentDescription contentDescription = new ContentDescription();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "icon_description":
				contentDescription.IconDescription = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "name":
				contentDescription.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "text":
				contentDescription.Text = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon_colors":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				contentDescription.IconColors = list.ToArray();
				break;
			}
			case "source_key":
				contentDescription.SourceKey = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon":
				contentDescription.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "only_popup":
				contentDescription.OnlyPopup = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return contentDescription;
	}

	private static object ReadYaml_Cost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Cost cost = new Cost();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "amount":
					cost.Amount = JToken.ReadFrom(reader);
					break;
				case "currency":
					cost.Currency = (Currency)Convert.ToInt32(reader.Value);
					break;
				case "voucher_amount":
					cost.VoucherAmount = Convert.ToInt32(reader.Value);
					break;
				case "voucher_id":
					cost.VoucherId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return cost;
	}

	private static object ReadYaml_CostsYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CostsYaml costsYaml = new CostsYaml();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		bool flag11 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "estate":
					costsYaml.Estate = (EstateCost)ReadYaml_EstateCost(reader, objectType, existingValue, serializer);
					flag = true;
					break;
				case "cargo":
					costsYaml.Cargo = (CargoCost)ReadYaml_CargoCost(reader, objectType, existingValue, serializer);
					flag2 = true;
					break;
				case "skill_untrain":
					costsYaml.SkillUntrainVoucher = (SkillUntrainInfo)ReadYaml_SkillUntrainInfo(reader, objectType, existingValue, serializer);
					flag3 = true;
					break;
				case "balloon_ticket":
					costsYaml.BalloonTicket = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag4 = true;
					break;
				case "artifact_rename":
					costsYaml.ArtifactRename = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag5 = true;
					break;
				case "clan_warphole_visit":
					costsYaml.ClanWarpholeVisit = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag6 = true;
					break;
				case "pet_revert_active_skill":
					costsYaml.PetRevertActiveSkill = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag7 = true;
					break;
				case "pet_revert_milestone":
					costsYaml.PetRevertMilestone = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag8 = true;
					break;
				case "pet_revert_rank":
					costsYaml.PetRevertRank = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag9 = true;
					break;
				case "reset_reform_slot":
					costsYaml.ResetReformSlot = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag10 = true;
					break;
				case "expert_reform_estimate":
					costsYaml.ReformTechSupportEstimate = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					flag11 = true;
					break;
				case "encyclopedia_mastery_swap":
					costsYaml.EncyclopediaMasterySwap = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					break;
				case "artifact_extend_floor":
					costsYaml.ArtifactExtendFloor = (Cost)ReadYaml_Cost(reader, objectType, existingValue, serializer);
					break;
				case "purchase_r_piece":
					costsYaml.RandomPiece = (PurchaseRandomPiece)ReadYaml_PurchaseRandomPiece(reader, objectType, existingValue, serializer);
					break;
				case "revive_immediately":
					costsYaml.ReviveImmediately = (ReviveImmediatelyCost)ReadYaml_ReviveImmediatelyCost(reader, objectType, existingValue, serializer);
					break;
				case "open_map":
					costsYaml.OpenMap = (OpenMapCost)ReadYaml_OpenMapCost(reader, objectType, existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'estate' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'cargo' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'skill_untrain' not found in JSON.");
		}
		if (!flag4)
		{
			throw new JsonSerializationException("Required property 'balloon_ticket' not found in JSON.");
		}
		if (!flag5)
		{
			throw new JsonSerializationException("Required property 'artifact_rename' not found in JSON.");
		}
		if (!flag6)
		{
			throw new JsonSerializationException("Required property 'clan_warphole_visit' not found in JSON.");
		}
		if (!flag7)
		{
			throw new JsonSerializationException("Required property 'pet_revert_active_skill' not found in JSON.");
		}
		if (!flag8)
		{
			throw new JsonSerializationException("Required property 'pet_revert_milestone' not found in JSON.");
		}
		if (!flag9)
		{
			throw new JsonSerializationException("Required property 'pet_revert_rank' not found in JSON.");
		}
		if (!flag10)
		{
			throw new JsonSerializationException("Required property 'reset_reform_slot' not found in JSON.");
		}
		if (!flag11)
		{
			throw new JsonSerializationException("Required property 'expert_reform_estimate' not found in JSON.");
		}
		return costsYaml;
	}

	private static object ReadYaml_Crack(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Crack crack = new Crack();
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "required_voucher_id")
				{
					crack.VoucherId = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'required_voucher_id' not found in JSON.");
		}
		return crack;
	}

	private static object ReadYaml_CropData(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CropData cropData = new CropData();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			CropInfo value = ((reader.TokenType != JsonToken.Null) ? ((CropInfo)ReadYaml_CropInfo(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				cropData.Add(text, value);
			}
		}
		return cropData;
	}

	private static object ReadYaml_CropInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CropInfo cropInfo = new CropInfo();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "grows_until":
					cropInfo.GrowsUntill = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag = true;
					break;
				case "required_water":
					cropInfo.RequiredWater = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag2 = true;
					break;
				case "required_fertilizer":
					cropInfo.RequiredFertilizer = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag3 = true;
					break;
				case "name":
					cropInfo.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "description":
					cropInfo.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "icon":
					cropInfo.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "color_r":
					cropInfo.ColorR = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "color_g":
					cropInfo.ColorG = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "color_b":
					cropInfo.ColorB = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "max_level":
					cropInfo.MaxLevel = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'grows_until' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'required_water' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'required_fertilizer' not found in JSON.");
		}
		return cropInfo;
	}

	private static object ReadYaml_WarpRushReward_CurrencyInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		WarpRushReward.CurrencyInfo currencyInfo = new WarpRushReward.CurrencyInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "currency_amount":
					currencyInfo.Amount = Convert.ToInt32(reader.Value);
					break;
				case "currency_type":
					currencyInfo.Type = (Currency)Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return currencyInfo;
	}

	private static object ReadYaml_Dash(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Dash dash = default(Dash);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "stamina")
				{
					dash.Stamina = Convert.ToInt32(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'stamina' not found in JSON.");
		}
		return dash;
	}

	private static object ReadYaml_DateTimeDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DateTimeDict dateTimeDict = new DateTimeDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			DateTimeYaml value = ((reader.TokenType != JsonToken.Null) ? ((DateTimeYaml)ReadYaml_DateTimeYaml(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				dateTimeDict.Add(text, value);
			}
		}
		return dateTimeDict;
	}

	private static object ReadYaml_DateTimeYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DateTimeYaml dateTimeYaml = new DateTimeYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "daytime":
				dateTimeYaml.Daytime = Convert.ToInt32(reader.Value);
				break;
			case "sunrise":
			{
				List<int> list2 = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item2 = Convert.ToInt32(reader.Value);
					list2.Add(item2);
				}
				dateTimeYaml.Sunrise = list2.ToArray();
				break;
			}
			case "sunset":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				dateTimeYaml.Sunset = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return dateTimeYaml;
	}

	private static object ReadYaml_DerivedReward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DerivedReward derivedReward = new DerivedReward();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "type":
					derivedReward.Type = (Shared.Faction.RewardType)Convert.ToInt32(reader.Value);
					break;
				case "modifier_id":
					derivedReward.ModifierId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "value":
					derivedReward.Value = Convert.ToSingle(reader.Value);
					break;
				case "recipe_id":
					derivedReward.RecipeId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "blueprint_id":
					derivedReward.BlueprintId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return derivedReward;
	}

	private static object ReadYaml_DerivedRewardData(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DerivedRewardData derivedRewardData = new DerivedRewardData();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "required_value":
					derivedRewardData.RequiredValue = Convert.ToInt32(reader.Value);
					break;
				case "reward_id":
					derivedRewardData.RewardId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return derivedRewardData;
	}

	private static object ReadYaml_DerivedRewardDatas(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DerivedRewardDatas derivedRewardDatas = new DerivedRewardDatas();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Derived key = (Derived)Convert.ToInt32(reader.Value);
			reader.Read();
			DerivedRewardData[] value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				List<DerivedRewardData> list = new List<DerivedRewardData>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					DerivedRewardData item = (DerivedRewardData)ReadYaml_DerivedRewardData(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				value = list.ToArray();
			}
			derivedRewardDatas.Add(key, value);
		}
		return derivedRewardDatas;
	}

	private static object ReadYaml_DerivedRewards(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DerivedRewards derivedRewards = new DerivedRewards();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			DerivedReward value = ((reader.TokenType != JsonToken.Null) ? ((DerivedReward)ReadYaml_DerivedReward(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				derivedRewards.Add(text, value);
			}
		}
		return derivedRewards;
	}

	private static object ReadYaml_Dialogue(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Dialogue dialogue = new Dialogue();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "blur":
				dialogue.Blur = Convert.ToBoolean(reader.Value);
				break;
			case "remote":
				dialogue.Remote = Convert.ToBoolean(reader.Value);
				break;
			case "talks":
			{
				List<MissionTalk> list = new List<MissionTalk>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					MissionTalk item = (MissionTalk)ReadYaml_MissionTalk(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				dialogue.Talks = list;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return dialogue;
	}

	private static object ReadYaml_Durability(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Durability durability = default(Durability);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "daily_velocity")
				{
					durability.DailyVelocity = Convert.ToSingle(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'daily_velocity' not found in JSON.");
		}
		return durability;
	}

	private static object ReadYaml_DurabilityResult(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		DurabilityResult durabilityResult = default(DurabilityResult);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "failure")
				{
					durabilityResult.Failure = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'failure' not found in JSON.");
		}
		return durabilityResult;
	}

	private static object ReadYaml_EffectDetail(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EffectDetail effectDetail = new EffectDetail();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "value":
					effectDetail.Value = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "type":
					effectDetail.Type = (EffectType)Convert.ToInt32(reader.Value);
					break;
				case "key":
					effectDetail.Key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return effectDetail;
	}

	private static object ReadYaml_Emoticon(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Emoticon emoticon = default(Emoticon);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "id":
					emoticon.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "default":
					emoticon.Default = Convert.ToBoolean(reader.Value);
					break;
				case "free":
					emoticon.Free = Convert.ToBoolean(reader.Value);
					break;
				case "icon":
					emoticon.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return emoticon;
	}

	private static object ReadYaml_Emotions(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Emotions emotions = new Emotions();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "emoticons":
			{
				List<Emoticon> list = new List<Emoticon>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Emoticon item = (Emoticon)ReadYaml_Emoticon(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				emotions.Emoticons = list.ToArray();
				break;
			}
			case "motions":
			{
				Dictionary<string, Motion> dictionary = new Dictionary<string, Motion>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Motion value = ((reader.TokenType != JsonToken.Null) ? ((Motion)ReadYaml_Motion(reader, objectType, existingValue, serializer)) : default(Motion));
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				emotions.Motions = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return emotions;
	}

	private static object ReadYaml_EncyclopediaCategories(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EncyclopediaCategories encyclopediaCategories = new EncyclopediaCategories();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			EncyclopediaType key = (EncyclopediaType)Convert.ToInt32(reader.Value);
			reader.Read();
			EncyclopediaCategory value = ((reader.TokenType != JsonToken.Null) ? ((EncyclopediaCategory)ReadYaml_EncyclopediaCategory(reader, objectType, existingValue, serializer)) : null);
			encyclopediaCategories.Add(key, value);
		}
		return encyclopediaCategories;
	}

	private static object ReadYaml_EncyclopediaCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EncyclopediaCategory encyclopediaCategory = new EncyclopediaCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "order":
					encyclopediaCategory.Order = Convert.ToInt32(reader.Value);
					break;
				case "name":
					encyclopediaCategory.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "icon":
					encyclopediaCategory.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return encyclopediaCategory;
	}

	private static object ReadYaml_EncyclopediaItem(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EncyclopediaItem encyclopediaItem = new EncyclopediaItem();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "masteries":
			{
				Dictionary<int, List<Dictionary<string, float>>> dictionary = new Dictionary<int, List<Dictionary<string, float>>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					List<Dictionary<string, float>> value;
					if (reader.TokenType == JsonToken.Null)
					{
						value = null;
					}
					else
					{
						List<Dictionary<string, float>> list = new List<Dictionary<string, float>>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							Dictionary<string, float> dictionary2 = new Dictionary<string, float>();
							while (reader.Read() && reader.TokenType != JsonToken.EndObject)
							{
								string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
								reader.Read();
								float value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
								if (text2 != null)
								{
									dictionary2.Add(text2, value2);
								}
							}
							list.Add(dictionary2);
						}
						value = list;
					}
					dictionary.Add(key, value);
				}
				encyclopediaItem.Masteries = dictionary;
				break;
			}
			case "max_level":
				encyclopediaItem.MaxLevel = Convert.ToInt32(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return encyclopediaItem;
	}

	private static object ReadYaml_EncyclopediaItems(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EncyclopediaItems encyclopediaItems = new EncyclopediaItems();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			EncyclopediaType key = (EncyclopediaType)Convert.ToInt32(reader.Value);
			reader.Read();
			Dictionary<string, EncyclopediaItem> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, EncyclopediaItem> dictionary = new Dictionary<string, EncyclopediaItem>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					EncyclopediaItem value2 = ((reader.TokenType != JsonToken.Null) ? ((EncyclopediaItem)ReadYaml_EncyclopediaItem(reader, objectType, existingValue, serializer)) : null);
					if (text != null)
					{
						dictionary.Add(text, value2);
					}
				}
				value = dictionary;
			}
			encyclopediaItems.Add(key, value);
		}
		return encyclopediaItems;
	}

	private static object ReadYaml_EncyclopediaModifiers(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EncyclopediaModifiers encyclopediaModifiers = new EncyclopediaModifiers();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "icon":
					encyclopediaModifiers.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "reduce_type":
					encyclopediaModifiers.ReduceType = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "description":
					encyclopediaModifiers.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "name":
					encyclopediaModifiers.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "increase_type":
					encyclopediaModifiers.IncreaseType = (IncreaseType)Convert.ToInt32(reader.Value);
					break;
				case "apply_type":
					encyclopediaModifiers.ApplyType = (ApplyType)Convert.ToInt32(reader.Value);
					break;
				case "inverse":
					encyclopediaModifiers.Inverse = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return encyclopediaModifiers;
	}

	private static object ReadYaml_EncyclopediaModifiersYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EncyclopediaModifiersYaml encyclopediaModifiersYaml = new EncyclopediaModifiersYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			EncyclopediaModifiers value = ((reader.TokenType != JsonToken.Null) ? ((EncyclopediaModifiers)ReadYaml_EncyclopediaModifiers(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				encyclopediaModifiersYaml.Add(text, value);
			}
		}
		return encyclopediaModifiersYaml;
	}

	private static object ReadYaml_Estate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Estate estate = default(Estate);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "activation_period")
			{
				Dictionary<OwnerType, int> dictionary = new Dictionary<OwnerType, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					OwnerType key = (OwnerType)Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				estate.ActivationPeriod = dictionary;
				flag = true;
			}
			else
			{
				reader.Skip();
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'activation_period' not found in JSON.");
		}
		return estate;
	}

	private static object ReadYaml_EstateCost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		EstateCost estateCost = default(EstateCost);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "extending_cost":
			{
				Dictionary<OwnerType, string> dictionary2 = new Dictionary<OwnerType, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					OwnerType key2 = (OwnerType)Convert.ToInt32(reader.Value);
					reader.Read();
					string value2 = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					dictionary2.Add(key2, value2);
				}
				estateCost.ExtendingCost = dictionary2;
				break;
			}
			case "expanding_cost":
			{
				Dictionary<OwnerType, string> dictionary = new Dictionary<OwnerType, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					OwnerType key = (OwnerType)Convert.ToInt32(reader.Value);
					reader.Read();
					string value = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					dictionary.Add(key, value);
				}
				estateCost.ExpandingCost = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return estateCost;
	}

	private static object ReadYaml_Explorer(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Explorer explorer = default(Explorer);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "search_cooltime")
				{
					explorer.SearchCooltime = Convert.ToInt32(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'search_cooltime' not found in JSON.");
		}
		return explorer;
	}

	private static object ReadYaml_Faction(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Faction faction = new Faction();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				faction.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				faction.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "friendship_label":
				faction.FriendshipLabel = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "unknown_text":
				faction.UnknownText = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "help_text":
				faction.HelpText = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "level_thresholds":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				faction.LevelThresholds = list.ToArray();
				break;
			}
			case "titles":
			{
				List<Gettext> list3 = new List<Gettext>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Gettext item3 = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					list3.Add(item3);
				}
				faction.Titles = list3.ToArray();
				break;
			}
			case "cooltime":
				faction.Cooltime = Convert.ToInt32(reader.Value);
				break;
			case "display_cooltime":
				faction.DisplayCooltime = Convert.ToBoolean(reader.Value);
				break;
			case "rewards":
			{
				List<FactionReward> list2 = new List<FactionReward>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					FactionReward item2 = (FactionReward)ReadYaml_FactionReward(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				faction.Rewards = list2.ToArray();
				break;
			}
			case "season":
				faction.Season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "open_limit":
				faction.OpenLimit = (OpenLimit)ReadYaml_OpenLimit(reader, objectType, existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return faction;
	}

	private static object ReadYaml_FactionInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		FactionInfo factionInfo = default(FactionInfo);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "mission")
				{
					factionInfo.Mission = (FactionInfo.MissionData)ReadYaml_FactionInfo_MissionData(reader, objectType, existingValue, serializer);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'mission' not found in JSON.");
		}
		return factionInfo;
	}

	private static object ReadYaml_FactionReward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		FactionReward factionReward = new FactionReward();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "money":
			{
				Dictionary<Currency, int> dictionary = new Dictionary<Currency, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					Currency key = (Currency)Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				factionReward.Money = dictionary;
				break;
			}
			case "title_id":
				factionReward.TitleId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return factionReward;
	}

	private static object ReadYaml_Factions(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Factions factions = new Factions();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			FactionType key = (FactionType)Convert.ToInt32(reader.Value);
			reader.Read();
			Faction value = ((reader.TokenType != JsonToken.Null) ? ((Faction)ReadYaml_Faction(reader, objectType, existingValue, serializer)) : null);
			factions.Add(key, value);
		}
		return factions;
	}

	private static object ReadYaml_FactionSupport(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		FactionSupport factionSupport = new FactionSupport();
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "support_level":
					factionSupport.SupportLevel = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag = true;
					break;
				case "support_level_step":
					factionSupport.SupportLevelStep = Convert.ToInt32(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'support_level' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'support_level_step' not found in JSON.");
		}
		return factionSupport;
	}

	private static object ReadYaml_FatigueCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Yaml.FatigueCategory fatigueCategory = new Yaml.FatigueCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "name":
					fatigueCategory.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "description":
					fatigueCategory.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "default_ratio":
					fatigueCategory.DefaultRatio = Convert.ToSingle(reader.Value);
					break;
				case "icon":
					fatigueCategory.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return fatigueCategory;
	}

	private static object ReadYaml_FatigueCategoryYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		FatigueCategoryYaml fatigueCategoryYaml = new FatigueCategoryYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Survival.FatigueCategory key = (Shared.Survival.FatigueCategory)Convert.ToInt32(reader.Value);
			reader.Read();
			Yaml.FatigueCategory value = ((reader.TokenType != JsonToken.Null) ? ((Yaml.FatigueCategory)ReadYaml_FatigueCategory(reader, objectType, existingValue, serializer)) : null);
			fatigueCategoryYaml.Add(key, value);
		}
		return fatigueCategoryYaml;
	}

	private static object ReadYaml_GeneratorData(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		GeneratorData generatorData = new GeneratorData();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "name")
				{
					generatorData.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return generatorData;
	}

	private static object ReadYaml_GeneratorYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		GeneratorYaml generatorYaml = new GeneratorYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			GeneratorData value = ((reader.TokenType != JsonToken.Null) ? ((GeneratorData)ReadYaml_GeneratorData(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				generatorYaml.Add(text, value);
			}
		}
		return generatorYaml;
	}

	private static object ReadYaml_IndicatorData(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		IndicatorData indicatorData = new IndicatorData();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "active":
					indicatorData.active = Convert.ToBoolean(reader.Value);
					break;
				case "icon":
					indicatorData.icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "color":
					indicatorData.color = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "size":
					indicatorData.size = Convert.ToInt32(reader.Value);
					break;
				case "visible_zoom":
					indicatorData.visible_zoom = Convert.ToSingle(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return indicatorData;
	}

	private static object ReadYaml_ItemContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ItemContent itemContent = new ItemContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "prototype_id":
				itemContent.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "level":
				itemContent.level = Convert.ToInt32(reader.Value);
				break;
			case "count":
				itemContent.count = Convert.ToInt32(reader.Value);
				break;
			case "colors":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				itemContent.colors = list.ToArray();
				break;
			}
			case "key":
				itemContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "hide_in_shop":
				itemContent.hide_in_shop = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return itemContent;
	}

	private static object ReadYaml_RankingReward_ItemInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RankingReward.ItemInfo itemInfo = new RankingReward.ItemInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "count":
				itemInfo.Count = Convert.ToInt32(reader.Value);
				break;
			case "level":
				itemInfo.Level = Convert.ToInt32(reader.Value);
				break;
			case "prototype_id":
				itemInfo.PrototypeId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "default_tags":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				itemInfo.DefaultTags = dictionary;
				break;
			}
			case "random_tags":
			{
				List<RankingReward.Tag> list2 = new List<RankingReward.Tag>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					RankingReward.Tag item2 = (RankingReward.Tag)ReadYaml_RankingReward_Tag(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				itemInfo.RandomTags = list2.ToArray();
				break;
			}
			case "rare_tags":
			{
				List<RankingReward.Tag> list = new List<RankingReward.Tag>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					RankingReward.Tag item = (RankingReward.Tag)ReadYaml_RankingReward_Tag(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				itemInfo.RareTags = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return itemInfo;
	}

	private static object ReadYaml_WarpRushReward_ItemInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		WarpRushReward.ItemInfo itemInfo = new WarpRushReward.ItemInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "count":
				itemInfo.Count = Convert.ToInt32(reader.Value);
				break;
			case "level":
				itemInfo.Level = Convert.ToInt32(reader.Value);
				break;
			case "prototype_id":
				itemInfo.PrototypeId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "default_tags":
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				itemInfo.DefaultTags = dictionary2;
				break;
			}
			case "random_tags":
			{
				Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text4 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value3 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text4 != null)
					{
						dictionary3.Add(text4, value3);
					}
				}
				itemInfo.RandomTags = dictionary3;
				break;
			}
			case "RareTags":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				itemInfo.RareTags = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return itemInfo;
	}

	private static object ReadYaml_ItemTextCondition(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ItemTextCondition itemTextCondition = new ItemTextCondition();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "item_category")
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				itemTextCondition.ItemCategory = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return itemTextCondition;
	}

	private static object ReadYaml_Job(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Yaml.Job job = new Yaml.Job();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "category_levels":
			{
				Dictionary<Shared.Skill.Category, int> dictionary = new Dictionary<Shared.Skill.Category, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					Shared.Skill.Category key = (Shared.Skill.Category)Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				job.category_levels = dictionary;
				break;
			}
			case "description":
				job.description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return job;
	}

	private static object ReadYaml_JobsYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JobsYaml jobsYaml = new JobsYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Player.Job key = (Shared.Player.Job)Convert.ToInt32(reader.Value);
			reader.Read();
			Yaml.Job value = ((reader.TokenType != JsonToken.Null) ? ((Yaml.Job)ReadYaml_Job(reader, objectType, existingValue, serializer)) : null);
			jobsYaml.Add(key, value);
		}
		return jobsYaml;
	}

	private static object ReadYaml_Chapter_Kind(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Chapter.Kind kind = (Chapter.Kind)Convert.ToInt32(reader.Value);
		return kind;
	}

	private static object ReadYaml_Market(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Market market = default(Market);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "listing_fee_rate":
					market.ListingFeeRate = Convert.ToDouble(reader.Value);
					break;
				case "sales_fee_threshold":
					market.SalesFeeThreshold = Convert.ToInt32(reader.Value);
					break;
				case "sales_fee_rates":
					market.SalesFeeRates = (SalesFeeRates)ReadYaml_SalesFeeRates(reader, objectType, existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return market;
	}

	private static object ReadYaml_MaxLevels(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MaxLevels maxLevels = default(MaxLevels);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "player":
					maxLevels.Player = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "resistance":
					maxLevels.Resistance = Convert.ToInt32(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'player' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'resistance' not found in JSON.");
		}
		return maxLevels;
	}

	private static object ReadYaml_MemoGroupDictionary(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MemoGroupDictionary memoGroupDictionary = new MemoGroupDictionary();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			MemoType key = (MemoType)Convert.ToInt32(reader.Value);
			reader.Read();
			Dictionary<int, MemoInfo> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<int, MemoInfo> dictionary = new Dictionary<int, MemoInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key2 = Convert.ToInt32(reader.Value);
					reader.Read();
					MemoInfo value2 = ((reader.TokenType != JsonToken.Null) ? ((MemoInfo)ReadYaml_MemoInfo(reader, objectType, existingValue, serializer)) : null);
					dictionary.Add(key2, value2);
				}
				value = dictionary;
			}
			memoGroupDictionary.Add(key, value);
		}
		return memoGroupDictionary;
	}

	private static object ReadYaml_MemoInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MemoInfo memoInfo = new MemoInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "name":
					memoInfo.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "content":
					memoInfo.content = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return memoInfo;
	}

	private static object ReadYaml_MemosYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MemosYaml memosYaml = new MemosYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "memos")
				{
					memosYaml.memos = (MemoGroupDictionary)ReadYaml_MemoGroupDictionary(reader, objectType, existingValue, serializer);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return memosYaml;
	}

	private static object ReadYaml_Messenger(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Yaml.Messenger messenger = default(Yaml.Messenger);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "name":
					messenger.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "portrait":
					messenger.Portrait = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return messenger;
	}

	private static object ReadYaml_MessengersYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MessengersYaml messengersYaml = new MessengersYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Faction.Messenger key = (Shared.Faction.Messenger)Convert.ToInt32(reader.Value);
			reader.Read();
			Yaml.Messenger value = ((reader.TokenType != JsonToken.Null) ? ((Yaml.Messenger)ReadYaml_Messenger(reader, objectType, existingValue, serializer)) : default(Yaml.Messenger));
			messengersYaml.Add(key, value);
		}
		return messengersYaml;
	}

	private static object ReadYaml_FactionInfo_MissionData(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		FactionInfo.MissionData missionData = default(FactionInfo.MissionData);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "activation_level":
					missionData.ActivationLevel = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "shuffle":
					missionData.Shuffle = (FactionInfo.MissionData.ShuffleData)ReadYaml_FactionInfo_MissionData_ShuffleData(reader, objectType, existingValue, serializer);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'activation_level' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'shuffle' not found in JSON.");
		}
		return missionData;
	}

	private static object ReadYaml_MissionTalk(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MissionTalk missionTalk = new MissionTalk();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "messenger":
					missionTalk.Messenger = (Shared.Faction.Messenger)Convert.ToInt32(reader.Value);
					break;
				case "message":
					missionTalk.Message = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "image":
					missionTalk.Image = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_portrait":
					missionTalk.HidePortrait = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return missionTalk;
	}

	private static object ReadYaml_ModularArtifactContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ModularArtifactContent modularArtifactContent = new ModularArtifactContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "prototype_id":
				modularArtifactContent.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "level":
				modularArtifactContent.level = Convert.ToInt32(reader.Value);
				break;
			case "artifact_id":
				modularArtifactContent.artifact_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "size_x":
				modularArtifactContent.size_x = Convert.ToInt32(reader.Value);
				break;
			case "size_y":
				modularArtifactContent.size_y = Convert.ToInt32(reader.Value);
				break;
			case "durability":
				modularArtifactContent.durability = Convert.ToSingle(reader.Value);
				break;
			case "overridden_parts":
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string value2 = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				modularArtifactContent.overridden_parts = dictionary2;
				break;
			}
			case "overridden_textures":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string value = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				modularArtifactContent.overridden_textures = dictionary;
				break;
			}
			case "key":
				modularArtifactContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "hide_in_shop":
				modularArtifactContent.hide_in_shop = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return modularArtifactContent;
	}

	private static object ReadYaml_MoneyContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		MoneyContent moneyContent = new MoneyContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "currency":
					moneyContent.currency = (Currency)Convert.ToInt32(reader.Value);
					break;
				case "amount":
					moneyContent.amount = Convert.ToInt64(reader.Value);
					break;
				case "key":
					moneyContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_in_shop":
					moneyContent.hide_in_shop = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return moneyContent;
	}

	private static object ReadYaml_Motion(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Motion motion = default(Motion);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "motion_names":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				motion.MotionNames = list.ToArray();
				break;
			}
			case "name":
				motion.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "free":
				motion.Free = Convert.ToBoolean(reader.Value);
				break;
			case "available":
				motion.Available = Convert.ToBoolean(reader.Value);
				break;
			case "payback_mileage":
				motion.PaybackMileage = Convert.ToInt32(reader.Value);
				break;
			case "tier":
				motion.Tier = (EmotionTier)Convert.ToInt32(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return motion;
	}

	private static object ReadYaml_Musician(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Musician musician = new Musician();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "max_savable_size":
					musician.MaxSavableSize = Convert.ToInt32(reader.Value);
					break;
				case "slot_count":
					musician.SlotCount = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return musician;
	}

	private static object ReadYaml_Natural(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Natural natural = new Natural();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "collectible_id":
				natural.collectible_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon":
				natural.icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "sprite_names":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				natural.sprite_names = list.ToArray();
				break;
			}
			case "name":
				natural.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "additive":
				natural.additive = Convert.ToBoolean(reader.Value);
				break;
			case "particle":
				natural.particle = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "survivability":
			{
				Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					bool value = reader.TokenType != JsonToken.Null && Convert.ToBoolean(reader.Value);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				natural.survivability = dictionary;
				break;
			}
			case "is_craft":
				natural.is_craft = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return natural;
	}

	private static object ReadYaml_NaturalComponentInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		NaturalComponentInfo naturalComponentInfo = new NaturalComponentInfo();
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "components":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				naturalComponentInfo.Components = list;
				flag = true;
				break;
			}
			case "poi_type":
				naturalComponentInfo.POIType = (PointOfInterest)Convert.ToInt32(reader.Value);
				flag2 = true;
				break;
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'components' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'poi_type' not found in JSON.");
		}
		return naturalComponentInfo;
	}

	private static object ReadYaml_OpenLimit(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		OpenLimit openLimit = new OpenLimit();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "ends_at":
					openLimit.EndsAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "starts_at":
					openLimit.StartsAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return openLimit;
	}

	private static object ReadYaml_OpenMapCost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		OpenMapCost openMapCost = new OpenMapCost();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "vouchers")
			{
				List<VoucherWithCommodity> list = new List<VoucherWithCommodity>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					VoucherWithCommodity item = (VoucherWithCommodity)ReadYaml_VoucherWithCommodity(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				openMapCost.Vouchers = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return openMapCost;
	}

	private static object ReadYaml_PerformanceVisibleInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PerformanceVisibleInfo performanceVisibleInfo = new PerformanceVisibleInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "order":
					performanceVisibleInfo.Order = Convert.ToInt32(reader.Value);
					break;
				case "type":
					performanceVisibleInfo.Type = (PerformanceVisibleType)Convert.ToInt32(reader.Value);
					break;
				case "min_value":
					performanceVisibleInfo.MinValue = Convert.ToSingle(reader.Value);
					break;
				case "digits":
					performanceVisibleInfo.Digits = Convert.ToInt32(reader.Value);
					break;
				case "negative":
					performanceVisibleInfo.Negative = Convert.ToBoolean(reader.Value);
					break;
				case "emphasize":
					performanceVisibleInfo.Emphasize = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return performanceVisibleInfo;
	}

	private static object ReadYaml_PerformanceVisibleInfoDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PerformanceVisibleInfoDict performanceVisibleInfoDict = new PerformanceVisibleInfoDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Dictionary<string, PerformanceVisibleInfo> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, PerformanceVisibleInfo> dictionary = new Dictionary<string, PerformanceVisibleInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					PerformanceVisibleInfo value2 = ((reader.TokenType != JsonToken.Null) ? ((PerformanceVisibleInfo)ReadYaml_PerformanceVisibleInfo(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value2);
					}
				}
				value = dictionary;
			}
			if (text != null)
			{
				performanceVisibleInfoDict.Add(text, value);
			}
		}
		return performanceVisibleInfoDict;
	}

	private static object ReadYaml_PeriodicCountsLimit(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PeriodicCountsLimit periodicCountsLimit = default(PeriodicCountsLimit);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "days":
					periodicCountsLimit.Days = Convert.ToInt32(reader.Value);
					break;
				case "counts":
					periodicCountsLimit.Counts = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return periodicCountsLimit;
	}

	private static object ReadYaml_PeriodicLimit(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PeriodicLimit periodicLimit = default(PeriodicLimit);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "days":
					periodicLimit.Days = Convert.ToInt32(reader.Value);
					break;
				case "renewal_period":
					periodicLimit.RenewalPeriod = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return periodicLimit;
	}

	private static object ReadYaml_PersonalRegion(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PersonalRegion personalRegion = new PersonalRegion();
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "region_template_ids")
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				personalRegion.RegionTemplateIds = list;
				flag = true;
			}
			else
			{
				reader.Skip();
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'region_template_ids' not found in JSON.");
		}
		return personalRegion;
	}

	private static object ReadYaml_PersonalResearch(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PersonalResearch personalResearch = new PersonalResearch();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "category":
					personalResearch.Category = (ResearchCategory)Convert.ToInt32(reader.Value);
					break;
				case "name":
					personalResearch.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "effect":
					personalResearch.Effect = (ResearchEffect)ReadYaml_ResearchEffect(reader, objectType, existingValue, serializer);
					break;
				case "currency":
					personalResearch.Currency = (Currency)Convert.ToInt32(reader.Value);
					break;
				case "amount":
					personalResearch.Amount = Convert.ToInt32(reader.Value);
					break;
				case "icon":
					personalResearch.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "tier":
					personalResearch.Tier = (LaboratoryTier)Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return personalResearch;
	}

	private static object ReadYaml_PersonalResearchs(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PersonalResearchs personalResearchs = new PersonalResearchs();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			PersonalResearch value = ((reader.TokenType != JsonToken.Null) ? ((PersonalResearch)ReadYaml_PersonalResearch(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				personalResearchs.Add(text, value);
			}
		}
		return personalResearchs;
	}

	private static object ReadYaml_Pet(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Pet pet = new Pet();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "type":
					pet.Type = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "name":
					pet.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "vehicle_entity_type":
					pet.VehicleEntityType = Convert.ToInt32(reader.Value);
					break;
				case "is_ridable":
					pet.IsRidable = Convert.ToBoolean(reader.Value);
					break;
				case "is_fightable":
					pet.IsFightable = Convert.ToBoolean(reader.Value);
					break;
				case "is_reinifiable":
					pet.IsReinifiable = Convert.ToBoolean(reader.Value);
					break;
				case "is_craft":
					pet.IsCraft = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return pet;
	}

	private static object ReadYaml_PetActiveSkill(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetActiveSkill petActiveSkill = new PetActiveSkill();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "description":
				petActiveSkill.OriginDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "context":
			{
				List<SkillContext> list = new List<SkillContext>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					SkillContext item = (SkillContext)Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				petActiveSkill.Contextes = list.ToArray();
				break;
			}
			case "type":
				petActiveSkill.Type = (SkillType)Convert.ToInt32(reader.Value);
				break;
			case "name":
				petActiveSkill.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				petActiveSkill.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "category_icon":
				petActiveSkill.CategoryIcon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "cooltime":
				petActiveSkill.Cooltime = Convert.ToDouble(reader.Value);
				break;
			case "duration":
				petActiveSkill.Duration = Convert.ToSingle(reader.Value);
				break;
			case "status_effect":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				petActiveSkill.StatusEffect = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return petActiveSkill;
	}

	private static object ReadYaml_PetActiveSkillCondition(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetActiveSkillCondition petActiveSkillCondition = new PetActiveSkillCondition();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "weight":
				petActiveSkillCondition.Weight = Convert.ToSingle(reader.Value);
				break;
			case "entity_type":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				petActiveSkillCondition.EntityType = list.ToArray();
				break;
			}
			case "tag_condition":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				petActiveSkillCondition.TagCondition = dictionary;
				break;
			}
			case "for_fightable":
				petActiveSkillCondition.ForFightable = Convert.ToBoolean(reader.Value);
				break;
			case "for_ridable":
				petActiveSkillCondition.ForRidable = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return petActiveSkillCondition;
	}

	private static object ReadYaml_PetActiveSkillConditionDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetActiveSkillConditionDict petActiveSkillConditionDict = new PetActiveSkillConditionDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			SkillRank key = (SkillRank)Convert.ToInt32(reader.Value);
			reader.Read();
			PetActiveSkillCondition value = ((reader.TokenType != JsonToken.Null) ? ((PetActiveSkillCondition)ReadYaml_PetActiveSkillCondition(reader, objectType, existingValue, serializer)) : null);
			petActiveSkillConditionDict.Add(key, value);
		}
		return petActiveSkillConditionDict;
	}

	private static object ReadYaml_PetActiveSkillConditions(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetActiveSkillConditions petActiveSkillConditions = new PetActiveSkillConditions();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			PetActiveSkillConditionDict value = ((reader.TokenType != JsonToken.Null) ? ((PetActiveSkillConditionDict)ReadYaml_PetActiveSkillConditionDict(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				petActiveSkillConditions.Add(text, value);
			}
		}
		return petActiveSkillConditions;
	}

	private static object ReadYaml_PetActiveSkillRankDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetActiveSkillRankDict petActiveSkillRankDict = new PetActiveSkillRankDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			SkillRank key = (SkillRank)Convert.ToInt32(reader.Value);
			reader.Read();
			PetActiveSkill value = ((reader.TokenType != JsonToken.Null) ? ((PetActiveSkill)ReadYaml_PetActiveSkill(reader, objectType, existingValue, serializer)) : null);
			petActiveSkillRankDict.Add(key, value);
		}
		return petActiveSkillRankDict;
	}

	private static object ReadYaml_PetActiveSkills(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetActiveSkills petActiveSkills = new PetActiveSkills();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			PetActiveSkillRankDict value = ((reader.TokenType != JsonToken.Null) ? ((PetActiveSkillRankDict)ReadYaml_PetActiveSkillRankDict(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				petActiveSkills.Add(text, value);
			}
		}
		return petActiveSkills;
	}

	private static object ReadYaml_PetExp(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetExp petExp = new PetExp();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			int key = Convert.ToInt32(reader.Value);
			reader.Read();
			PetExpTable value = ((reader.TokenType != JsonToken.Null) ? ((PetExpTable)ReadYaml_PetExpTable(reader, objectType, existingValue, serializer)) : null);
			petExp.Add(key, value);
		}
		return petExp;
	}

	private static object ReadYaml_PetExpTable(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetExpTable petExpTable = new PetExpTable();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "required_exp")
				{
					petExpTable.RequiredExp = ((reader.Value != null) ? reader.Value.ToString() : null);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return petExpTable;
	}

	private static object ReadYaml_Pets(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Pets pets = new Pets();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			int key = Convert.ToInt32(reader.Value);
			reader.Read();
			Pet value = ((reader.TokenType != JsonToken.Null) ? ((Pet)ReadYaml_Pet(reader, objectType, existingValue, serializer)) : null);
			pets.Add(key, value);
		}
		return pets;
	}

	private static object ReadYaml_PetTask(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetTask petTask = new PetTask();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "product_quantity":
				petTask.ProductQuantity = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "type":
				petTask.Type = (PetTaskType)Convert.ToInt32(reader.Value);
				break;
			case "name":
				petTask.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				petTask.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "duration":
				petTask.Duration = Convert.ToSingle(reader.Value);
				break;
			case "exp":
				petTask.Exp = Convert.ToInt32(reader.Value);
				break;
			case "hungry_required":
				petTask.HungryRequired = Convert.ToSingle(reader.Value);
				break;
			case "produced_prototype":
			{
				Dictionary<string, float[]> dictionary2 = new Dictionary<string, float[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float[] value2;
					if (reader.TokenType == JsonToken.Null)
					{
						value2 = null;
					}
					else
					{
						List<float> list2 = new List<float>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							float item2 = Convert.ToSingle(reader.Value);
							list2.Add(item2);
						}
						value2 = list2.ToArray();
					}
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				petTask.ProducedPrototype = dictionary2;
				break;
			}
			case "random_prototype":
			{
				Dictionary<string, float[]> dictionary = new Dictionary<string, float[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float[] value;
					if (reader.TokenType == JsonToken.Null)
					{
						value = null;
					}
					else
					{
						List<float> list = new List<float>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							float item = Convert.ToSingle(reader.Value);
							list.Add(item);
						}
						value = list.ToArray();
					}
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				petTask.RandomPrototype = dictionary;
				break;
			}
			case "unlock_level":
				petTask.UnlockLevel = Convert.ToInt32(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return petTask;
	}

	private static object ReadYaml_PetTasks(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PetTasks petTasks = new PetTasks();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			PetTask value = ((reader.TokenType != JsonToken.Null) ? ((PetTask)ReadYaml_PetTask(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				petTasks.Add(text, value);
			}
		}
		return petTasks;
	}

	private static object ReadYaml_Pioneer(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Pioneer pioneer = new Pioneer();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "daily_cost_exchange_rate":
			{
				List<PioneerCostExchangeRate> list3 = new List<PioneerCostExchangeRate>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PioneerCostExchangeRate item3 = (PioneerCostExchangeRate)ReadYaml_PioneerCostExchangeRate(reader, objectType, existingValue, serializer);
					list3.Add(item3);
				}
				pioneer.DailyCostExchangeRate = list3.ToArray();
				break;
			}
			case "grade_point":
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				pioneer.GradePoint = dictionary;
				break;
			}
			case "region_access":
			{
				List<int[]> list = new List<int[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					List<int> list2 = new List<int>();
					while (reader.Read() && reader.TokenType != JsonToken.EndArray)
					{
						int item = Convert.ToInt32(reader.Value);
						list2.Add(item);
					}
					int[] item2 = list2.ToArray();
					list.Add(item2);
				}
				pioneer.RegionAccess = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return pioneer;
	}

	private static object ReadYaml_PioneerCostExchangeRate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PioneerCostExchangeRate pioneerCostExchangeRate = new PioneerCostExchangeRate();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "grade":
				pioneerCostExchangeRate.Grade = Convert.ToInt32(reader.Value);
				break;
			case "rates":
			{
				List<PioneerRate> list = new List<PioneerRate>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PioneerRate item = (PioneerRate)ReadYaml_PioneerRate(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				pioneerCostExchangeRate.Rates = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return pioneerCostExchangeRate;
	}

	private static object ReadYaml_PioneerGradeReward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PioneerGradeReward pioneerGradeReward = new PioneerGradeReward();
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "grade":
					pioneerGradeReward.Grade = Convert.ToInt32(reader.Value);
					break;
				case "texts":
					pioneerGradeReward.Texts = (PioneerGradeRewardText)ReadYaml_PioneerGradeRewardText(reader, objectType, existingValue, serializer);
					flag = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'texts' not found in JSON.");
		}
		return pioneerGradeReward;
	}

	private static object ReadYaml_PioneerGradeRewards(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PioneerGradeRewards pioneerGradeRewards = new PioneerGradeRewards();
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "rewards")
			{
				List<PioneerGradeReward> list = new List<PioneerGradeReward>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PioneerGradeReward item = (PioneerGradeReward)ReadYaml_PioneerGradeReward(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				pioneerGradeRewards.Rewards = list.ToArray();
				flag = true;
			}
			else
			{
				reader.Skip();
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'rewards' not found in JSON.");
		}
		return pioneerGradeRewards;
	}

	private static object ReadYaml_PioneerGradeRewardText(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PioneerGradeRewardText pioneerGradeRewardText = new PioneerGradeRewardText();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "after":
					pioneerGradeRewardText.After = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "before":
					pioneerGradeRewardText.Before = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return pioneerGradeRewardText;
	}

	private static object ReadYaml_PioneerRate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PioneerRate pioneerRate = new PioneerRate();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "paid":
					pioneerRate.Paid = Convert.ToBoolean(reader.Value);
					break;
				case "point":
					pioneerRate.Point = Convert.ToInt32(reader.Value);
					break;
				case "rate":
					pioneerRate.Rate = Convert.ToSingle(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return pioneerRate;
	}

	private static object ReadYaml_PlayerAction(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerAction playerAction = new PlayerAction();
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "meta":
				playerAction.Meta = (PlayerActionMeta)ReadYaml_PlayerActionMeta(reader, objectType, existingValue, serializer);
				flag = true;
				break;
			case "attack_info":
			{
				List<PlayerActionAttackInfo> list = new List<PlayerActionAttackInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PlayerActionAttackInfo item = (PlayerActionAttackInfo)ReadYaml_PlayerActionAttackInfo(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				playerAction.AttackInfo = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'meta' not found in JSON.");
		}
		return playerAction;
	}

	private static object ReadYaml_PlayerActionAttackInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerActionAttackInfo playerActionAttackInfo = new PlayerActionAttackInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "damage_type":
					playerActionAttackInfo.DamageType = (DamageType)Convert.ToInt32(reader.Value);
					break;
				case "attack_time":
					playerActionAttackInfo.AttackTime = Convert.ToSingle(reader.Value);
					break;
				case "radius":
					playerActionAttackInfo.Radius = Convert.ToSingle(reader.Value);
					break;
				case "angles":
					playerActionAttackInfo.Angles = (Pair<float, float>)_pairConverter.ReadJson(reader, typeof(Pair<float, float>), existingValue, serializer);
					break;
				case "offset":
					playerActionAttackInfo.Offset = (Pair<float, float>)_pairConverter.ReadJson(reader, typeof(Pair<float, float>), existingValue, serializer);
					break;
				case "rect_half_size":
					playerActionAttackInfo.RectHalfSize = (Pair<float, float>)_pairConverter.ReadJson(reader, typeof(Pair<float, float>), existingValue, serializer);
					break;
				case "damage_angle":
					playerActionAttackInfo.DamageAngle = Convert.ToSingle(reader.Value);
					break;
				case "use_target_origin":
					playerActionAttackInfo.UseTargetOrigin = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return playerActionAttackInfo;
	}

	private static object ReadYaml_PlayerActionMeta(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerActionMeta playerActionMeta = new PlayerActionMeta();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "action_length":
				playerActionMeta.ActionLength = Convert.ToSingle(reader.Value);
				break;
			case "active_condition":
				playerActionMeta.ActiveCondition = (ActionActiveCondition)ReadYaml_ActionActiveCondition(reader, objectType, existingValue, serializer);
				break;
			case "cooltime":
				playerActionMeta.Cooldown = Convert.ToSingle(reader.Value);
				break;
			case "description":
				playerActionMeta.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "battle_action_type":
				playerActionMeta.BattleActionType = (BattleActionType)Convert.ToInt32(reader.Value);
				break;
			case "hide_when_deactive":
				playerActionMeta.HideWhenDeactive = Convert.ToBoolean(reader.Value);
				break;
			case "icon":
				playerActionMeta.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "motion":
				playerActionMeta.Motion = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "playback_rate":
				playerActionMeta.PlaybackRate = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					playerActionMeta.PlaybackRate = Convert.ToSingle(reader.Value);
				}
				break;
			case "name":
				playerActionMeta.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "prohibit_type":
				playerActionMeta.ProhibitType = (ProhibitType)Convert.ToInt32(reader.Value);
				break;
			case "prohibited_time":
			{
				Dictionary<ProhibitType, float> dictionary = new Dictionary<ProhibitType, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					ProhibitType key = (ProhibitType)Convert.ToInt32(reader.Value);
					reader.Read();
					float value = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					dictionary.Add(key, value);
				}
				playerActionMeta.ProhibitedTime = dictionary;
				break;
			}
			case "stamina":
				playerActionMeta.Stamina = Convert.ToSingle(reader.Value);
				break;
			case "slot":
				playerActionMeta.Slot = (PlayerActionSlot)ReadYaml_PlayerActionSlot(reader, objectType, existingValue, serializer);
				break;
			case "use_range":
				playerActionMeta.UseRange = Convert.ToSingle(reader.Value);
				break;
			case "casting_bar":
				playerActionMeta.CastingBar = (Pair<float, float>)_pairConverter.ReadJson(reader, typeof(Pair<float, float>), existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return playerActionMeta;
	}

	private static object ReadYaml_PlayerActions(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerActions playerActions = new PlayerActions();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			PlayerAction value = ((reader.TokenType != JsonToken.Null) ? ((PlayerAction)ReadYaml_PlayerAction(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				playerActions.Add(text, value);
			}
		}
		return playerActions;
	}

	private static object ReadYaml_PlayerActionSlot(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerActionSlot playerActionSlot = new PlayerActionSlot();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "id":
					playerActionSlot.Id = Convert.ToInt32(reader.Value);
					break;
				case "order":
					playerActionSlot.Order = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return playerActionSlot;
	}

	private static object ReadYaml_PlayerEntities(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerEntities playerEntities = new PlayerEntities();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "player")
				{
					playerEntities.player = (PlayerEntity)ReadYaml_PlayerEntity(reader, objectType, existingValue, serializer);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return playerEntities;
	}

	private static object ReadYaml_PlayerEntity(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerEntity playerEntity = new PlayerEntity();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "bare_hands":
				playerEntity.bare_hands = (Barehands)ReadYaml_Barehands(reader, objectType, existingValue, serializer);
				break;
			case "actions":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				playerEntity.actions = list.ToArray();
				break;
			}
			case "body_parts":
			{
				Dictionary<string, BodyParts> dictionary = new Dictionary<string, BodyParts>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					BodyParts value = ((reader.TokenType != JsonToken.Null) ? ((BodyParts)ReadYaml_BodyParts(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				playerEntity.body_parts = dictionary;
				break;
			}
			case "bound_radius":
				playerEntity.bound_radius = Convert.ToSingle(reader.Value);
				break;
			case "battle_retreat_time":
				playerEntity.battle_retreat_time = Convert.ToSingle(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return playerEntity;
	}

	private static object ReadYaml_PlayerStatistics(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PlayerStatistics playerStatistics = new PlayerStatistics();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "level_thresholds":
			{
				List<int> list2 = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item2 = Convert.ToInt32(reader.Value);
					list2.Add(item2);
				}
				playerStatistics.level_thresholds = list2.ToArray();
				break;
			}
			case "resistance_level_thresholds":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				playerStatistics.ResistanceExpTable = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return playerStatistics;
	}

	private static object ReadYaml_PromotionLink(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PromotionLink promotionLink = new PromotionLink();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "main_text":
					promotionLink.MainText = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "sub_text":
					promotionLink.SubText = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "hud_text":
					promotionLink.HudText = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "bg_color":
					promotionLink.BackgroundColor = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "image":
					promotionLink.Image = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "commodity_id":
					promotionLink.CommodityId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "web_link":
					promotionLink.WebLink = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "start_at":
					promotionLink.StartAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "end_at":
					promotionLink.EndAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return promotionLink;
	}

	private static object ReadYaml_Prototype(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Prototype prototype = new Prototype();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "min_level":
				prototype.MinLevel = Convert.ToInt32(reader.Value);
				break;
			case "max_level":
				prototype.MaxLevel = Convert.ToInt32(reader.Value);
				break;
			case "item_description":
				prototype.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "name":
				prototype.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				prototype.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "category":
				prototype.Category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "sub_categories":
			{
				List<string> list2 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list2.Add(item2);
				}
				prototype.SubCategories = list2.ToArray();
				break;
			}
			case "dump_locked":
				prototype.DumpLocked = Convert.ToBoolean(reader.Value);
				break;
			case "dyeables":
			{
				List<ColorChannel> list = new List<ColorChannel>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ColorChannel item = (ColorChannel)Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				prototype.Dyeables = list;
				break;
			}
			case "help":
				prototype.Help = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "color_r":
				prototype.ColorR = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "color_g":
				prototype.ColorG = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "color_b":
				prototype.ColorB = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "hiding_color":
				prototype.HidingColor = Convert.ToBoolean(reader.Value);
				break;
			case "immune_to_time":
				prototype.ImmuneToTime = Convert.ToBoolean(reader.Value);
				break;
			case "time_limited":
				prototype.TimeLimited = Convert.ToBoolean(reader.Value);
				break;
			case "size":
				prototype.Size = Convert.ToInt32(reader.Value);
				break;
			case "tags":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string value = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				prototype.Tags = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return prototype;
	}

	private static object ReadYaml_PrototypePreset(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PrototypePreset prototypePreset = new PrototypePreset();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				prototypePreset.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				prototypePreset.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				prototypePreset.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "level":
				prototypePreset.Level = Convert.ToInt32(reader.Value);
				break;
			case "max_durability":
				prototypePreset.MaxDurability = Convert.ToSingle(reader.Value);
				break;
			case "modifiable_count":
				prototypePreset.ModifiableCount = Convert.ToInt32(reader.Value);
				break;
			case "size":
				prototypePreset.Size = Convert.ToInt32(reader.Value);
				break;
			case "color_r":
				prototypePreset.ColorR = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "color_g":
				prototypePreset.ColorG = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "color_b":
				prototypePreset.ColorB = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "tags":
			{
				List<PrototypePresetTag> list4 = new List<PrototypePresetTag>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PrototypePresetTag item4 = (PrototypePresetTag)ReadYaml_PrototypePresetTag(reader, objectType, existingValue, serializer);
					list4.Add(item4);
				}
				prototypePreset.Tags = list4.ToArray();
				break;
			}
			case "performance":
			{
				List<PrototypePresetPerformance> list3 = new List<PrototypePresetPerformance>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PrototypePresetPerformance item3 = (PrototypePresetPerformance)ReadYaml_PrototypePresetPerformance(reader, objectType, existingValue, serializer);
					list3.Add(item3);
				}
				prototypePreset.Performances = list3.ToArray();
				break;
			}
			case "repair_requirement":
				prototypePreset.RepairRequirement = (PrototypePresetRepair)ReadYaml_PrototypePresetRepair(reader, objectType, existingValue, serializer);
				break;
			case "immune_to_time":
				prototypePreset.ImmuneToTime = Convert.ToBoolean(reader.Value);
				break;
			case "trade_locked":
				prototypePreset.TradeLocked = Convert.ToBoolean(reader.Value);
				break;
			case "dump_locked":
				prototypePreset.DumpLocked = Convert.ToBoolean(reader.Value);
				break;
			case "emotional_motions":
			{
				List<string> list2 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list2.Add(item2);
				}
				prototypePreset.EmotionalMotions = list2.ToArray();
				break;
			}
			case "ext_class_name":
				prototypePreset.ExtClassName = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "ext_class_args":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				prototypePreset.ExtClassArgs = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return prototypePreset;
	}

	private static object ReadYaml_PrototypePresetPerformance(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PrototypePresetPerformance prototypePresetPerformance = new PrototypePresetPerformance();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "id":
				prototypePresetPerformance.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "name":
				prototypePresetPerformance.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				prototypePresetPerformance.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "nums":
			{
				Dictionary<string, float> dictionary2 = new Dictionary<string, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				prototypePresetPerformance.Nums = dictionary2;
				break;
			}
			case "strs":
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string value = ((reader.TokenType != JsonToken.Null) ? ((reader.Value != null) ? reader.Value.ToString() : null) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				prototypePresetPerformance.Strs = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return prototypePresetPerformance;
	}

	private static object ReadYaml_PrototypePresetRepair(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PrototypePresetRepair prototypePresetRepair = new PrototypePresetRepair();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "tag":
					prototypePresetRepair.TagId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "perf":
					prototypePresetRepair.RepairPerformance = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return prototypePresetRepair;
	}

	private static object ReadYaml_PrototypePresetTag(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PrototypePresetTag prototypePresetTag = default(PrototypePresetTag);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "id":
					prototypePresetTag.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "level":
					prototypePresetTag.Level = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return prototypePresetTag;
	}

	private static object ReadYaml_PrototypeYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PrototypeYaml prototypeYaml = new PrototypeYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			List<Prototype> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				List<Prototype> list = new List<Prototype>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Prototype item = (Prototype)ReadYaml_Prototype(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				value = list;
			}
			if (text != null)
			{
				prototypeYaml.Add(text, value);
			}
		}
		return prototypeYaml;
	}

	private static object ReadYaml_PurchasableTime(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PurchasableTime purchasableTime = new PurchasableTime();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "purchase_starts_at":
					purchasableTime.PurchaseStartsAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "purchase_ends_at":
					purchasableTime.PurchaseEndsAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return purchasableTime;
	}

	private static object ReadYaml_PurchaseLimit(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PurchaseLimit purchaseLimit = default(PurchaseLimit);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "max_count":
				purchaseLimit.MaxCount = Convert.ToInt32(reader.Value);
				break;
			case "is_show_period":
				purchaseLimit.IsShowPeriod = Convert.ToBoolean(reader.Value);
				break;
			case "purchasable_times":
			{
				List<PurchasableTime> list = new List<PurchasableTime>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PurchasableTime item = (PurchasableTime)ReadYaml_PurchasableTime(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				purchaseLimit.PurchasableTimes = list.ToArray();
				break;
			}
			case "periodic_counts_limit":
				purchaseLimit.PeriodicCountsLimit = (PeriodicCountsLimit)ReadYaml_PeriodicCountsLimit(reader, objectType, existingValue, serializer);
				break;
			case "periodic_limit":
				purchaseLimit.PeriodicLimit = (PeriodicLimit)ReadYaml_PeriodicLimit(reader, objectType, existingValue, serializer);
				break;
			case "steam_dlc_only":
				purchaseLimit.IsSteamDlcOnly = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return purchaseLimit;
	}

	private static object ReadYaml_PurchaseRandomPiece(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PurchaseRandomPiece purchaseRandomPiece = default(PurchaseRandomPiece);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "need_warpgem":
					purchaseRandomPiece.NeedWarpGem = Convert.ToInt32(reader.Value);
					break;
				case "give_r_piece":
					purchaseRandomPiece.GiveRandomPiece = Convert.ToInt32(reader.Value);
					break;
				case "purchable_count":
					purchaseRandomPiece.PurchasableCount = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return purchaseRandomPiece;
	}

	private static object ReadYaml_PushCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PushCategory pushCategory = new PushCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "category_name":
				pushCategory.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "policies":
			{
				List<PushPolicy> list = new List<PushPolicy>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PushPolicy item = (PushPolicy)ReadYaml_PushPolicy(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				pushCategory.Policies = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return pushCategory;
	}

	private static object ReadYaml_PushCategoryYml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PushCategoryYml pushCategoryYml = new PushCategoryYml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "push_categories")
			{
				List<PushCategory> list = new List<PushCategory>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					PushCategory item = (PushCategory)ReadYaml_PushCategory(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				pushCategoryYml.PushCategories = list.ToArray();
			}
			else
			{
				reader.Skip();
			}
		}
		return pushCategoryYml;
	}

	private static object ReadYaml_PushPolicy(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PushPolicy pushPolicy = new PushPolicy();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "policy_name":
					pushPolicy.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "policy":
					pushPolicy.Policy = (Policy)Convert.ToInt32(reader.Value);
					break;
				case "local":
					pushPolicy.IsLocal = Convert.ToBoolean(reader.Value);
					break;
				case "id":
					pushPolicy.Id = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return pushPolicy;
	}

	private static object ReadYaml_PutInContainerInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		PutInContainerInfo putInContainerInfo = new PutInContainerInfo();
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "biomes":
			{
				List<Biome> list2 = new List<Biome>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Biome item2 = (Biome)Convert.ToInt32(reader.Value);
					list2.Add(item2);
				}
				putInContainerInfo.Biomes = list2.ToArray();
				flag = true;
				break;
			}
			case "tags":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				putInContainerInfo.Tags = list.ToArray();
				flag2 = true;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'biomes' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'tags' not found in JSON.");
		}
		return putInContainerInfo;
	}

	private static object ReadYaml_Season2_Quantity(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Season2.Quantity quantity = (Season2.Quantity)Convert.ToInt32(reader.Value);
		return quantity;
	}

	private static object ReadYaml_QuestMessages(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		QuestMessages questMessages = new QuestMessages();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "messenger":
					questMessages.Messenger = (Shared.Faction.Messenger)Convert.ToInt32(reader.Value);
					break;
				case "message":
					questMessages.Message = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "image":
					questMessages.Image = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_portrait":
					questMessages.HidePortrait = Convert.ToBoolean(reader.Value);
					break;
				case "remote":
					questMessages.Remote = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return questMessages;
	}

	private static object ReadYaml_QuestsYml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		QuestsYml questsYml = new QuestsYml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			QuestYml value = ((reader.TokenType != JsonToken.Null) ? ((QuestYml)ReadYaml_QuestYml(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				questsYml.Add(text, value);
			}
		}
		return questsYml;
	}

	private static object ReadYaml_QuestYml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		QuestYml questYml = new QuestYml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "quest_type":
				questYml.QuestType = (QuestType)Convert.ToInt32(reader.Value);
				break;
			case "category":
				questYml.Category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon":
				questYml.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "chapter_subject":
				questYml.ChapterSubject = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "subject":
				questYml.Subject = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				questYml.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "display_on_hud":
				questYml.DisplayOnHud = Convert.ToBoolean(reader.Value);
				break;
			case "auto_finish":
				questYml.AutoFinish = Convert.ToBoolean(reader.Value);
				break;
			case "order":
				questYml.Order = Convert.ToInt32(reader.Value);
				break;
			case "last_quest":
				questYml.LastQuest = Convert.ToBoolean(reader.Value);
				break;
			case "quest_start_messages":
			{
				List<QuestMessages> list2 = new List<QuestMessages>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					QuestMessages item2 = (QuestMessages)ReadYaml_QuestMessages(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				questYml.QuestStartMessages = list2.ToArray();
				break;
			}
			case "quest_end_messages":
			{
				List<QuestMessages> list = new List<QuestMessages>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					QuestMessages item = (QuestMessages)ReadYaml_QuestMessages(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				questYml.QuestEndMessages = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return questYml;
	}

	private static object ReadYaml_Ranking(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Ranking ranking = new Ranking();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "revision")
			{
				Dictionary<string, Revision> dictionary = new Dictionary<string, Revision>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Revision value = ((reader.TokenType != JsonToken.Null) ? ((Revision)ReadYaml_Revision(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				ranking.Revisions = dictionary;
			}
			else
			{
				reader.Skip();
			}
		}
		return ranking;
	}

	private static object ReadYaml_RankingReward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RankingReward rankingReward = new RankingReward();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "ranking_num":
				rankingReward.Ranking = Convert.ToInt32(reader.Value);
				break;
			case "ranking_percentage":
				rankingReward.RankingPecentage = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "rewards":
			{
				Dictionary<Shared.Faction.RewardType, JToken> dictionary = new Dictionary<Shared.Faction.RewardType, JToken>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					Shared.Faction.RewardType key = (Shared.Faction.RewardType)Convert.ToInt32(reader.Value);
					reader.Read();
					JToken value = ((reader.TokenType != JsonToken.Null) ? JToken.ReadFrom(reader) : null);
					dictionary.Add(key, value);
				}
				rankingReward.Rewards = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return rankingReward;
	}

	private static object ReadYaml_RankingRewards(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RankingRewards rankingRewards = new RankingRewards();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Rank.Category key = (Shared.Rank.Category)Convert.ToInt32(reader.Value);
			reader.Read();
			Dictionary<string, List<RankingReward>> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, List<RankingReward>> dictionary = new Dictionary<string, List<RankingReward>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					List<RankingReward> value2;
					if (reader.TokenType == JsonToken.Null)
					{
						value2 = null;
					}
					else
					{
						List<RankingReward> list = new List<RankingReward>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							RankingReward item = (RankingReward)ReadYaml_RankingReward(reader, objectType, existingValue, serializer);
							list.Add(item);
						}
						value2 = list;
					}
					if (text != null)
					{
						dictionary.Add(text, value2);
					}
				}
				value = dictionary;
			}
			rankingRewards.Add(key, value);
		}
		return rankingRewards;
	}

	private static object ReadYaml_Rankings(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Rankings rankings = new Rankings();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Rank.Category key = (Shared.Rank.Category)Convert.ToInt32(reader.Value);
			reader.Read();
			Ranking value = ((reader.TokenType != JsonToken.Null) ? ((Ranking)ReadYaml_Ranking(reader, objectType, existingValue, serializer)) : null);
			rankings.Add(key, value);
		}
		return rankings;
	}

	private static object ReadYaml_Recipe(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Recipe recipe = new Recipe();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "add_on":
			{
				Dictionary<string, string[]> dictionary2 = new Dictionary<string, string[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string[] value2;
					if (reader.TokenType == JsonToken.Null)
					{
						value2 = null;
					}
					else
					{
						List<string> list = new List<string>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							string item = ((reader.Value != null) ? reader.Value.ToString() : null);
							list.Add(item);
						}
						value2 = list.ToArray();
					}
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				recipe.add_on = dictionary2;
				break;
			}
			case "type":
				recipe.type = (CraftType)Convert.ToInt32(reader.Value);
				break;
			case "name":
				recipe.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				recipe.description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "count":
				recipe.count = Convert.ToInt32(reader.Value);
				break;
			case "deduct_modifiable_count":
				recipe.deduct_modifiable_count = Convert.ToBoolean(reader.Value);
				break;
			case "category":
				recipe.category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "subcategory":
				recipe.subcategory = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon":
				recipe.icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "min_level":
				recipe.min_level = Convert.ToInt32(reader.Value);
				break;
			case "max_level":
				recipe.max_level = Convert.ToInt32(reader.Value);
				break;
			case "duration_wait":
				recipe.duration_wait = Convert.ToInt32(reader.Value);
				break;
			case "entrusts":
				recipe.entrusts = Convert.ToBoolean(reader.Value);
				break;
			case "tool_tags":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				recipe.tool_tags = dictionary;
				break;
			}
			case "workbench_tags":
			{
				Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text4 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value3 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text4 != null)
					{
						dictionary3.Add(text4, value3);
					}
				}
				recipe.workbench_tags = dictionary3;
				break;
			}
			case "slots":
			{
				List<RecipeSlot> list2 = new List<RecipeSlot>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					RecipeSlot item2 = (RecipeSlot)ReadYaml_RecipeSlot(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				recipe.slots = list2.ToArray();
				break;
			}
			case "prototype_id":
				recipe.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "add_color_rate":
				recipe.add_color_rate = Convert.ToSingle(reader.Value);
				break;
			case "recipe_name_for_slot":
				recipe.recipe_name_for_slot = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "required_ability":
				recipe.required_ability = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					recipe.required_ability = (Derived)Convert.ToInt32(reader.Value);
				}
				break;
			case "required_recipe":
				recipe.required_recipe = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "season":
				recipe.season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "is_seasonal_region_bounded":
				recipe.IsSeasonalRegionBounded = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return recipe;
	}

	private static object ReadYaml_RecipeDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RecipeDict recipeDict = new RecipeDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Recipe value = ((reader.TokenType != JsonToken.Null) ? ((Recipe)ReadYaml_Recipe(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				recipeDict.Add(text, value);
			}
		}
		return recipeDict;
	}

	private static object ReadYaml_RecipeSlot(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RecipeSlot recipeSlot = new RecipeSlot();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "slot_id":
				recipeSlot.slot_id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "slot_name":
				recipeSlot.slot_name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "count_min":
				recipeSlot.count_min = Convert.ToInt32(reader.Value);
				break;
			case "count_max":
				recipeSlot.count_max = Convert.ToInt32(reader.Value);
				break;
			case "required_tags":
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text3 != null)
					{
						dictionary2.Add(text3, value2);
					}
				}
				recipeSlot.required_tags = dictionary2;
				break;
			}
			case "required_materials":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				recipeSlot.required_materials = dictionary;
				break;
			}
			case "source_info":
			{
				List<SlotSourceInfo> list = new List<SlotSourceInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					SlotSourceInfo item = (SlotSourceInfo)ReadYaml_SlotSourceInfo(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				recipeSlot.source_info = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return recipeSlot;
	}

	private static object ReadYaml_Recommends(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Recommends recommends = new Recommends();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "recommend_collecting_power":
					recommends.CollectingPower = Convert.ToInt32(reader.Value);
					break;
				case "recommend_combat_power":
					recommends.CombatPower = Convert.ToInt32(reader.Value);
					break;
				case "recommend_resistance_level":
					recommends.ResistanceLevel = Convert.ToInt32(reader.Value);
					break;
				case "required_resistance_level":
					recommends.RequiredResistanceLevel = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return recommends;
	}

	private static object ReadYaml_ReformTechSupport(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ReformTechSupport reformTechSupport = new ReformTechSupport();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "r_piece":
				reformTechSupport.RandomNumberPiece = Convert.ToInt32(reader.Value);
				break;
			case "tags":
			{
				Dictionary<string, ReformTechSupportTag> dictionary = new Dictionary<string, ReformTechSupportTag>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					ReformTechSupportTag value = ((reader.TokenType != JsonToken.Null) ? ((ReformTechSupportTag)ReadYaml_ReformTechSupportTag(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				reformTechSupport.Tags = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return reformTechSupport;
	}

	private static object ReadYaml_ReformTechSupportDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ReformTechSupportDict reformTechSupportDict = new ReformTechSupportDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			ReformTechSupport value = ((reader.TokenType != JsonToken.Null) ? ((ReformTechSupport)ReadYaml_ReformTechSupport(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				reformTechSupportDict.Add(text, value);
			}
		}
		return reformTechSupportDict;
	}

	private static object ReadYaml_ReformTechSupportTag(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ReformTechSupportTag reformTechSupportTag = new ReformTechSupportTag();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "min_level":
					reformTechSupportTag.MinLevel = Convert.ToInt32(reader.Value);
					break;
				case "max_level":
					reformTechSupportTag.MaxLevel = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return reformTechSupportTag;
	}

	private static object ReadYaml_RegionCoOp(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RegionCoOp regionCoOp = new RegionCoOp();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "co_op_todo_id":
					regionCoOp.CoOpTodoId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "subject":
					regionCoOp.Subject = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "description":
					regionCoOp.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "notice_icon":
					regionCoOp.NoticeIcon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return regionCoOp;
	}

	private static object ReadYaml_RegionCoOpDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RegionCoOpDict regionCoOpDict = new RegionCoOpDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Dictionary<string, RegionCoOp> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, RegionCoOp> dictionary = new Dictionary<string, RegionCoOp>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					RegionCoOp value2 = ((reader.TokenType != JsonToken.Null) ? ((RegionCoOp)ReadYaml_RegionCoOp(reader, objectType, existingValue, serializer)) : null);
					if (text2 != null)
					{
						dictionary.Add(text2, value2);
					}
				}
				value = dictionary;
			}
			if (text != null)
			{
				regionCoOpDict.Add(text, value);
			}
		}
		return regionCoOpDict;
	}

	private static object ReadYaml_RegionTemplate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RegionTemplate regionTemplate = new RegionTemplate();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "Id":
				regionTemplate.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "AvailableLevel":
				regionTemplate.AvailableLevel = Convert.ToInt32(reader.Value);
				break;
			case "level":
				regionTemplate.Level = Convert.ToInt32(reader.Value);
				break;
			case "biome_effects":
			{
				Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					string[] value;
					if (reader.TokenType == JsonToken.Null)
					{
						value = null;
					}
					else
					{
						List<string> list3 = new List<string>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							string item3 = ((reader.Value != null) ? reader.Value.ToString() : null);
							list3.Add(item3);
						}
						value = list3.ToArray();
					}
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				regionTemplate.BiomeEffects = dictionary;
				break;
			}
			case "expires_in":
				regionTemplate.ExpiresIn = Convert.ToDouble(reader.Value);
				break;
			case "lifespan_invisible":
				regionTemplate.LifespanInvisible = Convert.ToBoolean(reader.Value);
				break;
			case "cannotRevive":
				regionTemplate.CannotRevive = Convert.ToBoolean(reader.Value);
				break;
			case "factions":
			{
				List<FactionType> list = new List<FactionType>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					FactionType item = (FactionType)Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				regionTemplate.Factions = list;
				break;
			}
			case "role":
				regionTemplate.Role = (Role)Convert.ToInt32(reader.Value);
				break;
			case "tags":
			{
				List<string> list2 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list2.Add(item2);
				}
				regionTemplate.Tags = list2;
				break;
			}
			case "emblem":
				regionTemplate.Emblem = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "season":
				regionTemplate.Season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "unmatched_skill_and_recipe_hided":
				regionTemplate.UnmatchedSkillAndRecipeHided = Convert.ToBoolean(reader.Value);
				break;
			case "active":
				regionTemplate.Active = Convert.ToBoolean(reader.Value);
				break;
			case "apparent_climate":
				regionTemplate.ApparentClimate = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return regionTemplate;
	}

	private static object ReadYaml_RegionTemplateDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RegionTemplateDict regionTemplateDict = new RegionTemplateDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			RegionTemplate value = ((reader.TokenType != JsonToken.Null) ? ((RegionTemplate)ReadYaml_RegionTemplate(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				regionTemplateDict.Add(text, value);
			}
		}
		return regionTemplateDict;
	}

	private static object ReadYaml_RemodelingBlueprint(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RemodelingBlueprint remodelingBlueprint = new RemodelingBlueprint();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				remodelingBlueprint.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				remodelingBlueprint.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "min_level":
				remodelingBlueprint.MinLevel = Convert.ToInt32(reader.Value);
				break;
			case "max_level":
				remodelingBlueprint.MaxLevel = Convert.ToInt32(reader.Value);
				break;
			case "postprocess_time":
				remodelingBlueprint.PostprocessTime = Convert.ToInt32(reader.Value);
				break;
			case "tool_tags":
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				remodelingBlueprint.ToolTags = dictionary;
				break;
			}
			case "slots":
			{
				List<BlueprintSlot> list = new List<BlueprintSlot>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					BlueprintSlot item = (BlueprintSlot)ReadYaml_BlueprintSlot(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				remodelingBlueprint.Slots = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return remodelingBlueprint;
	}

	private static object ReadYaml_Repair(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Repair repair = default(Repair);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "artifact":
					repair.Artifact = (RepairItem)ReadYaml_RepairItem(reader, objectType, existingValue, serializer);
					flag = true;
					break;
				case "item":
					repair.Item = (RepairItem)ReadYaml_RepairItem(reader, objectType, existingValue, serializer);
					flag2 = true;
					break;
				case "repair_requirement_perf":
					repair.RepairRequirementFormula = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag3 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'artifact' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'item' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'repair_requirement_perf' not found in JSON.");
		}
		return repair;
	}

	private static object ReadYaml_RepairItem(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RepairItem repairItem = default(RepairItem);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "durability_result":
					repairItem.DurabilityResult = (DurabilityResult)ReadYaml_DurabilityResult(reader, objectType, existingValue, serializer);
					flag = true;
					break;
				case "limit_durability":
					repairItem.LimitDurability = Convert.ToSingle(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'durability_result' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'limit_durability' not found in JSON.");
		}
		return repairItem;
	}

	private static object ReadYaml_RequiredSkill(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RequiredSkill requiredSkill = new RequiredSkill();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "skill_category":
					requiredSkill.skill_category = (Shared.Skill.Category)Convert.ToInt32(reader.Value);
					break;
				case "level":
					requiredSkill.level = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return requiredSkill;
	}

	private static object ReadYaml_Research(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Research research = new Research();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "category":
					research.Category = (ResearchCategory)Convert.ToInt32(reader.Value);
					break;
				case "name":
					research.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "effect":
					research.Effect = (ResearchEffect)ReadYaml_ResearchEffect(reader, objectType, existingValue, serializer);
					break;
				case "currency":
					research.Currency = (Currency)Convert.ToInt32(reader.Value);
					break;
				case "amount":
					research.Amount = Convert.ToInt32(reader.Value);
					break;
				case "icon":
					research.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "tier":
					research.Tier = (LaboratoryTier)Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return research;
	}

	private static object ReadYaml_ResearchEffect(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ResearchEffect researchEffect = default(ResearchEffect);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "status_effect_id":
					researchEffect.StatusEffectId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "level":
					researchEffect.Level = Convert.ToInt32(reader.Value);
					break;
				case "apply_limits":
					researchEffect.ApplyLimits = (EffectApplyLimits)Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return researchEffect;
	}

	private static object ReadYaml_Resistance(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Resistance resistance = default(Resistance);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "types_by_biome")
			{
				Dictionary<Biome, Derived> dictionary = new Dictionary<Biome, Derived>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					Biome key = (Biome)Convert.ToInt32(reader.Value);
					reader.Read();
					Derived value = ((reader.TokenType != JsonToken.Null) ? ((Derived)Convert.ToInt32(reader.Value)) : Derived.MaxHealth);
					dictionary.Add(key, value);
				}
				resistance.TypeByBiome = dictionary;
			}
			else
			{
				reader.Skip();
			}
		}
		return resistance;
	}

	private static object ReadYaml_RestoreCost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RestoreCost restoreCost = default(RestoreCost);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "amount":
					restoreCost.Amount = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "currency":
					restoreCost.Currency = (Currency)Convert.ToInt32(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'amount' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'currency' not found in JSON.");
		}
		return restoreCost;
	}

	private static object ReadYaml_Revision(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Revision revision = new Revision();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "name":
					revision.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "finish_at":
					revision.FinishAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "starts_at":
					revision.StartsAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "reward_acquire_limit_at":
					revision.RewardAcquireLimitAt = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return revision;
	}

	private static object ReadYaml_ReviveImmediatelyCost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ReviveImmediatelyCost reviveImmediatelyCost = new ReviveImmediatelyCost();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "amount":
				reviveImmediatelyCost.Amount = Convert.ToInt32(reader.Value);
				break;
			case "currency":
				reviveImmediatelyCost.Currency = (Currency)Convert.ToInt32(reader.Value);
				break;
			case "vouchers":
			{
				List<VoucherWithCommodity> list = new List<VoucherWithCommodity>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					VoucherWithCommodity item = (VoucherWithCommodity)ReadYaml_VoucherWithCommodity(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				reviveImmediatelyCost.Vouchers = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return reviveImmediatelyCost;
	}

	private static object ReadYaml_Reward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Reward reward = new Reward();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "type":
				reward.Type = (Shared.Skill.RewardType)Convert.ToInt32(reader.Value);
				break;
			case "category":
				reward.Category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "category_level":
				reward.CategoryLevel = Convert.ToInt32(reader.Value);
				break;
			case "name":
				reward.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "recipe_ids":
			{
				List<string> list3 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list3.Add(item3);
				}
				reward.RecipeIds = list3.ToArray();
				break;
			}
			case "blueprint_ids":
			{
				List<string> list4 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item4 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list4.Add(item4);
				}
				reward.BlueprintIds = list4.ToArray();
				break;
			}
			case "modifiers":
			{
				Dictionary<string, float> dictionary = new Dictionary<string, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float value = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				reward.Modifiers = dictionary;
				break;
			}
			case "action_ids":
			{
				List<string> list2 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list2.Add(item2);
				}
				reward.ActionIds = list2.ToArray();
				break;
			}
			case "modifier":
				reward.Modifier = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "value":
				reward.Value = Convert.ToSingle(reader.Value);
				break;
			case "seed_id":
				reward.SeedId = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "entity_types":
			{
				List<int> list = new List<int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				reward.EntityTypes = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return reward;
	}

	private static object ReadYaml_RewardItem(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RewardItem rewardItem = new RewardItem();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "prototype_id":
					rewardItem.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "level":
					rewardItem.level = Convert.ToInt32(reader.Value);
					break;
				case "count":
					rewardItem.count = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return rewardItem;
	}

	private static object ReadYaml_RewardYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RewardYaml rewardYaml = new RewardYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Reward value = ((reader.TokenType != JsonToken.Null) ? ((Reward)ReadYaml_Reward(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				rewardYaml.Add(text, value);
			}
		}
		return rewardYaml;
	}

	private static object ReadYaml_Sailing(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Sailing sailing = default(Sailing);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "role_opening_levels")
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				sailing.RoleOpeningLevels = dictionary;
				flag = true;
			}
			else
			{
				reader.Skip();
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'role_opening_levels' not found in JSON.");
		}
		return sailing;
	}

	private static object ReadYaml_SalesFeeRates(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SalesFeeRates salesFeeRates = default(SalesFeeRates);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "under_threshold":
					salesFeeRates.UnderThreshold = Convert.ToDouble(reader.Value);
					flag = true;
					break;
				case "over_threshold":
					salesFeeRates.OverThreshold = Convert.ToDouble(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'under_threshold' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'over_threshold' not found in JSON.");
		}
		return salesFeeRates;
	}

	private static object ReadYaml_ScribbleCanvasStruct(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ScribbleCanvasStruct scribbleCanvasStruct = new ScribbleCanvasStruct();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "width":
					scribbleCanvasStruct.width = Convert.ToInt32(reader.Value);
					break;
				case "height":
					scribbleCanvasStruct.height = Convert.ToInt32(reader.Value);
					break;
				case "frame":
					scribbleCanvasStruct.frame = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return scribbleCanvasStruct;
	}

	private static object ReadYaml_ScribbleType(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ScribbleType scribbleType = new ScribbleType();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "text":
					scribbleType.text = Convert.ToBoolean(reader.Value);
					break;
				case "canvas":
					scribbleType.canvas = (ScribbleCanvasStruct)ReadYaml_ScribbleCanvasStruct(reader, objectType, existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return scribbleType;
	}

	private static object ReadYaml_Season2(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Season2 season = default(Season2);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "alpha_stone":
				season.AlphaStoneType = Convert.ToUInt16(reader.Value);
				break;
			case "bravo_stone":
				season.BravoStoneType = Convert.ToUInt16(reader.Value);
				break;
			case "charlie_stone":
				season.CharlieStoneType = Convert.ToUInt16(reader.Value);
				break;
			case "entree_level_limit":
				season.EntreeLevelLimit = Convert.ToInt32(reader.Value);
				break;
			case "resource_quantity":
			{
				Dictionary<string, float> dictionary2 = new Dictionary<string, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					if (text2 != null)
					{
						dictionary2.Add(text2, value2);
					}
				}
				season.ResourceQuantity = dictionary2;
				break;
			}
			case "voucher":
				season.Voucher = (Season2Voucher)ReadYaml_Season2Voucher(reader, objectType, existingValue, serializer);
				break;
			case "ranking":
			{
				List<Shared.Rank.Category> list = new List<Shared.Rank.Category>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Shared.Rank.Category item = (Shared.Rank.Category)Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				season.Rankings = list;
				break;
			}
			case "storm_sign_time":
				season.StormSignTime = Convert.ToInt32(reader.Value);
				break;
			case "weathers":
			{
				Dictionary<int, Season2.WeatherInfo> dictionary = new Dictionary<int, Season2.WeatherInfo>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					Season2.WeatherInfo value = ((reader.TokenType != JsonToken.Null) ? ((Season2.WeatherInfo)ReadYaml_Season2_WeatherInfo(reader, objectType, existingValue, serializer)) : default(Season2.WeatherInfo));
					dictionary.Add(key, value);
				}
				season.Weathers = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return season;
	}

	private static object ReadYaml_Season2Voucher(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Season2Voucher season2Voucher = default(Season2Voucher);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "amount":
					season2Voucher.Amount = Convert.ToInt32(reader.Value);
					break;
				case "voucher_id":
					season2Voucher.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "commodity_id":
					season2Voucher.CommodityId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return season2Voucher;
	}

	private static object ReadYaml_ShopCategories(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ShopCategories shopCategories = new ShopCategories();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "shop_ui_categories":
			{
				List<ShopCategory> list = new List<ShopCategory>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ShopCategory item = (ShopCategory)ReadYaml_ShopCategory(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				shopCategories.Categories = list.ToArray();
				break;
			}
			case "shop_ui_options":
				shopCategories.ShopUIOptions = (ShopUIOption)ReadYaml_ShopUIOption(reader, objectType, existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return shopCategories;
	}

	private static object ReadYaml_ShopCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ShopCategory shopCategory = new ShopCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "show_promotion":
				shopCategory.ShowPromotion = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					shopCategory.ShowPromotion = Convert.ToBoolean(reader.Value);
				}
				break;
			case "view_type":
				shopCategory.ViewType = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "childs":
			{
				List<ShopCategory> list2 = new List<ShopCategory>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ShopCategory item2 = (ShopCategory)ReadYaml_ShopCategory(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				shopCategory.Childs = list2.ToArray();
				break;
			}
			case "key":
				shopCategory.Key = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "name":
				shopCategory.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				shopCategory.Icon = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "commodities":
			{
				List<ShopCategoryCondition> list = new List<ShopCategoryCondition>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ShopCategoryCondition item = (ShopCategoryCondition)ReadYaml_ShopCategoryCondition(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				shopCategory.Conditions = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return shopCategory;
	}

	private static object ReadYaml_ShopCategoryCondition(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ShopCategoryCondition shopCategoryCondition = new ShopCategoryCondition();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "tag":
				shopCategoryCondition.Tag = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					shopCategoryCondition.Tag = (Tags)Convert.ToInt32(reader.Value);
				}
				break;
			case "type":
				shopCategoryCondition.Type = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					shopCategoryCondition.Type = (CommodityType)Convert.ToInt32(reader.Value);
				}
				break;
			case "ui_category":
				shopCategoryCondition.Category = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "commodity_ids":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				shopCategoryCondition.CommodityIds = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return shopCategoryCondition;
	}

	private static object ReadYaml_ShopContents(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ShopContents shopContents = default(ShopContents);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "items":
			{
				List<ItemContent> list8 = new List<ItemContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ItemContent item8 = (ItemContent)ReadYaml_ItemContent(reader, objectType, existingValue, serializer);
					list8.Add(item8);
				}
				shopContents.Items = list8.ToArray();
				break;
			}
			case "money":
			{
				List<MoneyContent> list5 = new List<MoneyContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					MoneyContent item5 = (MoneyContent)ReadYaml_MoneyContent(reader, objectType, existingValue, serializer);
					list5.Add(item5);
				}
				shopContents.Money = list5.ToArray();
				break;
			}
			case "status_effects":
			{
				List<StatusEffectsContent> list9 = new List<StatusEffectsContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					StatusEffectsContent item9 = (StatusEffectsContent)ReadYaml_StatusEffectsContent(reader, objectType, existingValue, serializer);
					list9.Add(item9);
				}
				shopContents.StatusEffects = list9.ToArray();
				break;
			}
			case "motions":
			{
				List<string> list3 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item3 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list3.Add(item3);
				}
				shopContents.Motions = list3.ToArray();
				break;
			}
			case "capsulated_modulars":
			{
				List<ModularArtifactContent> list6 = new List<ModularArtifactContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					ModularArtifactContent item6 = (ModularArtifactContent)ReadYaml_ModularArtifactContent(reader, objectType, existingValue, serializer);
					list6.Add(item6);
				}
				shopContents.Modulars = list6.ToArray();
				break;
			}
			case "vouchers":
			{
				List<VoucherContent> list2 = new List<VoucherContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					VoucherContent item2 = (VoucherContent)ReadYaml_VoucherContent(reader, objectType, existingValue, serializer);
					list2.Add(item2);
				}
				shopContents.Vouchers = list2.ToArray();
				break;
			}
			case "refill_vouchers":
			{
				List<VoucherContent> list7 = new List<VoucherContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					VoucherContent item7 = (VoucherContent)ReadYaml_VoucherContent(reader, objectType, existingValue, serializer);
					list7.Add(item7);
				}
				shopContents.RefillVouchers = list7.ToArray();
				break;
			}
			case "motion_ids":
			{
				List<string> list4 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item4 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list4.Add(item4);
				}
				shopContents.WeightedMotions = list4.ToArray();
				break;
			}
			case "weighted_items":
			{
				List<WeightedItemContent> list = new List<WeightedItemContent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					WeightedItemContent item = (WeightedItemContent)ReadYaml_WeightedItemContent(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				shopContents.WeightedItems = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return shopContents;
	}

	private static object ReadYaml_ShopUIOption(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ShopUIOption shopUIOption = new ShopUIOption();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "show_tradable":
					shopUIOption.ShowTradable = (ItemTextCondition)ReadYaml_ItemTextCondition(reader, objectType, existingValue, serializer);
					break;
				case "show_repairable":
					shopUIOption.ShowRepairable = (ItemTextCondition)ReadYaml_ItemTextCondition(reader, objectType, existingValue, serializer);
					break;
				case "show_dyeable":
					shopUIOption.ShowDyeable = (ItemTextCondition)ReadYaml_ItemTextCondition(reader, objectType, existingValue, serializer);
					break;
				case "show_dumpable":
					shopUIOption.ShowDumpable = (ItemTextCondition)ReadYaml_ItemTextCondition(reader, objectType, existingValue, serializer);
					break;
				case "show_avatar":
					shopUIOption.ShowAvatar = (ItemTextCondition)ReadYaml_ItemTextCondition(reader, objectType, existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return shopUIOption;
	}

	private static object ReadYaml_FactionInfo_MissionData_ShuffleData(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		FactionInfo.MissionData.ShuffleData shuffleData = default(FactionInfo.MissionData.ShuffleData);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "max_count":
					shuffleData.MaxCount = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "recharge_cooltime":
					shuffleData.RechargeCooltime = Convert.ToDouble(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'max_count' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'recharge_cooltime' not found in JSON.");
		}
		return shuffleData;
	}

	private static object ReadYaml_Skill(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Skill skill = new Skill();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "category":
				skill.Category = (Shared.Skill.Category)Convert.ToInt32(reader.Value);
				break;
			case "category_level":
				skill.CategoryLevel = Convert.ToInt32(reader.Value);
				break;
			case "name":
				skill.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				skill.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "description":
				skill.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "subcategory":
				skill.Subcategory = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "untrain_disabled":
				skill.UntrainDisabled = Convert.ToBoolean(reader.Value);
				break;
			case "skill_point":
				skill.SkillPoint = Convert.ToInt32(reader.Value);
				break;
			case "render_priority":
				skill.RenderPriority = Convert.ToInt32(reader.Value);
				break;
			case "rewards":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				skill.Rewards = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return skill;
	}

	private static object ReadYaml_SkillAdvice(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillAdvice skillAdvice = new SkillAdvice();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "sub_id":
					skillAdvice.sub_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "skill_id":
					skillAdvice.skill_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "level":
					skillAdvice.level = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return skillAdvice;
	}

	private static object ReadYaml_SkillCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillCategory skillCategory = new SkillCategory();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "exp_needed":
			{
				Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key2 = Convert.ToInt32(reader.Value);
					reader.Read();
					int value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary2.Add(key2, value2);
				}
				skillCategory.ExpNeeded = dictionary2;
				break;
			}
			case "research_times":
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					int value = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary.Add(key, value);
				}
				skillCategory.ResearchTimes = dictionary;
				break;
			}
			case "season":
				skillCategory.Season = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return skillCategory;
	}

	private static object ReadYaml_SkillCategoryYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillCategoryYaml skillCategoryYaml = new SkillCategoryYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Skill.Category key = (Shared.Skill.Category)Convert.ToInt32(reader.Value);
			reader.Read();
			SkillCategory value = ((reader.TokenType != JsonToken.Null) ? ((SkillCategory)ReadYaml_SkillCategory(reader, objectType, existingValue, serializer)) : null);
			skillCategoryYaml.Add(key, value);
		}
		return skillCategoryYaml;
	}

	private static object ReadYaml_SkillConstants(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillConstants skillConstants = default(SkillConstants);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "exp_increase_limit":
					skillConstants.ExpIncreaseLimit = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "research_reduce_time_limit":
					skillConstants.ResearchReduceTimeLimit = Convert.ToSingle(reader.Value);
					flag2 = true;
					break;
				case "research_time_reduce":
					skillConstants.ResearchTimeReduce = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag3 = true;
					break;
				case "survival_check_period":
					skillConstants.SurvivalCheckPeriod = Convert.ToInt32(reader.Value);
					flag4 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'exp_increase_limit' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'research_reduce_time_limit' not found in JSON.");
		}
		if (!flag3)
		{
			throw new JsonSerializationException("Required property 'research_time_reduce' not found in JSON.");
		}
		if (!flag4)
		{
			throw new JsonSerializationException("Required property 'survival_check_period' not found in JSON.");
		}
		return skillConstants;
	}

	private static object ReadYaml_SkillModifier(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillModifier skillModifier = new SkillModifier();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "icon":
					skillModifier.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "reduce_type":
					skillModifier.ReduceType = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "description":
					skillModifier.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "name":
					skillModifier.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "increase_type":
					skillModifier.IncreaseType = (IncreaseType)Convert.ToInt32(reader.Value);
					break;
				case "apply_type":
					skillModifier.ApplyType = (ApplyType)Convert.ToInt32(reader.Value);
					break;
				case "inverse":
					skillModifier.Inverse = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return skillModifier;
	}

	private static object ReadYaml_SkillModifierYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillModifierYaml skillModifierYaml = new SkillModifierYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			SkillModifier value = ((reader.TokenType != JsonToken.Null) ? ((SkillModifier)ReadYaml_SkillModifier(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				skillModifierYaml.Add(text, value);
			}
		}
		return skillModifierYaml;
	}

	private static object ReadYaml_SkillUntrain(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillUntrain skillUntrain = default(SkillUntrain);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "free_count":
					skillUntrain.FreeCount = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "max_count":
					skillUntrain.MaxCount = Convert.ToInt32(reader.Value);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'free_count' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'max_count' not found in JSON.");
		}
		return skillUntrain;
	}

	private static object ReadYaml_SkillUntrainCost(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillUntrainCost skillUntrainCost = new SkillUntrainCost();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "amount":
					skillUntrainCost.Amount = JToken.ReadFrom(reader);
					break;
				case "including_commodity_id":
					skillUntrainCost.IncludingCommodityId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "currency":
					skillUntrainCost.Currency = (Currency)Convert.ToInt32(reader.Value);
					break;
				case "voucher_amount":
					skillUntrainCost.VoucherAmount = Convert.ToInt32(reader.Value);
					break;
				case "voucher_id":
					skillUntrainCost.VoucherId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return skillUntrainCost;
	}

	private static object ReadYaml_SkillUntrainInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillUntrainInfo skillUntrainInfo = new SkillUntrainInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "currency":
				skillUntrainInfo.Currency = (Currency)Convert.ToInt32(reader.Value);
				break;
			case "amount":
				skillUntrainInfo.Amount = Convert.ToInt32(reader.Value);
				break;
			case "vouchers":
			{
				List<SkillUntrainCost> list = new List<SkillUntrainCost>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					SkillUntrainCost item = (SkillUntrainCost)ReadYaml_SkillUntrainCost(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				skillUntrainInfo.Vouchers = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return skillUntrainInfo;
	}

	private static object ReadYaml_SkillYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkillYaml skillYaml = new SkillYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			Shared.Skill.Category key = (Shared.Skill.Category)Convert.ToInt32(reader.Value);
			reader.Read();
			Dictionary<string, Dictionary<string, Skill[]>> value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				Dictionary<string, Dictionary<string, Skill[]>> dictionary = new Dictionary<string, Dictionary<string, Skill[]>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					Dictionary<string, Skill[]> value2;
					if (reader.TokenType == JsonToken.Null)
					{
						value2 = null;
					}
					else
					{
						Dictionary<string, Skill[]> dictionary2 = new Dictionary<string, Skill[]>();
						while (reader.Read() && reader.TokenType != JsonToken.EndObject)
						{
							string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
							reader.Read();
							Skill[] value3;
							if (reader.TokenType == JsonToken.Null)
							{
								value3 = null;
							}
							else
							{
								List<Skill> list = new List<Skill>();
								while (reader.Read() && reader.TokenType != JsonToken.EndArray)
								{
									Skill item = (Skill)ReadYaml_Skill(reader, objectType, existingValue, serializer);
									list.Add(item);
								}
								value3 = list.ToArray();
							}
							if (text2 != null)
							{
								dictionary2.Add(text2, value3);
							}
						}
						value2 = dictionary2;
					}
					if (text != null)
					{
						dictionary.Add(text, value2);
					}
				}
				value = dictionary;
			}
			skillYaml.Add(key, value);
		}
		return skillYaml;
	}

	private static object ReadYaml_SkipTutorialMissions(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SkipTutorialMissions skipTutorialMissions = new SkipTutorialMissions();
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "skip_time":
					skipTutorialMissions.SkipTime = Convert.ToInt32(reader.Value);
					flag = true;
					break;
				case "todo_id":
					skipTutorialMissions.TodoId = ((reader.Value != null) ? reader.Value.ToString() : null);
					flag2 = true;
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'skip_time' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'todo_id' not found in JSON.");
		}
		return skipTutorialMissions;
	}

	private static object ReadYaml_SlotSourceInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SlotSourceInfo slotSourceInfo = new SlotSourceInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "type":
					slotSourceInfo.type = (SourceDescription)Convert.ToInt32(reader.Value);
					break;
				case "collectible_id":
					slotSourceInfo.collectible_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "generator_id":
					slotSourceInfo.generator_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "recipe_id":
					slotSourceInfo.recipe_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "recipe_id2":
					slotSourceInfo.recipe_id2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "prototype_id":
					slotSourceInfo.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "text":
					slotSourceInfo.text = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return slotSourceInfo;
	}

	private static object ReadYaml_SpecialDealBanner(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SpecialDealBanner specialDealBanner = new SpecialDealBanner();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "banner_title":
					specialDealBanner.Title = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "banner_promotion_description":
					specialDealBanner.PromotionDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "banner_warning_description":
					specialDealBanner.WarningDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "banner_item_description":
					specialDealBanner.ItemDescription = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return specialDealBanner;
	}

	private static object ReadYaml_SpecialDealBannersDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SpecialDealBannersDict specialDealBannersDict = new SpecialDealBannersDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			SpecialDealBanner value = ((reader.TokenType != JsonToken.Null) ? ((SpecialDealBanner)ReadYaml_SpecialDealBanner(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				specialDealBannersDict.Add(text, value);
			}
		}
		return specialDealBannersDict;
	}

	private static object ReadYaml_Sprinkler(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Sprinkler sprinkler = default(Sprinkler);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "sprinkle_water")
				{
					sprinkler.SprinkleWater = (SprinkleWater)ReadYaml_SprinkleWater(reader, objectType, existingValue, serializer);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'sprinkle_water' not found in JSON.");
		}
		return sprinkler;
	}

	private static object ReadYaml_SprinkleWater(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SprinkleWater sprinkleWater = default(SprinkleWater);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "duration")
				{
					sprinkleWater.Duration = Convert.ToInt32(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'duration' not found in JSON.");
		}
		return sprinkleWater;
	}

	private static object ReadYaml_StatusEffectsContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		StatusEffectsContent statusEffectsContent = new StatusEffectsContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "status_effects_id":
					statusEffectsContent.status_effects_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "duration_days":
					statusEffectsContent.duration_days = Convert.ToSingle(reader.Value);
					break;
				case "key":
					statusEffectsContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_in_shop":
					statusEffectsContent.hide_in_shop = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return statusEffectsContent;
	}

	private static object ReadYaml_StatusEffectTemplate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		StatusEffectTemplate statusEffectTemplate = new StatusEffectTemplate();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "duration":
				statusEffectTemplate.Duration = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "min_level":
				statusEffectTemplate.MinLevel = Convert.ToInt32(reader.Value);
				break;
			case "max_level":
				statusEffectTemplate.MaxLevel = Convert.ToInt32(reader.Value);
				break;
			case "name":
				statusEffectTemplate.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				statusEffectTemplate.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "floating_icon":
				statusEffectTemplate.FloatingIcon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon":
				statusEffectTemplate.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon_color":
				statusEffectTemplate.IconColor = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "skin_effect":
				statusEffectTemplate.SkinEffect = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "expiration_extendable":
				statusEffectTemplate.ExpirationExtendable = Convert.ToBoolean(reader.Value);
				break;
			case "service":
				statusEffectTemplate.Service = Convert.ToBoolean(reader.Value);
				break;
			case "ui_group_icon":
				statusEffectTemplate.UIGroup = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "screen_effect":
				statusEffectTemplate.ScreenEffectName = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "effects":
			{
				List<EffectDetail> list = new List<EffectDetail>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					EffectDetail item = (EffectDetail)ReadYaml_EffectDetail(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				statusEffectTemplate.Effects = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return statusEffectTemplate;
	}

	private static object ReadYaml_StatusEffectTemplateYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		StatusEffectTemplateYaml statusEffectTemplateYaml = new StatusEffectTemplateYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			StatusEffectTemplate[] value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				List<StatusEffectTemplate> list = new List<StatusEffectTemplate>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					StatusEffectTemplate item = (StatusEffectTemplate)ReadYaml_StatusEffectTemplate(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				value = list.ToArray();
			}
			if (text != null)
			{
				statusEffectTemplateYaml.Add(text, value);
			}
		}
		return statusEffectTemplateYaml;
	}

	private static object ReadYaml_StoryYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		StoryYaml storyYaml = new StoryYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Chapters value = ((reader.TokenType != JsonToken.Null) ? ((Chapters)ReadYaml_Chapters(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				storyYaml.Add(text, value);
			}
		}
		return storyYaml;
	}

	private static object ReadYaml_SubCommodityAcceptLimit(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SubCommodityAcceptLimit subCommodityAcceptLimit = new SubCommodityAcceptLimit();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "expires_at")
				{
					subCommodityAcceptLimit.ExpiresAt = ((reader.Value != null) ? reader.Value.ToString() : null);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return subCommodityAcceptLimit;
	}

	private static object ReadYaml_SupplyLevel(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		SupplyLevel supplyLevel = default(SupplyLevel);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "level":
				supplyLevel.Level = Convert.ToInt32(reader.Value);
				break;
			case "level_up_count":
				supplyLevel.TotalSteps = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					supplyLevel.TotalSteps = Convert.ToInt32(reader.Value);
				}
				break;
			case "quantity":
				supplyLevel.Quantity = Convert.ToInt32(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return supplyLevel;
	}

	private static object ReadYaml_Survive(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Survive survive = default(Survive);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "reward_bonus":
					survive.RewardBonus = Convert.ToSingle(reader.Value);
					break;
				case "survived_count":
					survive.SurvivorCount = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return survive;
	}

	private static object ReadYaml_RankingReward_Tag(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		RankingReward.Tag tag = new RankingReward.Tag();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "level":
					tag.Level = Convert.ToInt32(reader.Value);
					break;
				case "tag_id":
					tag.TagId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return tag;
	}

	private static object ReadYaml_Tag(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Tag tag = new Tag();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				tag.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "category":
				tag.Category = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "icon":
				tag.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "group":
				tag.Group = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "purpose":
				tag.Purpose = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "type":
				tag.Type = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "visible_level":
				tag.VisibleLevel = Convert.ToBoolean(reader.Value);
				break;
			case "unsearchable":
				tag.Unsearchable = Convert.ToBoolean(reader.Value);
				break;
			case "visible":
				tag.Visible = Convert.ToBoolean(reader.Value);
				break;
			case "grade":
				tag.Grade = (TagGrade)Convert.ToInt32(reader.Value);
				break;
			case "description":
				tag.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "required_performance":
				tag.RequiredPerformance = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "pet_food_reference":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				tag.PetFoodReference = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return tag;
	}

	private static object ReadYaml_TagAllowAction(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TagAllowAction tagAllowAction = new TagAllowAction();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "default_actions":
			{
				List<string> list2 = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					list2.Add(item2);
				}
				tagAllowAction.DefaultActions = list2.ToArray();
				break;
			}
			case "skill_actions":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				tagAllowAction.SkillActions = list.ToArray();
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return tagAllowAction;
	}

	private static object ReadYaml_TagAllowActions(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TagAllowActions tagAllowActions = new TagAllowActions();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			TagAllowAction value = ((reader.TokenType != JsonToken.Null) ? ((TagAllowAction)ReadYaml_TagAllowAction(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				tagAllowActions.Add(text, value);
			}
		}
		return tagAllowActions;
	}

	private static object ReadYaml_TagYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TagYaml tagYaml = new TagYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Tag value = ((reader.TokenType != JsonToken.Null) ? ((Tag)ReadYaml_Tag(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				tagYaml.Add(text, value);
			}
		}
		return tagYaml;
	}

	private static object ReadYaml_Talk(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Talk talk = default(Talk);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "messenger":
				talk.Messenger = (Shared.Faction.Messenger)Convert.ToInt32(reader.Value);
				break;
			case "target":
				talk.Target = null;
				if (reader.TokenType == JsonToken.StartObject || reader.Value != null)
				{
					talk.Target = (Shared.Faction.Messenger)Convert.ToInt32(reader.Value);
				}
				break;
			case "message":
				talk.Message = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return talk;
	}

	private static object ReadYaml_Talks(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Talks talks = new Talks();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "friendship_point":
				talks.FriendshipPoint = Convert.ToInt32(reader.Value);
				break;
			case "notice_type":
				talks.NoticeType = (TalkType)Convert.ToInt32(reader.Value);
				break;
			case "talks":
			{
				List<Talk> list = new List<Talk>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Talk item = (Talk)ReadYaml_Talk(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				talks.List = list.ToArray();
				break;
			}
			case "title":
				talks.Title = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "IsRead":
				talks.IsRead = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		return talks;
	}

	private static object ReadYaml_TalksYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TalksYaml talksYaml = new TalksYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			FactionType key = (FactionType)Convert.ToInt32(reader.Value);
			reader.Read();
			Talks[] value;
			if (reader.TokenType == JsonToken.Null)
			{
				value = null;
			}
			else
			{
				List<Talks> list = new List<Talks>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					Talks item = (Talks)ReadYaml_Talks(reader, objectType, existingValue, serializer);
					list.Add(item);
				}
				value = list.ToArray();
			}
			talksYaml.Add(key, value);
		}
		return talksYaml;
	}

	private static object ReadYaml_Taming(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Taming taming = new Taming();
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "tamable_hp_rate":
					taming.TamableHpRate = Convert.ToSingle(reader.Value);
					flag = true;
					break;
				case "taming_cooltime":
					taming.TamingCooltime = Convert.ToSingle(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'tamable_hp_rate' not found in JSON.");
		}
		return taming;
	}

	private static object ReadYaml_TimelineCategory(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TimelineCategory timelineCategory = default(TimelineCategory);
		bool flag = false;
		bool flag2 = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				timelineCategory.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				flag = true;
				break;
			case "types":
			{
				List<TimelineEvent> list = new List<TimelineEvent>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					TimelineEvent item = (TimelineEvent)Convert.ToInt32(reader.Value);
					list.Add(item);
				}
				timelineCategory.Types = list.ToArray();
				flag2 = true;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'name' not found in JSON.");
		}
		if (!flag2)
		{
			throw new JsonSerializationException("Required property 'types' not found in JSON.");
		}
		return timelineCategory;
	}

	private static object ReadYaml_TimelineMessage(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TimelineMessage timelineMessage = new TimelineMessage();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "third_person_side":
					timelineMessage.third_person_side = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "target_side":
					timelineMessage.target_side = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "agent_side":
					timelineMessage.agent_side = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				case "simple_state":
					timelineMessage.simple_state = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return timelineMessage;
	}

	private static object ReadYaml_TimelineMessagesYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TimelineMessagesYaml timelineMessagesYaml = new TimelineMessagesYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			if (text != null && text == "messages")
			{
				Dictionary<int, TimelineMessage> dictionary = new Dictionary<int, TimelineMessage>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					int key = Convert.ToInt32(reader.Value);
					reader.Read();
					TimelineMessage value = ((reader.TokenType != JsonToken.Null) ? ((TimelineMessage)ReadYaml_TimelineMessage(reader, objectType, existingValue, serializer)) : null);
					dictionary.Add(key, value);
				}
				timelineMessagesYaml.messages = dictionary;
			}
			else
			{
				reader.Skip();
			}
		}
		return timelineMessagesYaml;
	}

	private static object ReadYaml_Title(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Title title = new Title();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "name":
				title.name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "description":
				title.description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "abilities":
			{
				Dictionary<Basic, int> dictionary2 = new Dictionary<Basic, int>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					Basic key = (Basic)Convert.ToInt32(reader.Value);
					reader.Read();
					int value2 = ((reader.TokenType != JsonToken.Null) ? Convert.ToInt32(reader.Value) : 0);
					dictionary2.Add(key, value2);
				}
				title.abilities = dictionary2;
				break;
			}
			case "modifiers":
			{
				Dictionary<string, float> dictionary = new Dictionary<string, float>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = ((reader.Value != null) ? reader.Value.ToString() : null);
					reader.Read();
					float value = ((reader.TokenType != JsonToken.Null) ? Convert.ToSingle(reader.Value) : 0f);
					if (text2 != null)
					{
						dictionary.Add(text2, value);
					}
				}
				title.modifiers = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return title;
	}

	private static object ReadYaml_TitleYaml(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		TitleYaml titleYaml = new TitleYaml();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Title value = ((reader.TokenType != JsonToken.Null) ? ((Title)ReadYaml_Title(reader, objectType, existingValue, serializer)) : null);
			if (text != null)
			{
				titleYaml.Add(text, value);
			}
		}
		return titleYaml;
	}

	private static object ReadYaml_ToDoContents(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		ToDoContents toDoContents = new ToDoContents();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "point":
					toDoContents.Point = Convert.ToInt32(reader.Value);
					break;
				case "subject":
					toDoContents.Subject = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return toDoContents;
	}

	private static object ReadYaml_CommodityCondition_Type(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		CommodityCondition.Type type = (CommodityCondition.Type)Convert.ToInt32(reader.Value);
		return type;
	}

	private static object ReadYaml_UnstableFactorDict(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		UnstableFactorDict unstableFactorDict = new UnstableFactorDict();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			int key = Convert.ToInt32(reader.Value);
			reader.Read();
			Recommends value = ((reader.TokenType != JsonToken.Null) ? ((Recommends)ReadYaml_Recommends(reader, objectType, existingValue, serializer)) : null);
			unstableFactorDict.Add(key, value);
		}
		return unstableFactorDict;
	}

	private static object ReadYaml_Voucher(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Voucher voucher = default(Voucher);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "count_max":
				voucher.CountMax = Convert.ToInt32(reader.Value);
				break;
			case "description":
				voucher.Description = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "expires_on":
				voucher.ExpiresOn = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "guide_type":
				voucher.GuideType = (GuideType)Convert.ToInt32(reader.Value);
				break;
			case "icon":
				voucher.Icon = ((reader.Value != null) ? reader.Value.ToString() : null);
				break;
			case "icon_colors":
			{
				List<string> list = new List<string>();
				while (reader.Read() && reader.TokenType != JsonToken.EndArray)
				{
					string item = ((reader.Value != null) ? reader.Value.ToString() : null);
					list.Add(item);
				}
				voucher.IconColors = list;
				break;
			}
			case "link":
				voucher.Link = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				break;
			case "name":
				voucher.Name = (Gettext)_gettextConverter.ReadJson(reader, typeof(Gettext), existingValue, serializer);
				flag = true;
				break;
			case "visible":
				voucher.Visible = Convert.ToBoolean(reader.Value);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'name' not found in JSON.");
		}
		return voucher;
	}

	private static object ReadYaml_VoucherContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		VoucherContent voucherContent = new VoucherContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "voucher_id":
					voucherContent.voucher_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "count":
					voucherContent.count = Convert.ToInt32(reader.Value);
					break;
				case "key":
					voucherContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_in_shop":
					voucherContent.hide_in_shop = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return voucherContent;
	}

	private static object ReadYaml_WarpRushReward_VoucherInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		WarpRushReward.VoucherInfo voucherInfo = new WarpRushReward.VoucherInfo();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "Id":
					voucherInfo.Id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "Count":
					voucherInfo.Count = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return voucherInfo;
	}

	private static object ReadYaml_Vouchers(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Vouchers vouchers = new Vouchers();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			Voucher value = ((reader.TokenType != JsonToken.Null) ? ((Voucher)ReadYaml_Voucher(reader, objectType, existingValue, serializer)) : default(Voucher));
			if (text != null)
			{
				vouchers.Add(text, value);
			}
		}
		return vouchers;
	}

	private static object ReadYaml_VoucherWithCommodity(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		VoucherWithCommodity voucherWithCommodity = default(VoucherWithCommodity);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "including_commodity_id":
					voucherWithCommodity.IncludingCommodityId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "voucher_id":
					voucherWithCommodity.VoucherId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return voucherWithCommodity;
	}

	private static object ReadYaml_War(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		War war = default(War);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "max_tuner_count")
				{
					war.MaxTunerCount = Convert.ToInt32(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'max_tuner_count' not found in JSON.");
		}
		return war;
	}

	private static object ReadYaml_Warehouse(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Warehouse warehouse = default(Warehouse);
		bool flag = false;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "section_size")
				{
					warehouse.SectionSize = Convert.ToInt32(reader.Value);
					flag = true;
				}
				else
				{
					reader.Skip();
				}
			}
		}
		if (!flag)
		{
			throw new JsonSerializationException("Required property 'section_size' not found in JSON.");
		}
		return warehouse;
	}

	private static object ReadYaml_WarpAccelerator(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Yaml.WarpAccelerator warpAccelerator = new Yaml.WarpAccelerator();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "breaktime":
					warpAccelerator.Breaktime = Convert.ToSingle(reader.Value);
					break;
				case "phase_time":
					warpAccelerator.PhaseTime = Convert.ToSingle(reader.Value);
					break;
				case "reward_time":
					warpAccelerator.RewardTime = Convert.ToSingle(reader.Value);
					break;
				case "inactivate_time":
					warpAccelerator.InactivateTime = Convert.ToSingle(reader.Value);
					break;
				case "max_phase":
					warpAccelerator.MaxPhase = Convert.ToInt32(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return warpAccelerator;
	}

	private static object ReadYaml_WarpRushReward(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		WarpRushReward warpRushReward = new WarpRushReward();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "currency":
					warpRushReward.Currency = (WarpRushReward.CurrencyInfo)ReadYaml_WarpRushReward_CurrencyInfo(reader, objectType, existingValue, serializer);
					break;
				case "item":
					warpRushReward.Item = (WarpRushReward.ItemInfo)ReadYaml_WarpRushReward_ItemInfo(reader, objectType, existingValue, serializer);
					break;
				case "recipe_id":
					warpRushReward.Recipe = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "blueprint_id":
					warpRushReward.BlueprintId = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "Title":
					warpRushReward.Title = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "Voucher":
					warpRushReward.Voucher = (WarpRushReward.VoucherInfo)ReadYaml_WarpRushReward_VoucherInfo(reader, objectType, existingValue, serializer);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return warpRushReward;
	}

	private static object ReadYaml_WarpRushRewards(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		WarpRushRewards warpRushRewards = new WarpRushRewards();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType == JsonToken.Null)
			{
				continue;
			}
			switch (text)
			{
			case "level_rewards":
			{
				Dictionary<ResourceType, Dictionary<int, List<WarpRushReward>>> dictionary4 = new Dictionary<ResourceType, Dictionary<int, List<WarpRushReward>>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					ResourceType key4 = (ResourceType)Convert.ToInt32(reader.Value);
					reader.Read();
					Dictionary<int, List<WarpRushReward>> value4;
					if (reader.TokenType == JsonToken.Null)
					{
						value4 = null;
					}
					else
					{
						Dictionary<int, List<WarpRushReward>> dictionary5 = new Dictionary<int, List<WarpRushReward>>();
						while (reader.Read() && reader.TokenType != JsonToken.EndObject)
						{
							int key5 = Convert.ToInt32(reader.Value);
							reader.Read();
							List<WarpRushReward> value5;
							if (reader.TokenType == JsonToken.Null)
							{
								value5 = null;
							}
							else
							{
								List<WarpRushReward> list3 = new List<WarpRushReward>();
								while (reader.Read() && reader.TokenType != JsonToken.EndArray)
								{
									WarpRushReward item3 = (WarpRushReward)ReadYaml_WarpRushReward(reader, objectType, existingValue, serializer);
									list3.Add(item3);
								}
								value5 = list3;
							}
							dictionary5.Add(key5, value5);
						}
						value4 = dictionary5;
					}
					dictionary4.Add(key4, value4);
				}
				warpRushRewards.LevelRewards = dictionary4;
				break;
			}
			case "supply_rewards":
			{
				Dictionary<ResourceType, Dictionary<int, List<WarpRushReward>>> dictionary2 = new Dictionary<ResourceType, Dictionary<int, List<WarpRushReward>>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					ResourceType key2 = (ResourceType)Convert.ToInt32(reader.Value);
					reader.Read();
					Dictionary<int, List<WarpRushReward>> value2;
					if (reader.TokenType == JsonToken.Null)
					{
						value2 = null;
					}
					else
					{
						Dictionary<int, List<WarpRushReward>> dictionary3 = new Dictionary<int, List<WarpRushReward>>();
						while (reader.Read() && reader.TokenType != JsonToken.EndObject)
						{
							int key3 = Convert.ToInt32(reader.Value);
							reader.Read();
							List<WarpRushReward> value3;
							if (reader.TokenType == JsonToken.Null)
							{
								value3 = null;
							}
							else
							{
								List<WarpRushReward> list2 = new List<WarpRushReward>();
								while (reader.Read() && reader.TokenType != JsonToken.EndArray)
								{
									WarpRushReward item2 = (WarpRushReward)ReadYaml_WarpRushReward(reader, objectType, existingValue, serializer);
									list2.Add(item2);
								}
								value3 = list2;
							}
							dictionary3.Add(key3, value3);
						}
						value2 = dictionary3;
					}
					dictionary2.Add(key2, value2);
				}
				warpRushRewards.SupplyRewards = dictionary2;
				break;
			}
			case "supply_level":
			{
				Dictionary<ResourceType, List<SupplyLevel>> dictionary = new Dictionary<ResourceType, List<SupplyLevel>>();
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					ResourceType key = (ResourceType)Convert.ToInt32(reader.Value);
					reader.Read();
					List<SupplyLevel> value;
					if (reader.TokenType == JsonToken.Null)
					{
						value = null;
					}
					else
					{
						List<SupplyLevel> list = new List<SupplyLevel>();
						while (reader.Read() && reader.TokenType != JsonToken.EndArray)
						{
							SupplyLevel item = (SupplyLevel)ReadYaml_SupplyLevel(reader, objectType, existingValue, serializer);
							list.Add(item);
						}
						value = list;
					}
					dictionary.Add(key, value);
				}
				warpRushRewards.SupplyLevels = dictionary;
				break;
			}
			default:
				reader.Skip();
				break;
			}
		}
		return warpRushRewards;
	}

	private static object ReadYaml_Season2_WeatherInfo(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Season2.WeatherInfo weatherInfo = default(Season2.WeatherInfo);
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				if (text != null && text == "weather_id")
				{
					weatherInfo.WeatherId = ((reader.Value != null) ? reader.Value.ToString() : null);
				}
				else
				{
					reader.Skip();
				}
			}
		}
		return weatherInfo;
	}

	private static object ReadYaml_WeightedItemContent(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		WeightedItemContent weightedItemContent = new WeightedItemContent();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = ((reader.Value != null) ? reader.Value.ToString() : null);
			reader.Read();
			if (reader.TokenType != JsonToken.Null)
			{
				switch (text)
				{
				case "prototype_id":
					weightedItemContent.prototype_id = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "level":
					weightedItemContent.level = Convert.ToInt32(reader.Value);
					break;
				case "weight":
					weightedItemContent.weight = Convert.ToSingle(reader.Value);
					break;
				case "key":
					weightedItemContent.key = ((reader.Value != null) ? reader.Value.ToString() : null);
					break;
				case "hide_in_shop":
					weightedItemContent.hide_in_shop = Convert.ToBoolean(reader.Value);
					break;
				default:
					reader.Skip();
					break;
				}
			}
		}
		return weightedItemContent;
	}
}
