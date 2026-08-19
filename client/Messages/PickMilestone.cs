using MsgPack;

namespace Messages;

public struct PickMilestone
{
	public const uint TypeCode = 800012u;

	public string PetId;

	public static void Pack(Packer packer, PickMilestone val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(800012u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
	}

	public static PickMilestone Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PickMilestone result = default(PickMilestone);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PickMilestone PetId={PetId}>";
	}
}
