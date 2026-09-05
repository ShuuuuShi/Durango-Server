using MsgPack;

namespace Messages;

public struct EntityRescueRequested
{
	public const uint TypeCode = 119911u;

	public string EntityId;

	public double At;

	public static void Pack(Packer packer, EntityRescueRequested val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(119911u);
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

	public static EntityRescueRequested Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EntityRescueRequested result = default(EntityRescueRequested);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.At = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<EntityRescueRequested EntityId={EntityId} At={At}>";
	}
}
