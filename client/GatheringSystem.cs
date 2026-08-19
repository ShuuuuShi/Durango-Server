using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Crafting;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Timer;
using Durango.MotionInfo;
using Durango.Network;
using Durango.Terrain;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Item;
using UnityEngine;

public class GatheringSystem : GameSystem<GatheringSystem>
{
	private readonly List<GatheringData> _gatheringList = new List<GatheringData>();

	public GatheringData _currentGatheringData;

	[CanBeNull]
	private ItemData _currentGatheringTool;

	private string _lastGatherSize;

	private readonly HashSet<string> _hasCollectiblePermissionEntities = new HashSet<string>();

	public PredictTimer _collectTimer;

	[CompilerGenerated]
	private static Connection.MessageHandler<ToolNeeded> cache0;

	private GatheringData CurrentGatheringData
	{
		get
		{
			return _currentGatheringData;
		}
		set
		{
			if (_currentGatheringData != value)
			{
				_currentGatheringData = value;
				_collectTimer.Stop();
			}
		}
	}

	public bool IsGathering => CurrentGatheringData != null;

	public event Action<Item> ItemCollected;

	public event Action<SkillNeeded> SkillNeeded;

	public event Action<string, bool> CollectiblePermissionChanged;

	public event Action TargetRunOut;

	public event Action GatheringFailed;

