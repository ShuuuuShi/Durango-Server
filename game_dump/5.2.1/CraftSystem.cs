using System;
using System.Collections.Generic;
using Crafting;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Item;

public class CraftSystem : GameSystem<CraftSystem>
{
	public struct CraftQueueItem
	{
		public Recipe Recipe;

		public Dictionary<string, ItemData[]> Materials;

		public ItemData Tool;

		public Artifact Workbench;

		public Dictionary<string, string[]> MakeMaterialIds()
		{
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			foreach (KeyValuePair<string, ItemData[]> material in Materials)
			{
				dictionary.Add(material.Key, Util.ItemsToIds(material.Value));
			}
			return dictionary;
		}

		public List<TagData> CreateMaterialsTags()
		{
			List<TagData> list = new List<TagData>();
			foreach (KeyValuePair<string, ItemData[]> material in Materials)
			{
				for (int i = 0; i < material.Value.Length; i++)
				{
					list.AddRange(material.Value[i].Tags);
				}
			}
			return list;
		}
	}

	private readonly CraftSlotContainer _slotContainer = new CraftSlotContainer();

	private readonly Queue<CraftQueueItem> _craftQueue = new Queue<CraftQueueItem>();

	public PredictTimer CraftingTimer { get; private set; }

	public CraftQueueItem LastCraftData { get; private set; }

	public CraftSlotContainer SlotContainer => _slotContainer;

	public event Action<string, Crafted> CraftSucceed;

	public event Action EntrustedCraftStarted;

	public event Action<string, ActionInfo> CraftFailed;

	public event Action<ItemData> TechSupportSucceed;

