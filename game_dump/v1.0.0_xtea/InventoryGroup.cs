using System.Collections.Generic;
using InteractionData;
using ItemSystem;
using L10N;
using Messages;
using Shared.Item;
using Shared.System;
using UnityEngine;

public class InventoryGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private InventoryContainer _inventory;

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Inventory_Open_01.wav", "Sound/Effect/UI/UI_Menu_Inventory_Close_01.wav");
		base.OnClose();
	}

	private void Start()
	{
		_titleWidget.OnBack += Close;
		_titleWidget.OnClose += base.ForceClose;
		_inventory.Closed += base.ForceClose;
		base.OnCloseSucceed += OnCloseSucess;
		AddInteractionHandler();
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().OnCollected += OnCollected;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().OnCollected -= OnCollected;
	}

	private void OnCloseSucess()
	{
		if (_inventory.ItemsRepositionFlag)
		{
			GameSystem<InventorySystem>.Instance().UpdateItemListOrder();
		}
		GameSystem<InventorySystem>.Instance().ResetTrakingInventory();
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.SetReviveReward, delegate
		{
			OpenDeadMode();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.DrawWater, delegate
		{
			ulong id = InteractionSystem.CurrentMenu.Id;
			GameSystem<InventorySystem>.Instance().DrawWater(id);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.PetInventory, delegate(InteractionObject obj)
		{
			OpenReinsInventory(obj.EntityId);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.FeedPet, delegate(InteractionObject obj)
		{
			ItemData reins = GameSystem<InventorySystem>.Instance().PlayerInventory.Find(obj.EntityId);
			if (reins.Reins != null)
			{
				PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
				popupItemSelector.SetTitle(T._("먹이 주기"));
				popupItemSelector.Set(delegate(ItemData data)
				{
					int i = 0;
					for (int size = KUtility.GetSize(reins.Reins.EatableTags); i < size; i++)
					{
						if (data.HasTag(reins.Reins.EatableTags[i]))
						{
							return true;
						}
					}
					return false;
				}, -1, T._("먹이 주기"), displayTooltip: true, null, delegate(IList<ItemData> items)
				{
					if (items != null && items.Count != 0)
					{
						GameSystem<InventorySystem>.Instance().FeedPet(obj.EntityId, Util.ItemsToIds(items));
					}
				});
				popupItemSelector.Show();
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Inventory, delegate(InteractionObject target)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			InventoryGroup inventoryGroup3 = UIManager.FindScript<InventoryGroup>();
			inventoryGroup3.OpenArtifactInventory(target.EntityId, new Point2(target.Tile));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Trough, delegate(InteractionObject target)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			InventoryGroup inventoryGroup2 = UIManager.FindScript<InventoryGroup>();
			inventoryGroup2.OpenArtifactInventory(target.EntityId, new Point2(target.Tile));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.UseWarehouse, delegate(InteractionObject target)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			InventoryGroup inventoryGroup = UIManager.FindScript<InventoryGroup>();
			inventoryGroup.OpenWarehouseInventory(target.EntityId, new Point2(target.Tile));
		});
	}

	private void OpenArtifactInventory(ulong target, Point2 tile)
	{
		GameSystem<InventorySystem>.Instance().SetArtifactInventory(target, tile);
		_inventory.SetInventoryMode(ItemSystem.Inventory.InventoryMode.Exchange);
		Open();
		BuildSystem.ArtifactAction("inventory", target, tile);
	}

	private void OpenReinsInventory(ulong target)
	{
		GameSystem<InventorySystem>.Instance().SetReinsInventory(target);
		_inventory.SetInventoryMode(ItemSystem.Inventory.InventoryMode.Exchange);
		Open();
	}

	public void OpenWarehouseInventory(ulong target, Point2 tile)
	{
		GameSystem<InventorySystem>.Instance().SetWarehouseInventory(target, tile);
		_inventory.SetInventoryMode(ItemSystem.Inventory.InventoryMode.Exchange);
		Open();
	}

	private void OpenDeadMode()
	{
		_inventory.SetInventoryMode(ItemSystem.Inventory.InventoryMode.Dead);
		Open();
		UIManager.MessageBox.Show(T._("도움을 요청하며 제시할 보상을 골라주십시오."));
	}

	private void OnCollected(ItemData item, Collected m)
	{
		string text = null;
		string text2 = null;
		string text3 = $"#item_collect_{m.Result}";
		switch (m.Result)
		{
		case Result.BigFailure:
			text = LocalizeSystem.Get(text3);
			text2 = Util.ActionInfoDetailString(m.ActionInfo);
			break;
		case Result.Failure:
		case Result.Success:
		case Result.GreatSuccess:
			if (item != null)
			{
				text = LocalizeSystem.Format(text3, item.Name, item.Level.ToString());
				text2 = Util.ItemQualityString(item);
			}
			break;
		}
		string fullPath = null;
		switch (m.Result)
		{
		case Result.BigFailure:
		case Result.Failure:
			fullPath = "Sound/Effect/UI/UI_Gathering_Fail_01.wav";
			break;
		case Result.Success:
			fullPath = "Sound/Effect/UI/UI_Gathering_Success_01.wav";
			break;
		case Result.GreatSuccess:
			fullPath = "Sound/Effect/UI/UI_Gathering_Great_01.wav";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (item != null)
			{
				text = string.Format("[{1}:1.5] {0}", text, item.Icon);
			}
			RewardAlarmGroup rewardAlarmGroup = UIManager.FindScript<RewardAlarmGroup>();
			if ((Object)(object)rewardAlarmGroup != (Object)null)
			{
				rewardAlarmGroup.Show(text, text2.Trim(), RewardAlarmGroup.RewardEffectType.Collect);
			}
			SoundManager.Cache(fullPath);
			SoundManager.Play(fullPath);
		}
	}

	public ItemIcon2 FindItem(ItemData item)
	{
		return _inventory.ItemList.Find(item);
	}

	public Transform GetUseButtonTransform()
	{
		return ((Component)_inventory.Buttons.GetUseButton()).transform;
	}

	public void SelectItem(ItemData item)
	{
		_inventory.ItemList.SelectItem(item);
	}
}
