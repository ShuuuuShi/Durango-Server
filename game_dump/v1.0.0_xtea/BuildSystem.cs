using System;
using System.Collections;
using System.Collections.Generic;
using BuildData;
using Building_;
using ItemSystem;
using JetBrains.Annotations;
using K1Network;
using L10N;
using Messages;
using Shared.Economy;
using Shared.MessageBoard;
using Shared.System;
using TimerData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class BuildSystem : GameSystem<BuildSystem>
{
	private const string BuildFinishedParticle = "Particle/FX_BuildingCom_01.prefab";

	private const string BuildFinishedSound = "Sound/Effect/UI/UI_Building_Success_01.wav";

	private const string CapsulateArtifactParticle = "Particle/FX_CapsulateArtifact.prefab";

	private const string CapsulateArtifactSound = "Sound/Effect/UI/UI_Capsulate_Artifact_01.wav";

	private readonly BuildSlotContainer _slotContainer = new BuildSlotContainer();

	public BuildSlotContainer SlotContainer => _slotContainer;

	public event Action<Artifact, Destructing> DestructedReplied;

	public event Action<Artifact> BuildFinished;

	public event Action<Artifact> BuildCompleted;

	public event Action<Artifact> ArtifactAppeared;

	public event Action SetHomeSucceed;

	public event Action SetBaseSucceed;

	public event Action<float> BuildStarted;

	public event Action ArtifactOccupied;

	public void ConstructionSiteSelect(Building_.Blueprint blueprint)
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			Vehicle.RequestUnmountIfRiding(immediately: true, delegate
			{
				ConstructionSiteSelectInternal(blueprint);
			});
		}
		else
		{
			ConstructionSiteSelectInternal(blueprint);
		}
	}

	private void ConstructionSiteSelectInternal(Building_.Blueprint blueprint)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		BuildManager buildManager = KSingleton<BuildManager>.Instance();
		ArtifactBuildInfo buildInfo = new ArtifactBuildInfo
		{
			Blueprint = blueprint,
			WorldTile = buildManager.WorldTilePos,
			Size = buildManager.Size,
			Rotated = buildManager.Rotated
		};
		Point2 correctedWorldTilePos = buildManager.CorrectedWorldTilePos;
		Vector3 val = TerrainA6.WorldPositionToClientPosition(TerrainA6.TilePositionToWorldPosition(correctedWorldTilePos));
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector((float)buildInfo.Size.x, 0f, (float)buildInfo.Size.y);
		val += val2 * 200f * 0.5f;
		KSingleton<PlayerController>.Instance().MoveToTarget(val, delegate
		{
			SendStartBuildingMessage(buildInfo);
		}, 141f);
		buildManager.ResetBuildingMode();
	}

	public void DestructArtifact(Artifact artifact)
	{
		if ((Object)(object)artifact == (Object)null)
		{
			return;
		}
		DestructArtifact destructArtifact = default(DestructArtifact);
		destructArtifact.EntityId = artifact.EntityId;
		destructArtifact.Tile = artifact.WorldTile;
		DestructArtifact msg = destructArtifact;
		Connections.Frontend.Send(msg).On(delegate(Destructing destructing, PacketHeader _)
		{
			if (this.DestructedReplied != null)
			{
				this.DestructedReplied(artifact, destructing);
			}
		});
	}

	public void RequestRepairCost(Artifact artifact, bool disabled)
	{
		RequestArtifactRepairCost requestArtifactRepairCost = default(RequestArtifactRepairCost);
		requestArtifactRepairCost.EntityId = artifact.EntityId;
		requestArtifactRepairCost.Tile = artifact.WorldTile;
		RequestArtifactRepairCost msg = requestArtifactRepairCost;
		if (disabled)
		{
			RepairArtifact(artifact, new KeyValuePair<int, int>(0, 0));
			return;
		}
		Connections.Frontend.Send(msg).On(delegate(ArtifactRepairCost costMsg, PacketHeader _)
		{
			int num = Mathf.Min(costMsg.CostRange.Key, costMsg.CostRange.Value);
			long balance = GameSystem<InventorySystem>.Instance().PlayerInventory.GetBalance(Currency.TStone);
			if (balance >= num)
			{
				UIManager.MessageBox.Show(LocalizeSystem.Format("#interaction_repair_cost_okcancel", ItemSystem.Inventory.CurrencyFormat(num, Currency.TStone)), delegate(bool ok)
				{
					if (ok)
					{
						RepairArtifact(artifact, costMsg.CostRange);
					}
				});
			}
			else
			{
				UIManager.MessageBox.Show(LocalizeSystem.Format("#interaction_repair_cost_ok", ItemSystem.Inventory.CurrencyFormat(num, Currency.TStone), ItemSystem.Inventory.CurrencyFormat(balance, Currency.TStone)));
			}
		});
	}

	private void RepairArtifact(Artifact artifact, KeyValuePair<int, int> costRange)
	{
		Connections.Frontend.Send(new RepairArtifact
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			CostRange = costRange
		}).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			MotionMap.Instance().GetBuildMotion("Repair", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(msg.Duration, artifact.Blueprint.Icon, motion, equip);
		}).On(delegate(RepairArtifactResult resultMsg, PacketHeader _)
		{
			UIManager.SystemMsg(resultMsg.Text);
		});
	}

	public static void CompleteArtifact(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			Connections.Frontend.Send(new CompleteArtifact
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}
	}

	public static void Rest(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			Connections.Frontend.Send(new RestOn
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}
	}

	public static void ArtifactAction(string action, Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			ArtifactAction(action, artifact.EntityId, artifact.WorldTile);
		}
	}

	public static void ArtifactAction(string action, ulong entityId, Point2 tile)
	{
		Connections.Frontend.Send(new RequestArtifact
		{
			EntityId = entityId,
			Tile = tile,
			Action = action
		});
	}

	public static void FarmingAction(string action, Artifact farm, ItemData item, Connection.MessageHandler<Messages.Timer> onTimer = null)
	{
		if (!((Object)(object)farm == (Object)null))
		{
			RequestFarm requestFarm = default(RequestFarm);
			requestFarm.Action = action;
			requestFarm.EntityId = farm.EntityId;
			requestFarm.Tile = farm.WorldTile;
			requestFarm.ItemId = item?.Id ?? 0;
			RequestFarm msg = requestFarm;
			Connections.Frontend.Send(msg).On(onTimer);
		}
	}

	public static void FarmingAction(string action, Artifact farm, IList<ItemData> items, Connection.MessageHandler<Messages.Timer> onTimer = null)
	{
		if (!((Object)(object)farm == (Object)null))
		{
			RequestFarm requestFarm = default(RequestFarm);
			requestFarm.Action = action;
			requestFarm.EntityId = farm.EntityId;
			requestFarm.Tile = farm.WorldTile;
			requestFarm.ItemId = ((items != null && items.Count != 0) ? items[0].Id : 0);
			RequestFarm msg = requestFarm;
			Connections.Frontend.Send(msg).On(onTimer);
		}
	}

	public static void WaterPlant(Artifact farm, IList<ItemData> items)
	{
		if (!((Object)(object)farm == (Object)null) && items != null && items.Count != 0)
		{
			ulong[] array = new ulong[items.Count];
			for (int i = 0; i < items.Count; i++)
			{
				array[i] = items[i].Id;
			}
			WaterPlant waterPlant = default(WaterPlant);
			waterPlant.EntityId = farm.EntityId;
			waterPlant.Tile = farm.WorldTile;
			waterPlant.ItemIds = array;
			WaterPlant msg = waterPlant;
			Connections.Frontend.Send(msg).On(delegate(Messages.Timer timerMsg, PacketHeader header)
			{
				IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(new TimerData.Timer("watering", timerMsg.Duration));
				iconProgressGauge.SetIcon(IconMap.Get(Interaction.Watering));
				KSingleton<PlayerController>.Instance().Motion("Farming_Water", timerMsg.Duration);
			});
		}
	}

	public static void GrowRapidly(Artifact farm)
	{
		Connections.Frontend.Send(new GrowRapidly
		{
			EntityId = farm.EntityId,
			Tile = farm.WorldTile
		}).On<OK>(delegate
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			ParticleManager.Emit("Particle/FX_BuildingCom_01.prefab", farm.Center, Quaternion.identity);
			SoundManager.Play("Sound/Effect/UI/UI_Building_Success_01.wav", farm.Center);
		});
	}

	public static void FertilizePlant(Artifact farm, IList<ItemData> items)
	{
		if (!((Object)(object)farm == (Object)null) && items != null && items.Count != 0)
		{
			ulong[] array = new ulong[items.Count];
			for (int i = 0; i < items.Count; i++)
			{
				array[i] = items[i].Id;
			}
			FertilizePlant fertilizePlant = default(FertilizePlant);
			fertilizePlant.EntityId = farm.EntityId;
			fertilizePlant.Tile = farm.WorldTile;
			fertilizePlant.ItemIds = array;
			FertilizePlant msg = fertilizePlant;
			Connections.Frontend.Send(msg).On(delegate(Messages.Timer timerMsg, PacketHeader header)
			{
				IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(new TimerData.Timer("fertilizing", timerMsg.Duration));
				iconProgressGauge.SetIcon(IconMap.Get(Interaction.Fertilize));
				KSingleton<PlayerController>.Instance().Motion("Farming_Hoe", timerMsg.Duration);
			});
		}
	}

	public static void SetAsHome(ulong entityId, Point2 tile)
	{
		SetAsHome setAsHome = default(SetAsHome);
		setAsHome.EntityId = entityId;
		setAsHome.Tile = tile;
		SetAsHome msg = setAsHome;
		Connections.Frontend.Send(msg).On<OK>(delegate
		{
			if (GameSystem<BuildSystem>.Instance().SetHomeSucceed != null)
			{
				GameSystem<BuildSystem>.Instance().SetHomeSucceed();
			}
		});
	}

	public static void SetAsBase(ulong entityId, Point2 tile)
	{
		SetAsBase setAsBase = default(SetAsBase);
		setAsBase.EntityId = entityId;
		setAsBase.Tile = tile;
		SetAsBase msg = setAsBase;
		Connections.Frontend.Send(msg).On<OK>(delegate
		{
			if (GameSystem<BuildSystem>.Instance().SetBaseSucceed != null)
			{
				GameSystem<BuildSystem>.Instance().SetBaseSucceed();
			}
		});
	}

	public static void FireBurnable(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			Connections.Frontend.Send(new FireBurnable
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}
	}

	public static void ExtinguishBurnable(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			Connections.Frontend.Send(new ExtinguishBurnable
			{
				EntityId = artifact.EntityId,
				Tile = artifact.WorldTile
			});
		}
	}

	public static void Scribble(Artifact artifact, Drawing type, byte[] data)
	{
		Connections.Frontend.Send(new Scribble
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			Type = type,
			Data = data
		});
	}

	public static void Rename(Artifact artifact, string name)
	{
		Connections.Frontend.Send(new Rename
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			Name = name
		});
	}

	public static void ActiavteMarket(Artifact artifact, string name)
	{
		Connections.Frontend.Send(new ActivateMarket
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			Name = name
		});
	}

	public static void GetAddons(ModularArtifact artifact, Action<AddOns> addons)
	{
		Connections.Frontend.Send(new GetAddOns
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On(delegate(AddOns msg, PacketHeader header)
		{
			addons(msg);
		});
	}

	public static void PlaceAddons(ModularArtifact artifact, ModularAddons addons)
	{
		Connections.Frontend.Send(new PlaceAddOns
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			PrevAddOnPlacements = artifact.GetAddons().GetAddonIds(),
			AddOnPlacements = addons.GetAddonIds()
		}).On(delegate(Error msg, PacketHeader header)
		{
			artifact.UpdateWalls(artifact.WallModel, artifact.GetAddons());
			artifact.Models.SetActive(active: true);
			GameManager.DefaultErrorHandler(msg, header);
		});
	}

	private void SendStartBuildingMessage(ArtifactBuildInfo buildInfo)
	{
		TileObject tileObject = TerrainA6.GetTileObject(buildInfo.WorldTile);
		ModularArtifact modularArtifact = ((tileObject != null && !((Object)(object)tileObject.Artifact == (Object)null)) ? tileObject.Artifact.GetArtifactComponent<ModularArtifact>() : null);
		OccupyArtifactSite occupyArtifactSite = default(OccupyArtifactSite);
		occupyArtifactSite.Tile = new KeyValuePair<int, int>(buildInfo.WorldTile.x, buildInfo.WorldTile.y);
		occupyArtifactSite.Size = new KeyValuePair<int, int>(buildInfo.Size.x, buildInfo.Size.y);
		occupyArtifactSite.BlueprintId = buildInfo.Blueprint.Id;
		occupyArtifactSite.Rotated = buildInfo.Rotated;
		occupyArtifactSite.ModularEntityId = modularArtifact?.EntityId ?? 0;
		OccupyArtifactSite msg = occupyArtifactSite;
		ArtifactBuildInfo copied = buildInfo;
		Connections.Frontend.Send(msg).On(delegate(Messages.Timer timerMsg, PacketHeader _)
		{
			MotionMap.Instance().GetBuildMotion("Occupy", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(timerMsg.Duration, buildInfo.Blueprint.Icon, motion, equip);
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Construct, buildInfo.Blueprint.LocalizedName);
		}).On(delegate(Error errorMsg, PacketHeader errorHeader)
		{
			if (errorMsg.TypeName == "AlreadyHaveOne")
			{
				AskGiveupAndTryMore(copied);
			}
			else
			{
				GameManager.DefaultErrorHandler(errorMsg, errorHeader);
			}
		});
	}

	private void AskGiveupAndTryMore(ArtifactBuildInfo buildInfo)
	{
		UIManager.MessageBox.Show(T._("이미 사유지를 가지고 있습니다.\n다른 곳에 소유한 사유지를 포기하고 이 곳에 새로 선언하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(default(GiveupEstate)).On<OK>(delegate
				{
					SendStartBuildingMessage(buildInfo);
				});
			}
		});
	}

	private void FillArtifactData(Artifact artifact, ArtifactState states, Tags tagList)
	{
		UpdateArtifactState(artifact, states);
		UpdateTagList(artifact, tagList);
	}

	private void UpdateArtifactState(Artifact artifact, ArtifactState state, double eventTime = -1.0)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			artifact.SetArtifactState(state, eventTime);
		}
	}

	private void UpdateTagList(Artifact artifact, Tags msg)
	{
		if (!((Object)(object)artifact == (Object)null) && msg._Tags != null)
		{
			artifact.SetTagList(msg._Tags);
		}
	}

	private void OnOccupiedMsg(Occupied msg, PacketHeader header)
	{
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(msg.EntityId);
		if ((Object)(object)artifact == (Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(CoOnOccupiedMsg(msg));
		}
		else
		{
			RequestArtifactMaterials(artifact);
		}
	}

	private IEnumerator CoOnOccupiedMsg(Occupied msg)
	{
		Artifact artifact = null;
		for (int i = 0; i < 5; i++)
		{
			artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(msg.EntityId);
			if ((Object)(object)artifact != (Object)null)
			{
				break;
			}
			yield return (object)new WaitForSeconds(0.5f);
		}
		if (Object.op_Implicit((Object)(object)artifact))
		{
			RequestArtifactMaterials(artifact);
		}
	}

	private void OnArtifactBuiltMsg(ArtifactBuilt msg, PacketHeader header)
	{
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(msg.EntityId);
		if (!((Object)(object)artifact == (Object)null) && PlayerBehavior.LocalPlayer.EntityId == msg.BuilderId && this.BuildFinished != null)
		{
			this.BuildFinished(artifact);
		}
	}

	private void OnAppearArtifactMsg(AppearArtifact msg, PacketHeader header)
	{
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().AddArtifact(msg.EntityId, msg.Tile, msg.EntityType, msg.Rotation, new Point2(msg.Size.Key, msg.Size.Value), msg.Height);
		if (!((Object)(object)artifact == (Object)null))
		{
			artifact.FounderId = msg.FounderEntityId;
			UpdateArtifactData(artifact, msg.States, msg.Tags);
			artifact.UpdateDisplay(msg.Display);
			if (this.ArtifactAppeared != null)
			{
				this.ArtifactAppeared(artifact);
			}
		}
	}

	public void UpdateArtifactData([NotNull] Artifact artifact, ArtifactState state, Tags tagList)
	{
		if (KSingleton<TerrainA6>.Instance().IsChunkLoading && (Object)(object)TerrainA6.GetChunkFromTile(artifact.WorldTile) == (Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(WaitLoadingOnAppearArtifactMsg(artifact, state, tagList));
		}
		else
		{
			FillArtifactData(artifact, state, tagList);
		}
	}

	private IEnumerator WaitLoadingOnAppearArtifactMsg([NotNull] Artifact artifact, ArtifactState states, Tags tagList)
	{
		while (KSingleton<TerrainA6>.Instance().IsChunkLoading && (Object)(object)TerrainA6.GetChunkFromTile(artifact.WorldTile) == (Object)null)
		{
			yield return null;
		}
		FillArtifactData(artifact, states, tagList);
	}

	private void OnArtifactStateMsg(ArtifactState msg, PacketHeader header)
	{
		ulong entityId = msg.EntityId;
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(entityId);
		UpdateArtifactState(artifact, msg, header.Time);
	}

	private void OnTagListMsg(Tags msg, PacketHeader header)
	{
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(msg.EntityId);
		UpdateTagList(artifact, msg);
	}

	private void OnArtifactCompletedMsg(ArtifactCompleted msg, PacketHeader header)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(msg.EntityId);
		if (!((Object)(object)artifact == (Object)null))
		{
			ParticleManager.Emit("Particle/FX_BuildingCom_01.prefab", artifact.Center, Quaternion.identity);
			SoundManager.Play("Sound/Effect/UI/UI_Building_Success_01.wav", artifact.Center);
			if (artifact.FounderId == PlayerBehavior.LocalPlayer.EntityId && this.BuildCompleted != null)
			{
				this.BuildCompleted(artifact);
			}
		}
	}

	private void OnArtifactCapsulatedMsg(ArtifactCapsulated msg, PacketHeader header)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tilePosition = msg.Tile.ToVector2() + new Vector2((float)msg.Size.Key, (float)msg.Size.Value) * 0.5f;
		Vector3 val = TerrainA6.TilePositionToClientPosition(tilePosition);
		ParticleManager.Emit("Particle/FX_CapsulateArtifact.prefab", val, Quaternion.identity);
		SoundManager.Play("Sound/Effect/UI/UI_Capsulate_Artifact_01.wav", val);
	}

	private void OnArtifactPlacedMsg(ArtifactPlaced msg, PacketHeader header)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tilePosition = msg.Tile.ToVector2() + new Vector2((float)msg.Size.Key, (float)msg.Size.Value) * 0.5f;
		Vector3 val = TerrainA6.TilePositionToClientPosition(tilePosition);
		ParticleManager.Emit("Particle/FX_BuildingCom_01.prefab", val, Quaternion.identity);
		SoundManager.Play("Sound/Effect/UI/UI_Building_Success_01.wav", val);
	}

	private void OnArtifactMaterialsMsg(ArtifactMaterials msg, PacketHeader header)
	{
		if (SetMaterials(msg))
		{
			_slotContainer.OnSlotMaterialUpdate();
		}
	}

	private void OnArtifactResponseMsg(ArtifactResponse msg, PacketHeader header)
	{
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(msg.ArtifactId);
		if ((Object)(object)artifact == (Object)null)
		{
			return;
		}
		switch (msg.Action)
		{
		case "rest":
			if (msg.Success)
			{
				DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Rest);
				GameSystem<PlayGuideSystem>.Instance().EventOccured("rest", null);
			}
			break;
		}
	}

	private void OnDepartTutorialReady(DepartTutorialReady msg, PacketHeader header)
	{
		Connections.Frontend.Send(new DepartTutorialFor
		{
			TargetRegionId = msg.TargetRegionId,
			EntryPointOffset = msg.EntryPointOffset
		});
	}

	private void Awake()
	{
		Connections.Frontend.On<Occupied>(OnOccupiedMsg);
		Connections.Frontend.On<ArtifactBuilt>(OnArtifactBuiltMsg);
		Connections.Frontend.On<AppearArtifact>(OnAppearArtifactMsg);
		Connections.Frontend.On<ArtifactState>(OnArtifactStateMsg);
		Connections.Frontend.On<Tags>(OnTagListMsg);
		Connections.Frontend.On<ArtifactCompleted>(OnArtifactCompletedMsg);
		Connections.Frontend.On<ArtifactCapsulated>(OnArtifactCapsulatedMsg);
		Connections.Frontend.On<ArtifactPlaced>(OnArtifactPlacedMsg);
		Connections.Frontend.On<ArtifactMaterials>(OnArtifactMaterialsMsg);
		Connections.Frontend.On<ArtifactResponse>(OnArtifactResponseMsg);
		Connections.Frontend.On<DepartTutorialReady>(OnDepartTutorialReady);
		Connections.Frontend.On(delegate(ArtifactDisplay msg, PacketHeader header)
		{
			ulong entityId = msg.EntityId;
			Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(entityId);
			if (Object.op_Implicit((Object)(object)artifact))
			{
				artifact.UpdateDisplay(msg);
			}
		});
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				ParticleManager.Cache("Particle/FX_BuildingCom_01.prefab");
				SoundManager.Cache("Sound/Effect/UI/UI_Building_Success_01.wav");
				PlayerBehavior.LocalPlayer.TileChanged += LocalPlayer_TileChanged;
			}
		};
	}

	public void InteractionBuildArtifact(Artifact artifact)
	{
		RequestArtifactMaterials(artifact);
	}

	public void PutMaterials()
	{
		SendPutMaterialsMessage();
	}

	public void Build()
	{
		SendBuildMessage();
	}

	public void RequestEstimateResult()
	{
		switch (_slotContainer.State)
		{
		default:
			_slotContainer.UpdateEstimateResult(null);
			break;
		case BuildSlotContainer.BuildState.ReadyToPutMaterials:
		case BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild:
		case BuildSlotContainer.BuildState.ReadyToBuild:
			SendEstimateResultMessage();
			break;
		}
	}

	public static void OccupyBuildingSite_OnPlay(ProgressGauge gauge)
	{
		IconProgressGauge iconProgressGauge = gauge as IconProgressGauge;
		if ((Object)(object)iconProgressGauge != (Object)null)
		{
			iconProgressGauge.SetIcon(GameSystem<EquipSystem>.Instance().Barehands.Icon);
		}
		MotionMap.Instance().GetBuildMotion("Occupy", null, out var motion, out var equip);
		KSingleton<PlayerController>.Instance().Motion(equip: equip, motionState: motion, time: gauge.RemainTime());
		DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Construct);
	}

	public int GetSkipPostprocessCost([NotNull] Artifact artifact)
	{
		if (Singleton<CashYaml>.Instance == null)
		{
			return -1;
		}
		TimerData.Timer timer = ((artifact.PostProcessTimer != null) ? artifact.PostProcessTimer : null);
		if (timer == null || timer.IsStop)
		{
			return -1;
		}
		float remain = timer.Remain;
		float num = 0f;
		float num2 = 1f;
		float num3 = 0f;
		float num4 = 1f;
		for (int i = 0; i < Singleton<CashYaml>.Instance.instant_construction.Length; i++)
		{
			int[] array = Singleton<CashYaml>.Instance.instant_construction[i];
			num = num3;
			num2 = num4;
			num3 = array[0];
			num4 = array[1];
			if (remain < num3)
			{
				break;
			}
		}
		float num5 = (remain - num) / (num3 - num);
		float num6 = num2 + (num4 - num2) * num5;
		return (int)num6;
	}

	public void SkipPostprocessCost([NotNull] Artifact artifact, int cost)
	{
		Connections.Frontend.Send(new SkipPostprocess
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile,
			Cost = cost
		});
	}

	public void CapsulateArtifact([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new GetCapsulatingCost
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On(delegate(Cost msg, PacketHeader _)
		{
			if (msg.Amount <= 0)
			{
				DoCapsulateArtifact(artifact);
			}
			GameSystem<InventorySystem>.Instance().PlayerInventory.ShowPayConfirm(msg.Amount, msg.Currency, T._("포장에는 {0:가} 필요합니다.\n현재 보유량 {1}"), delegate(bool ok)
			{
				if (ok)
				{
					DoCapsulateArtifact(artifact);
				}
			});
		});
	}

	private void DoCapsulateArtifact([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new CapsulateArtifact
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		}).On(delegate(Messages.Timer timerMsg, PacketHeader header)
		{
			MotionMap.Instance().GetBuildMotion("Capsulate", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(timerMsg.Duration, artifact.Blueprint.Icon, motion, equip);
		});
	}

	public void PlaceCapsulatedArtifact(ulong itemId, string icon, Point2 tile, bool rotated, Vector3 center)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = TerrainA6.TilePositionToClientPosition(tile.ToVector2() - Vector2.one * 0.2f);
		KSingleton<PlayerController>.Instance().MoveToTarget(pos, delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			KSingleton<PlayerController>.Instance().RotateToPosition(center);
			DoPlaceCapsulatedArtifact(itemId, icon, tile, rotated, center);
		});
	}

	private void DoPlaceCapsulatedArtifact(ulong itemId, string icon, Point2 tile, bool rotated, Vector3 center)
	{
		Connections.Frontend.Send(new PlaceCapsulatedArtifact
		{
			ItemId = itemId,
			Tile = tile,
			Rotated = rotated
		}).On(delegate(Messages.Timer timerMsg, PacketHeader header)
		{
			MotionMap.Instance().GetBuildMotion("PlaceCapsulated", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(timerMsg.Duration, icon, motion, equip);
		});
	}

	public void PutInCage(Cage cage, ulong reins)
	{
		Connections.Frontend.Send(new PutInCage
		{
			CageEntityId = cage.EntityId,
			CageTile = cage.WorldTile,
			PetId = reins
		});
	}

	public void TakeOutCage(Cage cage, ulong reins)
	{
		Connections.Frontend.Send(new TakeOutFromCage
		{
			CageEntityId = cage.EntityId,
			CageTile = cage.WorldTile,
			PetId = reins
		});
	}

	private void RequestArtifactMaterials(Artifact artifact)
	{
		GetArtifact getArtifact = default(GetArtifact);
		getArtifact.EntityId = artifact.EntityId;
		getArtifact.Tile = artifact.WorldTile;
		GetArtifact msg = getArtifact;
		Connections.Frontend.Send(msg).On(delegate(ArtifactMaterials artifactMaterials, PacketHeader _)
		{
			if (!artifact.BuildCompleted)
			{
				_slotContainer.Set(artifact, GameSystem<InventorySystem>.Instance().PlayerInventory);
				SetMaterials(artifactMaterials);
				_slotContainer.SelectFirstIncompletedSlot();
				if (this.ArtifactOccupied != null)
				{
					this.ArtifactOccupied();
				}
			}
		});
	}

	private bool SetMaterials(ArtifactMaterials msg)
	{
		if ((Object)(object)_slotContainer.Artifact == (Object)null || _slotContainer.Artifact.EntityId != msg.EntityId)
		{
			return false;
		}
		_slotContainer.SetPrevMaterial(msg.Materials);
		return true;
	}

	private void SendPutMaterialsMessage()
	{
		if (!((Object)(object)_slotContainer.Artifact == (Object)null))
		{
			PutMaterialsIntoArtifact putMaterialsIntoArtifact = default(PutMaterialsIntoArtifact);
			putMaterialsIntoArtifact.EntityId = _slotContainer.Artifact.EntityId;
			putMaterialsIntoArtifact.Tile = _slotContainer.Artifact.WorldTile;
			putMaterialsIntoArtifact.Materials = _slotContainer.CreateMaterialsDictionary();
			PutMaterialsIntoArtifact msg = putMaterialsIntoArtifact;
			Connections.Frontend.Send(msg);
		}
	}

	private void SendBuildMessage()
	{
		if ((Object)(object)_slotContainer.Artifact == (Object)null)
		{
			return;
		}
		ulong? toolItemId = _slotContainer.GetToolItemId();
		BuildArtifact buildArtifact = default(BuildArtifact);
		buildArtifact.EntityId = _slotContainer.Artifact.EntityId;
		buildArtifact.Tile = _slotContainer.Artifact.WorldTile;
		buildArtifact.ToolItemId = ((!toolItemId.HasValue) ? 0 : toolItemId.Value);
		BuildArtifact msg = buildArtifact;
		bool confirming = false;
		Connections.Frontend.Send(msg).On(delegate(Messages.Timer timer, PacketHeader _)
		{
			if (this.BuildStarted != null)
			{
				this.BuildStarted(timer.Duration);
			}
		}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
		{
			confirming = true;
			UIManager.MessageBox.Show(T._("에너지가 모자라는 상태로 이 행동을 하면 건강이 소모됩니다."), delegate(int select)
			{
				Confirm confirm = default(Confirm);
				confirm.Confirmation = select == 0;
				Confirm msg2 = confirm;
				Connection frontend = Connections.Frontend;
				ulong replyOf = header.ReplyOf;
				frontend.Send(msg2, noReply: false, replyOf);
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

	private void SendEstimateResultMessage()
	{
		ulong? toolItemId = _slotContainer.GetToolItemId();
		EstimateBuild estimateBuild = default(EstimateBuild);
		estimateBuild.EntityId = _slotContainer.Artifact.EntityId;
		estimateBuild.Tile = _slotContainer.Artifact.WorldTile;
		estimateBuild.ToolId = ((!toolItemId.HasValue) ? 0 : toolItemId.Value);
		estimateBuild.Materials = _slotContainer.CreateMaterialsDictionary();
		EstimateBuild msg = estimateBuild;
		Connections.Frontend.Send(msg).On(delegate(BuildEstimation estimation, PacketHeader _)
		{
			_slotContainer.UpdateEstimateResult(estimation);
		}).On<Error>(delegate
		{
			_slotContainer.UpdateEstimateResult(null);
		});
	}

	private void LocalPlayer_TileChanged(Point2 prev, Point2 current)
	{
		TileObject tileObject = TerrainA6.GetTileObject(prev);
		TileObject currentTileObject = PlayerBehavior.LocalPlayer.CurrentTileObject;
		Artifact artifact = tileObject?.Artifact;
		Artifact artifact2 = currentTileObject?.Artifact;
		if ((Object)(object)artifact != (Object)null && (Object)(object)artifact != (Object)(object)artifact2)
		{
			OnExitArtifact(artifact);
		}
		if ((Object)(object)artifact2 != (Object)null && (Object)(object)artifact != (Object)(object)artifact2)
		{
			OnEnterArtifact(artifact2);
		}
	}

	private void OnEnterArtifact(Artifact artifact)
	{
		artifact.OnPlayerEnter();
	}

	private void OnExitArtifact(Artifact artifact)
	{
		artifact.OnPlayerExit();
	}
}
