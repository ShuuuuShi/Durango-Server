using MsgPack;

namespace Messages;

public struct ItemTodoCondition
{
	public string TagId;

	public string PrototypeId;

	public string CollectibleId;

	public string GeneratorId;

	public int ItemLevel;

	public static void Pack(Packer packer, ItemTodoCondition val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.TagId == null)
		{
			packer.PackNull();
		}
		else if (val.TagId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TagId);
		}
		if (val.PrototypeId == null)
		{
			packer.PackNull();
		}
		else if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		if (val.CollectibleId == null)
		{
			packer.PackNull();
		}
		else if (val.CollectibleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CollectibleId);
		}
		if (val.GeneratorId == null)
		{
			packer.PackNull();
		}
		else if (val.GeneratorId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.GeneratorId);
		}
		packer.Pack(val.ItemLevel);
	}

	public static ItemTodoCondition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ItemTodoCondition result = default(ItemTodoCondition);
		if (unpacker.LastReadData.IsNil)
		{
			result.TagId = null;
		}
		else
		{
			string tagId = unpacker.LastReadData.AsString();
			result.TagId = tagId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PrototypeId = null;
		}
		else
		{
			string prototypeId = unpacker.LastReadData.AsString();
			result.PrototypeId = prototypeId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CollectibleId = null;
		}
		else
		{
			string collectibleId = unpacker.LastReadData.AsString();
			result.CollectibleId = collectibleId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.GeneratorId = null;
		}
		else
		{
			string generatorId = unpacker.LastReadData.AsString();
			result.GeneratorId = generatorId;
		}
		unpacker.Read();
		result.ItemLevel = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ItemTodoCondition TagId={TagId} PrototypeId={PrototypeId} CollectibleId={CollectibleId} GeneratorId={GeneratorId} ItemLevel={ItemLevel}>";
	}
}
