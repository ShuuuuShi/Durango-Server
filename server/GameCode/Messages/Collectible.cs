using MsgPack;

namespace Messages;

public struct Collectible
{
	public const uint TypeCode = 2019u;

	public string EntityId;

	public string CollectibleId;

	public string Size;

	public Generator[] Generators;

	public string CriticalGenerator;

	public static void Pack(Packer packer, Collectible val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2019u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
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
		if (val.Size == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Size);
		}
		if (val.Generators == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Generators.Length);
			for (int i = 0; i < val.Generators.Length; i++)
			{
				Generator.Pack(packer, val.Generators[i]);
			}
		}
		if (val.CriticalGenerator == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CriticalGenerator);
		}
	}

	public static Collectible Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Collectible result = default(Collectible);
		result.EntityId = unpacker.LastReadData.AsString();
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
		result.Size = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Generators = new Generator[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Generator reference = ref result.Generators[i];
			reference = Generator.Unpack(unpacker);
		}
		unpacker.Read();
		result.CriticalGenerator = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Collectible EntityId={EntityId} CollectibleId={CollectibleId} Size={Size} Generators={Generators} CriticalGenerator={CriticalGenerator}>";
	}
}
