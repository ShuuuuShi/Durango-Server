using MsgPack;

namespace Messages;

public struct Skill
{
	public string SkillId;

	public int Level;

	public string SubId;

	public static void Pack(Packer packer, Skill val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.SkillId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillId);
		}
		packer.Pack(val.Level);
		if (val.SubId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SubId);
		}
	}

	public static Skill Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Skill result = default(Skill);
		result.SkillId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.SubId = ((MessagePackObject)(ref lastReadData3)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Skill SkillId={SkillId} Level={Level} SubId={SubId}>";
	}
}
