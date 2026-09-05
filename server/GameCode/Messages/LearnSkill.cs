using MsgPack;

namespace Messages;

public struct LearnSkill
{
	public const uint TypeCode = 2048u;

	public string SkillId;

	public int Level;

	public string SubId;

	public static void Pack(Packer packer, LearnSkill val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2048u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
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

	public static LearnSkill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LearnSkill result = default(LearnSkill);
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SubId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<LearnSkill SkillId={SkillId} Level={Level} SubId={SubId}>";
	}
}
