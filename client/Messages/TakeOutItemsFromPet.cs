using MsgPack;

namespace Messages;

public struct TakeOutItemsFromPet
{
	public const uint TypeCode = 807u;

	public string PetId;

	public string[] ItemIds;

	public static void Pack(Packer packer, TakeOutItemsFromPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(807u);
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
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemIds.Length);
		for (int i = 0; i < val.ItemIds.Length; i++)
		{
			if (val.ItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemIds[i]);
			}
		}
	}

	public static TakeOutItemsFromPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TakeOutItemsFromPet result = default(TakeOutItemsFromPet);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TakeOutItemsFromPet PetId={PetId} ItemIds={ItemIds}>";
	}
}
