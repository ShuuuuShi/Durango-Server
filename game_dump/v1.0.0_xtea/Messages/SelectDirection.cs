using MsgPack;
using Shared.Battle;

namespace Messages;

public struct SelectDirection
{
	public const uint TypeCode = 2428u;

	public DamageDirection Direction;

	public static void Pack(Packer packer, SelectDirection val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2428u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Direction);
	}

	public static SelectDirection Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SelectDirection result = default(SelectDirection);
		if (num < 0 || 3 < num)
		{
			result.Direction = DamageDirection.Invalid;
		}
		else
		{
			result.Direction = (DamageDirection)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SelectDirection Direction={Direction}>";
	}
}
