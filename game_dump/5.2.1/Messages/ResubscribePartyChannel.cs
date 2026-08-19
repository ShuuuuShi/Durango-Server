using MsgPack;

namespace Messages;

public struct ResubscribePartyChannel
{
	public const uint TypeCode = 28u;

	public string PartyId;

	public static void Pack(Packer packer, ResubscribePartyChannel val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(28u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.PartyId == null)
		{
			packer.PackNull();
		}
		else if (val.PartyId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PartyId);
		}
	}

	public static ResubscribePartyChannel Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ResubscribePartyChannel result = default(ResubscribePartyChannel);
		if (unpacker.LastReadData.IsNil)
		{
			result.PartyId = null;
		}
		else
		{
			string partyId = unpacker.LastReadData.AsString();
			result.PartyId = partyId;
		}
		return result;
	}

	public override string ToString()
	{
		return "<ResubscribePartyChannel PartyId=" + PartyId + ">";
	}
}
