using MsgPack;

namespace Messages;

public struct Rewarded
{
	public const uint TypeCode = 2065u;

	public object Effect;

	public RewardInfo Reward;

	public static void Pack(Packer packer, Rewarded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2065u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Effect == null)
		{
			packer.PackNull();
		}
		else if (val.Effect is HuntRewardEffect)
		{
			HuntRewardEffect.Pack(packer, (HuntRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is LevelUpEffect)
		{
			LevelUpEffect.Pack(packer, (LevelUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is CategoryLevelUpRewardEffect)
		{
			CategoryLevelUpRewardEffect.Pack(packer, (CategoryLevelUpRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is MissionCompletedEffect)
		{
			MissionCompletedEffect.Pack(packer, (MissionCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is ExplorePOIEffect)
		{
			ExplorePOIEffect.Pack(packer, (ExplorePOIEffect)val.Effect, hint: true);
		}
		else if (val.Effect is RepairEffect)
		{
			RepairEffect.Pack(packer, (RepairEffect)val.Effect, hint: true);
		}
		else if (val.Effect is AdviceCompletedEffect)
		{
			AdviceCompletedEffect.Pack(packer, (AdviceCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is AttendanceTakenEffect)
		{
			AttendanceTakenEffect.Pack(packer, (AttendanceTakenEffect)val.Effect, hint: true);
		}
		else if (val.Effect is PetLevelUpEffect)
		{
			PetLevelUpEffect.Pack(packer, (PetLevelUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is PetTaskFinishedEffect)
		{
			PetTaskFinishedEffect.Pack(packer, (PetTaskFinishedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is ResistanceLevelUpEffect)
		{
			ResistanceLevelUpEffect.Pack(packer, (ResistanceLevelUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is PioneerGradeUpEffect)
		{
			PioneerGradeUpEffect.Pack(packer, (PioneerGradeUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is ArchipelagoRegionRewardsEffect)
		{
			ArchipelagoRegionRewardsEffect.Pack(packer, (ArchipelagoRegionRewardsEffect)val.Effect, hint: true);
		}
		else if (val.Effect is RankingRewardEffect)
		{
			RankingRewardEffect.Pack(packer, (RankingRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is TamingCompletedEffect)
		{
			TamingCompletedEffect.Pack(packer, (TamingCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is DailyMissionCompletedEffect)
		{
			DailyMissionCompletedEffect.Pack(packer, (DailyMissionCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is SkillRewardEffect)
		{
			SkillRewardEffect.Pack(packer, (SkillRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is OfferCompletedEffect)
		{
			OfferCompletedEffect.Pack(packer, (OfferCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is FactionLevelUpEffect)
		{
			FactionLevelUpEffect.Pack(packer, (FactionLevelUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is S02SupplyRewardsEffect)
		{
			S02SupplyRewardsEffect.Pack(packer, (S02SupplyRewardsEffect)val.Effect, hint: true);
		}
		else if (val.Effect is OpenRewardBoxEffect)
		{
			OpenRewardBoxEffect.Pack(packer, (OpenRewardBoxEffect)val.Effect, hint: true);
		}
		else if (val.Effect is AttachmentReceivedEffect)
		{
			AttachmentReceivedEffect.Pack(packer, (AttachmentReceivedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is WarpAccelerationRewardsEffect)
		{
			WarpAccelerationRewardsEffect.Pack(packer, (WarpAccelerationRewardsEffect)val.Effect, hint: true);
		}
		RewardInfo.Pack(packer, val.Reward);
	}

	public static Rewarded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Rewarded result = default(Rewarded);
		result.Effect = null;
		if (unpacker.ReadUInt32(out var result2))
		{
			switch (result2)
			{
			case 2061u:
				result.Effect = HuntRewardEffect.Unpack(unpacker);
				break;
			case 2062u:
				result.Effect = LevelUpEffect.Unpack(unpacker);
				break;
			case 2064u:
				result.Effect = CategoryLevelUpRewardEffect.Unpack(unpacker);
				break;
			case 2078u:
				result.Effect = MissionCompletedEffect.Unpack(unpacker);
				break;
			case 2079u:
				result.Effect = ExplorePOIEffect.Unpack(unpacker);
				break;
			case 2082u:
				result.Effect = RepairEffect.Unpack(unpacker);
				break;
			case 2083u:
				result.Effect = AdviceCompletedEffect.Unpack(unpacker);
				break;
			case 2084u:
				result.Effect = AttendanceTakenEffect.Unpack(unpacker);
				break;
			case 2086u:
				result.Effect = PetLevelUpEffect.Unpack(unpacker);
				break;
			case 2087u:
				result.Effect = PetTaskFinishedEffect.Unpack(unpacker);
				break;
			case 20620u:
				result.Effect = ResistanceLevelUpEffect.Unpack(unpacker);
				break;
			case 20621u:
				result.Effect = PioneerGradeUpEffect.Unpack(unpacker);
				break;
			case 240003u:
				result.Effect = ArchipelagoRegionRewardsEffect.Unpack(unpacker);
				break;
			case 20871u:
				result.Effect = RankingRewardEffect.Unpack(unpacker);
				break;
			case 2060u:
				result.Effect = TamingCompletedEffect.Unpack(unpacker);
				break;
			case 19843572u:
				result.Effect = DailyMissionCompletedEffect.Unpack(unpacker);
				break;
			case 2063u:
				result.Effect = SkillRewardEffect.Unpack(unpacker);
				break;
			case 2066u:
				result.Effect = OfferCompletedEffect.Unpack(unpacker);
				break;
			case 2068u:
				result.Effect = FactionLevelUpEffect.Unpack(unpacker);
				break;
			case 222212u:
				result.Effect = S02SupplyRewardsEffect.Unpack(unpacker);
				break;
			case 29875326u:
				result.Effect = OpenRewardBoxEffect.Unpack(unpacker);
				break;
			case 20841u:
				result.Effect = AttachmentReceivedEffect.Unpack(unpacker);
				break;
			case 21112517u:
				result.Effect = WarpAccelerationRewardsEffect.Unpack(unpacker);
				break;
			default:
				Debug.LogError("Unexpected type code: " + result2);
				break;
			}
		}
		unpacker.Read();
		result.Reward = RewardInfo.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Rewarded Effect={Effect} Reward={Reward}>";
	}
}
