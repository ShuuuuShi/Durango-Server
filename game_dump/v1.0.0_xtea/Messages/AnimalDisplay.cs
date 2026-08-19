using MsgPack;

namespace Messages;

public struct AnimalDisplay
{
	public const uint TypeCode = 2432u;

	public ulong EntityId;

	public static void Pack(Packer packer, AnimalDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2432u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static AnimalDisplay Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AnimalDisplay result = default(AnimalDisplay);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<AnimalDisplay EntityId={EntityId}>";
	}
}
