using MsgPack;

namespace Messages;

public struct Mannequin
{
	public const uint TypeCode = 19726u;

	public string EntityId;

	public Item? Head;

	public Item? Body;

	public static void Pack(Packer packer, Mannequin val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(19726u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (!val.Head.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Item.Pack(packer, val.Head.Value);
		}
		if (!val.Body.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Item.Pack(packer, val.Body.Value);
		}
	}

	public static Mannequin Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Mannequin result = default(Mannequin);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Head = null;
		}
		else
		{
			Item value = Item.Unpack(unpacker);
			result.Head = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Body = null;
		}
		else
		{
			Item value2 = Item.Unpack(unpacker);
			result.Body = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Mannequin EntityId={EntityId} Head={Head} Body={Body}>";
	}
}
