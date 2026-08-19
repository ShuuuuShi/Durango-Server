using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Building;
using Durango.Logic.Clusters;
using Durango.Logic.Timer;
using Durango.MotionInfo;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using Messages;
using Shared.Etc;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class BuildSystem : GameSystem<BuildSystem>
{
	public struct GridResult
	{
		public Building.Blueprint Blueprint;

		public Point2 Tile;

		public Point2 Size;

		public int? Floor;

		public int? Stories;

		public Rotation Rotation;

		public string BlueprintId
		{
			get
			{
				if (Blueprint == null)
				{
					return null;
				}
				return Blueprint.Id;
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoOnOccupiedMsg_003Ed__32 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Occupied msg;

		public BuildSystem _003C_003E4__this;

		private Artifact _003Cartifact_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoOnOccupiedMsg_003Ed__32(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cartifact_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			BuildSystem buildSystem = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cartifact_003E5__2 = null;
				_003Ci_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__3++;
				break;
			}
			if (_003Ci_003E5__3 < 5)
			{
				_003Cartifact_003E5__2 = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(msg.EntityId);
				if (!(_003Cartifact_003E5__2 != null))
				{
					_003C_003E2__current = new WaitForSeconds(0.5f);
					_003C_003E1__state = 1;
					return true;
				}
			}
			if ((bool)_003Cartifact_003E5__2)
			{
				buildSystem.OnArtifactOccupied(_003Cartifact_003E5__2);
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const string BuildFinishedParticle = "Particle/FX_BuildingCom_01.prefab";

	private const string BuildFinishedSound = "UI_Building_Success_01";

	private const string CapsulateArtifactParticle = "Particle/FX_CapsulateArtifact.prefab";

	private const string CapsulateArtifactSound = "UI_Capsulate_Artifact_01";

	private readonly BuildSlotContainer _slotContainer = new BuildSlotContainer();

	public PredictTimer OccupyTimer { get; private set; }

	public PredictTimer BuildTimer { get; private set; }

	public string OccupiedBlueprintId { get; private set; }

	public BuildSlotContainer SlotContainer => _slotContainer;

	public event Action ArtifactOccupied;

	public event Action<Artifact> BuildFinished;

	public event Action<Artifact> BuildCompleted;

	private void Awake()
	{
		Connections.Frontend.On<ArtifactBuilt>(OnArtifactBuiltMsg);
		Connections.Frontend.On<ArtifactCompleted>(OnArtifactCompletedMsg);
		Connections.Frontend.On<ArtifactCapsulated>(OnArtifactCapsulatedMsg);
		Connections.Frontend.On<ArtifactPlaced>(OnArtifactPlacedMsg);
		Connections.Frontend.On<ArtifactMaterials>(OnArtifactMaterialsMsg);
		Connections.Frontend.On<ArtifactResponse>(OnArtifactResponseMsg);
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			if (!GameManager.IsPrologueMode)
			{
				ParticleManager.Cache("Particle/FX_BuildingCom_01.prefab");
				SoundManager.PrepareEvent("UI_Building_Success_01");
				ParticleManager.Cache("Particle/FX_CapsulateArtifact.prefab");
				SoundManager.PrepareEvent("UI_Capsulate_Artifact_01");
				OccupyTimer = new PredictTimer(GameManager.PlayerId, "Occupy");
				BuildTimer = new PredictTimer(GameManager.PlayerId, "Building");
			}
		};
	}

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().PostTouched += OnPostTouched;
	}

	public static void PlayBuildFinished(Vector3 pos)
	{
		ParticleManager.Emit("Particle/FX_BuildingCom_01.prefab", pos, Quaternion.identity);
		SoundManager.PlayEvent("UI_Building_Success_01", SoundPosition.Fix(pos));
	}

	private IEnumerator CoOnOccupiedMsg(Occupied msg)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoOnOccupiedMsg_003Ed__32(0)
		{
			_003C_003E4__this = this,
			msg = msg
		};
	}

	private void OnArtifactOccupied(Artifact artifact)
	{
		_slotContainer.Set(artifact, artifact.Blueprint, GameSystem<InventorySystem>.Instance().PlayerInventory);
		if (this.ArtifactOccupied != null)
		{
			this.ArtifactOccupied();
		}
	}

	private void OnArtifactBuiltMsg(ArtifactBuilt msg, PacketHeader header)
	{
		Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(msg.EntityId);
		if (!(artifact == null) && PlayerBehavior.LocalPlayer.EntityId == msg.BuilderId && this.BuildFinished != null)
		{
			this.BuildFinished(artifact);
		}
	}

	private void OnArtifactCompletedMsg(ArtifactCompleted msg, PacketHeader header)
	{
		Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(msg.EntityId);
		if (!(artifact == null))
		{
			PlayBuildFinished(artifact.Center);
			if (artifact.FounderId == PlayerBehavior.LocalPlayer.EntityId && this.BuildCompleted != null)
			{
				this.BuildCompleted(artifact);
			}
		}
	}

	private static void OnArtifactCapsulatedMsg(ArtifactCapsulated msg, PacketHeader header)
	{
		Vector3 vector = Util.TilePositionToClientPosition(msg.Tile.ToVector2() + msg.Size.ToVector2() * 0.5f);
		int? floor = msg.Floor;
		if (floor.HasValue)
		{
			vector.y += floor.Value * 200;
		}
		ParticleManager.Emit("Particle/FX_CapsulateArtifact.prefab", vector, Quaternion.identity);
		SoundManager.PlayEvent("UI_Capsulate_Artifact_01", SoundPosition.Fix(vector));
	}

	private static void OnArtifactPlacedMsg(ArtifactPlaced msg, PacketHeader header)
	{
		Vector3 pos = Util.TilePositionToClientPosition(msg.Tile.ToVector2() + msg.Size.ToVector2() * 0.5f);
		int? floor = msg.Floor;
		if (floor.HasValue)
		{
			pos.y += floor.Value * 200;
		}
		PlayBuildFinished(pos);
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
		if (!(Durango.Utils.Singleton<ArtifactManager>.Instance().Find(msg.ArtifactId) == null))
		{
			string action = msg.Action;
			if (action != null && action == "rest" && msg.Success)
			{
				GameSystem<PlayGuideSystem>.Instance().NotifyEventOccured("rest", null);
			}
		}
	}

	public void InteractionBuildArtifact(Artifact artifact)
	{
		RequestArtifactMaterials(artifact);
	}

	public void PutMaterials(Action onSccuess)
	{
		if (_slotContainer.Artifact == null)
		{
			return;
		}
		PutMaterialsIntoArtifact putMaterialsIntoArtifact = default(PutMaterialsIntoArtifact);
		putMaterialsIntoArtifact.EntityId = _slotContainer.Artifact.EntityId;
		putMaterialsIntoArtifact.Tile = _slotContainer.Artifact.WorldTile;
		putMaterialsIntoArtifact.Materials = _slotContainer.CreateFirstMaterialsDictionary(canFinished: false);
		PutMaterialsIntoArtifact msg = putMaterialsIntoArtifact;
		Connections.Frontend.Send(msg).On<OK>(delegate
		{
			if (onSccuess != null)
			{
				onSccuess();
			}
		});
	}

	public void Build()
	{
		if (_slotContainer.Artifact == null)
		{
			return;
		}
		SoundManager.PlayEvent("ui_button_build");
		string toolItemId = _slotContainer.GetToolItemId();
		BuildArtifact buildArtifact = default(BuildArtifact);
		buildArtifact.EntityId = _slotContainer.Artifact.EntityId;
		buildArtifact.Tile = _slotContainer.Artifact.WorldTile;
		buildArtifact.ToolItemId = toolItemId;
		BuildArtifact msg = buildArtifact;
		BuildTimer.Play(Connections.Frontend.Ping + 10f);
		bool confirming = false;
		Connections.Frontend.Send(msg).On(delegate(Messages.Timer timer, PacketHeader _)
		{
			BuildTimer.Play(timer.Duration);
		}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
		{
			BuildTimer.Pause();
			confirming = true;
			LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
			{
				if (result == LowEnergyWarning.Result.IgnoreWarning)
				{
					BuildTimer.Play(Connections.Frontend.Ping + 10f);
				}
				else
				{
					BuildTimer.Stop();
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
				BuildTimer.Stop();
			});
	}

	public void Remodeling()
	{
		if (_slotContainer.Artifact == null)
		{
			return;
		}
		SoundManager.PlayEvent("ui_button_build");
		string toolItemId = _slotContainer.GetToolItemId();
		RemodelArtifact remodelArtifact = default(RemodelArtifact);
		remodelArtifact.EntityId = _slotContainer.Artifact.EntityId;
		remodelArtifact.Tile = _slotContainer.Artifact.WorldTile;
		remodelArtifact.SlotId = _slotContainer.Blueprint.Id;
		remodelArtifact.Materials = _slotContainer.CreateFirstMaterialsDictionary(canFinished: false);
		remodelArtifact.ToolItemId = toolItemId;
		RemodelArtifact msg = remodelArtifact;
		BuildTimer.Play(Connections.Frontend.Ping + 10f);
		bool confirming = false;
		Connections.Frontend.Send(msg).On(delegate(Messages.Timer timer, PacketHeader _)
		{
			BuildTimer.Play(timer.Duration);
		}).On(delegate(EnergyWarning warningMsg, PacketHeader header)
		{
			BuildTimer.Pause();
			confirming = true;
			LowEnergyWarning.Show(warningMsg, header, delegate(LowEnergyWarning.Result result)
			{
				if (result == LowEnergyWarning.Result.IgnoreWarning)
				{
					BuildTimer.Play(Connections.Frontend.Ping + 10f);
				}
				else
				{
					BuildTimer.Stop();
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
				BuildTimer.Stop();
			});
	}

	public static void PlaceCapsulatedArtifact(string itemId, string icon, Point2 tile, int? floor, Point2 size, Rotation rotation)
	{
		Vector3 pos = Util.TilePositionToClientPosition(tile.ToVector2() + size.ToVector2() * 0.5f);
		Durango.Utils.Singleton<PlayerController>.Instance().MoveToPosition(pos, delegate
		{
			Vector3 vector = PlayerBehavior.LocalPlayer.CurrentPosition - pos;
			vector.y = 0f;
			if (!(vector.magnitude >= 400f))
			{
				Durango.Utils.Singleton<PlayerController>.Instance().RotateToPosition(pos);
				if (PlayerBehavior.LocalPlayer.IsRiding)
				{
					VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
					{
						DoPlaceCapsulatedArtifact(itemId, icon, tile, floor, rotation);
					});
				}
				else
				{
					DoPlaceCapsulatedArtifact(itemId, icon, tile, floor, rotation);
				}
			}
		}, 10f, completeIfBlocked: true);
	}

	private static void DoPlaceCapsulatedArtifact(string itemId, string icon, Point2 tile, int? floor, Rotation rotation)
	{
		Connections.Frontend.Send(new PlaceCapsulatedArtifact
		{
			ItemId = itemId,
			Tile = tile,
			Floor = floor,
			Rotation = rotation
		}).On(delegate(Messages.Timer timerMsg, PacketHeader header)
		{
			MotionMap.Instance().GetBuildMotion("PlaceCapsulated", null, out var motion, out var equip);
			TimerSystem.SetGaugeAndPlayMotion(timerMsg.Duration, icon, motion, "PlaceCapsulated", equip);
		});
	}

	private static void ShowLoadingRing(Vector3 worldTile)
	{
		UIManager.Popup.LoadingRing.AttachToClientPosition(Util.TilePositionToClientPosition(worldTile));
	}

	private static void HideLoadingRing()
	{
		LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
		if (loadingRing.AttachMode == LoadingRingWidget.Mode.ClientPosition)
		{
			loadingRing.Hide();
		}
	}

	private void RequestArtifactMaterials(Artifact artifact)
	{
		GetArtifact getArtifact = default(GetArtifact);
		getArtifact.EntityId = artifact.EntityId;
		getArtifact.Tile = artifact.WorldTile;
		GetArtifact msg = getArtifact;
		ShowLoadingRing(artifact.CenterTile);
		Connections.Frontend.Send(msg).On(delegate(ArtifactMaterials artifactMaterials, PacketHeader _)
		{
			if (!artifact.BuildCompleted)
			{
				_slotContainer.Set(artifact, artifact.Blueprint, GameSystem<InventorySystem>.Instance().PlayerInventory);
				SetMaterials(artifactMaterials);
				_slotContainer.SelectFirstIncompletedSlot();
				if (this.ArtifactOccupied != null)
				{
					this.ArtifactOccupied();
				}
			}
		}).All(delegate
		{
			HideLoadingRing();
		});
	}

	private bool SetMaterials(ArtifactMaterials msg)
	{
		if (_slotContainer.Artifact == null || _slotContainer.Artifact.EntityId != msg.EntityId)
		{
			return false;
		}
		_slotContainer.SetPrevMaterial(msg.Materials);
		return true;
	}

	public static void EstimateBuild(EstimateBuild info, Action<BuildEstimation?> onResult)
	{
		Connections.Frontend.Send(info).On(delegate(BuildEstimation estimation, PacketHeader header)
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

	public static void EstimateRemodeling(EstimateRemodeling info, Action<BuildEstimation?> onResult)
	{
		Connections.Frontend.Send(info).On(delegate(BuildEstimation estimation, PacketHeader header)
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

	private void OnPostTouched(InteractionMenuList menuList, InteractionObject target)
	{
		Artifact artifact = target?.GetTargetComponent<Artifact>();
		if (artifact == null)
		{
			return;
		}
		if (artifact.PostProcessTimer != null && !artifact.PostProcessTimer.IsStop)
		{
			menuList.Add(new InteractionMenuData(Interaction.SkipPostprocess));
		}
		if (artifact.BuildCompleted)
		{
			Mode clusterMode = GameManager.ClusterMode;
			if (clusterMode == Mode.Online || clusterMode == Mode.Editable)
			{
				int[] floorableTypes = Yaml.Util.Singleton<Constants>.Instance.ArtifactFloor.FloorableTypes;
				if (floorableTypes != null && Array.IndexOf(floorableTypes, artifact.EntityType) != -1)
				{
					InteractionMenuData data = new InteractionMenuData(Interaction.ExtendFloor);
					data.Disabled = artifact.Stories.Value.GetValueOrDefault() >= Yaml.Util.Singleton<Constants>.Instance.ArtifactFloor.MaxStories;
					menuList.Add(data);
				}
			}
		}
		if (GameManager.ClusterMode == Mode.Editable && artifact.GetArtifactComponent<ModularArtifact>() == null && artifact.Models.HasPatternTex())
		{
			menuList.Add(new InteractionMenuData(Interaction.RemodelArtifact));
		}
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
		}).All(delegate(Packet packet)
		{
			if (packet.Header.TypeCode != 1231)
			{
				artifact.UpdateWalls(artifact.GetAddons());
				artifact.Models.SetActive(active: true);
			}
		});
	}

	public void OccupyArtifactSite(GridResult result)
	{
		OccupyArtifactSite(result, null);
	}

	public void OccupyArtifactSite(GridResult result, string itemId)
	{
		Vector3 pos = Util.WorldPositionToClientPosition(Util.TilePositionToWorldPosition(result.Tile));
		Vector3 vector = new Vector3(result.Size.x, 0f, result.Size.y);
		pos += vector * 200f * 0.5f;
		TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(result.Tile);
		EnterableArtifact enterableArtifact = ((tileObject != null && !(tileObject.Artifact == null)) ? tileObject.Artifact.GetArtifactComponent<EnterableArtifact>() : null);
		string modularEntityId = ((enterableArtifact != null) ? enterableArtifact.EntityId : string.Empty);
		string blueprintId = result.BlueprintId;
		OccupyArtifactSite msg = new OccupyArtifactSite
		{
			BlueprintId = blueprintId,
			ItemId = itemId,
			Tile = result.Tile,
			Floor = result.Floor,
			Size = result.Size,
			Rotation = result.Rotation,
			ModularEntityId = modularEntityId
		};
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(blueprintId);
		if (blueprint != null && blueprint.IsModular)
		{
			msg.Stories = 1;
		}
		Durango.Utils.Singleton<PlayerController>.Instance().MoveToPosition(pos, delegate
		{
			VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
			{
				SendOccupyArtifactSite(msg, blueprintId);
			});
		}, 141f);
	}

	private void SendOccupyArtifactSite(OccupyArtifactSite msg, string blueprintId)
	{
		OccupiedBlueprintId = blueprintId;
		OccupyTimer.Play(Connections.Frontend.Ping + 10f);
		Connections.Frontend.Send(msg).On(delegate(Messages.Timer timer, PacketHeader header)
		{
			if (!OccupyTimer.Timer.IsStop)
			{
				OccupyTimer.Play(timer.Duration);
			}
		}).On(delegate(Occupied occupied, PacketHeader header)
		{
			StartCoroutine(CoOnOccupiedMsg(occupied));
		})
			.Rest(delegate
			{
				OccupyTimer.Stop();
			});
	}
}
