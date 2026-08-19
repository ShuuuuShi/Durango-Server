using MsgPack;

namespace Messages;

public struct UsePetActiveSkill
{
	public const uint TypeCode = 800200u;

	public string SkillId;

	public static void Pack(Packer packer, UsePetActiveSkill val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(800200u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.SkillId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillId);
		}
	}

	public static UsePetActiveSkill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UsePetActiveSkill result = default(UsePetActiveSkill);
		result.SkillId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<UsePetActiveSkill SkillId=" + SkillId + ">";
	}
}
