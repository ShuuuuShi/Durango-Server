using MsgPack;

namespace Messages;

public struct RenamePet
{
	public const uint TypeCode = 804u;

	public string PetId;

	public string Name;

	public static void Pack(Packer packer, RenamePet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(804u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
	}

	public static RenamePet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RenamePet result = default(RenamePet);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RenamePet PetId=" + PetId + " Name=" + Name + ">";
	}
}
