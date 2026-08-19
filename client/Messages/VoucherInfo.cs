using MsgPack;

namespace Messages;

public struct VoucherInfo
{
	public string VoucherId;

	public int Count;

	public static void Pack(Packer packer, VoucherInfo val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.VoucherId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VoucherId);
		}
		packer.Pack(val.Count);
	}

	public static VoucherInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		VoucherInfo result = default(VoucherInfo);
		result.VoucherId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<VoucherInfo VoucherId={VoucherId} Count={Count}>";
	}
}
