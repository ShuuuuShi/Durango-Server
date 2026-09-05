using MsgPack;

namespace Messages;

public struct PetsInfo
{
	public const uint TypeCode = 29912233u;

	public Pets Pets;

	public Pets GrazedPets;

	public int GrazableCount;

	public static void Pack(Packer packer, PetsInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(29912233u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		Pets.Pack(packer, val.Pets);
		Pets.Pack(packer, val.GrazedPets);
		packer.Pack(val.GrazableCount);
	}

	public static PetsInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetsInfo result = default(PetsInfo);
		result.Pets = Pets.Unpack(unpacker);
		unpacker.Read();
		result.GrazedPets = Pets.Unpack(unpacker);
		unpacker.Read();
		result.GrazableCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<PetsInfo Pets={Pets} GrazedPets={GrazedPets} GrazableCount={GrazableCount}>";
	}
}
