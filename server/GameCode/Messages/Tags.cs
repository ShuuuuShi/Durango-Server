using MsgPack;

namespace Messages;

public struct Tags
{
	public const uint TypeCode = 313u;

	public string EntityId;

	public Tag[] _Tags;

	public static void Pack(Packer packer, Tags val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(313u);
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
		if (val._Tags == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Tags.Length);
		for (int i = 0; i < val._Tags.Length; i++)
		{
			Tag.Pack(packer, val._Tags[i]);
		}
	}

	public static Tags Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Tags result = default(Tags);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result._Tags = new Tag[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Tag reference = ref result._Tags[i];
			reference = Tag.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Tags EntityId={EntityId} _Tags={_Tags}>";
	}
}
