using MsgPack;

namespace Messages;

public struct RequestFriend
{
	public const uint TypeCode = 1451212u;

	public string EntityId;

	public static void Pack(Packer packer, RequestFriend val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451212u);
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

	public static RequestFriend Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RequestFriend result = default(RequestFriend);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RequestFriend EntityId=" + EntityId + ">";
	}
}
