using MsgPack;

namespace Messages;

public struct SetCargoWarpholeTaxRate
{
	public const uint TypeCode = 3816u;

	public float Rate;

	public static void Pack(Packer packer, SetCargoWarpholeTaxRate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3816u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Rate);
	}

	public static SetCargoWarpholeTaxRate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetCargoWarpholeTaxRate result = default(SetCargoWarpholeTaxRate);
		result.Rate = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<SetCargoWarpholeTaxRate Rate={Rate}>";
	}
}
