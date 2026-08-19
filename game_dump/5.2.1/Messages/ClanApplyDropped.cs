using MsgPack;

namespace Messages;

public struct ClanApplyDropped
{
	public const uint TypeCode = 98274512u;

	public string ClanId;

	public string ClanName;

	public bool Expired;

	public static void Pack(Packer packer, ClanApplyDropped val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(98274512u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.ClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanId);
		}
		if (val.ClanName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanName);
		}
		packer.Pack(val.Expired);
	}

	public static ClanApplyDropped Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ClanApplyDropped result = default(ClanApplyDropped);
		result.ClanId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ClanName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Expired = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ClanApplyDropped ClanId={ClanId} ClanName={ClanName} Expired={Expired}>";
	}
}
