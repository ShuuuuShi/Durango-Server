using MsgPack;

namespace Messages;

public struct PlayerCPR
{
	public const uint TypeCode = 1001u;

	public double SentAt;

	public ulong RescuerId;

	public ulong TargetId;

	public string State;

	public static void Pack(Packer packer, PlayerCPR val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(1001u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.SentAt);
		packer.Pack(val.RescuerId);
		packer.Pack(val.TargetId);
		if (val.State == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.State);
		}
	}

	public static PlayerCPR Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerCPR result = default(PlayerCPR);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.RescuerId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.TargetId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.State = ((MessagePackObject)(ref lastReadData4)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerCPR SentAt={SentAt} RescuerId={RescuerId} TargetId={TargetId} State={State}>";
	}
}
