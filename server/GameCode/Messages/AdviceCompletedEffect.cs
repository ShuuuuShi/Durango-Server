using MsgPack;
using Shared.System;

namespace Messages;

public struct AdviceCompletedEffect
{
	public const uint TypeCode = 2083u;

	public Shared.System.RewardEffect Type;

	public string TitleId;

	public static void Pack(Packer packer, AdviceCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2083u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		if (val.TitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TitleId);
		}
	}

	public static AdviceCompletedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AdviceCompletedEffect result = default(AdviceCompletedEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.TitleId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AdviceCompletedEffect Type={Type} TitleId={TitleId}>";
	}
}
