using MsgPack;

namespace Messages;

public struct DiscoverAnimal
{
	public const uint TypeCode = 5002u;

	public string EntityId;

	public static void Pack(Packer packer, DiscoverAnimal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5002u);
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

	public static DiscoverAnimal Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DiscoverAnimal result = default(DiscoverAnimal);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<DiscoverAnimal EntityId=" + EntityId + ">";
	}
}
