using MsgPack;

namespace Messages;

public struct ExpGained
{
	public const uint TypeCode = 3707u;

	public int Exp;

	public int BonusExp;

	public static void Pack(Packer packer, ExpGained val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3707u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Exp);
		packer.Pack(val.BonusExp);
	}

	public static ExpGained Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ExpGained result = default(ExpGained);
		result.Exp = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.BonusExp = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ExpGained Exp={Exp} BonusExp={BonusExp}>";
	}
}
