using System;
using Durango.Network;
using JetBrains.Annotations;
using Messages;

public class CargoWarpholeSystem : GameSystem<CargoWarpholeSystem>
{
	public static void ActivateCargoReceiver(string id, Point2 tile)
	{
		Connections.Frontend.Send(new ActivateCargoReceiver
		{
			EntityId = id,
			Tile = tile
		});
	}

	public static void SendCargo(string id, Point2 tile, CargoReceiver receiver, string[] itemIds, Action<CargoReceiver> onSuccess)
	{
		Connections.Frontend.Send(new SendCargo
		{
			EntityId = id,
			Tile = tile,
			Destination = new CargoDestination
			{
				EntityId = receiver.EntityId,
				Tile = receiver.Tile,
				RegionId = receiver.Region.Id
			},
			ItemIds = itemIds
		}).On(delegate(CargoReceiver msg, PacketHeader header)
		{
			if (onSuccess != null)
			{
				onSuccess(msg);
			}
		});
	}

	public static void GetCargoReceivers(InteractionObject obj, [NotNull] Action<CargoReceivers> onResult)
	{
		Connections.Frontend.Send(new GetCargoReceivers
		{
			EntityId = obj.EntityId,
			Tile = new Point2(obj.Tile)
		}).On(delegate(CargoReceivers msg, PacketHeader header)
		{
			onResult(msg);
		});
	}

	public static void GetReceivedItems(string id, Point2 tile, [NotNull] Action<ReceivedItems> onResult)
	{
		Connections.Frontend.Send(new GetReceivedItems
		{
			EntityId = id,
			Tile = tile
		}).On(delegate(ReceivedItems items, PacketHeader header)
		{
			onResult(items);
		});
	}
}
