using System.Collections.Generic;
using MsgPack;
using Shared.Chat;

namespace Messages;

public struct ToggleClanNotification
{
	public const uint TypeCode = 4025u;

	public Dictionary<ChannelType, bool> ChannelNotificationsEnabled;

	public static void Pack(Packer packer, ToggleClanNotification val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4025u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ChannelNotificationsEnabled == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.ChannelNotificationsEnabled.Count);
		foreach (KeyValuePair<ChannelType, bool> item in val.ChannelNotificationsEnabled)
		{
			packer.Pack((int)item.Key);
			packer.Pack(item.Value);
		}
	}

	public static ToggleClanNotification Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ToggleClanNotification result = default(ToggleClanNotification);
		result.ChannelNotificationsEnabled = new Dictionary<ChannelType, bool>(num, default(ChannelTypeComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			ChannelType key = ((num2 >= 0 && 6 >= num2) ? ((ChannelType)num2) : ChannelType.Invalid);
			unpacker.Read();
			bool value = unpacker.LastReadData.AsBoolean();
			result.ChannelNotificationsEnabled.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ToggleClanNotification ChannelNotificationsEnabled={ChannelNotificationsEnabled}>";
	}
}
