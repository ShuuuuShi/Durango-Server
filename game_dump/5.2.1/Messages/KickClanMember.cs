using MsgPack;

namespace Messages;

public struct KickClanMember
{
	public const uint TypeCode = 3661u;

	public string EntityId;

	public static void Pack(Packer packer, KickClanMember val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3661u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static KickClanMember Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		KickClanMember result = default(KickClanMember);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<KickClanMember EntityId=" + EntityId + ">";
	}
}
