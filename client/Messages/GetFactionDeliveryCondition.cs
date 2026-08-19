using MsgPack;
using Shared.Faction;

namespace Messages;

public struct GetFactionDeliveryCondition
{
	public const uint TypeCode = 3612u;

	public PropKey Target;

	public FactionType FactionType;

	public static void Pack(Packer packer, GetFactionDeliveryCondition val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3612u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		PropKey.Pack(packer, val.Target);
		packer.Pack((int)val.FactionType);
	}

	public static GetFactionDeliveryCondition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetFactionDeliveryCondition result = default(GetFactionDeliveryCondition);
		result.Target = PropKey.Unpack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 101 < num)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetFactionDeliveryCondition Target={Target} FactionType={FactionType}>";
	}
}
