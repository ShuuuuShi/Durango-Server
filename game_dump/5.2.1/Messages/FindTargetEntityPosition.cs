using MsgPack;
using Shared.Etc;

namespace Messages;

public struct FindTargetEntityPosition
{
	public const uint TypeCode = 3950u;

	public ushort EntityType;

	public ReasonFindTarget ReasonFindTarget;

	public static void Pack(Packer packer, FindTargetEntityPosition val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3950u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityType);
		packer.Pack((int)val.ReasonFindTarget);
	}

	public static FindTargetEntityPosition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FindTargetEntityPosition result = default(FindTargetEntityPosition);
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 1 || 1 < num)
		{
			result.ReasonFindTarget = ReasonFindTarget.Invalid;
		}
		else
		{
			result.ReasonFindTarget = (ReasonFindTarget)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FindTargetEntityPosition EntityType={EntityType} ReasonFindTarget={ReasonFindTarget}>";
	}
}
