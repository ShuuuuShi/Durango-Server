using MsgPack;

namespace Messages;

public struct EntityKey
{
	public string EntityId;

	public static void Pack(Packer packer, EntityKey val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static EntityKey Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EntityKey result = default(EntityKey);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<EntityKey EntityId=" + EntityId + ">";
	}
}
