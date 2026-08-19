using MsgPack;

namespace Messages;

public struct Tune
{
	public const uint TypeCode = 2400u;

	public ulong EntityId;

	public string SessionToken;

	public double SyncedAt;

	public static void Pack(Packer packer, Tune val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2400u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		if (val.SessionToken == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SessionToken);
		}
		packer.Pack(val.SyncedAt);
	}

	public static Tune Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Tune result = default(Tune);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SessionToken = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.SyncedAt = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Tune EntityId={EntityId} SessionToken={SessionToken} SyncedAt={SyncedAt}>";
	}
}
