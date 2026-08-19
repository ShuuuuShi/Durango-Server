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
		unpacker.Read();
		Skill result = default(Skill);
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SubId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Skill SkillId={SkillId} Level={Level} SubId={SubId}>";
	}
}
