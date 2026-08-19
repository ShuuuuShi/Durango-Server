using MsgPack;

namespace Messages;

public struct AppearEpicNPC
{
	public const uint TypeCode = 3141592u;

	public EpicNPC[] Npcs;

	public static void Pack(Packer packer, AppearEpicNPC val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3141592u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Npcs == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Npcs.Length);
		for (int i = 0; i < val.Npcs.Length; i++)
		{
			EpicNPC.Pack(packer, val.Npcs[i]);
		}
	}

	public static AppearEpicNPC Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AppearEpicNPC result = default(AppearEpicNPC);
		result.Npcs = new EpicNPC[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref EpicNPC reference = ref result.Npcs[i];
			reference = EpicNPC.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AppearEpicNPC Npcs={Npcs}>";
	}
}
