using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Generator
{
	public string Id;

	public int Level;

	public string Name;

	public string Icon;

	public int Amount;

	public float Effort;

	public float Duration;

	public Dictionary<string, int> ToolRequirements;

	public bool Enabled;

	public static void Pack(Packer packer, Generator val, bool hint = false)
	{
		packer.PackArrayHeader(9);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.Level);
		packer.PackString(val.Name);
		if (val.Icon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Icon);
		}
		packer.Pack(val.Amount);
		packer.Pack(val.Effort);
		packer.Pack(val.Duration);
		if (val.ToolRequirements == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ToolRequirements.Count);
			foreach (KeyValuePair<string, int> toolRequirement in val.ToolRequirements)
			{
				if (toolRequirement.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(toolRequirement.Key);
				}
				packer.Pack(toolRequirement.Value);
			}
		}
		packer.Pack(val.Enabled);
	}

	public static Generator Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Generator result = default(Generator);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Icon = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Amount = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Effort = ((MessagePackObject)(ref lastReadData5)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Duration = ((MessagePackObject)(ref lastReadData6)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		result.ToolRequirements = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData8)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData9)).AsInt32();
			result.ToolRequirements.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		result.Enabled = ((MessagePackObject)(ref lastReadData10)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Generator Id={Id} Level={Level} Name={Name} Icon={Icon} Amount={Amount} Effort={Effort} Duration={Duration} ToolRequirements={ToolRequirements} Enabled={Enabled}>";
	}
}
