using System;
using System.Collections;
using System.Collections.Generic;
using Building_;
using JetBrains.Annotations;
using Messages;
using Shared.Etc;
using UnityEngine;

public class StaticObjectManager : KSingleton<StaticObjectManager>
{
	[SerializeField]
	private GameObject _artifactPrefab;

	[SerializeField]
	private string[] _estateFencePrefab;

	[SerializeField]
	private string[] _clanEstateFencePrefab;

	[SerializeField]
	private string[] _estateLinePrefabs;

	[SerializeField]
	private string[] _invalidEsateLinePrefabs;

	private readonly Dictionary<ulong, Artifact> _artifacts = new Dictionary<ulong, Artifact>();

	private GameObject _loading;

	public event Action<Artifact> ArtifactAdded;

	public event Action<Artifact> ArtifactRemoved;

	private void Start()
	{
		KSingleton<TerrainA6>.Instance().LoadingChunksFinished += TerrainA6_LoadingChunksFinished;
		KSingleton<GameManager>.Instance().PreReconnect += GameManager_PreReconnect;
	}

	private void GameManager_PreReconnect()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		RemoveAllArtifacts();
	}

	private void RemoveAllArtifacts()
	{
		Artifact[] array = new Artifact[_artifacts.Values.Count];
		_artifacts.Values.CopyTo(array, 0);
		foreach (Artifact artifact in array)
		{
			if ((Object)(object)artifact != (Object)null)
			{
				RemoveArtifact(artifact);
			}
		}
		_artifacts.Clear();
	}

	private void TerrainA6_LoadingChunksFinished()
	{
		UpdateArtifactArea();
	}

	public IEnumerator AddStaticObject(Point2 worldTile, GameObject staticObject, bool center)
	{
		if ((Object)(object)_loading == (Object)null)
		{
			_loading = new GameObject("Loading Objects");
			_loading.transform.localPosition = new Vector3(0f, 10000f, 0f);
		}
		staticObject.transform.parent = _loading.transform;
		staticObject.transform.localPosition = Vector3.zero;
		TerrainChunkA6 chunk;
		while (true)
		{
			if ((Object)(object)staticObject == (Object)null)
			{
				yield break;
			}
			chunk = TerrainA6.GetChunkFromTile(worldTile);
			if ((Object)(object)chunk == (Object)null || chunk.IsLoading())
			{
				yield return null;
				continue;
			}
			break;
		}
		Point2 tile = chunk.FromWorldTile(worldTile);
		chunk.StaticObjectChunk.AttachObject(tile, staticObject, center, Vector3.zero, Quaternion.identity);
		Artifact artifact = staticObject.GetComponent<Artifact>();
		if ((Object)(object)artifact != (Object)null)
		{
			ArtifactIntoTileObject(artifact);
		}
	}

	public bool HandleDisappearMsg(DisappearEntity msg)
	{
		Artifact artifact = FindArtifact(msg.EntityId);
		if (Object.op_Implicit((Object)(object)artifact))
		{
			RemoveArtifact(artifact);
			return true;
		}
		return false;
	}

	public void RemoveImmovable(Point2 worldTile, ulong entityId = 0, double eventAt = -1.0)
	{
		TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(worldTile);
		if ((Object)(object)chunkFromTile == (Object)null)
		{
			return;
		}
		TileObject tileObject = TerrainA6.GetTileObject(worldTile);
		if (tileObject == null)
		{
			return;
		}
		if ((Object)(object)tileObject.NaturalObject != (Object)null)
		{
			chunkFromTile.RemoveFromCollisionGrid(tileObject.NaturalObject, updatePartial: true);
			tileObject.RemoveStaticObject();
		}
		else
		{
			if (!((Object)(object)tileObject.Artifact != (Object)null))
			{
				return;
			}
			double bufferedServerTime_Enhanced = Connections.Frontend.GetBufferedServerTime_Enhanced();
			float delay = (float)(eventAt - bufferedServerTime_Enhanced);
			KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
			{
				if (!((Object)(object)tileObject.Artifact == (Object)null))
				{
					if (entityId == 0L || tileObject.Artifact.EntityId == entityId)
					{
						RemoveArtifact(tileObject.Artifact);
					}
					else
					{
						ModularArtifact artifactComponent = tileObject.Artifact.GetArtifactComponent<ModularArtifact>();
						if (artifactComponent != null)
						{
							Artifact interior = artifactComponent.GetInterior(worldTile - artifactComponent.WorldTile);
							if ((Object)(object)interior != (Object)null)
							{
								RemoveArtifact(interior);
							}
						}
					}
				}
			}, delay);
		}
	}

	public Artifact AddArtifact(ulong entityId, Point2 worldTile, ushort entityType, Rotation rotation, Point2 size, int height)
	{
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(entityType);
		if (blueprint == null)
		{
			return null;
		}
		string id = blueprint.Id;
		rotation = ((!blueprint.RotationDisabled) ? rotation : Rotation.None);
		Artifact artifact = _artifacts.Get(entityId);
		if ((Object)(object)artifact == (Object)null)
		{
			artifact = CreateArtifactObject(blueprint);
			if ((Object)(object)artifact == (Object)null)
			{
				Debug.LogError((object)$"Create Artifact Error: {id}({entityType})");
				return null;
			}
			artifact.SetEntity(entityId, entityType, worldTile);
			artifact.Init(id, worldTile.x, worldTile.y, rotation, size, height);
			_artifacts[entityId] = artifact;
			((MonoBehaviour)this).StartCoroutine(AddStaticObject(worldTile, ((Component)artifact).gameObject, center: false));
			if (this.ArtifactAdded != null)
			{
				this.ArtifactAdded(artifact);
			}
		}
		return artifact;
	}

	private Artifact CreateArtifactObject(Blueprint blueprint)
	{
		GameObject val = Object.Instantiate<GameObject>(_artifactPrefab);
		((Object)val).name = blueprint.Id;
		Artifact artifact = val.AddComponent<Artifact>();
		int i = 0;
		for (int size = KUtility.GetSize(blueprint.Components); i < size; i++)
		{
			ArtifactComponent artifactComponent = MakeComponentScript(blueprint.Components[i]);
			if (artifactComponent != null)
			{
				artifact.AddArtifactComponent(artifactComponent);
			}
		}
		return artifact;
	}

	private static ArtifactComponent MakeComponentScript(string component)
	{
		switch (component)
		{
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
		case "Estate":
		case "ClanEstate":
			return new EstateFlag();
		case "Modular":
			return new ModularArtifact();
		case "Cage":
			return new Cage();
		case "Bridge":
			return new Bridge();
		case "Defensive":
			return new Defensive();
		case "Warehouse":
			return new Warehouse();
		case "Laboratory":
			return new Laboratory();
		default:
			return null;
		}
	}

	private void RemoveArtifact([NotNull] Artifact artifact)
	{
		if (this.ArtifactRemoved != null)
		{
			this.ArtifactRemoved(artifact);
		}
		artifact.OnRemoved();
		ReleaseOccupiedTiles(artifact.WorldTile, artifact.Size);
		_artifacts.Remove(artifact.EntityId);
		Object.Destroy((Object)(object)((Component)artifact).gameObject);
	}

	private static void ReleaseOccupiedTiles(Point2 tile, Point2 size)
	{
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				Point2 worldTile = tile + new Point2(j, i);
				TerrainA6.GetTileObject(worldTile, warning: false)?.RemoveStaticObject(fastRemove: true);
			}
		}
	}

	private void UpdateArtifactArea()
	{
		ulong[] array = new ulong[_artifacts.Keys.Count];
		_artifacts.Keys.CopyTo(array, 0);
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			Artifact artifact = _artifacts.Get(array[i]);
			if ((Object)(object)artifact == (Object)null)
			{
				_artifacts.Remove(array[i]);
				continue;
			}
			Point2 worldTile = artifact.WorldTile;
			TerrainChunkA6 chunkFromTile = TerrainA6.GetChunkFromTile(worldTile);
			if ((Object)(object)chunkFromTile == (Object)null)
			{
				RemoveArtifact(artifact);
			}
			else
			{
				ArtifactIntoTileObject(artifact);
			}
		}
	}

	public Artifact FindArtifact(ulong entityId)
	{
		return _artifacts.Get(entityId);
	}

	private void ArtifactIntoTileObject(Artifact artifact)
	{
		Point2 worldTile = artifact.WorldTile;
		Point2 size = artifact.Size;
		ModularArtifact artifactComponent = artifact.GetArtifactComponent<ModularArtifact>();
		bool flag = false;
		for (int i = 0; i < size.y; i++)
		{
			for (int j = 0; j < size.x; j++)
			{
				Point2 point = worldTile + new Point2(j, i);
				TileObject tileObject = TerrainA6.GetTileObject(point, warning: false);
				if (tileObject == null || (Object)(object)tileObject.Artifact == (Object)(object)artifact)
				{
					continue;
				}
				flag = true;
				if (artifactComponent == null)
				{
					ModularArtifact modularArtifact = ((!((Object)(object)tileObject.Artifact == (Object)null)) ? tileObject.Artifact.GetArtifactComponent<ModularArtifact>() : null);
					if (modularArtifact == null)
					{
						tileObject.SetArtifact(artifact);
					}
					else
					{
						modularArtifact.SetInterior(point - modularArtifact.WorldTile, artifact);
					}
				}
				else
				{
					Artifact artifact2 = tileObject.Artifact;
					artifactComponent.SetInterior(point - artifactComponent.WorldTile, artifact2);
					tileObject.SetArtifact(artifact);
				}
			}
		}
		for (int k = -1; k < size.y + 1; k++)
		{
			RemoveGrass(worldTile + new Point2(-1, k));
			RemoveGrass(worldTile + new Point2(size.x, k));
		}
		for (int l = -1; l < size.x + 1; l++)
		{
			RemoveGrass(worldTile + new Point2(l, -1));
			RemoveGrass(worldTile + new Point2(l, size.y));
		}
		if (flag)
		{
			artifact.ArtifactPlaced();
		}
	}

	public void RemoveGrass(Point2 tile)
	{
		TerrainA6.GetTileObject(tile, warning: false)?.RemoveGrassSprite();
	}

	public string GetEsateFencePath(int index, bool valid, bool clanFlag)
	{
		string[] array = ((!clanFlag) ? ((!valid) ? null : _estateFencePrefab) : ((!valid) ? null : _clanEstateFencePrefab));
		if (array == null)
		{
			return null;
		}
		if (array.Length == 0)
		{
			return null;
		}
		return array[index];
	}

	public string GetEstateLinePath(bool valid, bool clanFlag)
	{
		return (!valid) ? _invalidEsateLinePrefabs[clanFlag ? 1 : 0] : _estateLinePrefabs[clanFlag ? 1 : 0];
	}
}
