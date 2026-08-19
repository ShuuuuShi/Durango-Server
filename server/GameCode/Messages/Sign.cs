using MsgPack;

namespace Messages;

public struct Sign
{
	public string EntityId;

	public string SessionToken;

	public static void Pack(Packer packer, Sign val, bool hint = false)
	{
		packer.PackArrayHeader(2);
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
	}

	public static Sign Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Sign result = default(Sign);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SessionToken = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Sign EntityId={EntityId} SessionToken={SessionToken}>";
	}
}
