using MsgPack;

namespace Messages;

public struct UseActionTodo
{
	public const uint TypeCode = 3526u;

	public int TargetEntityType;

	public int Count;

	public int RequiredCount;

	public string SkillRewardId;

	public string ActionName;

	public static void Pack(Packer packer, UseActionTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3526u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack(val.TargetEntityType);
		packer.Pack(val.Count);
		packer.Pack(val.RequiredCount);
		if (val.SkillRewardId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillRewardId);
		}
		packer.PackString(val.ActionName);
	}

	public static UseActionTodo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UseActionTodo result = default(UseActionTodo);
		result.TargetEntityType = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RequiredCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SkillRewardId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ActionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<UseActionTodo TargetEntityType={TargetEntityType} Count={Count} RequiredCount={RequiredCount} SkillRewardId={SkillRewardId} ActionName={ActionName}>";
	}
}
