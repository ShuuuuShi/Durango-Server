using MsgPack;

namespace Messages;

public struct CompletedCraters
{
	public const uint TypeCode = 914u;

	public Point2[] Craters;

	public static void Pack(Packer packer, CompletedCraters val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(914u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Craters == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Craters.Length);
		for (int i = 0; i < val.Craters.Length; i++)
		{
			packer.PackArrayHeader(2);
			packer.Pack((ushort)val.Craters[i].x);
			packer.Pack((ushort)val.Craters[i].y);
		}
	}

	public static CompletedCraters Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		CompletedCraters result = default(CompletedCraters);
		result.Craters = new Point2[num];
		ushort num2 = default(ushort);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadUInt16(ref num2);
			result.Craters[i].x = num2;
			unpacker.ReadUInt16(ref num2);
			result.Craters[i].y = num2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CompletedCraters Craters={Craters}>";
	}
}
