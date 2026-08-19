using MsgPack;

namespace Messages;

public struct GetPetInventory
{
	public const uint TypeCode = 49823u;

	public string EntityId;

	public static void Pack(Packer packer, GetPetInventory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(49823u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static GetPetInventory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetPetInventory result = default(GetPetInventory);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetPetInventory EntityId={EntityId}>";
	}
}
