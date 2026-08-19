using MsgPack;

namespace Messages;

public struct RequestDumpedPersonalIsland
{
	public const uint TypeCode = 381922u;

	public string PlayerEntityId;

	public static void Pack(Packer packer, RequestDumpedPersonalIsland val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(381922u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.PlayerEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerEntityId);
		}
	}

	public static RequestDumpedPersonalIsland Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RequestDumpedPersonalIsland result = default(RequestDumpedPersonalIsland);
		result.PlayerEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RequestDumpedPersonalIsland PlayerEntityId=" + PlayerEntityId + ">";
	}
}
