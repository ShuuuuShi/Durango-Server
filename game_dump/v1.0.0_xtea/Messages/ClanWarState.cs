using MsgPack;

namespace Messages;

public struct ClanWarState
{
	public const uint TypeCode = 3674u;

	public EnemyClan[] EnemyClans;

	public static void Pack(Packer packer, ClanWarState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3674u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EnemyClans == null)
		{
			packer.PackNull();
			return;
		}
		if (val.EnemyClans == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EnemyClans.Length);
		for (int i = 0; i < val.EnemyClans.Length; i++)
		{
			EnemyClan.Pack(packer, val.EnemyClans[i]);
		}
	}

	public static ClanWarState Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ClanWarState result = default(ClanWarState);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.EnemyClans = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			EnemyClan[] array = new EnemyClan[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				ref EnemyClan reference = ref array[i];
				reference = EnemyClan.Unpack(unpacker);
			}
			result.EnemyClans = array;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ClanWarState EnemyClans={EnemyClans}>";
	}
}
