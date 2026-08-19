using MsgPack;
using Shared.System;

namespace Messages;

public struct AttachmentReceivedEffect
{
	public const uint TypeCode = 20841u;

	public Shared.System.RewardEffect Type;

	public static void Pack(Packer packer, AttachmentReceivedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20841u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Type);
	}

	public static AttachmentReceivedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AttachmentReceivedEffect result = default(AttachmentReceivedEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AttachmentReceivedEffect Type={Type}>";
	}
}
