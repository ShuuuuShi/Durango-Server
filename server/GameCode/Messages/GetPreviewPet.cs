using MsgPack;
using Shared.Animal;

namespace Messages;

public struct GetPreviewPet
{
	public const uint TypeCode = 181120u;

	public ushort PetEntityType;

	public PetRank Rank;

	public int Level;

	public static void Pack(Packer packer, GetPreviewPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(181120u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.PetEntityType);
		packer.Pack((int)val.Rank);
		packer.Pack(val.Level);
	}

	public static GetPreviewPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetPreviewPet result = default(GetPreviewPet);
		result.PetEntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 10 || 14 < num)
		{
			result.Rank = PetRank.Invalid;
		}
		else
		{
			result.Rank = (PetRank)num;
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GetPreviewPet PetEntityType={PetEntityType} Rank={Rank} Level={Level}>";
	}
}
