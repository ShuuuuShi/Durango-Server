using MsgPack;

namespace Messages;

public struct EngagementAgreementChanged
{
	public const uint TypeCode = 1444250u;

	public bool Agreed;

	public static void Pack(Packer packer, EngagementAgreementChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1444250u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Agreed);
	}

	public static EngagementAgreementChanged Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EngagementAgreementChanged result = default(EngagementAgreementChanged);
		result.Agreed = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<EngagementAgreementChanged Agreed={Agreed}>";
	}
}
