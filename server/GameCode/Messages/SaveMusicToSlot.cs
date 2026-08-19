using MsgPack;

namespace Messages;

public struct SaveMusicToSlot
{
	public const uint TypeCode = 47852456u;

	public int Slot;

	public Music Music;

	public static void Pack(Packer packer, SaveMusicToSlot val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(47852456u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Slot);
		Music.Pack(packer, val.Music);
	}

	public static SaveMusicToSlot Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SaveMusicToSlot result = default(SaveMusicToSlot);
		result.Slot = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Music = Music.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SaveMusicToSlot Slot={Slot} Music={Music}>";
	}
}
