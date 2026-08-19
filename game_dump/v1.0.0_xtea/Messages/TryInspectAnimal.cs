using MsgPack;

namespace Messages;

public struct TryInspectAnimal
{
	public const uint TypeCode = 3602u;

	public ulong EntityId;

	public static void Pack(Packer packer, TryInspectAnimal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3602u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static TryInspectAnimal Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TryInspectAnimal result = default(TryInspectAnimal);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<TryInspectAnimal EntityId={EntityId}>";
	}
}
