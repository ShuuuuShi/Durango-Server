using MsgPack;

namespace Messages;

public struct KickVisitor
{
	public const uint TypeCode = 20424u;

	public string EntityId;

	public bool Silent;

	public static void Pack(Packer packer, KickVisitor val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(20424u);
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
		packer.Pack(val.Silent);
	}

	public static KickVisitor Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		KickVisitor result = default(KickVisitor);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Silent = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<KickVisitor EntityId={EntityId} Silent={Silent}>";
	}
}
