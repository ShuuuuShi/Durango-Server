using MsgPack;

namespace Messages;

public struct Auth
{
	public const uint TypeCode = 1u;

	public ulong EntityId;

	public string SessionToken;

	public string ClientVersion;

	public string DeviceModel;

	public static void Pack(Packer packer, Auth val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(1u);
		}
		else
		{
			packer.PackArrayHeader(4);
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
		if (val.ClientVersion == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClientVersion);
		}
		if (val.DeviceModel == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.DeviceModel);
		}
	}

	public static Auth Unpack(Unpacker unpacker)
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
		Auth result = default(Auth);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SessionToken = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ClientVersion = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.DeviceModel = ((MessagePackObject)(ref lastReadData4)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Auth EntityId={EntityId} SessionToken={SessionToken} ClientVersion={ClientVersion} DeviceModel={DeviceModel}>";
	}
}
