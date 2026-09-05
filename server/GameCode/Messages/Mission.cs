using MsgPack;
using Shared.Faction;

namespace Messages;

public struct Mission
{
	public string Id;

	public string RegionId;

	public FactionType Faction;

	public string Subject;

	public string Description;

	public MissionToDo[] Todos;

	public RewardInfo? Reward;

	public MissionBonusReward? BonusReward;

	public double? StartedAt;

	public int? TimeLimit;

	public static void Pack(Packer packer, Mission val, bool hint = false)
	{
		packer.PackArrayHeader(10);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.Pack((int)val.Faction);
		packer.PackString(val.Subject);
		packer.PackString(val.Description);
		if (val.Todos == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Todos.Length);
			for (int i = 0; i < val.Todos.Length; i++)
			{
				MissionToDo.Pack(packer, val.Todos[i]);
			}
		}
		if (!val.Reward.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RewardInfo.Pack(packer, val.Reward.Value);
		}
		if (!val.BonusReward.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			MissionBonusReward.Pack(packer, val.BonusReward.Value);
		}
		if (!val.StartedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.StartedAt.Value);
		}
		if (!val.TimeLimit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.TimeLimit.Value);
		}
	}

	public static Mission Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Mission result = default(Mission);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 101 < num)
		{
			result.Faction = FactionType.Invalid;
		}
		else
		{
			result.Faction = (FactionType)num;
		}
		unpacker.Read();
		result.Subject = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Description = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Todos = new MissionToDo[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref MissionToDo reference = ref result.Todos[i];
			reference = MissionToDo.Unpack(unpacker);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Reward = null;
		}
		else
		{
			RewardInfo value = RewardInfo.Unpack(unpacker);
			result.Reward = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BonusReward = null;
		}
		else
		{
			MissionBonusReward value2 = MissionBonusReward.Unpack(unpacker);
			result.BonusReward = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.StartedAt = null;
		}
		else
		{
			double value3 = unpacker.LastReadData.AsDouble();
			result.StartedAt = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TimeLimit = null;
		}
		else
		{
			int value4 = unpacker.LastReadData.AsInt32();
			result.TimeLimit = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Mission Id={Id} RegionId={RegionId} Faction={Faction} Subject={Subject} Description={Description} Todos={Todos} Reward={Reward} BonusReward={BonusReward} StartedAt={StartedAt} TimeLimit={TimeLimit}>";
	}
}
