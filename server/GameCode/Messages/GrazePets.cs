using MsgPack;

namespace Messages;

public struct GrazePets
{
	public const uint TypeCode = 800000u;

	public string[] PetIdsToGraze;

	public static void Pack(Packer packer, GrazePets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(800000u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.PetIdsToGraze == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.PetIdsToGraze.Length);
		for (int i = 0; i < val.PetIdsToGraze.Length; i++)
		{
			if (val.PetIdsToGraze[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.PetIdsToGraze[i]);
			}
		}
	}

	public static GrazePets Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GrazePets result = default(GrazePets);
		result.PetIdsToGraze = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.PetIdsToGraze[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("<GrazePets PetIdsToGraze={0}>", PetIdsToGraze);
	}
}
