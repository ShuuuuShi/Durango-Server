using MsgPack;

namespace Messages;

public struct PlayMusic
{
	public const uint TypeCode = 3802u;

	public int Slot;

	public string InstrumentItemId;

	public static void Pack(Packer packer, PlayMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3802u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Slot);
		if (val.InstrumentItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.InstrumentItemId);
		}
	}

	public static PlayMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayMusic result = default(PlayMusic);
		result.Slot = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.InstrumentItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayMusic Slot={Slot} InstrumentItemId={InstrumentItemId}>";
	}
}
