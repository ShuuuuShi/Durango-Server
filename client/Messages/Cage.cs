using MsgPack;

namespace Messages;

public struct Cage
{
	public const uint TypeCode = 811u;

	public byte Size;

	public byte RemainSize;

	public Pets Pets;

	public static void Pack(Packer packer, Cage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(811u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.Size);
		packer.Pack(val.RemainSize);
		Pets.Pack(packer, val.Pets);
	}

	public static Cage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Cage result = default(Cage);
		result.Size = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.RemainSize = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.Pets = Pets.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Cage Size={Size} RemainSize={RemainSize} Pets={Pets}>";
	}
}
