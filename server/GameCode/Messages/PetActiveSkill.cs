using MsgPack;
using Shared.Pet;

namespace Messages;

public struct PetActiveSkill
{
	public string SkillId;

	public SkillRank Rank;

	public static void Pack(Packer packer, PetActiveSkill val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.SkillId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillId);
		}
		packer.Pack((int)val.Rank);
	}

	public static PetActiveSkill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetActiveSkill result = default(PetActiveSkill);
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 10 || 14 < num)
		{
			result.Rank = SkillRank.Invalid;
		}
		else
		{
			result.Rank = (SkillRank)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PetActiveSkill SkillId={SkillId} Rank={Rank}>";
	}
}
