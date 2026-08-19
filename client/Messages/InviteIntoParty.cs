using MsgPack;

namespace Messages;

public struct InviteIntoParty
{
	public const uint TypeCode = 20004u;

	public string InviteeEntityId;

	public static void Pack(Packer packer, InviteIntoParty val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20004u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.InviteeEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.InviteeEntityId);
		}
	}

	public static InviteIntoParty Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InviteIntoParty result = default(InviteIntoParty);
		result.InviteeEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<InviteIntoParty InviteeEntityId={InviteeEntityId}>";
	}
}
