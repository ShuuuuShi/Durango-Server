using MsgPack;

namespace Messages;

public struct DiscoveryInfo
{
	public const uint TypeCode = 5001u;

	public string TemplateId;

	public Pair<string, bool>[] BiocomNames;

	public Pair<ushort, bool>[] AnimalTypes;

	public static void Pack(Packer packer, DiscoveryInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(5001u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.TemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TemplateId);
		}
		if (val.BiocomNames == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.BiocomNames.Length);
			for (int i = 0; i < val.BiocomNames.Length; i++)
			{
				packer.PackArrayHeader(2);
				packer.PackString(val.BiocomNames[i].Item1);
				packer.Pack(val.BiocomNames[i].Item2);
			}
		}
		if (val.AnimalTypes == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.AnimalTypes.Length);
		for (int j = 0; j < val.AnimalTypes.Length; j++)
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.AnimalTypes[j].Item1);
			packer.Pack(val.AnimalTypes[j].Item2);
		}
	}

	public static DiscoveryInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DiscoveryInfo result = default(DiscoveryInfo);
		result.TemplateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.BiocomNames = new Pair<string, bool>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			unpacker.Read();
			bool item2 = unpacker.LastReadData.AsBoolean();
			ref Pair<string, bool> reference = ref result.BiocomNames[i];
			reference = new Pair<string, bool>(item, item2);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.AnimalTypes = new Pair<ushort, bool>[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			unpacker.Read();
			ushort item3 = unpacker.LastReadData.AsUInt16();
			unpacker.Read();
			bool item4 = unpacker.LastReadData.AsBoolean();
			ref Pair<ushort, bool> reference2 = ref result.AnimalTypes[j];
			reference2 = new Pair<ushort, bool>(item3, item4);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DiscoveryInfo TemplateId={TemplateId} BiocomNames={BiocomNames} AnimalTypes={AnimalTypes}>";
	}
}
