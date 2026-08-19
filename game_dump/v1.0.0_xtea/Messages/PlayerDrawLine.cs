using MsgPack;

namespace Messages;

public struct PlayerDrawLine
{
	public const uint TypeCode = 1016u;

	public ulong PlayerId;

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
		packer.Pack(val.PlayerId);
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerDrawLine result = default(PlayerDrawLine);
		result.PlayerId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
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
