using MsgPack;

namespace Messages;

public struct Feeding
{
	public const uint TypeCode = 805u;

	public ulong PetId;

	public ulong[] FoodIds;

	public static void Pack(Packer packer, Feeding val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(805u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.PetId);
		if (val.FoodIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.FoodIds.Length);
		for (int i = 0; i < val.FoodIds.Length; i++)
		{
			packer.Pack(val.FoodIds[i]);
		}
	}

	public static Feeding Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Feeding result = default(Feeding);
		result.PetId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.FoodIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] foodIds = result.FoodIds;
			int num2 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			foodIds[num2] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Feeding PetId={PetId} FoodIds={FoodIds}>";
	}
}
