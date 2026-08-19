using MsgPack;

namespace Messages;

public struct Home
{
	public int Capacity;

	public ulong[] ResidentEntityIds;

	public static void Pack(Packer packer, Home val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.Capacity);
		if (val.ResidentEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ResidentEntityIds.Length);
		for (int i = 0; i < val.ResidentEntityIds.Length; i++)
		{
			packer.Pack(val.ResidentEntityIds[i]);
		}
	}

	public static Home Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Home result = default(Home);
		result.Capacity = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.ResidentEntityIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] residentEntityIds = result.ResidentEntityIds;
			int num2 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			residentEntityIds[num2] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Home Capacity={Capacity} ResidentEntityIds={ResidentEntityIds}>";
	}
}
