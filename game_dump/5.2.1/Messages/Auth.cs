using MsgPack;

namespace Messages;

public struct Auth
{
	public const uint TypeCode = 1u;

	public string EntityId;

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
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
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
		unpacker.Read();
		Auth result = default(Auth);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SessionToken = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ClientVersion = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.DeviceModel = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<Auth EntityId=" + EntityId + " SessionToken=" + SessionToken + " ClientVersion=" + ClientVersion + " DeviceModel=" + DeviceModel + ">";
	}
}
