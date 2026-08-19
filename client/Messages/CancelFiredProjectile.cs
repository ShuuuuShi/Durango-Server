using MsgPack;

namespace Messages;

public struct CancelFiredProjectile
{
	public const uint TypeCode = 312098u;

	public string EntityId;

	public static void Pack(Packer packer, CancelFiredProjectile val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(312098u);
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

	public static CancelFiredProjectile Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CancelFiredProjectile result = default(CancelFiredProjectile);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CancelFiredProjectile EntityId={EntityId}>";
	}
}
