using MsgPack;

namespace Messages;

public struct UserFirstPurchaseHistory
{
	public const uint TypeCode = 856721u;

	public UserFirstPurchase[] _UserFirstPurchaseHistory;

	public static void Pack(Packer packer, UserFirstPurchaseHistory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(856721u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._UserFirstPurchaseHistory == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._UserFirstPurchaseHistory.Length);
		for (int i = 0; i < val._UserFirstPurchaseHistory.Length; i++)
		{
			UserFirstPurchase.Pack(packer, val._UserFirstPurchaseHistory[i]);
		}
	}

	public static UserFirstPurchaseHistory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		UserFirstPurchaseHistory result = default(UserFirstPurchaseHistory);
		result._UserFirstPurchaseHistory = new UserFirstPurchase[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref UserFirstPurchase reference = ref result._UserFirstPurchaseHistory[i];
			reference = UserFirstPurchase.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<UserFirstPurchaseHistory _UserFirstPurchaseHistory={_UserFirstPurchaseHistory}>";
	}
}
