using MsgPack;

namespace Messages;

public struct ExpandEstate
{
	public const uint TypeCode = 2421u;

	public string EstateId;

	public Point2 Cell;

	public static void Pack(Packer packer, ExpandEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2421u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.Cell.x);
		packer.Pack((byte)val.Cell.y);
	}

	public static ExpandEstate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ExpandEstate result = default(ExpandEstate);
		result.EstateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadByte(out var result2);
		result.Cell.x = result2;
		unpacker.ReadByte(out result2);
		result.Cell.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<ExpandEstate EstateId={EstateId} Cell={Cell}>";
	}
}
