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
		unpacker.Read();
		HuntGoal result = default(HuntGoal);
		result.TargetEntityType = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SkillRewardId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ActionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.RequiredCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<HuntGoal TargetEntityType={TargetEntityType} SkillRewardId={SkillRewardId} ActionName={ActionName} RequiredCount={RequiredCount}>";
	}
}
