using MsgPack;

namespace Messages;

public struct Appliers
{
	public ulong[] ApplierEntityIds;

	public static void Pack(Packer packer, Appliers val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		if (val.ApplierEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ApplierEntityIds.Length);
		for (int i = 0; i < val.ApplierEntityIds.Length; i++)
		{
			packer.Pack(val.ApplierEntityIds[i]);
		}
	}

	public static Appliers Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Appliers result = default(Appliers);
		result.ApplierEntityIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] applierEntityIds = result.ApplierEntityIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			applierEntityIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Appliers ApplierEntityIds={ApplierEntityIds}>";
	}
}
