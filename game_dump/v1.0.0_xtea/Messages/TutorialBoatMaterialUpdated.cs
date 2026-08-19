using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TutorialBoatMaterialUpdated
{
	public const uint TypeCode = 2418u;

	public ulong SessionId;

	public string PlayerName;

	public KeyValuePair<string, int>[] Materials;

	public static void Pack(Packer packer, TutorialBoatMaterialUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2418u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.SessionId);
		if (val.PlayerName == null)
		{
			packer.PackNull();
		}
		else if (val.PlayerName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerName);
		}
		if (val.Materials == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Materials.Length);
		for (int i = 0; i < val.Materials.Length; i++)
		{
			packer.PackArrayHeader(2);
			packer.PackString(val.Materials[i].Key);
			packer.Pack(val.Materials[i].Value);
		}
	}

	public static TutorialBoatMaterialUpdated Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TutorialBoatMaterialUpdated result = default(TutorialBoatMaterialUpdated);
		result.SessionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.PlayerName = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string playerName = ((MessagePackObject)(ref lastReadData3)).AsString();
			result.PlayerName = playerName;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Materials = new KeyValuePair<string, int>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string key = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			ref KeyValuePair<string, int> reference = ref result.Materials[i];
			reference = new KeyValuePair<string, int>(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TutorialBoatMaterialUpdated SessionId={SessionId} PlayerName={PlayerName} Materials={Materials}>";
	}
}
