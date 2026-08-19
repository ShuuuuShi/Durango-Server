using MsgPack;

namespace Messages;

public struct Feeding
{
	public const uint TypeCode = 805u;

	public string PetId;

	public string[] FoodIds;

	public static void Pack(Packer packer, Feeding val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(805u);
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
		if (val.FoodIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.FoodIds.Length);
		for (int i = 0; i < val.FoodIds.Length; i++)
		{
			if (val.FoodIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.FoodIds[i]);
			}
		}
	}

	public static Feeding Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Feeding result = default(Feeding);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.FoodIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.FoodIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Feeding PetId={PetId} FoodIds={FoodIds}>";
	}
}
