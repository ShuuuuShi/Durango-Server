using MsgPack;

namespace Messages;

public struct FollowingStatus
{
	public const uint TypeCode = 2407u;

	public string EntityId;

	public bool Online;

	public static void Pack(Packer packer, FollowingStatus val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2407u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.Online);
	}

	public static FollowingStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FollowingStatus result = default(FollowingStatus);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Online = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<FollowingStatus EntityId={EntityId} Online={Online}>";
	}
}
