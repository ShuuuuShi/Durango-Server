using System;
using Durango.Network;
using Durango.UI.Control;
using InteractionData;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class CargoWarpholeGroup : UIBase
{
	public enum WarpholeType
	{
		[T.EnumName("개인 화물 워프홀")]
		Private,
		[T.EnumName("부족 화물 워프홀")]
		Clan
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private InWarpholeWidget _inWarpholeWidget;

	[SerializeField]
	private OutWarpholeWidget _outWarpholeWidget;

	private WarpholeType _type;

	public string Id { get; private set; }

	public Point2 Tile { get; private set; }

	public WarpholeType Type
	{
		get
		{
			return _type;
		}
		private set
		{
			_type = value;
			_titleWidget.Object.SetTitle(_type.GetName());
		}
	}

	private void Start()
	{
		AddInteractionHandler();
		base.TryClose();
	}

	protected override bool TryOpen()
	{
		_inWarpholeWidget.Close(instant: true);
		_outWarpholeWidget.Close(instant: true);
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		if (_outWarpholeWidget.gameObject.activeSelf && _outWarpholeWidget.Alpha >= 1f && !_outWarpholeWidget.Back())
		{
			return false;
		}
		return base.TryClose();
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ActivateCargoReceiver, delegate(InteractionObject obj)
		{
			CargoWarpholeSystem.ActivateCargoReceiver(obj.EntityId, new Point2(obj.Tile));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.WarpCargoToClan, delegate(InteractionObject obj)
		{
			base.Open();
			Id = obj.EntityId;
			Tile = new Point2(obj.Tile);
			Type = WarpholeType.Clan;
			CargoWarpholeSystem.GetCargoReceivers(obj, OnClanCargoReceivers);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.WarpCargoToPrivate, delegate(InteractionObject obj)
		{
			base.Open();
			Id = obj.EntityId;
			Tile = new Point2(obj.Tile);
			Type = WarpholeType.Private;
			CargoWarpholeSystem.GetCargoReceivers(obj, OnPrivateCargoReceivers);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetCargoItems, delegate(InteractionObject target)
		{
			Artifact targetComponent2 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent2 == null))
			{
				OpenCargoReceiver(targetComponent2.EntityId, targetComponent2.WorldTile);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.StartToOccupyCargoWarphole, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				StartOccupying(targetComponent);
			}
		});
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	private void OpenCargoReceiver(string id, Point2 tile)
	{
		base.Open();
		Id = id;
		Tile = tile;
		_titleWidget.Object.SetTitle(T._("화물 워프홀"));
		CargoWarpholeSystem.GetReceivedItems(id, tile, OnReceivedItems);
	}

	private void StartOccupying(Artifact artifact)
	{
		Connections.Frontend.Send(new OccupyCargoWarphole
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private void OnPrivateCargoReceivers(CargoReceivers receivers)
	{
		if (!receivers.PrivateReceiver.HasValue)
		{
			ForceClose();
		}
		else
		{
			_inWarpholeWidget.Open(receivers.PrivateReceiver.Value, receivers.CostPerSize);
		}
	}

	private void OnClanCargoReceivers(CargoReceivers receivers)
	{
		if (!receivers.ClanReceiver.HasValue)
		{
			ForceClose();
		}
		else
		{
			_inWarpholeWidget.Open(receivers.ClanReceiver.Value, receivers.CostPerSize);
		}
	}

	private void OnReceivedItems(ReceivedItems items)
	{
		_outWarpholeWidget.Open(items);
	}
}
