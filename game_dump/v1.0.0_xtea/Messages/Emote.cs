using MsgPack;

namespace Messages;

public struct Emote
{
	public const uint TypeCode = 3u;

	public ulong EntityId;

	public uint Emoticon;

	public float Power;

	public static void Pack(Packer packer, Emote val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.Emoticon);
		packer.Pack(val.Power);
	}

	public static Emote Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Emote result = default(Emote);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Emoticon = ((MessagePackObject)(ref lastReadData2)).AsUInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Power = ((MessagePackObject)(ref lastReadData3)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Emote EntityId={EntityId} Emoticon={Emoticon} Power={Power}>";
	}
}
