using MsgPack;

namespace Messages;

public struct ExploredPOIs
{
	public const uint TypeCode = 903u;

	public PointOfInterest[] POIs;

	public static void Pack(Packer packer, ExploredPOIs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(903u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.POIs == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.POIs.Length);
		for (int i = 0; i < val.POIs.Length; i++)
		{
			PointOfInterest.Pack(packer, val.POIs[i]);
		}
	}

	public static ExploredPOIs Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ExploredPOIs result = default(ExploredPOIs);
		result.POIs = new PointOfInterest[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref PointOfInterest reference = ref result.POIs[i];
			reference = PointOfInterest.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ExploredPOIs POIs={POIs}>";
	}
}
