using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TutorialSession
{
	public ulong SessionId;

	public ulong[] Players;

	public Dictionary<string, int> Materials;

	public static void Pack(Packer packer, TutorialSession val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.SessionId);
		if (val.Players == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Players.Length);
			for (int i = 0; i < val.Players.Length; i++)
			{
				packer.Pack(val.Players[i]);
			}
		}
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Materials.Count);
		foreach (KeyValuePair<string, int> material in val.Materials)
		{
			if (material.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(material.Key);
			}
			packer.Pack(material.Value);
		}
	}

	public static TutorialSession Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TutorialSession result = default(TutorialSession);
		result.SessionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Players = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ulong[] players = result.Players;
			int num2 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			players[num2] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Materials = new Dictionary<string, int>(num3);
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData5)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			result.Materials.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TutorialSession SessionId={SessionId} Players={Players} Materials={Materials}>";
	}
}
