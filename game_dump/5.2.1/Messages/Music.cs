using MsgPack;

namespace Messages;

public struct Music
{
	public const uint TypeCode = 47852455u;

	public string Name;

	public byte[] Data;

	public float Duration;

	public string Publisher;

	public static void Pack(Packer packer, Music val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(47852455u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		if (val.Data == null)
		{
			packer.PackNull();
		}
		else if (val.Data == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Data);
		}
		packer.Pack(val.Duration);
		if (val.Publisher == null)
		{
			packer.PackNull();
		}
		else if (val.Publisher == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Publisher);
		}
	}

	public static Music Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Music result = default(Music);
		result.Name = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Data = null;
		}
		else
		{
			byte[] data = unpacker.LastReadData.AsBinary();
			result.Data = data;
		}
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Publisher = null;
		}
		else
		{
			string publisher = unpacker.LastReadData.AsString();
			result.Publisher = publisher;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Music Name={Name} Data={Data} Duration={Duration} Publisher={Publisher}>";
	}
}
