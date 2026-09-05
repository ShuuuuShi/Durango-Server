using MsgPack;

namespace Messages;

public struct AcceptTENCoupon
{
	public const uint TypeCode = 2345690u;

	public string CouponNum;

	public string ToyToken;

	public static void Pack(Packer packer, AcceptTENCoupon val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2345690u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.CouponNum == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CouponNum);
		}
		if (val.ToyToken == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ToyToken);
		}
	}

	public static AcceptTENCoupon Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptTENCoupon result = default(AcceptTENCoupon);
		result.CouponNum = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ToyToken = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptTENCoupon CouponNum={CouponNum} ToyToken={ToyToken}>";
	}
}
