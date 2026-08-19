using MsgPack;

namespace Messages;

public struct PlayerDrawLine
{
	public const uint TypeCode = 1016u;

	public string PlayerId;

	public DrawLineBase[] DrawCommands;

	public static void Pack(Packer packer, PlayerDrawLine val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1016u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PlayerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerId);
		}
		if (val.DrawCommands == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.DrawCommands.Length);
		for (int i = 0; i < val.DrawCommands.Length; i++)
		{
			DrawLineBase.Pack(packer, val.DrawCommands[i]);
		}
	}

	public static PlayerDrawLine Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerDrawLine result = default(PlayerDrawLine);
		result.PlayerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.DrawCommands = new DrawLineBase[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref DrawLineBase reference = ref result.DrawCommands[i];
			reference = DrawLineBase.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerDrawLine PlayerId={PlayerId} DrawCommands={DrawCommands}>";
	}
}
