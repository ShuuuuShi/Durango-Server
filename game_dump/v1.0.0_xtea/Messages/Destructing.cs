using MsgPack;

namespace Messages;

public struct Destructing
{
	public const uint TypeCode = 2007u;

	public float Duration;

	public byte ToolType;

	public static void Pack(Packer packer, Destructing val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2007u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Duration);
		packer.Pack(val.ToolType);
	}

	public static Destructing Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Destructing result = default(Destructing);
		result.Duration = ((MessagePackObject)(ref lastReadData)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ToolType = ((MessagePackObject)(ref lastReadData2)).AsByte();
		return result;
	}

	public override string ToString()
	{
		return $"<Destructing Duration={Duration} ToolType={ToolType}>";
	}
}
