using MsgPack;

namespace Messages;

public struct WalletUpdated
{
	public const uint TypeCode = 5198234u;

	public string EntityId;

	public Wallet Wallet;

	public static void Pack(Packer packer, WalletUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5198234u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		Wallet.Pack(packer, val.Wallet);
	}

	public static WalletUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WalletUpdated result = default(WalletUpdated);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Wallet = Wallet.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<WalletUpdated EntityId={EntityId} Wallet={Wallet}>";
	}
}
