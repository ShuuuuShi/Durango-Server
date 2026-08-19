using MsgPack;

namespace Messages;

public struct Unfollow
{
	public const uint TypeCode = 2410u;

	public string EntityId;

	public static void Pack(Packer packer, Unfollow val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2410u);
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

	public static Unfollow Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Unfollow result = default(Unfollow);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Unfollow EntityId={EntityId}>";
	}
}
