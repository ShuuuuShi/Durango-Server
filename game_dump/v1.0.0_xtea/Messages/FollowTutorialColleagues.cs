using MsgPack;

namespace Messages;

public struct FollowTutorialColleagues
{
	public const uint TypeCode = 2419u;

	public ulong[] Colleagues;

	public static void Pack(Packer packer, FollowTutorialColleagues val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2419u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Colleagues == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Colleagues.Length);
		for (int i = 0; i < val.Colleagues.Length; i++)
		{
			packer.Pack(val.Colleagues[i]);
		}
	}

	public static FollowTutorialColleagues Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FollowTutorialColleagues result = default(FollowTutorialColleagues);
		result.Colleagues = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] colleagues = result.Colleagues;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			colleagues[num2] = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FollowTutorialColleagues Colleagues={Colleagues}>";
	}
}
