using MsgPack;

namespace Messages;

public struct RejectPartyInvitation
{
	public const uint TypeCode = 20007u;

	public string InviteeEntityId;

	public static void Pack(Packer packer, RejectPartyInvitation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20007u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.InviteeEntityId == null)
		{
			packer.PackNull();
		}
		else if (val.InviteeEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.InviteeEntityId);
		}
	}

	public static RejectPartyInvitation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RejectPartyInvitation result = default(RejectPartyInvitation);
		if (unpacker.LastReadData.IsNil)
		{
			result.InviteeEntityId = null;
		}
		else
		{
			string inviteeEntityId = unpacker.LastReadData.AsString();
			result.InviteeEntityId = inviteeEntityId;
		}
		return result;
	}

	public override string ToString()
	{
		return "<RejectPartyInvitation InviteeEntityId=" + InviteeEntityId + ">";
	}
}
