using MsgPack;

namespace Messages;

public struct Following
{
	public const uint TypeCode = 2403u;

	public ulong[] FollowingEntityIds;

	public static void Pack(Packer packer, Following val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2403u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.FollowingEntityIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.FollowingEntityIds.Length);
		for (int i = 0; i < val.FollowingEntityIds.Length; i++)
		{
			packer.Pack(val.FollowingEntityIds[i]);
		}
	}

	public static Following Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Following result = default(Following);
		result.FollowingEntityIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] followingEntityIds = result.FollowingEntityIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			followingEntityIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Following FollowingEntityIds={FollowingEntityIds}>";
	}
}
