using MsgPack;

namespace Messages;

public struct UntrainSkill
{
	public const uint TypeCode = 2049u;

	public string SkillId;

	public int Level;

	public string SubId;

	public string VoucherId;

	public static void Pack(Packer packer, UntrainSkill val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2049u);
		}
		else
		{
			packer.PackArrayHeader(4);
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
		if (val.VoucherId == null)
		{
			packer.PackNull();
		}
		else if (val.VoucherId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VoucherId);
		}
	}

	public static UntrainSkill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UntrainSkill result = default(UntrainSkill);
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SubId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.VoucherId = null;
		}
		else
		{
			string voucherId = unpacker.LastReadData.AsString();
			result.VoucherId = voucherId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<UntrainSkill SkillId={SkillId} Level={Level} SubId={SubId} VoucherId={VoucherId}>";
	}
}
