using MsgPack;

namespace Messages;

public struct GrazedPets
{
	public const uint TypeCode = 29912241u;

	public Pet[] Data;

	public static void Pack(Packer packer, GrazedPets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(29912241u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Data == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Data.Length);
		for (int i = 0; i < val.Data.Length; i++)
		{
			Pet.Pack(packer, val.Data[i]);
		}
	}

	public static GrazedPets Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GrazedPets result = default(GrazedPets);
		result.Data = new Pet[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Pet reference = ref result.Data[i];
			reference = Pet.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GrazedPets Data={Data}>";
	}
}
