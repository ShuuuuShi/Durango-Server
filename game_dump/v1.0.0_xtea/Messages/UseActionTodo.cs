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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		UseActionTodo result = default(UseActionTodo);
		result.TargetEntityType = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Count = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.RequiredCount = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.SkillRewardId = ((MessagePackObject)(ref lastReadData4)).AsString();
		unpacker.Read();
		result.ActionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<UseActionTodo TargetEntityType={TargetEntityType} Count={Count} RequiredCount={RequiredCount} SkillRewardId={SkillRewardId} ActionName={ActionName}>";
	}
}
