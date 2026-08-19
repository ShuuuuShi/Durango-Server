using MsgPack;

namespace Messages;

public struct EntityRevived
{
	public const uint TypeCode = 119119u;

	public string EntityId;

	public double At;

	public static void Pack(Packer packer, EntityRevived val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(119119u);
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

	public static EntityRevived Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EntityRevived result = default(EntityRevived);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.At = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<EntityRevived EntityId={EntityId} At={At}>";
	}
}
