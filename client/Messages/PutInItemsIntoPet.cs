using MsgPack;

namespace Messages;

public struct PutInItemsIntoPet
{
	public const uint TypeCode = 806u;

	public string PetId;

	public string[] ItemIds;

	public static void Pack(Packer packer, PutInItemsIntoPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(806u);
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

	public static PutInItemsIntoPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PutInItemsIntoPet result = default(PutInItemsIntoPet);
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
		return $"<PutInItemsIntoPet PetId={PetId} ItemIds={ItemIds}>";
	}
}