	private void Awake()
	{
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			CraftingTimer = new PredictTimer(GameManager.PlayerId, "item_crafting");
		};
	}

	public void Craft()
	{
		MakeCraftQueue();
		DoNextCraftQueue();
	}

	public void TechSupport()
	{
		if (_slotContainer.TechSupportBaseSlotInfo == null || _slotContainer.Workbench == null)
		{
			return;
		}
		TechSupportTarget target = _slotContainer.TechSupportBaseSlotInfo.Target;
		if (target.Item == null)
		{
			return;
		}
		Dictionary<string, ItemData[]> dictionary = _slotContainer.CreateMaterialItemsDictionary(0, canFinished: true);
		if (dictionary == null)
		{
			return;
		}
		CraftQueueItem craftQueueItem = default(CraftQueueItem);
		craftQueueItem.Recipe = _slotContainer.Recipe;
		craftQueueItem.Materials = dictionary;
		craftQueueItem.Tool = ((!_slotContainer.Tool.ToolRequired) ? null : _slotContainer.Tool.GetSelectedItem());
		craftQueueItem.Workbench = _slotContainer.Workbench;
		Connections.Frontend.Send(new RequestTechSupport
		{
			EntityId = craftQueueItem.Workbench.EntityId,
			Tile = craftQueueItem.Workbench.WorldTile,
			ItemId = target.Item.Id,
			ReformSlotIndex = target.ReformSlotIndex,
			Materials = craftQueueItem.MakeMaterialIds()
		}).On<OK>(delegate
		{
			if (this.TechSupportSucceed != null)
			{
				this.TechSupportSucceed(target.Item);
			}
		});
	}

	private void MakeCraftQueue()
	{
		_craftQueue.Clear();
		int quantity = _slotContainer.Quantity;
		for (int i = 0; i < quantity; i++)
		{
			Dictionary<string, ItemData[]> dictionary = _slotContainer.CreateMaterialItemsDictionary(i, canFinished: true);
			if (dictionary != null)
			{
				CraftQueueItem item = default(CraftQueueItem);
				item.Recipe = _slotContainer.Recipe;
				item.Tool = ((!_slotContainer.Tool.ToolRequired) ? null : _slotContainer.Tool.GetSelectedItem());
				item.Workbench = _slotContainer.Workbench;
				item.Materials = dictionary;
				_craftQueue.Enqueue(item);
				continue;
			}
			break;
		}
	}

	public void DoNextCraftQueue()
	{
		if (_craftQueue.Count != 0)
		{
			CraftQueueItem craftQueueItem2 = (LastCraftData = _craftQueue.Dequeue());
			CraftQueueItem craftQueueItem3 = craftQueueItem2;
			Craft craft = default(Craft);
			craft.RecipeId = craftQueueItem3.Recipe.Id;
			craft.Materials = craftQueueItem3.MakeMaterialIds();
			craft.ToolItemId = ((craftQueueItem3.Tool != null) ? craftQueueItem3.Tool.Id : string.Empty);
			craft.Workbench = ((!(craftQueueItem3.Workbench == null)) ? new PropKey?(craftQueueItem3.Workbench.GetPropKey()) : null);
			Craft msg = craft;
			RegisterPostCraftEvents(Connections.Frontend.Send(msg), craftQueueItem3.Recipe.Id);
		}
	}

	private void RegisterPostCraftEvents(ReplyMessageHandlerRegistrar handler, string recipeId)
	{
		CraftingTimer.Play(Connections.Frontend.Ping + 10f);
		bool confirming = false;
		handler.On(delegate(Messages.Timer timer, PacketHeader header)
		{
			if (!CraftingTimer.Timer.IsStop)
			{
				CraftingTimer.Play(timer.Duration);
			}
		}).On<CraftStartedOnWorkbench>(delegate
		{
			if (this.EntrustedCraftStarted != null)
			{
				this.EntrustedCraftStarted();
			}
			DoNextCraftQueue();
		}).On(delegate(Crafted crafted, PacketHeader header)
		{
			if (crafted.Result == Result.BigFailure)
			{
				if (this.CraftFailed != null)
				{
					this.CraftFailed(recipeId, crafted.ActionInfo);
				}
			}
			else if (this.CraftSucceed != null)
			{
				this.CraftSucceed(recipeId, crafted);
			}
			DoNextCraftQueue();
		})
			.On(delegate(EnergyWarning warningMsg, PacketHeader header)
			{
				CraftingTimer.Pause();
				confirming = true;
				LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
				{
					if (result == LowEnergyWarning.Result.IgnoreWarning)
					{
						CraftingTimer.Play(Connections.Frontend.Ping + 10f);
					}
					else
					{
						CraftingTimer.Stop();
					}
					confirming = false;
				});
			})
			.Rest(delegate
			{
				if (confirming)
				{
					confirming = false;
					LowEnergyWarning.Hide();
				}
				CraftingTimer.Stop();
			});
	}

	public static void CraftEstimation(EstimateCraft info, Action<CraftEstimationInfo?> onResult)
	{
		Connections.Frontend.Send(info).On(delegate(CraftEstimationInfo estimation, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(estimation);
			}
		}).Rest(delegate
		{
			if (onResult != null)
			{
				onResult(null);
			}
		});
	}

	public static void DyeingEstimate(Artifact workbench, [NotNull] ItemData item, [NotNull] ItemData dye, ColorChannel channel, [NotNull] Action<CraftEstimation?> onResult)
	{
		Recipe recipe = ((!dye.HasTag("decolorizer")) ? GameSystem<RecipeSystem>.Instance().GetDyeingRecipe(channel) : GameSystem<RecipeSystem>.Instance().GetBleachingRecipe(channel));
		if (recipe != null)
		{
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
			for (int i = 0; i < recipe.Slots.Length; i++)
			{
				RecipeSlot recipeSlot = recipe.Slots[i];
				dictionary.Add(recipeSlot.Id, (!recipeSlot.IsBase) ? new string[1] { dye.Id } : new string[1] { item.Id });
			}
			EstimateCraft estimateCraft = default(EstimateCraft);
			estimateCraft.RecipeId = recipe.Id;
			estimateCraft.Materials = dictionary;
			estimateCraft.Workbench = workbench.GetPropKey();
			EstimateCraft msg = estimateCraft;
			Connections.Frontend.Send(msg).On(delegate(CraftEstimationInfo r, PacketHeader h)
			{
				onResult(r.CraftEstimation);
			});
		}
	}

	public void Dyeing([NotNull] Artifact workbench, [NotNull] ItemData item, [NotNull] ItemData dye, ColorChannel channel)
	{
		_craftQueue.Clear();
		CraftQueueItem lastCraftData = default(CraftQueueItem);
		bool flag = dye.HasTag("decolorizer");
		lastCraftData.Recipe = ((!flag) ? GameSystem<RecipeSystem>.Instance().GetDyeingRecipe(channel) : GameSystem<RecipeSystem>.Instance().GetBleachingRecipe(channel));
		lastCraftData.Materials = new Dictionary<string, ItemData[]>();
		int i = 0;
		for (int size = KUtility.GetSize(lastCraftData.Recipe.Slots); i < size; i++)
		{
			RecipeSlot recipeSlot = lastCraftData.Recipe.Slots[i];
			lastCraftData.Materials.Add(recipeSlot.Id, (!recipeSlot.IsBase) ? new ItemData[1] { dye } : new ItemData[1] { item });
		}
		ReplyMessageHandlerRegistrar handler = ((!flag) ? Connections.Frontend.Send(new Dye
		{
			Workbench = workbench.GetPropKey(),
			Channel = channel,
			Materials = lastCraftData.MakeMaterialIds()
		}) : Connections.Frontend.Send(new Bleach
		{
			Workbench = workbench.GetPropKey(),
			Channel = channel,
			Materials = lastCraftData.MakeMaterialIds()
		}));
		LastCraftData = lastCraftData;
		RegisterPostCraftEvents(handler, lastCraftData.Recipe.Id);
	}

	public static void SkipEntrustedCraft(PropKey propKey, string craftingId, Action<bool> onResult = null)
	{
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new SkipEntrustedCraft
		{
			EntityId = propKey.EntityId,
			Tile = propKey.Tile,
			CraftingId = craftingId
		});
		if (onResult != null)
		{
			replyMessageHandlerRegistrar.All(delegate(Packet packet)
			{
				onResult(Packet.IsSuccess(packet));
			});
		}
	}

	public static void CancelCrafting(PropKey propKey, string craftingId, Action<bool> onResult = null)
	{
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new CancelCrafting
		{
			EntityId = propKey.EntityId,
			Tile = propKey.Tile,
			CraftingId = craftingId
		});
		if (onResult != null)
		{
			replyMessageHandlerRegistrar.All(delegate(Packet packet)
			{
				onResult(Packet.IsSuccess(packet));
			});
		}
	}
}
