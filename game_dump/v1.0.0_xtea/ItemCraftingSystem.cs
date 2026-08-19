using System;
using System.Collections.Generic;
using Crafting;
using ItemSystem;
using JetBrains.Annotations;
using K1Network;
using L10N;
using Messages;
using Shared.Item;
using TimerData;
using UnityEngine;

public class ItemCraftingSystem : GameSystem<ItemCraftingSystem>
{
	private const string CraftStartedParticle = "Particle/FX_Entrust_Prop.prefab";

	private const string CraftStartedSound = "Sound/Effect/UI/UI_Entrust_Prop.wav";

	private readonly CraftSlotContainer _slotContainer = new CraftSlotContainer();

	private ExpectedResultInfo _expectedResultInfo = new ExpectedResultInfo();

	public CraftSlotContainer SlotContainer => _slotContainer;

	public IExpectedResultInfo ExpectedResult => (!_expectedResultInfo.IsValid) ? null : _expectedResultInfo;

	public event Action CraftStartedOnWorkbench;

	public event Action<IList<ItemData>, string> CraftingFinished;

	public event Action<string, ActionInfo> CraftFailed;

	public void Craft()
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			Vehicle.RequestUnmountIfRiding(immediately: true, delegate
			{
				SendCraftMessage();
			});
		}
		else
		{
			SendCraftMessage();
		}
	}

	public void RequestEstimateResult()
	{
		if (_slotContainer.State == CraftSlotContainer.CraftState.ReadyToCraft)
		{
			SendEstimateResultMessage();
		}
		else
		{
			_slotContainer.UpdateEstimateResult(null);
		}
	}

	private void SendCraftMessage()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Craft craft = default(Craft);
		craft.RecipeId = _slotContainer.Recipe.Id;
		craft.Materials = _slotContainer.CreateMaterialsDictionary();
		craft.ToolItemId = _slotContainer.GetToolItemId();
		craft.WorkbenchEntityId = _slotContainer.GetWorkbenchEntityId();
		craft.WorkbenchTile = _slotContainer.GetWorkbenchTile();
		Craft msg = craft;
		Vector3? center = null;
		if ((Object)(object)_slotContainer.Workbench != (Object)null)
		{
			center = _slotContainer.Workbench.Center;
		}
		RegisterPostCraftEvents(Connections.Frontend.Send(msg), center);
	}

	private void RegisterPostCraftEvents(ReplyMessageHandlerRegistrar handler, Vector3? center = null)
	{
		bool confirming = false;
		handler.On<Messages.Timer>(OnTimer).On<CraftStartedOnWorkbench>(delegate
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (this.CraftStartedOnWorkbench != null)
			{
				this.CraftStartedOnWorkbench();
			}
			if (center.HasValue)
			{
				ParticleManager.Emit("Particle/FX_Entrust_Prop.prefab", center.Value, Quaternion.identity);
				SoundManager.Play("Sound/Effect/UI/UI_Entrust_Prop.wav");
			}
		}).On<Crafted>(OnCrafted)
			.On(delegate(EnergyWarning warningMsg, PacketHeader header)
			{
				confirming = true;
				UIManager.MessageBox.Show(T._("에너지가 모자라는 상태로 이 행동을 하면 건강이 소모됩니다."), delegate(int select)
				{
					Confirm confirm = default(Confirm);
					confirm.Confirmation = select == 0;
					Confirm msg = confirm;
					Connection frontend = Connections.Frontend;
					ulong replyOf = header.ReplyOf;
					frontend.Send(msg, noReply: false, replyOf);
					confirming = false;
				}, T._("실행"), T._("취소"));
			})
			.On<TimedOut>(delegate
			{
				if (confirming)
				{
					confirming = false;
					UIManager.MessageBox.Hide();
				}
			});
	}

	private void OnTimer(Messages.Timer msg, PacketHeader header)
	{
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		TimerData.Timer timer = new TimerData.Timer(PlayerBehavior.LocalPlayer.EntityId, "item_crafting", msg.Duration);
		if (_slotContainer.IsInit)
		{
			ItemWindowProgressGauge itemWindowProgressGauge = TimerData.Timer.Play<ItemWindowProgressGauge>(timer);
			List<ItemData> list = new List<ItemData>();
			for (int i = 0; i < _slotContainer.SlotCount; i++)
			{
				SlotInfo slotInfo = _slotContainer.GetSlotInfo(i);
				if (slotInfo != null)
				{
					list.AddRange(slotInfo.SelectedItems);
				}
			}
			itemWindowProgressGauge.SetData(_slotContainer.Recipe.LocalizedName, list);
			List<TagData> tags = _slotContainer.CreateMaterialsTags();
			MotionMap.Instance().GetCraftMotion(_slotContainer.Recipe.Id, (!((Object)(object)_slotContainer.Workbench != (Object)null)) ? string.Empty : _slotContainer.Workbench.ArtifactId, tags, out var motion, out var equip);
			if (equip == null)
			{
				equip = Util.GetItemModel((!_slotContainer.Tool.ToolRequired) ? null : _slotContainer.Tool.GetSelectedItem(), PlayerBehavior.LocalPlayer.IsMale);
			}
			if ((Object)(object)_slotContainer.Workbench != (Object)null)
			{
				KSingleton<PlayerController>.Instance().RotateToPosition(_slotContainer.Workbench.InteractionPosition, bSnap: true);
			}
			KSingleton<PlayerController>.Instance().Motion(equip: equip, motionState: motion, time: itemWindowProgressGauge.RemainTime());
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Craft, _slotContainer.Recipe.LocalizedName);
		}
		else
		{
			TimerData.Timer.Play<DefaultProgressGauge>(timer);
		}
	}

	private void OnCrafted(Crafted crafted, PacketHeader header)
	{
		if (crafted.Result == Result.BigFailure && this.CraftFailed != null)
		{
			this.CraftFailed(_slotContainer.Recipe.Id, crafted.ActionInfo);
		}
		if (this.CraftingFinished != null)
		{
			this.CraftingFinished(CreateItemDataArray(crafted.Items), _slotContainer.Recipe.Id);
		}
		if (crafted.Result == Result.GreatSuccess)
		{
			PlayerController.PlayRewardMotion(RewardAlarmGroup.RewardReason.Success);
		}
	}

	private void SendEstimateResultMessage()
	{
		EstimateCraft estimateCraft = default(EstimateCraft);
		estimateCraft.RecipeId = _slotContainer.Recipe.Id;
		estimateCraft.Materials = _slotContainer.CreateMaterialsDictionary();
		estimateCraft.ToolItemId = _slotContainer.GetToolItemId();
		estimateCraft.WorkbenchEntityId = _slotContainer.GetWorkbenchEntityId();
		estimateCraft.WorkbenchTile = _slotContainer.GetWorkbenchTile();
		EstimateCraft msg = estimateCraft;
		Connections.Frontend.Send(msg).On(delegate(CraftEstimation estimation, PacketHeader _)
		{
			_slotContainer.UpdateEstimateResult(estimation);
		}).On<Error>(delegate
		{
			_slotContainer.UpdateEstimateResult(null);
		});
	}

	public static void DyeingEstimate(Artifact workbench, ItemData item, ItemData dye, ColorChannel channel, [NotNull] Action<CraftEstimation> onResult)
	{
		Recipe recipe = ((!dye.HasTag("decolorizer")) ? GameSystem<RecipeSystem>.Instance().GetDyeingRecipe(channel) : GameSystem<RecipeSystem>.Instance().GetBleachingRecipe(channel));
		if (recipe != null)
		{
			Dictionary<string, ulong[]> dictionary = new Dictionary<string, ulong[]>();
			for (int i = 0; i < recipe.Slots.Length; i++)
			{
				RecipeSlot recipeSlot = recipe.Slots[i];
				dictionary.Add(recipeSlot.Id, (!recipeSlot.IsModifyBase) ? new ulong[1] { dye.Id } : new ulong[1] { item.Id });
			}
			EstimateCraft estimateCraft = default(EstimateCraft);
			estimateCraft.RecipeId = recipe.Id;
			estimateCraft.Materials = dictionary;
			estimateCraft.WorkbenchEntityId = workbench.EntityId;
			estimateCraft.WorkbenchTile = workbench.WorldTile;
			EstimateCraft msg = estimateCraft;
			Connections.Frontend.Send(msg).On(delegate(CraftEstimation r, PacketHeader h)
			{
				onResult(r);
			});
		}
	}

	public void Dyeing(Artifact workbench, ItemData item, ItemData dye, ColorChannel channel)
	{
		Recipe recipe = ((!dye.HasTag("decolorizer")) ? GameSystem<RecipeSystem>.Instance().GetDyeingRecipe(channel) : GameSystem<RecipeSystem>.Instance().GetBleachingRecipe(channel));
		SlotContainer.Set(recipe, workbench, GameSystem<InventorySystem>.Instance().PlayerInventory);
		for (int i = 0; i < SlotContainer.SlotCount; i++)
		{
			SlotInfo slotInfo = SlotContainer.GetSlotInfo(i);
			slotInfo.AddSelectedItems((!(slotInfo.Id == "base")) ? new ItemData[1] { dye } : new ItemData[1] { item });
		}
		Dictionary<string, ulong[]> materials = SlotContainer.CreateMaterialsDictionary();
		ReplyMessageHandlerRegistrar handler = ((!dye.HasTag("decolorizer")) ? Connections.Frontend.Send(new Dye
		{
			WorkbenchEntityId = workbench.EntityId,
			WorkbenchTile = workbench.WorldTile,
			Channel = channel,
			Materials = materials
		}) : Connections.Frontend.Send(new Bleach
		{
			WorkbenchEntityId = workbench.EntityId,
			WorkbenchTile = workbench.WorldTile,
			Channel = channel,
			Materials = materials
		}));
		RegisterPostCraftEvents(handler);
	}

	private static ItemData[] CreateItemDataArray(Item[] items)
	{
		ItemData[] array = new ItemData[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			Item itemInfo = items[i];
			array[i] = new ItemData(itemInfo);
		}
		return array;
	}
}
