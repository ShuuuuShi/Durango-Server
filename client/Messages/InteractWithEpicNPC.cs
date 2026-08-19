using MsgPack;
using Shared.Quest;

namespace Messages;

public struct InteractWithEpicNPC
{
	public const uint TypeCode = 3141593u;

	public EpicNPCType Npc;

	public string[] ItemIds;

	public static void Pack(Packer packer, InteractWithEpicNPC val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3141593u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Npc);
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemIds.Length);
		for (int i = 0; i < val.ItemIds.Length; i++)
		{
			if (val.ItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemIds[i]);
			}
		}
	}

	public static InteractWithEpicNPC Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		InteractWithEpicNPC result = default(InteractWithEpicNPC);
		if (num < 0 || 1 < num)
		{
			result.Npc = EpicNPCType.Invalid;
		}
		else
		{
			result.Npc = (EpicNPCType)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InteractWithEpicNPC Npc={Npc} ItemIds={ItemIds}>";
	}
}
