using MsgPack;

namespace Messages;

public struct SkillNeeded
{
	public const uint TypeCode = 2449u;

	public string SkillId;

	public int Level;

	public string SubId;

	public static void Pack(Packer packer, SkillNeeded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2449u);
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

	public static SkillNeeded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkillNeeded result = default(SkillNeeded);
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SubId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SkillNeeded SkillId={SkillId} Level={Level} SubId={SubId}>";
	}
}
