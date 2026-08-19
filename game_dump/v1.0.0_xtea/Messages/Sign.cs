using MsgPack;

namespace Messages;

public struct Sign
{
	public ulong EntityId;

	public string SessionToken;

	public static void Pack(Packer packer, Sign val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.EntityId);
		if (val.SessionToken == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SessionToken);
		}
	}

	public static Sign Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Sign result = default(Sign);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SessionToken = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Sign EntityId={EntityId} SessionToken={SessionToken}>";
	}
}
