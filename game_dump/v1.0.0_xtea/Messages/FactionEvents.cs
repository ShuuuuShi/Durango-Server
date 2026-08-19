using MsgPack;

namespace Messages;

public struct FactionEvents
{
	public const uint TypeCode = 3622u;

	public FactionEvent[] Events;

	public static void Pack(Packer packer, FactionEvents val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3622u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Events == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Events.Length);
		for (int i = 0; i < val.Events.Length; i++)
		{
			FactionEvent.Pack(packer, val.Events[i]);
		}
	}

	public static FactionEvents Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionEvents result = default(FactionEvents);
		result.Events = new FactionEvent[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref FactionEvent reference = ref result.Events[i];
			reference = FactionEvent.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FactionEvents Events={Events}>";
	}
}
