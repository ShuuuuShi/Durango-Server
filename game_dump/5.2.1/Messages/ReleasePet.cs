using MsgPack;

namespace Messages;

public struct ReleasePet
{
	public const uint TypeCode = 74012u;

	public string PetId;

	public static void Pack(Packer packer, ReleasePet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(74012u);
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

	public static ReleasePet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReleasePet result = default(ReleasePet);
		result.PetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<ReleasePet PetId=" + PetId + ">";
	}
}
