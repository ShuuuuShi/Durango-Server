using MsgPack;

namespace Messages;

public struct BuildEstimation
{
	public const uint TypeCode = 2415u;

	public int Level;

	public float Durability;

	public static void Pack(Packer packer, BuildEstimation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2415u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Level);
		packer.Pack(val.Durability);
	}

	public static BuildEstimation Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		BuildEstimation result = default(BuildEstimation);
		result.Level = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Durability = ((MessagePackObject)(ref lastReadData2)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<BuildEstimation Level={Level} Durability={Durability}>";
	}
}
