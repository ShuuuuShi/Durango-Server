using MsgPack;

namespace Messages;

public struct SharedMusic
{
	public const uint TypeCode = 47852458u;

	public int RefCount;

	public Music Music;

	public static void Pack(Packer packer, SharedMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(47852458u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.RefCount);
		Music.Pack(packer, val.Music);
	}

	public static SharedMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SharedMusic result = default(SharedMusic);
		result.RefCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Music = Music.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SharedMusic RefCount={RefCount} Music={Music}>";
	}
}
