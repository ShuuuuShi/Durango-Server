using System;
using System.Collections.Generic;
using System.Diagnostics;
using Building;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Entity.Artifact;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Etc;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ArtifactManager : Durango.Utils.Singleton<ArtifactManager>
{
	[SerializeField]
	private GameObject _artifactPrefab;

	[SerializeField]
	private GameObject _siteIconPrefab;

	[SerializeField]
	private string _estateFencePrefab;

	[SerializeField]
	private string _estateLinePrefabs;

	[SerializeField]
	private ParticleType _modularArtifactMoodEffect;

	private readonly Dictionary<string, Artifact> _artifacts = new Dictionary<string, Artifact>();

	private readonly List<Artifact> _willBeRemoved = new List<Artifact>();

	public event Action<Artifact> Added;

	public event Action<Artifact> Removed;

	public event Action<Artifact> StateChanged;

	public event Action<Artifact, ArtifactState> StateChangedWithPrev;

	private void Start()
	{
		Durango.Utils.Singleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
		Durango.Utils.Singleton<TerrainBase>.Instance().CenterChunkChanged += Terrain_CenterChunkChanged;
		Durango.Utils.Singleton<TerrainBase>.Instance().ChunkLoaded += TerrainA6_ChunkLoaded;
		PlayerBehavior.LocalPlayer.TileChanged += LocalPlayer_TileChanged;
		Observable<byte> floor = PlayerBehavior.LocalPlayer.Floor;
		floor.Changed = (Action<byte>)Delegate.Combine(floor.Changed, new Action<byte>(LocalPlayer_FloorChanged));
		Connections.Frontend.On<AppearArtifact>(OnAppearArtifactMsg);
		Connections.Frontend.On(delegate(ArtifactState msg, PacketHeader header)
		{
			UpdateArtifactState(Find(msg.EntityId), msg, header.Time);
		});
		Connections.Frontend.On(delegate(Bandstand msg, PacketHeader header)
		{
			Artifact artifact3 = Find(msg.PropKey.EntityId);
			if (!(artifact3 == null))
			{
				ArtifactState artifactState = artifact3.ArtifactState;
				artifactState.Bandstand = msg;
				UpdateArtifactState(artifact3, artifactState, header.Time);
			}
		});
		Connections.Frontend.On(delegate(Tags msg, PacketHeader header)
		{
			UpdateTagList(Find(msg.EntityId), msg);
		});
		Connections.Frontend.On(delegate(ArtifactDisplay msg, PacketHeader header)
		{
			Artifact artifact2 = Find(msg.EntityId);
			if ((bool)artifact2)
			{
				artifact2.UpdateDisplay(msg);
			}
		});
		Connections.Frontend.On(delegate(InteriorSetEffectChanged msg, PacketHeader header)
		{
			Artifact artifact = Find(msg.ModularEntityId);
			if ((bool)artifact)
			{
				ModularArtifactInteriorSetEffectChanged(artifact, msg.ModularArtifactState);
			}
		});
	}

	private void GameManager_PreReconnect()
	{
		StopAllCoroutines();
		RemoveAll();
	}

	private void Terrain_CenterChunkChanged()
	{
		TerrainBase terrain = Durango.Utils.Singleton<TerrainBase>.Instance();
		foreach (KeyValuePair<string, Artifact> artifact in _artifacts)
		{
			Artifact value = artifact.Value;
			if (!(value == null) && IsFarArtifact(terrain, value))
			{
				_willBeRemoved.Add(value);
			}
		}
		foreach (Artifact item in _willBeRemoved)
		{
			if (!(item == null))
			{
				Remove(item);
			}
		}
		_willBeRemoved.Clear();
	}

	private static bool IsFarArtifact(TerrainBase terrain, [NotNull] Artifact artifact)
	{
		Point2 worldTile = artifact.WorldTile;
		Point2 size = artifact.Size;
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				Point2 coord = Util.TilePositionToChunkCoords(worldTile + new Point2(j, i));
				if (terrain.IsVisibleChunk(coord))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void TerrainA6_ChunkLoaded(TerrainChunkBase chunk)
	{
		foreach (KeyValuePair<string, Artifact> artifact in _artifacts)
		{
			Artifact value = artifact.Value;
			if (!(value == null) && !value.IsFullyAttached())
			{
				ArtifactIntoTileObject(value);
			}
		}
	}

	private static void LocalPlayer_TileChanged(Point2 prev, Point2 current)
	{
		if (!PlayerBehavior.LocalPlayer.Driver.IsHovering)
		{
			TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(prev);
			TileObject tileObject2 = PlayerBehavior.LocalPlayer.GetTileObject();
			Artifact artifact = tileObject?.Artifact;
			Artifact artifact2 = tileObject2?.Artifact;
			if (artifact != null && artifact != artifact2)
			{
				artifact.OnPlayerExit();
			}
			if (artifact2 != null && artifact != artifact2)
			{
				artifact2.OnPlayerEnter();
			}
		}
	}

	private static void LocalPlayer_FloorChanged(byte floor)
	{
		TileObject tileObject = PlayerBehavior.LocalPlayer.GetTileObject();
		if (tileObject != null && tileObject.Artifact != null)
		{
			tileObject.Artifact.OnPlayerFloorChange();
		}
	}

	[Conditional("MAP_EXPORTER_BUILD")]
	public void AddArtifact(AppearArtifact msg)
	{
		OnAppearArtifactMsg(msg, default(PacketHeader));
	}

	private void OnAppearArtifactMsg(AppearArtifact msg, PacketHeader header)
	{
		Artifact artifact = Add(msg.EntityId, msg.Tile, msg.EntityType, msg.Rotation, msg.Size, msg.Floor, msg.Stories, msg.HasRoof, msg.Height);
		if (!(artifact == null))
		{
			artifact.FounderId = msg.FounderEntityId;
			UpdateArtifactState(artifact, msg.States, header.Time);
			UpdateTagList(artifact, msg.Tags);
			artifact.UpdateDisplay(msg.Display);
		}
	}

	private void UpdateArtifactState(Artifact artifact, ArtifactState state, double eventTime)
	{
		if (!(artifact == null))
		{
			ArtifactState artifactState = artifact.ArtifactState;
			artifact.SetArtifactState(state, eventTime);
			if (this.StateChanged != null)
			{
				this.StateChanged(artifact);
			}
			if (this.StateChangedWithPrev != null)
			{
				this.StateChangedWithPrev(artifact, artifactState);
			}
		}
	}

	private static void UpdateTagList(Artifact artifact, Tags msg)
	{
		if (!(artifact == null) && msg._Tags != null)
		{
			artifact.SetTagList(msg._Tags);
		}
	}

	private void ModularArtifactInteriorSetEffectChanged([NotNull] Artifact artifact, ArtifactState state)
	{
		if (ShowArtifactInteriorMsg(artifact, state, hidePreviousMsg: true))
		{
			SoundManager.PlayEvent("ui_setmood_activation");
			ParticleManager.Emit(_modularArtifactMoodEffect.Path, artifact.Center, Quaternion.identity);
		}
	}

	public void RemoveAll()
	{
		Artifact[] array = new Artifact[_artifacts.Count];
		_artifacts.Values.CopyTo(array, 0);
		Artifact[] array2 = array;
		foreach (Artifact artifact in array2)
		{
			if (artifact != null)
			{
				Remove(artifact);
			}
		}
		_artifacts.Clear();
	}

	public bool HandleDisappearMsg(DisappearEntity msg)
	{
		if (RemoveArtifact(msg.EntityId))
		{
			return true;
		}
		return false;
	}

	public bool RemoveArtifact(string entityId)
	{
		Artifact artifact = Find(entityId);
		if ((bool)artifact)
		{
			Remove(artifact);
			return true;
		}
		return false;
	}

	public void HandleDisappearEntitiesMsg(DisappearEntities msg)
	{
		string[] entityIds = msg.EntityIds;
		foreach (string entityId in entityIds)
		{
			RemoveArtifact(entityId);
		}
	}

	public Artifact Find(string entityId)
	{
		return _artifacts.Get(entityId);
	}

	public IEnumerable<Artifact> GetArtifacts()
	{
		return _artifacts.Values;
	}

	public Artifact Add(string entityId, Point2 worldTile, ushort entityType, Rotation rotation, Point2 size, int? floor, int? stories, bool? hasRoof, int height)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(entityType);
		if (blueprint == null)
		{
			return null;
		}
		Artifact artifact = _artifacts.Get(entityId);
		if (artifact == null)
		{
			artifact = Create(blueprint);
			artifact.SetEntity(entityId, entityType, worldTile);
			artifact.Init(blueprint, worldTile, rotation, size, floor, stories, hasRoof, height);
			_artifacts[entityId] = artifact;
			if (this.Added != null)
			{
				this.Added(artifact);
			}
		}
		else
		{
			artifact.SetStories(stories, hasRoof);
		}
		ArtifactIntoTileObject(artifact);
		return artifact;
	}

	private Artifact Create(Building.Blueprint blueprint)
	{
		GameObject obj = UnityEngine.Object.Instantiate(_artifactPrefab, base.transform);
		obj.name = blueprint.Id;
		Artifact component = obj.GetComponent<Artifact>();
		int i = 0;
		for (int size = KUtility.GetSize(blueprint.Components); i < size; i++)
		{
			ArtifactComponent artifactComponent = MakeComponentScript(blueprint.Components[i]);
			if (artifactComponent != null)
			{
				component.AddArtifactComponent(artifactComponent);
			}
		}
		return component;
	}

	private static ArtifactComponent MakeComponentScript(string component)
	{
		switch (component)
		{
		case "WarpAccelerator":
			return new WarpAccelerator();
		case "Fence":
			return new Fence();
		case "Gate":
			return new Gate();
		case "Scribble":
			return new MessageBoard();
		case "Growable":
			return new Farm();
		case "Road":
			return new Road();
		case "Trap":
			return new Trap();
		case "Modular":
			return new ModularArtifact();
		case "FourSideEnterable":
			return new FourSideEnterable();
		case "Landmark":
			return new TwoSideEnterable();
		case "DomesticCage":
			return new Entity.Artifact.DomesticCage();
		case "Cage":
		case "GrowCage":
			return new Cage();
		case "Bridge":
			return new Bridge();
		case "Defensive":
			return new Defensive();
		case "Landowner":
			return new Landowner();
		case "Catapult":
			return new Catapult();
		case "Sprinklable":
			return new Sprinklable();
		case "Mannequin":
			return new Mannequin();
		case "Stairs":
			return new Stairs();
		default:
			return null;
		}
	}

	public void Remove([NotNull] Artifact artifact)
	{
		if (!(_artifacts.Get(artifact.EntityId) == null))
		{
			_artifacts.Remove(artifact.EntityId);
			if (this.Removed != null)
			{
				this.Removed(artifact);
			}
			artifact.OnRemoved();
			UnityEngine.Object.Destroy(artifact.gameObject);
		}
	}

	private void ArtifactIntoTileObject(Artifact artifact)
	{
		Point2 worldTile = artifact.WorldTile;
		Point2 size = artifact.Size;
		int num = Mathf.Max(1, artifact.Height);
		bool hasValue = artifact.Floor.HasValue;
		bool flag = false;
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				Point2 point = worldTile + new Point2(j, i);
				TileObject tileObject = Durango.Utils.Singleton<TerrainBase>.Instance().GetTileObject(point, warning: false);
				if (tileObject == null || tileObject.Artifact == artifact)
				{
					continue;
				}
				Artifact artifact2 = tileObject.Artifact;
				if (hasValue)
				{
					if (artifact2 == null)
					{
						continue;
					}
					for (int k = 0; k < num; k++)
					{
						artifact2.SetInterior(point - artifact2.WorldTile, artifact.Floor.Value, k, artifact);
					}
				}
				else
				{
					if (artifact2 != null && artifact.GetArtifactComponent<EnterableArtifact>() == null)
					{
						continue;
					}
					tileObject.SetArtifact(artifact);
				}
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int l = -1; l < size.y + 1; l++)
		{
			for (int m = -1; m < size.x + 1; m++)
			{
				Point2 point2 = worldTile + new Point2(m, l);
				TerrainChunkBase chunkFromTile = Durango.Utils.Singleton<TerrainBase>.Instance().GetChunkFromTile(point2);
				if (!(chunkFromTile == null))
				{
					Point2 tile = chunkFromTile.FromWorldTile(point2);
					chunkFromTile.RemoveGrass(tile);
				}
			}
		}
		artifact.ArtifactPlaced();
		if (!artifact.Stories.Value.HasValue)
		{
			return;
		}
		foreach (KeyValuePair<string, Artifact> artifact3 in _artifacts)
		{
			Artifact value = artifact3.Value;
			if (!(value == null) && !value.IsFullyAttached())
			{
				Point2 point3 = value.WorldTile - worldTile;
				if (point3.x >= 0 && point3.x < size.x && point3.y >= 0 && point3.y < size.y)
				{
					ArtifactIntoTileObject(value);
				}
			}
		}
	}

	public GameObject GetSiteIconPrefab()
	{
		return _siteIconPrefab;
	}

	public string GetEsateFencePath()
	{
		return _estateFencePrefab;
	}

	public string GetEstateLinePath()
	{
		return _estateLinePrefabs;
	}

	public bool ShowArtifactInteriorMsg([NotNull] Artifact artifact, ArtifactState state, bool hidePreviousMsg = false)
	{
		if (artifact.Blueprint == null || !artifact.Blueprint.InteriorSetEffect)
		{
			return false;
		}
		string interiorSetName = GetInteriorSetName(state.InteriorSet);
		string interiorMoodName = GetInteriorMoodName(state.InteriorMood);
		string text = ((!string.IsNullOrEmpty(interiorSetName)) ? (string.IsNullOrEmpty(interiorMoodName) ? interiorSetName : (interiorMoodName + " " + interiorSetName)) : (string.IsNullOrEmpty(interiorMoodName) ? string.Empty : T._("{0} 공간", interiorMoodName)));
		if (!string.IsNullOrEmpty(text))
		{
			if (hidePreviousMsg)
			{
				HideArtifactInteriorMsg();
			}
			UIManager.Alarm.RewardAlarm("artifact_interior", new AlarmRewardQueue.Args
			{
				Main = text
			}, AlarmGroup.RewardEffectType.ArtifactInterior);
			return true;
		}
		return false;
	}

	public void HideArtifactInteriorMsg()
	{
		UIManager.Alarm.StopRewardAlarm("artifact_interior");
	}

	private static string GetInteriorMoodName(ArtifactMood? mood)
	{
		ArtifactInteriorMood artifactInteriorMood = null;
		if (mood.HasValue && !string.IsNullOrEmpty(mood.Value.SelectedId))
		{
			artifactInteriorMood = Yaml.Util.Singleton<ArtifactSetEffectsYaml>.Instance.InteriorMood.Get(mood.Value.SelectedId);
		}
		if (artifactInteriorMood != null)
		{
			return artifactInteriorMood.Name;
		}
		return string.Empty;
	}

	private static string GetInteriorSetName(ArtifactSet? set)
	{
		ArtifactInteriorSet artifactInteriorSet = null;
		if (set.HasValue && !string.IsNullOrEmpty(set.Value.SelectedId))
		{
			artifactInteriorSet = Yaml.Util.Singleton<ArtifactSetEffectsYaml>.Instance.InteriorSet.Get(set.Value.SelectedId);
		}
		if (artifactInteriorSet != null)
		{
			return artifactInteriorSet.Name;
		}
		return string.Empty;
	}
}
