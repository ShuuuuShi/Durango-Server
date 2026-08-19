using MsgPack;

namespace Messages;

public struct StartTimer
{
	public const uint TypeCode = 124u;

	public ulong EntityId;

	public string Subject;

	public float Current;

	public float Time;

	public float AdditionalTime;

	public static void Pack(Packer packer, StartTimer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(124u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack(val.EntityId);
		if (val.Subject == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Subject);
		}
		packer.Pack(val.Current);
		packer.Pack(val.Time);
		packer.Pack(val.AdditionalTime);
	}

	public static StartTimer Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		StartTimer result = default(StartTimer);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Subject = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Current = ((MessagePackObject)(ref lastReadData3)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Time = ((MessagePackObject)(ref lastReadData4)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.AdditionalTime = ((MessagePackObject)(ref lastReadData5)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<StartTimer EntityId={EntityId} Subject={Subject} Current={Current} Time={Time} AdditionalTime={AdditionalTime}>";
	}
}
