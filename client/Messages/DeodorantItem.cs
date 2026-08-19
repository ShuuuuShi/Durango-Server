using MsgPack;

namespace Messages;

public struct DeodorantItem
{
	public const uint TypeCode = 55555u;

	public string StatusEffectId;

	public static void Pack(Packer packer, DeodorantItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(55555u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.StatusEffectId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.StatusEffectId);
		}
	}

	public static DeodorantItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DeodorantItem result = default(DeodorantItem);
		result.StatusEffectId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<DeodorantItem StatusEffectId={StatusEffectId}>";
	}
}
