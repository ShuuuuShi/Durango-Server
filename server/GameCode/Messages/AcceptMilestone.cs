using MsgPack;

namespace Messages;

public struct AcceptMilestone
{
	public const uint TypeCode = 800015u;

	public string PetId;

	public static void Pack(Packer packer, AcceptMilestone val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(800015u);
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

	public static AcceptMilestone Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptMilestone result = default(AcceptMilestone);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptMilestone PetId={PetId}>";
	}
}
