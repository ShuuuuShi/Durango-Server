using MsgPack;

namespace Messages;

public struct ResurrectPet
{
	public const uint TypeCode = 239187u;

	public string PetId;

	public static void Pack(Packer packer, ResurrectPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(239187u);
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

	public static ResurrectPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ResurrectPet result = default(ResurrectPet);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ResurrectPet PetId={PetId}>";
	}
}
