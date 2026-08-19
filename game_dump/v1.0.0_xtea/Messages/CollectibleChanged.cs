using MsgPack;

namespace Messages;

public struct CollectibleChanged
{
	public const uint TypeCode = 2417u;

	public ulong EntityId;

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
		packer.Pack(val.EntityId);
	}

	public static CollectibleChanged Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		CollectibleChanged result = default(CollectibleChanged);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<CollectibleChanged EntityId={EntityId}>";
	}
}
