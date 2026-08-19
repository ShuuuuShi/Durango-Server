using MsgPack;

namespace Messages;

public struct SendFactionSupportRequest
{
	public const uint TypeCode = 725982u;

	public string RequestId;

	public static void Pack(Packer packer, SendFactionSupportRequest val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(725982u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.RequestId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RequestId);
		}
	}

	public static SendFactionSupportRequest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SendFactionSupportRequest result = default(SendFactionSupportRequest);
		result.RequestId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SendFactionSupportRequest RequestId={RequestId}>";
	}
}
