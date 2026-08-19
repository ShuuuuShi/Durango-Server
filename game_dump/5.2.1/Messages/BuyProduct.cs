using MsgPack;

namespace Messages;

public struct BuyProduct
{
	public const uint TypeCode = 2071u;

	public string ProductId;

	public string[] PaymentIds;

	public static void Pack(Packer packer, BuyProduct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2071u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ProductId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ProductId);
		}
		if (val.PaymentIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.PaymentIds.Length);
		for (int i = 0; i < val.PaymentIds.Length; i++)
		{
			if (val.PaymentIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.PaymentIds[i]);
			}
		}
	}

	public static BuyProduct Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BuyProduct result = default(BuyProduct);
		result.ProductId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.PaymentIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.PaymentIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<BuyProduct ProductId={ProductId} PaymentIds={PaymentIds}>";
	}
}
