using MsgPack;

namespace Messages;

public struct Collectible
{
	public const uint TypeCode = 2019u;

	public ulong EntityId;

	public string CollectibleId;

	public string Size;

	public Generator[] Generators;

	public static void Pack(Packer packer, Collectible val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2019u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
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
			return;
		}
		packer.PackArrayHeader(val.Generators.Length);
		for (int i = 0; i < val.Generators.Length; i++)
		{
			Generator.Pack(packer, val.Generators[i]);
		}
	}

	public static Collectible Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Collectible result = default(Collectible);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.CollectibleId = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string collectibleId = ((MessagePackObject)(ref lastReadData3)).AsString();
			result.CollectibleId = collectibleId;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Size = ((MessagePackObject)(ref lastReadData4)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.Generators = new Generator[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Generator reference = ref result.Generators[i];
			reference = Generator.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Collectible EntityId={EntityId} CollectibleId={CollectibleId} Size={Size} Generators={Generators}>";
	}
}
