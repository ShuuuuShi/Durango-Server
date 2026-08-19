using MsgPack;

namespace Messages;

public struct Resurrect
{
	public const uint TypeCode = 132u;

	public string EntityId;

	public float Score;

	public static void Pack(Packer packer, Resurrect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(132u);
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
		packer.Pack(val.Score);
	}

	public static Resurrect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Resurrect result = default(Resurrect);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Score = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Resurrect EntityId={EntityId} Score={Score}>";
	}
}
