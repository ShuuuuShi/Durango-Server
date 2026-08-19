using MsgPack;

namespace Messages;

public struct CargoReceivers
{
	public const uint TypeCode = 3813u;

	public CargoReceiver? PrivateReceiver;

	public CargoReceiver? ClanReceiver;

	public int CostPerSize;

	public static void Pack(Packer packer, CargoReceivers val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3813u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (!val.PrivateReceiver.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			CargoReceiver.Pack(packer, val.PrivateReceiver.Value);
		}
		if (!val.ClanReceiver.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			CargoReceiver.Pack(packer, val.ClanReceiver.Value);
		}
		packer.Pack(val.CostPerSize);
	}

	public static CargoReceivers Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CargoReceivers result = default(CargoReceivers);
		if (unpacker.LastReadData.IsNil)
		{
			result.PrivateReceiver = null;
		}
		else
		{
			CargoReceiver value = CargoReceiver.Unpack(unpacker);
			result.PrivateReceiver = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ClanReceiver = null;
		}
		else
		{
			CargoReceiver value2 = CargoReceiver.Unpack(unpacker);
			result.ClanReceiver = value2;
		}
		unpacker.Read();
		result.CostPerSize = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<CargoReceivers PrivateReceiver={PrivateReceiver} ClanReceiver={ClanReceiver} CostPerSize={CostPerSize}>";
	}
}
