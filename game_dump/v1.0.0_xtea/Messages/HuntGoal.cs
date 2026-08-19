using MsgPack;

namespace Messages;

public struct HuntGoal
{
	public const uint TypeCode = 2513u;

	public int TargetEntityType;

	public string SkillRewardId;

	public string ActionName;

	public int RequiredCount;

	public static void Pack(Packer packer, HuntGoal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2513u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.TargetEntityType);
		if (val.SkillRewardId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillRewardId);
		}
		packer.PackString(val.ActionName);
		packer.Pack(val.RequiredCount);
	}

	public static HuntGoal Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		HuntGoal result = default(HuntGoal);
		result.TargetEntityType = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SkillRewardId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		result.ActionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.RequiredCount = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<HuntGoal TargetEntityType={TargetEntityType} SkillRewardId={SkillRewardId} ActionName={ActionName} RequiredCount={RequiredCount}>";
	}
}
