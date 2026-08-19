using MsgPack;

namespace Messages;

public struct StatusEffects
{
	public const uint TypeCode = 317u;

	public ulong EntityId;

	public StatusEffect[] _StatusEffects;

	public static void Pack(Packer packer, StatusEffects val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(317u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		if (val._StatusEffects == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._StatusEffects.Length);
		for (int i = 0; i < val._StatusEffects.Length; i++)
		{
			StatusEffect.Pack(packer, val._StatusEffects[i]);
		}
	}

	public static StatusEffects Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		StatusEffects result = default(StatusEffects);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result._StatusEffects = new StatusEffect[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref StatusEffect reference = ref result._StatusEffects[i];
			reference = StatusEffect.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<StatusEffects EntityId={EntityId} _StatusEffects={_StatusEffects}>";
	}
}
