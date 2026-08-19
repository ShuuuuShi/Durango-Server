using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.UI.Control;
using InteractionData;
using L10N;
using Messages;
using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

public class DeliveryGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private DeliveryWidget _deliveryWidget;

	public string EntityId { get; private set; }

	public Point2 Tile { get; private set; }

	public FactionType Faction { get; private set; }

	public bool CanDeliver => !_deliveryWidget.ConfirmButton.Disabled;

	private void Start()
	{
		_deliveryWidget.DeliveryConfirmed += OnDeliveryConfirmed;
		AddInteractionHandlers();
		TryClose();
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	public void Open(string entityId, Point2 tile, FactionType faction)
	{
		EntityId = entityId;
		Tile = tile;
		Faction = faction;
		_titleWidget.Object.SetTitle(T._("캠프창고"));
		base.Open();
		_deliveryWidget.Widget.alpha = 0f;
		UIManager.Popup.LoadingRing.AttachToWidget(_deliveryWidget.gameObject);
		FactionSystem.GetFactionDeliveryConditions(entityId, tile, faction, OnFactionDeliveryCondition);
	}

	public Transform GetSelectableItemTranform()
	{
		ItemIconWidget firstSelectableEnabledItemOrNull = _deliveryWidget.GetFirstSelectableEnabledItemOrNull();
		if (firstSelectableEnabledItemOrNull != null)
		{
			return firstSelectableEnabledItemOrNull.transform;
		}
		return null;
	}

	public Transform GetConfirmButtonTransform()
	{
		return _deliveryWidget.ConfirmButton.transform;
	}

	private void AddInteractionHandlers()
	{
		Interaction[] obj = new Interaction[6]
		{
			Interaction.DeliveryChlorophylForum,
			Interaction.DeliveryChamberOfPioneer,
			Interaction.DeliveryTheFirm,
			Interaction.DeliveryTheCommittee,
			Interaction.DeliveryLama,
			Interaction.DeliveryRescueTf
		};
		Interaction interaction = obj[0];
		Interaction[] array = obj;
		foreach (Interaction interaction2 in array)
		{
			FactionType factionType = (FactionType)(interaction2 - interaction);
			GameSystem<InteractionSystem>.Instance().AddInteractionHandler(interaction2, delegate(InteractionObject target)
			{
				Open(target.EntityId, new Point2(target.Tile), factionType);
			});
		}
	}

	private void OnFactionDeliveryCondition(FactionDeliveryCondition condition)
	{
		_deliveryWidget.Alpha = 1f;
		UIManager.Popup.LoadingRing.DetachFromWidget(_deliveryWidget.gameObject);
		_deliveryWidget.Set(condition);
	}

	private void OnDeliveryConfirmed(List<ItemData> items)
	{
		if (items.FirstOrDefault((ItemData elem) => elem.Locked) != null)
		{
			UIManager.SystemMsg(T._("<em>잠금</em> 설정된 아이템은 캠프창고에 넣을 수 없습니다."));
			return;
		}
		FactionSystem.DeliveryItems(EntityId, Tile, Faction, Util.ItemsToIds(items));
		ForceClose();
	}
}
