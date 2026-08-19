using MsgPack;

namespace Messages;

public struct CollectibleChanged
{
	public const uint TypeCode = 2417u;

	public string EntityId;

	public static void Pack(Packer packer, CollectibleChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2417u);
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

	public static CollectibleChanged Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CollectibleChanged result = default(CollectibleChanged);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CollectibleChanged EntityId={EntityId}>";
	}
}
