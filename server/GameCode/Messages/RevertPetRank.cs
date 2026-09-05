using MsgPack;

namespace Messages;

public struct RevertPetRank
{
	public const uint TypeCode = 74014u;

	public string PetId;

	public static void Pack(Packer packer, RevertPetRank val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(74014u);
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

	public static RevertPetRank Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RevertPetRank result = default(RevertPetRank);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RevertPetRank PetId={PetId}>";
	}
}
