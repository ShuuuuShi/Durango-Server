using MsgPack;

namespace Messages;

public struct AcceptPetRank
{
	public const uint TypeCode = 74016u;

	public string PetId;

	public static void Pack(Packer packer, AcceptPetRank val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(74016u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
	}

	public static AcceptPetRank Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptPetRank result = default(AcceptPetRank);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<AcceptPetRank PetId=" + PetId + ">";
	}
}
