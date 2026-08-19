using MsgPack;

namespace Messages;

public struct RemoveFriend
{
	public const uint TypeCode = 1451217u;

	public string EntityId;

	public static void Pack(Packer packer, RemoveFriend val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451217u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static RemoveFriend Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemoveFriend result = default(RemoveFriend);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RemoveFriend EntityId={EntityId}>";
	}
}
