using MsgPack;

namespace Messages;

public struct GetSkills
{
	public const uint TypeCode = 2047u;

	public static void Pack(Packer packer, GetSkills val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2047u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSkills Unpack(Unpacker unpacker)
	{
		GetSkills result = default(GetSkills);
		return result;
	}

	public override string ToString()
	{
		return "<GetSkills>";
	}
}
