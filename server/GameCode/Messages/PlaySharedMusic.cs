using MsgPack;

namespace Messages;

public struct PlaySharedMusic
{
	public const uint TypeCode = 47852451u;

	public string SharedSheetId;

	public string InstrumentItemId;

	public static void Pack(Packer packer, PlaySharedMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(47852451u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.SharedSheetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SharedSheetId);
		}
		if (val.InstrumentItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.InstrumentItemId);
		}
	}

	public static PlaySharedMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlaySharedMusic result = default(PlaySharedMusic);
		result.SharedSheetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.InstrumentItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlaySharedMusic SharedSheetId={SharedSheetId} InstrumentItemId={InstrumentItemId}>";
	}
}
