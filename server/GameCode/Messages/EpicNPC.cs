using MsgPack;
using Shared.Quest;

namespace Messages;

public struct EpicNPC
{
	public EpicNPCType Npc;

	public bool Interaction;

	public Pair<string, int>? Need;

	public static void Pack(Packer packer, EpicNPC val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.Npc);
		packer.Pack(val.Interaction);
		if (!val.Need.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		if (val.Need.Value.Item1 == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Need.Value.Item1);
		}
		packer.Pack(val.Need.Value.Item2);
	}

	public static EpicNPC Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		EpicNPC result = default(EpicNPC);
		if (num < 0 || 1 < num)
		{
			result.Npc = EpicNPCType.Invalid;
		}
		else
		{
			result.Npc = (EpicNPCType)num;
		}
		unpacker.Read();
		result.Interaction = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Need = null;
		}
		else
		{
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			int item2 = unpacker.LastReadData.AsInt32();
			Pair<string, int> value = new Pair<string, int>(item, item2);
			result.Need = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EpicNPC Npc={Npc} Interaction={Interaction} Need={Need}>";
	}
}
