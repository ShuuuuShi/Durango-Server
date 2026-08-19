using MsgPack;

namespace Messages;

public struct GetMilestoneCandidate
{
	public const uint TypeCode = 800010u;

	public string PetId;

	public int MilestoneId;

	public static void Pack(Packer packer, GetMilestoneCandidate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(800010u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
		packer.Pack(val.MilestoneId);
	}

	public static GetMilestoneCandidate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetMilestoneCandidate result = default(GetMilestoneCandidate);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.MilestoneId = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GetMilestoneCandidate PetId={PetId} MilestoneId={MilestoneId}>";
	}
}
