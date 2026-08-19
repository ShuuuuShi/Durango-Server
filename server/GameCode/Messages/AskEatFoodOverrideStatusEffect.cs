using MsgPack;

namespace Messages;

public struct AskEatFoodOverrideStatusEffect
{
	public const uint TypeCode = 282001u;

	public string ItemId;

	public static void Pack(Packer packer, AskEatFoodOverrideStatusEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(282001u);
		}
		else
		{
			packer.PackArrayHeader(1);
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

	public static AskEatFoodOverrideStatusEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AskEatFoodOverrideStatusEffect result = default(AskEatFoodOverrideStatusEffect);
		result.ItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AskEatFoodOverrideStatusEffect ItemId={ItemId}>";
	}
}
