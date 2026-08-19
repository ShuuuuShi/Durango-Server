using MsgPack;
using Shared.Estate;

namespace Messages;

public struct DeclareEstate
{
	public const uint TypeCode = 2422u;

	public OwnerType OwnerType;

	public Point2 Cell;

	public static void Pack(Packer packer, DeclareEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2422u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.OwnerType);
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.Cell.x);
		packer.Pack((byte)val.Cell.y);
	}

	public static DeclareEstate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DeclareEstate result = default(DeclareEstate);
		if (num < 0 || 4 < num)
		{
			result.OwnerType = OwnerType.Invalid;
		}
		else
		{
			result.OwnerType = (OwnerType)num;
		}
		unpacker.Read();
		unpacker.ReadByte(out var result2);
		result.Cell.x = result2;
		unpacker.ReadByte(out result2);
		result.Cell.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<DeclareEstate OwnerType={OwnerType} Cell={Cell}>";
	}
}
