using MsgPack;

namespace Messages;

public struct DumpedPersonalIsland
{
	public const uint TypeCode = 381923u;

	public int PlayerSlot;

	public string TerrainId;

	public AppearPlayer AppearPlayer;

	public AppearArtifact[] Artifacts;

	public Item[] InventoryItems;

	public byte[] Garden;

	public static void Pack(Packer packer, DumpedPersonalIsland val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(381923u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack(val.PlayerSlot);
		if (val.TerrainId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TerrainId);
		}
		AppearPlayer.Pack(packer, val.AppearPlayer);
		if (val.Artifacts == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Artifacts.Length);
			for (int i = 0; i < val.Artifacts.Length; i++)
			{
				AppearArtifact.Pack(packer, val.Artifacts[i]);
			}
		}
		if (val.InventoryItems == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.InventoryItems.Length);
			for (int j = 0; j < val.InventoryItems.Length; j++)
			{
				Item.Pack(packer, val.InventoryItems[j]);
			}
		}
		if (val.Garden == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Garden);
		}
	}

	public static DumpedPersonalIsland Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DumpedPersonalIsland result = default(DumpedPersonalIsland);
		result.PlayerSlot = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.TerrainId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.AppearPlayer = AppearPlayer.Unpack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Artifacts = new AppearArtifact[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref AppearArtifact reference = ref result.Artifacts[i];
			reference = AppearArtifact.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.InventoryItems = new Item[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Item reference2 = ref result.InventoryItems[j];
			reference2 = Item.Unpack(unpacker);
		}
		unpacker.Read();
		result.Garden = unpacker.LastReadData.AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<DumpedPersonalIsland PlayerSlot={PlayerSlot} TerrainId={TerrainId} AppearPlayer={AppearPlayer} Artifacts={Artifacts} InventoryItems={InventoryItems} Garden={Garden}>";
	}
}
