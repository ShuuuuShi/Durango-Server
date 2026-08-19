using System.Collections.Generic;
using Durango.Logic.Item;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class IconMoveEffectGroup : UIBase
{
	[SerializeField]
	private IconMoveEffectWidget _iconMoveEffectWidget;

	public Vector3 LootingEndPos { get; private set; }

	public Vector3 PutinStartPos { get; private set; }

	private void Start()
	{
		GameSystem<CraftSystem>.Instance().CraftSucceed += OnSuccessCraft;
		GameSystem<GatheringSystem>.Instance().ItemCollected += OnItemCollected;
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		Vector3 normalized = new Vector3(UIManager.ScreenWidth, UIManager.ScreenHeight).normalized;
		LootingEndPos = -normalized * 150f;
		PutinStartPos = -new Vector3(UIManager.ScreenWidth, UIManager.ScreenHeight) * 0.5f + normalized * 100f;
	}

	public void RegisterSlotContainer(SlotContainer container, Vector3 pos)
	{
		for (int i = 0; i < container.SlotCount; i++)
		{
			SlotInfo slotInfo = container.GetSlotInfo(i);
			if (slotInfo == null)
			{
				continue;
			}
			foreach (ItemData selectedItem in slotInfo.SelectedItems)
			{
				Register(IconMoveEffectWidget.EffectType.PutIn, selectedItem.Icon, PutinStartPos, pos);
			}
		}
	}

	public void RegisterSlotContainer(CraftSystem.CraftQueueItem craftInfo, Vector3 pos)
	{
		foreach (KeyValuePair<string, ItemData[]> material in craftInfo.Materials)
		{
			ItemData[] value = material.Value;
			foreach (ItemData itemData in value)
			{
				Register(IconMoveEffectWidget.EffectType.PutIn, itemData.Icon, PutinStartPos, pos);
			}
		}
	}

	public void Register(IconMoveEffectWidget.EffectType type, ItemIcon icon, Vector3 start, Vector3 end)
	{
		_iconMoveEffectWidget.Register(type, icon, start, end);
	}

	private void OnSuccessCraft(string recipeId, Crafted crafted)
	{
		int i = 0;
		for (int size = KUtility.GetSize(crafted.Items); i < size; i++)
		{
			Item item = crafted.Items[i];
			Register(IconMoveEffectWidget.EffectType.Loot, new ItemIcon(item), Vector3.zero, LootingEndPos);
		}
	}

	private void OnItemCollected(Item item)
	{
		ShowGatheringItemEffect(new ItemIcon(item));
	}

	public void ShowGatheringItemEffect(ItemIcon icon)
	{
		Register(IconMoveEffectWidget.EffectType.Loot, icon, Vector3.zero, LootingEndPos);
	}
}
