using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Actions
{
	public const uint TypeCode = 315u;

	public Dictionary<string, bool> ActionSetAvailabilities;

	public static void Pack(Packer packer, Actions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(315u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ActionSetAvailabilities == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.ActionSetAvailabilities.Count);
		foreach (KeyValuePair<string, bool> actionSetAvailability in val.ActionSetAvailabilities)
		{
			if (actionSetAvailability.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(actionSetAvailability.Key);
			}
			packer.Pack(actionSetAvailability.Value);
		}
	}

	public static Actions Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Actions result = default(Actions);
		result.ActionSetAvailabilities = new Dictionary<string, bool>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			bool value = ((MessagePackObject)(ref lastReadData3)).AsBoolean();
			result.ActionSetAvailabilities.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Actions ActionSetAvailabilities={ActionSetAvailabilities}>";
	}
}
