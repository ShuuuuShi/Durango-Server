using MsgPack;

namespace Messages;

public struct EntityDied
{
	public const uint TypeCode = 119u;

	public string EntityId;

	public double At;

	public static void Pack(Packer packer, EntityDied val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(119u);
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
		packer.Pack(val.At);
	}

	public static EntityDied Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EntityDied result = default(EntityDied);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.At = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<EntityDied EntityId={EntityId} At={At}>";
	}
}
