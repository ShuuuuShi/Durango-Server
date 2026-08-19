using MsgPack;

namespace Messages;

public struct Postprocess
{
	public double StartedAt;

	public double EndsAt;

	public ulong[] Helpers;

	public int MaxHelperCount;

	public static void Pack(Packer packer, Postprocess val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.StartedAt);
		packer.Pack(val.EndsAt);
		if (val.Helpers == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Helpers.Length);
			for (int i = 0; i < val.Helpers.Length; i++)
			{
				packer.Pack(val.Helpers[i]);
			}
		}
		packer.Pack(val.MaxHelperCount);
	}

	public static Postprocess Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Postprocess result = default(Postprocess);
		result.StartedAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EndsAt = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Helpers = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] helpers = result.Helpers;
			int num2 = i;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			helpers[num2] = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.MaxHelperCount = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Postprocess StartedAt={StartedAt} EndsAt={EndsAt} Helpers={Helpers} MaxHelperCount={MaxHelperCount}>";
	}
}
