using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct GrowCage
{
	public const uint TypeCode = 65100u;

	public byte Size;

	public byte RemainSize;

	public Pets Pets;

	public Dictionary<string, TaskStatus> Tasks;

	public static void Pack(Packer packer, GrowCage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(65100u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.Size);
		packer.Pack(val.RemainSize);
		Pets.Pack(packer, val.Pets);
		if (val.Tasks == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Tasks.Count);
		foreach (KeyValuePair<string, TaskStatus> task in val.Tasks)
		{
			if (task.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(task.Key);
			}
			TaskStatus.Pack(packer, task.Value);
		}
	}

	public static GrowCage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GrowCage result = default(GrowCage);
		result.Size = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.RemainSize = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.Pets = Pets.Unpack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Tasks = new Dictionary<string, TaskStatus>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			TaskStatus value = TaskStatus.Unpack(unpacker);
			result.Tasks.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GrowCage Size={Size} RemainSize={RemainSize} Pets={Pets} Tasks={Tasks}>";
	}
}
