using MsgPack;

namespace Messages;

public struct PlayEmoticon
{
	public const uint TypeCode = 9592636u;

	public string EntityId;

	public string EmoticonId;

	public static void Pack(Packer packer, PlayEmoticon val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(9592636u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.EmoticonId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EmoticonId);
		}
	}

	public static PlayEmoticon Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayEmoticon result = default(PlayEmoticon);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EmoticonId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayEmoticon EntityId={EntityId} EmoticonId={EmoticonId}>";
	}
}
