using System;
using System.Collections.Generic;
using ItemSystem;
using Messages;

public class PackArtifactSystem : GameSystem<PackArtifactSystem>
{
	public static ItemData GetPackage()
	{
		ItemSystem.Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
		List<ItemData> items = playerInventory.Items;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].RawPrototypename == "artifact_package")
			{
				return items[i];
			}
		}
		return null;
	}

	public static void StartPack(int size, Action onSuccess)
	{
		Connections.Frontend.Send(new StartPacking
		{
			Size = size
		}).On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public static void PackArtifact(Artifact artifact, Action onSuccess = null)
	{
		Connections.Frontend.Send(new PackArtifact
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public static void UnpackArtifact(ItemSystem.ArtifactCapsule capsule, Point2 tile, bool rotated)
	{
		Connections.Frontend.Send(new UnpackArtifact
		{
			EntityId = capsule.EntityId,
			Tile = tile,
			Rotated = rotated
		});
	}

	public static void FinishPacking()
	{
		Connections.Frontend.Send(default(FinishPacking));
	}
}
