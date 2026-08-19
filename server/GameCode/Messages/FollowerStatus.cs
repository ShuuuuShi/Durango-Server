using MsgPack;

namespace Messages;

public struct FollowerStatus
{
	public const uint TypeCode = 2406u;

	public string EntityId;

	public bool Followed;

	public static void Pack(Packer packer, FollowerStatus val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2406u);
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
		packer.Pack(val.Followed);
	}

	public static FollowerStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FollowerStatus result = default(FollowerStatus);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Followed = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<FollowerStatus EntityId={EntityId} Followed={Followed}>";
	}
}
