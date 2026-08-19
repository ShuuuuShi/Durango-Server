using MsgPack;

namespace Messages;

public struct Resurrect
{
	public const uint TypeCode = 132u;

	public ulong EntityId;

	public float Score;

	public static void Pack(Packer packer, Resurrect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(132u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.Score);
	}

	public static Resurrect Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Resurrect result = default(Resurrect);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Score = ((MessagePackObject)(ref lastReadData2)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Resurrect EntityId={EntityId} Score={Score}>";
	}
}
