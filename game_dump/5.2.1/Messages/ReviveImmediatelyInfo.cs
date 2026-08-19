using MsgPack;
using Shared.Economy;

namespace Messages;

public struct ReviveImmediatelyInfo
{
	public const uint TypeCode = 210102u;

	public int UsedCount;

	public Money TotalCost;

	public static void Pack(Packer packer, ReviveImmediatelyInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(210102u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.UsedCount);
		packer.PackArrayHeader(2);
		packer.Pack(val.TotalCost.Amount);
		packer.Pack((int)val.TotalCost.Currency);
	}

	public static ReviveImmediatelyInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReviveImmediatelyInfo result = default(ReviveImmediatelyInfo);
		result.UsedCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		unpacker.ReadInt32(out var result2);
		unpacker.ReadInt32(out var result3);
		result.TotalCost = new Money(result2, (Currency)result3);
		return result;
	}

	public override string ToString()
	{
		return $"<ReviveImmediatelyInfo UsedCount={UsedCount} TotalCost={TotalCost}>";
	}
}
