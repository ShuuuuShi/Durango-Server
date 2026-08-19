using MsgPack;

namespace Messages;

public struct FeedingSuccess
{
	public const uint TypeCode = 813u;

	public string PetId;

	public static void Pack(Packer packer, FeedingSuccess val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(813u);
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

	public static FeedingSuccess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FeedingSuccess result = default(FeedingSuccess);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<FeedingSuccess PetId=" + PetId + ">";
	}
}
