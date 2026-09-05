using MsgPack;

namespace Messages;

public struct Musician
{
	public const uint TypeCode = 815u;

	public string EntityId;

	public Music? Music;

	public double? PlayedAt;

	public string Timbre;

	public static void Pack(Packer packer, Musician val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(815u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (!val.Music.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Music.Pack(packer, val.Music.Value);
		}
		if (!val.PlayedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PlayedAt.Value);
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

	public static Musician Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Musician result = default(Musician);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Music = null;
		}
		else
		{
			Music value = Messages.Music.Unpack(unpacker);
			result.Music = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PlayedAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.PlayedAt = value2;
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
		return $"<Musician EntityId={EntityId} Music={Music} PlayedAt={PlayedAt} Timbre={Timbre}>";
	}
}
