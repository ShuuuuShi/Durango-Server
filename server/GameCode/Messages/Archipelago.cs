using MsgPack;

namespace Messages;

public struct Archipelago
{
	public const uint TypeCode = 2053u;

	public string Id;

	public string TemplateId;

	public int UnstableFactor;

	public string Name;

	public double ExpiresAt;

	public ArchipelagoRegionInfo[] IncludedRegions;

	public static void Pack(Packer packer, Archipelago val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(2053u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (val.TemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TemplateId);
		}
		packer.Pack(val.UnstableFactor);
		packer.PackString(val.Name);
		packer.Pack(val.ExpiresAt);
		if (val.IncludedRegions == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.IncludedRegions.Length);
		for (int i = 0; i < val.IncludedRegions.Length; i++)
		{
			ArchipelagoRegionInfo.Pack(packer, val.IncludedRegions[i]);
		}
	}

	public static Archipelago Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Archipelago result = default(Archipelago);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TemplateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.UnstableFactor = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.ExpiresAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.IncludedRegions = new ArchipelagoRegionInfo[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ArchipelagoRegionInfo reference = ref result.IncludedRegions[i];
			reference = ArchipelagoRegionInfo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Archipelago Id={Id} TemplateId={TemplateId} UnstableFactor={UnstableFactor} Name={Name} ExpiresAt={ExpiresAt} IncludedRegions={IncludedRegions}>";
	}
}
