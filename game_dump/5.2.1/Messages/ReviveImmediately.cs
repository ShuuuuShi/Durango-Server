using MsgPack;

namespace Messages;

public struct ReviveImmediately
{
	public const uint TypeCode = 210201u;

	public string VoucherId;

	public static void Pack(Packer packer, ReviveImmediately val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(210201u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.VoucherId == null)
		{
			packer.PackNull();
		}
		else if (val.VoucherId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VoucherId);
		}
	}

	public static ReviveImmediately Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReviveImmediately result = default(ReviveImmediately);
		if (unpacker.LastReadData.IsNil)
		{
			result.VoucherId = null;
		}
		else
		{
			string voucherId = unpacker.LastReadData.AsString();
			result.VoucherId = voucherId;
		}
		return result;
	}

	public override string ToString()
	{
		return "<ReviveImmediately VoucherId=" + VoucherId + ">";
	}
}
