using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct UpdateAdvisorProgress
{
	public const uint TypeCode = 3505u;

	public KeyValuePair<int, int> Progress;

	public static void Pack(Packer packer, UpdateAdvisorProgress val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3505u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Progress.Key);
		packer.Pack(val.Progress.Value);
	}

	public static UpdateAdvisorProgress Unpack(Unpacker unpacker)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		UpdateAdvisorProgress result = default(UpdateAdvisorProgress);
		result.Progress = new KeyValuePair<int, int>(key, value);
		return result;
	}

	public override string ToString()
	{
		return $"<UpdateAdvisorProgress Progress={Progress}>";
	}
}
