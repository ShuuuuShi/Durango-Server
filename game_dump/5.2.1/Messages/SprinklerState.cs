using MsgPack;

namespace Messages;

public struct SprinklerState
{
	public int ChargeCapacity;

	public int ChargeCount;

	public int FertilizerInventory;

	public int FertilizerCount;

	public int ChargeDelay;

	public int SprinkleWaterCount;

	public static void Pack(Packer packer, SprinklerState val, bool hint = false)
	{
		packer.PackArrayHeader(6);
		packer.Pack(val.ChargeCapacity);
		packer.Pack(val.ChargeCount);
		packer.Pack(val.FertilizerInventory);
		packer.Pack(val.FertilizerCount);
		packer.Pack(val.ChargeDelay);
		packer.Pack(val.SprinkleWaterCount);
	}

	public static SprinklerState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SprinklerState result = default(SprinklerState);
		result.ChargeCapacity = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ChargeCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.FertilizerInventory = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.FertilizerCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ChargeDelay = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.SprinkleWaterCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SprinklerState ChargeCapacity={ChargeCapacity} ChargeCount={ChargeCount} FertilizerInventory={FertilizerInventory} FertilizerCount={FertilizerCount} ChargeDelay={ChargeDelay} SprinkleWaterCount={SprinkleWaterCount}>";
	}
}
