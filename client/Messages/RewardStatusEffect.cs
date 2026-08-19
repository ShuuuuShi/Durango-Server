using MsgPack;

namespace Messages;

public struct RewardStatusEffect
{
	public string StatusEffectId;

	public int Level;

	public float Duration;

	public static void Pack(Packer packer, RewardStatusEffect val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.StatusEffectId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.StatusEffectId);
		}
		packer.Pack(val.Level);
		packer.Pack(val.Duration);
	}

	public static RewardStatusEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RewardStatusEffect result = default(RewardStatusEffect);
		result.StatusEffectId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RewardStatusEffect StatusEffectId={StatusEffectId} Level={Level} Duration={Duration}>";
	}
}
