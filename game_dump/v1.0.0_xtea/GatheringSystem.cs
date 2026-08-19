using System;
using System.Collections.Generic;
using InteractionData;
using ItemSystem;
using JetBrains.Annotations;
using K1Network;
using L10N;
using Messages;
using Shared.Item;
using Shared.System;
using TimerData;
using UnityEngine;

public class GatheringSystem : GameSystem<GatheringSystem>
{
	private readonly List<GatheringData> _gatheringList = new List<GatheringData>();

	private readonly List<GatheringQueueData> _gatheringQueue = new List<GatheringQueueData>();

	private GatheringData _currentGatheringData;

	private ItemData _currentGatheringTool;

	private TimerData.Timer _currentGatheringTimer;

	private string _requireTag;

	private string _lastGatherSize;

	public List<GatheringQueueData> GatheringQueue => _gatheringQueue;

	public event Action GatheringQueueUpdated;

	public event Action<string> CollectError;

	private void Awake()
	{
		Connections.Frontend.On<Collectible>(CollectibleReceived);
		Connections.Frontend.On<CollectibleChanged>(OnCollectibleChanged);
		GameSystem<InventorySystem>.Instance().PlayerItemExpired += OnItemExpired;
		GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += OnTargetSelect;
	}

	private void ResetGatheringData()
	{
		_currentGatheringData = null;
		_gatheringQueue.Clear();
		OnUpdateGatheringQueue();
		if (_currentGatheringTimer != null)
		{
			_currentGatheringTimer.Stop();
		}
	}

