using MsgPack;

namespace Messages;

public struct AppearEntity
{
	public string EntityId;

	public ushort EntityType;

	public bool IsAlive;

	public static void Pack(Packer packer, AppearEntity val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EntityType);
		packer.Pack(val.IsAlive);
	}

	public static AppearEntity Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AppearEntity result = default(AppearEntity);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsAlive = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<AppearEntity EntityId={EntityId} EntityType={EntityType} IsAlive={IsAlive}>";
	}
}
