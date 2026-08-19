using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PackedArtifact
{
	public ulong EntityId;

	public string BlueprintId;

	public int ArtifactLevel;

	public Tag[] Tags;

	public Performance[] Performance;

	public ArtifactDisplay Display;

	public Dictionary<string, string> LookNames;

	public static void Pack(Packer packer, PackedArtifact val, bool hint = false)
	{
		packer.PackArrayHeader(7);
		packer.Pack(val.EntityId);
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
		packer.Pack(val.ArtifactLevel);
		if (val.Tags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Tags.Length);
			for (int i = 0; i < val.Tags.Length; i++)
			{
				Tag.Pack(packer, val.Tags[i]);
			}
		}
		if (val.Performance == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Performance.Length);
			for (int j = 0; j < val.Performance.Length; j++)
			{
				Messages.Performance.Pack(packer, val.Performance[j]);
			}
		}
		ArtifactDisplay.Pack(packer, val.Display);
		if (val.LookNames == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.LookNames.Count);
		foreach (KeyValuePair<string, string> lookName in val.LookNames)
		{
			if (lookName.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(lookName.Key);
			}
			packer.PackString(lookName.Value);
		}
	}

	public static PackedArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PackedArtifact result = default(PackedArtifact);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.BlueprintId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ArtifactLevel = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Tags = new Tag[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Tag reference = ref result.Tags[i];
			reference = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.Performance = new Performance[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Performance reference2 = ref result.Performance[j];
			reference2 = Messages.Performance.Unpack(unpacker);
		}
		unpacker.Read();
		result.Display = ArtifactDisplay.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		result.LookNames = new Dictionary<string, string>(num3);
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData7)).AsString();
			unpacker.Read();
			string value = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.LookNames.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PackedArtifact EntityId={EntityId} BlueprintId={BlueprintId} ArtifactLevel={ArtifactLevel} Tags={Tags} Performance={Performance} Display={Display} LookNames={LookNames}>";
	}
}
