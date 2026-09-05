using MsgPack;
using Shared.Estate;

namespace Messages;

public struct ReturnToEstate
{
	public const uint TypeCode = 10190234u;

	public OwnerType OwnerType;

	public static void Pack(Packer packer, ReturnToEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(10190234u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.OwnerType);
	}

	public static ReturnToEstate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ReturnToEstate result = default(ReturnToEstate);
		if (num < 0 || 4 < num)
		{
			result.OwnerType = OwnerType.Invalid;
		}
		else
		{
			result.OwnerType = (OwnerType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ReturnToEstate OwnerType={OwnerType}>";
	}
}
