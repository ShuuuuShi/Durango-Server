using MsgPack;

namespace Messages;

public struct RedrawActiveSkill
{
	public const uint TypeCode = 800102u;

	public string PetId;

	public PetActiveSkill Skill;

	public bool WithVoucher;

	public static void Pack(Packer packer, RedrawActiveSkill val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(800102u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
		PetActiveSkill.Pack(packer, val.Skill);
		packer.Pack(val.WithVoucher);
	}

	public static RedrawActiveSkill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RedrawActiveSkill result = default(RedrawActiveSkill);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Skill = PetActiveSkill.Unpack(unpacker);
		unpacker.Read();
		result.WithVoucher = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<RedrawActiveSkill PetId={PetId} Skill={Skill} WithVoucher={WithVoucher}>";
	}
}
