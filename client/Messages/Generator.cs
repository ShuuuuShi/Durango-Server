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
		unpacker.Read();
		Generator result = default(Generator);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Icon = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Amount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Effort = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ToolRequirements = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.ToolRequirements.Add(key, value);
		}
		unpacker.Read();
		result.Enabled = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Generator Id={Id} Level={Level} Name={Name} Icon={Icon} Amount={Amount} Effort={Effort} Duration={Duration} ToolRequirements={ToolRequirements} Enabled={Enabled}>";
	}
}
