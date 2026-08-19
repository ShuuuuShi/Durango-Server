using System.Collections.Generic;
using MsgPack;
using Shared.Social;

namespace Messages;

public struct SetSocialOptions
{
	public const uint TypeCode = 24002u;

	public Dictionary<SocialOptionType, bool> Options;

	public static void Pack(Packer packer, SetSocialOptions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(24002u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Options == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Options.Count);
		foreach (KeyValuePair<SocialOptionType, bool> option in val.Options)
		{
			packer.Pack((int)option.Key);
			packer.Pack(option.Value);
		}
	}

	public static SetSocialOptions Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SetSocialOptions result = default(SetSocialOptions);
		result.Options = new Dictionary<SocialOptionType, bool>(num, default(SocialOptionTypeComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			SocialOptionType key = ((num2 >= 0 && 0 >= num2) ? ((SocialOptionType)num2) : SocialOptionType.Invalid);
			unpacker.Read();
			bool value = unpacker.LastReadData.AsBoolean();
			result.Options.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetSocialOptions Options={Options}>";
	}
}
