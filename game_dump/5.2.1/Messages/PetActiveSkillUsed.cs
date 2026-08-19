using MsgPack;

namespace Messages;

public struct PetActiveSkillUsed
{
	public const uint TypeCode = 800201u;

	public string SkillId;

	public string ClipName;

	public static void Pack(Packer packer, PetActiveSkillUsed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(800201u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.SkillId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillId);
		}
		if (val.ClipName == null)
		{
			packer.PackNull();
		}
		else if (val.ClipName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClipName);
		}
	}

	public static PetActiveSkillUsed Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetActiveSkillUsed result = default(PetActiveSkillUsed);
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ClipName = null;
		}
		else
		{
			string clipName = unpacker.LastReadData.AsString();
			result.ClipName = clipName;
		}
		return result;
	}

	public override string ToString()
	{
		return "<PetActiveSkillUsed SkillId=" + SkillId + " ClipName=" + ClipName + ">";
	}
}