	private void OnCollectibleChanged(CollectibleChanged msg, PacketHeader header)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null && target.EntityId == msg.EntityId)
		{
			RequestCollectibleMsg(target.EntityId, new Point2(target.Tile));
		}
	}

	private void RequestCollectibleMsg(ulong entityId, Point2 tile)
	{
		Connections.Frontend.Send(new GetCollectible
		{
			EntityId = entityId,
			Tile = tile
		}).On<Collectible>(CollectibleReceived);
	}

	private void CollectibleReceived(Collectible msg, PacketHeader header)
	{
		SetCollectible(msg, refreshInventory: false);
	}

	public void SetCollectible(Collectible msg, bool refreshInventory)
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
		if (refreshInventory)
		{
			GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		}
		int j = 0;
		for (int num = msg.Generators.Length; j < num; j++)
		{
			Generator gen = msg.Generators[j];
			int num2 = GatheringDataIndexOf(gen.Id);
			GatheringData gatheringData;
			if (num2 != -1)
			{
				gatheringData = _gatheringList[num2];
				gatheringData.Set(gen);
			}
			else
			{
				gatheringData = new GatheringData(gen);
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
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		int num4 = 506;
		for (int num5 = menuList.Count - 1; num5 >= 0; num5--)
		{
			if (menuList[num5].IsServer && menuList[num5].Action == num4)
			{
				menuList.RemoveAt(num5);
			}
		}
		for (int k = 0; k < _gatheringList.Count; k++)
		{
			InteractionMenuData data = new InteractionMenuData(Shared.System.Interaction.Collect);
			data.Set(_gatheringList[k]);
			if (_currentGatheringData == _gatheringList[k])
			{
				data.SetTimer(_currentGatheringTimer);
			}
			menuList.Add(data);
		}
		menuList.Apply();
	}

	private void OnTargetSelect(InteractionObject obj)
	{
		ResetGatheringData();
	}

	private void OnItemExpired(ulong id)
	{
		int i = 0;
		for (int count = _gatheringList.Count; i < count; i++)
		{
			GatheringData gatheringData = _gatheringList[i];
			if (gatheringData.BestTool.Id == id)
			{
				GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded(OnUpdateInventory);
				break;
			}
		}
	}

	private void OnUpdateInventory()
	{
		if (_gatheringList != null)
		{
			int i = 0;
			for (int count = _gatheringList.Count; i < count; i++)
			{
				GatheringData gatheringData = _gatheringList[i];
				gatheringData.FindBestTool(GameSystem<InventorySystem>.Instance().PlayerInventory.Items);
			}
		}
	}

	public void Gathering(string id)
	{
		GatheringData gatheringData = FindGatheringData(id);
		if (gatheringData != null)
		{
			Gathering(gatheringData);
		}
	}

	private void Gathering([NotNull] GatheringData data)
	{
		ItemData currentTool = data.BestTool;
		_requireTag = data.CanGateringWithThisTool(currentTool);
		if (_requireTag == null)
		{
			if (_currentGatheringData == null)
			{
				Gathering(data, currentTool);
			}
		}
		else if (_currentGatheringData != null)
		{
			int num = data.Amount;
			if (_currentGatheringData.Id == data.Id)
			{
				num--;
			}
			for (int i = 0; i < _gatheringQueue.Count; i++)
			{
				if (_gatheringQueue[i].Data.Id == data.Id)
				{
					num--;
				}
			}
			if (num > 0 && data.BestPerformance > 0)
			{
				_gatheringQueue.Add(new GatheringQueueData(data));
			}
			else if (num < 0)
			{
				for (int num2 = _gatheringQueue.Count - 1; num2 >= 0; num2--)
				{
					if (_gatheringQueue[num2].Data.Id == data.Id)
					{
						_gatheringQueue.RemoveAt(num2);
						num++;
					}
					if (num == 0)
					{
						break;
					}
				}
			}
			OnUpdateGatheringQueue();
		}
		else if (currentTool.Like)
		{
			UIManager.MessageBox.Show(T._("잠금 설정된 도구를 사용하시겠습니까? 잠금이 해제됩니다."), delegate(bool ok)
			{
				if (ok)
				{
					Connections.Frontend.Send(new LabelItems
					{
						Label = 1,
						Active = false,
						ItemIds = new ulong[1] { currentTool.Id }
					});
					Gathering(data, currentTool);
				}
			});
		}
		else
		{
			Gathering(data, currentTool);
		}
	}

	private void Gathering([NotNull] GatheringData data, ItemData tool)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null && !((Object)(object)target.Target == (Object)null))
		{
			_currentGatheringData = data;
			_currentGatheringTool = tool;
			float distanceThresh = InteractionSystem.CalcInteractionDistance(target);
			KSingleton<PlayerController>.Instance().MoveToTarget(target.Position, ReadyForGathering, distanceThresh);
		}
	}

	private int GatheringDataIndexOf(string id)
	{
		int i = 0;
		for (int count = _gatheringList.Count; i < count; i++)
		{
			if (_gatheringList[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private GatheringData FindGatheringData(string id)
	{
		int num = GatheringDataIndexOf(id);
		return (num != -1) ? _gatheringList[num] : null;
	}

	private void ReadyForGathering(GameObject obj)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		bool confirming = false;
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		Connections.Frontend.Send(new Collect
		{
			EntityId = lastInteractionTarget.EntityId,
			Tile = new Point2((int)lastInteractionTarget.Tile.x, (int)lastInteractionTarget.Tile.y),
			GeneratorId = _currentGatheringData.Id,
			Level = _currentGatheringData.Level,
			ToolItemId = ((_currentGatheringTool == null) ? 0 : _currentGatheringTool.Id)
		}).On<Messages.Timer>(OnGatheringTimer).On(delegate(Collected msg, PacketHeader _)
		{
			GameSystem<InventorySystem>.Instance().CollectedReceived(msg);
			OnGatheringSuccess(msg.RanOut, msg.Result);
		})
			.On(delegate(ToolNeeded msg, PacketHeader _)
			{
				Action onClick = ((KUtility.GetSize(msg.RecipeIds) != 0) ? ((Action)delegate
				{
					UIManager.FindScript<RecipeSelectorGroup>().Open(RecipeSystem.RecipeType.Crafting, msg.RecipeIds[0]);
				}) : null);
				UIManager.SystemMsg("tool_needed", T._("도구가 필요합니다: {0}", msg.TagNames), 3f, onClick);
			})
			.On(delegate(Error msg, PacketHeader header)
			{
				OnGatheringFail();
				if (this.CollectError != null)
				{
					this.CollectError(msg.TypeName);
				}
				GameManager.DefaultErrorHandler(msg, header);
			})
			.On(delegate(SkillNeeded msg, PacketHeader header)
			{
				GameSystem<SkillSystem>.Instance().OnSkillNeededMsg(msg, header);
			})
			.On(delegate(EnergyWarning msg, PacketHeader header)
			{
				confirming = true;
				UIManager.MessageBox.Show(T._("에너지가 모자라는 상태로 이 행동을 하면 건강이 소모됩니다."), delegate(int select)
				{
					bool flag = select == 0;
					Confirm confirm = default(Confirm);
					confirm.Confirmation = flag;
					Confirm msg2 = confirm;
					Connection frontend = Connections.Frontend;
					ulong replyOf = header.ReplyOf;
					frontend.Send(msg2, noReply: false, replyOf);
					confirming = false;
					if (!flag)
					{
						ResetGatheringData();
					}
				}, T._("실행"), T._("취소"));
			})
			.On<TimedOut>(delegate
			{
				if (confirming)
				{
					confirming = false;
					UIManager.MessageBox.Hide();
					OnGatheringFail();
				}
			});
	}

	private void OnGatheringTimer(Messages.Timer msg, PacketHeader header)
	{
		if (_currentGatheringData == null)
		{
			return;
		}
		if (_currentGatheringTimer == null)
		{
			_currentGatheringTimer = new TimerData.Timer("collect", msg.Duration);
			_currentGatheringTimer.Finished += delegate(TimerData.Timer timer)
			{
				if (timer.IsInterrupt)
				{
					OnGatheringFail();
				}
			};
		}
		else
		{
			_currentGatheringTimer.SetDuration("collect", msg.Duration);
		}
		GameSystem<TimerSystem>.Instance().Register(_currentGatheringTimer);
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		string itemModel = Util.GetItemModel(_currentGatheringTool, PlayerBehavior.LocalPlayer.IsMale);
		string motionState = null;
		if (target != null)
		{
			ImmovableBase targetComponent = target.GetTargetComponent<ImmovableBase>();
			if ((Object)(object)targetComponent != (Object)null)
			{
				BiomeSpriteInfo biomeSpriteInfo = TerrainDataHelper.GetBiomeSpriteInfo(targetComponent.EntityType);
				motionState = MotionMap.Instance().GetGatheringMotion(_requireTag, _currentGatheringData.Id, biomeSpriteInfo, _lastGatherSize);
			}
			else
			{
				CharacterBehavior targetComponent2 = target.GetTargetComponent<CharacterBehavior>();
				if ((Object)(object)targetComponent2 != (Object)null)
				{
					int entityTypeId = targetComponent2.EntityTypeId;
					motionState = MotionMap.Instance().GetGatheringMotion(_requireTag, _currentGatheringData.Id, entityTypeId, _lastGatherSize);
				}
			}
		}
		InteractionMenuData data = new InteractionMenuData(Shared.System.Interaction.Collect);
		data.Set(_currentGatheringData);
		data.SetTimer(_currentGatheringTimer);
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Add(data);
		menuList.Apply();
		KSingleton<PlayerController>.Instance().Motion(equip: itemModel, color: _currentGatheringTool.Colors, motionState: motionState, time: msg.Duration + 1f);
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Gather);
	}

	private void OnGatheringSuccess(bool ranOut, Result result)
	{
		_currentGatheringData = null;
		if (ranOut)
		{
			ResetGatheringData();
		}
		if (_gatheringQueue.Count > 0)
		{
			GatheringData data = _gatheringQueue[0].Data;
			_gatheringQueue.RemoveAt(0);
			Gathering(data);
			OnUpdateGatheringQueue();
			return;
		}
		KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
		if (result == Result.BigFailure)
		{
			KSingleton<PlayerController>.Instance().Motion("Craft_Fail", 0f, 1f, forceTransition: true);
		}
		OnUpdateGatheringQueue();
	}

	private void OnGatheringFail()
	{
		ResetGatheringData();
		KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
	}

	private void OnUpdateGatheringQueue()
	{
		if (this.GatheringQueueUpdated != null)
		{
			this.GatheringQueueUpdated();
		}
	}

	public void RemoveGatheringQueue(string id)
	{
		for (int i = 0; i < _gatheringQueue.Count; i++)
		{
			if (_gatheringQueue[i].Data.Id == id)
			{
				_gatheringQueue.RemoveAt(i);
				break;
			}
		}
		OnUpdateGatheringQueue();
	}
}
