using MsgPack;

namespace Messages;

public struct PetActiveSkillCanceled
{
	public const uint TypeCode = 800202u;

	public string SkillId;

	public static void Pack(Packer packer, PetActiveSkillCanceled val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(800202u);
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

	public static PetActiveSkillCanceled Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetActiveSkillCanceled result = default(PetActiveSkillCanceled);
		result.SkillId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<PetActiveSkillCanceled SkillId=" + SkillId + ">";
	}
}
