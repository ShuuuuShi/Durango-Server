using MsgPack;

namespace Messages;

public struct BiomeFatigue
{
	public float Velocity;

	public int ResistanceLevel;

	public int EquipsResistance;

	public static void Pack(Packer packer, BiomeFatigue val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.Velocity);
		packer.Pack(val.ResistanceLevel);
		packer.Pack(val.EquipsResistance);
	}

	public static BiomeFatigue Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BiomeFatigue result = default(BiomeFatigue);
		result.Velocity = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.ResistanceLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.EquipsResistance = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<BiomeFatigue Velocity={Velocity} ResistanceLevel={ResistanceLevel} EquipsResistance={EquipsResistance}>";
	}
}
