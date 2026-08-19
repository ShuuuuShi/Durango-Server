using MsgPack;

namespace Messages;

public struct Pets
{
	public const uint TypeCode = 29912234u;

	public Pet[] Data;

	public static void Pack(Packer packer, Pets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(29912234u);
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

	public static Pets Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Pets result = default(Pets);
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
		return $"<Pets Data={Data}>";
	}
}
