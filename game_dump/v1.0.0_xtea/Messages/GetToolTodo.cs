using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct GetToolTodo
{
	public const uint TypeCode = 3524u;

	public Dictionary<string, int> RequiredTags;

	public static void Pack(Packer packer, GetToolTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3524u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.RequiredTags == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.RequiredTags.Count);
		foreach (KeyValuePair<string, int> requiredTag in val.RequiredTags)
		{
			if (requiredTag.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(requiredTag.Key);
			}
			packer.Pack(requiredTag.Value);
		}
	}

	public static GetToolTodo Unpack(Unpacker unpacker)
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
		GetToolTodo result = default(GetToolTodo);
		result.RequiredTags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			result.RequiredTags.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetToolTodo RequiredTags={RequiredTags}>";
	}
}
