using MsgPack;

namespace Messages;

public struct ReinifyPet
{
	public const uint TypeCode = 74013u;

	public string PetId;

	public string ItemId;

	public static void Pack(Packer packer, ReinifyPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(74013u);
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
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
	}

	public static ReinifyPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReinifyPet result = default(ReinifyPet);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ReinifyPet PetId={PetId} ItemId={ItemId}>";
	}
}
