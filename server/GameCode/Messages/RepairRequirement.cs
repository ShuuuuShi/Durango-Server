using MsgPack;

namespace Messages;

public struct RepairRequirement
{
	public string TagId;

	public int RepairPerformance;

	public static void Pack(Packer packer, RepairRequirement val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.TagId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TagId);
		}
		packer.Pack(val.RepairPerformance);
	}

	public static RepairRequirement Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RepairRequirement result = default(RepairRequirement);
		result.TagId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RepairPerformance = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RepairRequirement TagId={TagId} RepairPerformance={RepairPerformance}>";
	}
}