	private void Awake()
	{
		Connections.Frontend.On<Collectible>(OnCollectible);
		Connections.Frontend.On<CollectibleChanged>(OnCollectibleChanged);
		Connections.Frontend.On<Collected>(OnCollected);
		Connections.Frontend.On<CollectibleDisplay>(OnCollectibleDisplay);
		GameSystem<InventorySystem>.Instance().PlayerItemExpired += InventorySystem_PlayerItemExpired;
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += InteractionSystem_InteractionTargetSelected;
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			InitCollectTimer();
		};
		Singleton<GameManager>.Instance().PreReconnect += delegate
		{
			_hasCollectiblePermissionEntities.Clear();
		};
	}

	private void InitCollectTimer()
	{
		_collectTimer = new PredictTimer(GameManager.PlayerId, Interaction.Collect.ToString());
		GameSystem<InteractionSystem>.Instance().MenuList.RegisterTimer(Interaction.Collect, _collectTimer, () => (CurrentGatheringData != null) ? CurrentGatheringData.GeneratorId : null);
	}

	private void OnCollectible(Collectible msg, PacketHeader header)
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		SetCollectible(msg, menuList);
		menuList.Apply();
	}

	private void OnCollectibleChanged(CollectibleChanged msg, PacketHeader header)
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null && target.EntityId == msg.EntityId)
		{
			RequestCollectibleMsg(target.EntityId, new Point2(target.Tile));
		}
	}

	private void RequestCollectibleMsg(string entityId, Point2 tile)
	{
		Connections.Frontend.Send(new GetCollectible
		{
			EntityId = entityId,
			Tile = tile
		}).On<Collectible>(OnCollectible);
	}

	private void OnCollected(Collected msg, PacketHeader header)
	{
		PlayCollectedAlarm(msg);
		Item obj = msg.Items.FirstOrDefault();
		if (!string.IsNullOrEmpty(obj.Id) && this.ItemCollected != null)
		{
			this.ItemCollected(obj);
		}
		CurrentGatheringData = null;
		if (msg.RanOut && this.TargetRunOut != null)
		{
			this.TargetRunOut();
		}
		if (msg.Result == Result.BigFailure)
		{
			PlayerController.MotionUpdater.Motion("Craft_Fail");
		}
	}

	private void PlayCollectedAlarm(Collected m)
	{
		Item item = m.Items.FirstOrDefault();
		AlarmRewardQueue.Args args = default(AlarmRewardQueue.Args);
		args.Sub = Durango.Logic.Item.Util.ItemQualityString(item);
		args.Icon = new ItemIcon(item);
		AlarmRewardQueue.Args args2 = args;
		string eventName;
		AlarmGroup.RewardEffectType type;
		switch (m.Result)
		{
		default:
			return;
		case Result.BigFailure:
			args2.Main = T._("채집 실패");
			args2.Sub = T._("성공 확률 {0:P0}", m.ActionInfo.SuccessRatio);
			args2.Icon = ((CurrentGatheringData != null) ? CurrentGatheringData.Icon : "icon_question");
			eventName = "UI_Gathering_Fail_01";
			type = AlarmGroup.RewardEffectType.CollectFail;
			break;
		case Result.Failure:
			args2.Main = T._("{1:lv:} <em>{0}</em> 채집 실수", item.Name, item.Level);
			eventName = "UI_Gathering_Fail_01";
			type = AlarmGroup.RewardEffectType.Collect;
			break;
		case Result.Success:
			args2.Main = T._("{1:lv:} <em>{0}</em> 채집", item.Name, item.Level);
			eventName = "UI_Gathering_Success_01";
			type = AlarmGroup.RewardEffectType.Collect;
			break;
		case Result.GreatSuccess:
			args2.Main = T._("{1:lv:} <em>{0}</em> 채집 <em>대성공!</em>", item.Name, item.Level);
			eventName = "UI_Gathering_Great_01";
			type = AlarmGroup.RewardEffectType.Collect;
			break;
		}
		UIManager.Alarm.RewardAlarm(args2, type);
		SoundManager.PlayEvent(eventName);
	}

	private void OnCollectibleDisplay(CollectibleDisplay msg, PacketHeader header)
	{
		UpdateCollectibleDisplay(msg.EntityId, msg);
	}

	public void UpdateCollectibleDisplay(string entityId, CollectibleDisplay? msg)
	{
		if (msg.HasValue && msg.Value.DistributableEntities.Contains(PlayerBehavior.LocalPlayer.EntityId))
		{
			if (_hasCollectiblePermissionEntities.Add(entityId) && this.CollectiblePermissionChanged != null)
			{
				this.CollectiblePermissionChanged(entityId, arg2: true);
			}
		}
		else if (_hasCollectiblePermissionEntities.Remove(entityId) && this.CollectiblePermissionChanged != null)
		{
			this.CollectiblePermissionChanged(entityId, arg2: false);
		}
	}

	public bool HasCollectiblePermission(string entityId)
	{
		return _hasCollectiblePermissionEntities.Contains(entityId);
	}

	public void SetCollectible(Collectible msg, InteractionMenuList menuList)
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target == null || target.EntityId != msg.EntityId)
		{
			return;
		}
		_lastGatherSize = msg.Size;
		int i = 0;
		for (int count = _gatheringList.Count; i < count; i++)
		{
			_gatheringList[i].IsValid = false;
		}
		int j = 0;
		for (int num = msg.Generators.Length; j < num; j++)
		{
			Generator gen = msg.Generators[j];
			bool isCritical = gen.Id == msg.CriticalGenerator;
			int num2 = GatheringDataIndexOf(gen.Id);
			GatheringData gatheringData;
			if (num2 != -1)
			{
				gatheringData = _gatheringList[num2];
				gatheringData.Set(gen, isCritical);
			}
			else
			{
				gatheringData = new GatheringData(gen, isCritical);
				_gatheringList.Add(gatheringData);
			}
			gatheringData.FindBestTool(GameSystem<InventorySystem>.Instance().PlayerInventory.Items);
		}
		for (int num3 = _gatheringList.Count - 1; num3 >= 0; num3--)
		{
			if (!_gatheringList[num3].IsValid)
			{
				_gatheringList.RemoveAt(num3);
			}
		}
		for (int num4 = menuList.Count - 1; num4 >= 0; num4--)
		{
			if (menuList[num4].Action == Interaction.Collect)
			{
				menuList.RemoveAt(num4);
			}
		}
		Touched lastTouched = GameSystem<InteractionSystem>.Instance().LastTouched;
		int parentLevel = ((lastTouched.EntityId == msg.EntityId) ? lastTouched.Level : 0);
		for (int k = 0; k < _gatheringList.Count; k++)
		{
			InteractionMenuData data = new InteractionMenuData(_gatheringList[k], parentLevel);
			menuList.Add(data);
		}
		if (KUtility.GetSize(msg.Generators) > 0 && HasCollectiblePermission(msg.EntityId))
		{
			menuList.Add(new InteractionMenuData(Interaction.GiveUpDistribution));
		}
	}

	private void InventorySystem_PlayerItemExpired(ItemExpired expired)
	{
		int i = 0;
		for (int count = _gatheringList.Count; i < count; i++)
		{
			GatheringData gatheringData = _gatheringList[i];
			if (gatheringData.BestTool != null && expired.ItemNames.ContainsKey(gatheringData.BestTool.Id))
			{
				InventoryUpdated();
				break;
			}
		}
	}

	private void InventoryUpdated()
	{
		if (_gatheringList != null)
		{
			int i = 0;
			for (int count = _gatheringList.Count; i < count; i++)
			{
				_gatheringList[i].FindBestTool(GameSystem<InventorySystem>.Instance().PlayerInventory.Items);
			}
		}
	}

	public void Gathering(string id)
	{
		UIManager.SystemMsg("test");
		GatheringData gatheringData = FindGatheringData(id);
		if (gatheringData != null)
		{
			Gathering(gatheringData);
		}
	}

	private void Gathering([NotNull] GatheringData data)
	{
		ItemData currentTool = data.BestTool;
		if (data.CanGateringWithThisTool(currentTool) == null)
		{
			DoGathering(data, currentTool);
		}
		else if (currentTool != null)
		{
			UIManager.MessageBox.ShowLockConfirm(currentTool, delegate
			{
				DoGathering(data, currentTool);
			});
		}
	}

	private void DoGathering([NotNull] GatheringData data, [CanBeNull] ItemData tool)
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null && !(target.Target == null))
		{
			StopCollectTimer();
			CurrentGatheringData = data;
			_currentGatheringTool = tool;
			float distance = target.CalcInteractionDistance();
			Singleton<PlayerController>.Instance().MoveToTarget(target.Target, ReadyForGathering, distance);
		}
	}

	private int GatheringDataIndexOf(string id)
	{
		int i = 0;
		for (int count = _gatheringList.Count; i < count; i++)
		{
			if (_gatheringList[i].GeneratorId == id)
			{
				return i;
			}
		}
		return -1;
	}

	private GatheringData FindGatheringData(string id)
	{
		int num = GatheringDataIndexOf(id);
		if (num == -1)
		{
			return null;
		}
		return _gatheringList[num];
	}

	private void ReadyForGathering()
	{
		bool confirming = false;
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		if (lastInteractionTarget == null || CurrentGatheringData == null)
		{
			return;
		}
		if (CurrentGatheringData.IsAvailableForGathering())
		{
			MakePredictGahteringTimer();
		}
		Connections.Frontend.Send(new Collect
		{
			EntityId = lastInteractionTarget.EntityId,
			Tile = new Point2((int)lastInteractionTarget.Tile.x, (int)lastInteractionTarget.Tile.y),
			GeneratorId = CurrentGatheringData.GeneratorId,
			Level = CurrentGatheringData.Level,
			ToolItemId = ((_currentGatheringTool == null) ? string.Empty : _currentGatheringTool.Id)
		}).On<Messages.Timer>(OnGatheringTimer).On<ToolNeeded>(OnToolNeeded)
			.On(delegate(SkillNeeded msg, PacketHeader header)
			{
				GameSystem<SkillSystem>.Instance().SkillNeeded(msg);
				if (this.SkillNeeded != null)
				{
					this.SkillNeeded(msg);
				}
			})
			.On(delegate(EnergyWarning msg, PacketHeader header)
			{
				if (PauseCollectTimer())
				{
					confirming = true;
					LowEnergyWarning.Show(msg, header, delegate(LowEnergyWarning.Result result)
					{
						if (result == LowEnergyWarning.Result.IgnoreWarning)
						{
							MakePredictGahteringTimer();
						}
						else
						{
							StopCollectTimer();
							OnGatheringFailed();
						}
						confirming = false;
					});
				}
			})
			.All(delegate(Packet packet)
			{
				uint typeCode = packet.Header.TypeCode;
				if (typeCode != 104 && typeCode != 1134 && typeCode != 2019 && typeCode != 3648)
				{
					if (confirming)
					{
						confirming = false;
						LowEnergyWarning.Hide();
					}
					StopCollectTimer();
					OnGatheringFailed();
				}
			});
	}

	private bool PauseCollectTimer()
	{
		if (_collectTimer.Timer.IsStop)
		{
			return false;
		}
		_collectTimer.Pause();
		return true;
	}

	private void PlayCollectTimer(float duration)
	{
		_collectTimer.Play(duration);
	}

	private void StopCollectTimer()
	{
		_collectTimer.Stop(raiseEvent: false);
	}

	private static void OnToolNeeded(ToolNeeded msg, PacketHeader header)
	{
		int num = 0;
		foreach (KeyValuePair<string, int> tag in msg.Tags)
		{
			num = Mathf.Max(tag.Value, num);
		}
		string comment = ((num <= 1) ? T._("다음 중 하나의 도구가 필요합니다.\n<em>{0}</em>", msg.TagNames) : T._("다음 중 하나의 도구가 필요합니다.\n{1:lv:}이상 <em>{0}</em>", msg.TagNames, num));
		ShowRequireTagPopup(msg.RecipeIds, msg.Tags, num, comment);
	}

	public void GiveUpDistribution(PropKey key)
	{
		Connections.Frontend.Send(new GiveUpDistribution
		{
			EntityId = key.EntityId,
			Tile = key.Tile
		});
	}

	public static void ShowRequireTagPopup(string[] recipeIds, Dictionary<string, int> requireTags, int level, string comment)
	{
		ConfirmPopup confirmPopup = UIManager.Popup.Tooltip<ConfirmPopup>();
		if (KUtility.GetSize(recipeIds) > 0)
		{
			Recipe recipe = null;
			for (int i = 0; i < recipeIds.Length; i++)
			{
				Recipe recipe2 = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeIds[i]);
				if (recipe2 != null && recipe2.Available)
				{
					recipe = recipe2;
					break;
				}
			}
			confirmPopup.AddButton(new MessageBox.Button((recipe != null) ? T._("제작하기") : T._("스킬 배우기")), delegate
			{
				RecipeSelectorGroup.OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Crafting, (recipe != null) ? recipe.Id : recipeIds[0]);
			});
		}
		if (KUtility.GetSize(requireTags) > 0 && GameSystem<MenuSystem>.Instance().IsEnabled(Durango.Logic.MenuType.Market))
		{
			confirmPopup.AddButton(T._("장터 구매"), delegate
			{
				MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
				List<TagFilterBase.Tag> list = new List<TagFilterBase.Tag>();
				foreach (KeyValuePair<string, int> requireTag in requireTags)
				{
					list.Add(new TagFilterBase.Tag(requireTag.Key, requireTag.Value));
				}
				marketGroup.OpenAndSearch(new OrTagFilter(list), null, level);
			});
		}
		confirmPopup.Show(comment, 600f);
	}

	private void MakePredictGahteringTimer()
	{
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		if (lastInteractionTarget == null || CurrentGatheringData == null)
		{
			StopCollectTimer();
			return;
		}
		PlayCollectTimer(CurrentGatheringData.Duration + Connections.Frontend.Ping + 2f);
		string model = _currentGatheringTool.GetModel(PlayerBehavior.LocalPlayer.IsMale);
		string motion = null;
		string toolTag = ((CurrentGatheringData.BestTool != null) ? CurrentGatheringData.CanGateringWithThisTool(CurrentGatheringData.BestTool) : "bare_hands");
		ImmovableBase targetComponent = lastInteractionTarget.GetTargetComponent<ImmovableBase>();
		if (targetComponent != null)
		{
			BiomeSpriteInfo biomeSpriteInfo = DataHelper.GetBiomeSpriteInfo(targetComponent.EntityType);
			motion = MotionMap.Instance().GetGatheringMotion(toolTag, CurrentGatheringData.GeneratorId, targetComponent.EntityType, biomeSpriteInfo, _lastGatherSize);
		}
		else
		{
			CharacterBehavior targetComponent2 = lastInteractionTarget.GetTargetComponent<CharacterBehavior>();
			if (targetComponent2 != null)
			{
				int entityTypeId = targetComponent2.EntityTypeId;
				motion = MotionMap.Instance().GetGatheringMotion(toolTag, CurrentGatheringData.GeneratorId, entityTypeId, _lastGatherSize);
			}
		}
		_collectTimer.SetMotion(motion, model, (_currentGatheringTool == null) ? default(ItemColor) : _currentGatheringTool.Colors, 0.5f);
	}

	private void OnGatheringTimer(Messages.Timer msg, PacketHeader header)
	{
		if (CurrentGatheringData == null)
		{
			StopCollectTimer();
			return;
		}
		CurrentGatheringData.Duration = msg.Duration;
		if (CurrentGatheringData.DurationChanged != null)
		{
			CurrentGatheringData.DurationChanged(msg.Duration);
		}
		PlayCollectTimer(msg.Duration);
	}

	private void InteractionSystem_InteractionTargetSelected(InteractionObject interactionObject)
	{
		CurrentGatheringData = null;
	}

	private void OnGatheringFailed()
	{
		StopCollectTimer();
		CurrentGatheringData = null;
		if (this.GatheringFailed != null)
		{
			this.GatheringFailed();
		}
	}
}
