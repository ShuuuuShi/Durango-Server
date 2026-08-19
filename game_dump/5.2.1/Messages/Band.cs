using MsgPack;

namespace Messages;

public struct Band
{
	public const uint TypeCode = 63459084u;

	public string Musician;

	public string MusicName;

	public string Timbre;

	public static void Pack(Packer packer, Band val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(63459084u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Musician == null)
		{
			packer.PackNull();
		}
		else if (val.Musician == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Musician);
		}
		if (val.MusicName == null)
		{
			packer.PackNull();
		}
		else if (val.MusicName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MusicName);
		}
		if (val.Timbre == null)
		{
			packer.PackNull();
		}
		else if (val.Timbre == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Timbre);
		}
	}

	public static Band Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Band result = default(Band);
		if (unpacker.LastReadData.IsNil)
		{
			result.Musician = null;
		}
		else
		{
			string musician = unpacker.LastReadData.AsString();
			result.Musician = musician;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.MusicName = null;
		}
		else
		{
			string musicName = unpacker.LastReadData.AsString();
			result.MusicName = musicName;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Timbre = null;
		}
		else
		{
			string timbre = unpacker.LastReadData.AsString();
			result.Timbre = timbre;
		}
		return result;
	}

	public override string ToString()
	{
		return "<Band Musician=" + Musician + " MusicName=" + MusicName + " Timbre=" + Timbre + ">";
	}
}
