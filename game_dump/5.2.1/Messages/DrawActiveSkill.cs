using MsgPack;

namespace Messages;

public struct DrawActiveSkill
{
	public const uint TypeCode = 800101u;

	public string PetId;

	public static void Pack(Packer packer, DrawActiveSkill val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(800101u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
	}

	public static DrawActiveSkill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DrawActiveSkill result = default(DrawActiveSkill);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<DrawActiveSkill PetId=" + PetId + ">";
	}
}
