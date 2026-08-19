using MsgPack;

namespace Messages;

public struct EnterBattle
{
	public const uint TypeCode = 3495u;

	public ulong EntityId;

	public static void Pack(Packer packer, EnterBattle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3495u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static EnterBattle Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		EnterBattle result = default(EnterBattle);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<EnterBattle EntityId={EntityId}>";
	}
}
