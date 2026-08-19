using System;
using Durango.MotionInfo;
using Durango.Network;
using InteractionData;
using Messages;

public class RepairSystem : GameSystem<RepairSystem>
{
	public static void RepairItem(string targetItem, string[] kitItems, Action<bool> onResult)
	{
		RegisterPostRepairEvents(Connections.Frontend.Send(new RepairItem
		{
			ItemId = targetItem,
			KitItemIds = kitItems
		}), onResult);
	}

	public static void RepairArtifact(string id, Point2 tile, string[] kitItems, Action<bool> onResult)
	{
		RegisterPostRepairEvents(Connections.Frontend.Send(new RepairArtifact
		{
			EntityId = id,
			Tile = tile,
			KitItemIds = kitItems
		}), onResult);
	}

	private static void RegisterPostRepairEvents(ReplyMessageHandlerRegistrar handler, Action<bool> onResult)
	{
		bool confirming = false;
		handler.On(delegate(Timer msg, PacketHeader header)
		{
			MotionMap.Instance().GetBuildMotion("Repair", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(msg.Duration, IconMap.Get(Interaction.RepairArtifact), motion, "Repair", equip);
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
		{
			confirming = true;
			LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
			{
				if (result != 0 && onResult != null)
				{
					onResult(obj: false);
				}
				confirming = false;
			});
		}).Rest(delegate
		{
			if (confirming)
			{
				confirming = false;
				LowEnergyWarning.Hide();
			}
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void RepairImmediate(string id, Point2 tile, Cost cost)
	{
		Connections.Frontend.Send(new RepairImmediate
		{
			EntityId = id,
			Tile = tile,
			Cost = cost
		});
	}
}
