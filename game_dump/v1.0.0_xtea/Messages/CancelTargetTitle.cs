using MsgPack;

namespace Messages;

public struct CancelTargetTitle
{
	public const uint TypeCode = 2504u;

	public static void Pack(Packer packer, CancelTargetTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2504u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static CancelTargetTitle Unpack(Unpacker unpacker)
	{
		CancelTargetTitle result = default(CancelTargetTitle);
		return result;
	}

	public override string ToString()
	{
		return "<CancelTargetTitle>";
	}
}
