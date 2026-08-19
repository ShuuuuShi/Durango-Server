using System;
using System.Collections.Generic;
using UnityEngine;
using tk2dRuntime;
using tk2dRuntime.TileMap;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/TileMap/TileMap")]
public class tk2dTileMap : MonoBehaviour, ISpriteCollectionForceBuild
{
	[Serializable]
	public class TilemapPrefabInstance
	{
		public int x;

		public int y;

		public int layer;

		public GameObject instance;
	}

	[Flags]
	public enum BuildFlags
	{
		Default = 0,
		EditMode = 1,
		ForceBuild = 2
	}

	public string editorDataGUID = string.Empty;

	public tk2dTileMapData data;

	public GameObject renderData;

	[SerializeField]
	private tk2dSpriteCollectionData spriteCollection;

	[SerializeField]
	private int spriteCollectionKey;

	public int width = 128;

	public int height = 128;

	public int partitionSizeX = 32;

	public int partitionSizeY = 32;

	[SerializeField]
	private Layer[] layers;

	[SerializeField]
	private ColorChannel colorChannel;

	[SerializeField]
	private GameObject prefabsRoot;

	[SerializeField]
	private List<TilemapPrefabInstance> tilePrefabsList = new List<TilemapPrefabInstance>();

	[SerializeField]
	private bool _inEditMode;

	public string serializedMeshPath;

	public tk2dSpriteCollectionData Editor__SpriteCollection
	{
		get
		{
			return spriteCollection;
		}
		set
		{
			spriteCollection = value;
		}
	}

	public tk2dSpriteCollectionData SpriteCollectionInst
	{
		get
		{
			if ((Object)(object)spriteCollection != (Object)null)
			{
				return spriteCollection.inst;
			}
			return null;
		}
	}

	public bool AllowEdit => _inEditMode;

	public List<TilemapPrefabInstance> TilePrefabsList => tilePrefabsList;

	public Layer[] Layers
	{
		get
		{
			return layers;
		}
		set
		{
			layers = value;
		}
	}

	public ColorChannel ColorChannel
	{
		get
		{
			return colorChannel;
		}
		set
		{
			colorChannel = value;
		}
	}

	public GameObject PrefabsRoot
	{
		get
		{
			return prefabsRoot;
		}
		set
		{
			prefabsRoot = value;
		}
	}

	private void Awake()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (Object.op_Implicit((Object)(object)SpriteCollectionInst) && (SpriteCollectionInst.buildKey != spriteCollectionKey || SpriteCollectionInst.needMaterialInstance))
		{
			flag = false;
		}
		if ((int)Application.platform == 7 || (int)Application.platform == 0)
		{
			if ((Application.isPlaying && _inEditMode) || !flag)
			{
				EndEditMode();
			}
			else if ((Object)(object)spriteCollection != (Object)null && (Object)(object)data != (Object)null && (Object)(object)renderData == (Object)null)
			{
				Build(BuildFlags.ForceBuild);
			}
		}
		else if (_inEditMode)
		{
			Debug.LogError((object)("Tilemap " + ((Object)this).name + " is still in edit mode. Please fix.Building overhead will be significant."));
			EndEditMode();
		}
		else if (!flag)
		{
			Build(BuildFlags.ForceBuild);
		}
		else if ((Object)(object)spriteCollection != (Object)null && (Object)(object)data != (Object)null && (Object)(object)renderData == (Object)null)
		{
			Build(BuildFlags.ForceBuild);
		}
	}

	private void OnDestroy()
	{
		if (layers != null)
		{
			Layer[] array = layers;
			foreach (Layer layer in array)
			{
				layer.DestroyGameData(this);
			}
		}
		if ((Object)(object)renderData != (Object)null)
		{
			tk2dUtil.DestroyImmediate((Object)(object)renderData);
		}
	}

	public void Build()
	{
		Build(BuildFlags.Default);
	}

	public void ForceBuild()
	{
		Build(BuildFlags.ForceBuild);
	}

	private void ClearSpawnedInstances()
	{
		if (layers == null)
		{
			return;
		}
		BuilderUtil.HideTileMapPrefabs(this);
		for (int i = 0; i < layers.Length; i++)
		{
			Layer layer = layers[i];
			for (int j = 0; j < layer.spriteChannel.chunks.Length; j++)
			{
				SpriteChunk spriteChunk = layer.spriteChannel.chunks[j];
				if (!((Object)(object)spriteChunk.gameObject == (Object)null))
				{
					Transform transform = spriteChunk.gameObject.transform;
					List<Transform> list = new List<Transform>();
					for (int k = 0; k < transform.childCount; k++)
					{
						list.Add(transform.GetChild(k));
					}
					for (int l = 0; l < list.Count; l++)
					{
						tk2dUtil.DestroyImmediate((Object)(object)((Component)list[l]).gameObject);
					}
				}
			}
		}
	}

	private void SetPrefabsRootActive(bool active)
	{
		if ((Object)(object)prefabsRoot != (Object)null)
		{
			tk2dUtil.SetActive(prefabsRoot, active);
		}
	}

	public void Build(BuildFlags buildFlags)
	{
		if (!((Object)(object)data != (Object)null) || !((Object)(object)spriteCollection != (Object)null))
		{
			return;
		}
		if (data.tilePrefabs == null)
		{
			data.tilePrefabs = (GameObject[])(object)new GameObject[SpriteCollectionInst.Count];
		}
		else if (data.tilePrefabs.Length != SpriteCollectionInst.Count)
		{
			Array.Resize(ref data.tilePrefabs, SpriteCollectionInst.Count);
		}
		BuilderUtil.InitDataStore(this);
		if (Object.op_Implicit((Object)(object)SpriteCollectionInst))
		{
			SpriteCollectionInst.InitMaterialIds();
		}
		bool flag = (buildFlags & BuildFlags.ForceBuild) != 0;
		if (Object.op_Implicit((Object)(object)SpriteCollectionInst) && SpriteCollectionInst.buildKey != spriteCollectionKey)
		{
			flag = true;
		}
		Dictionary<Layer, bool> dictionary = new Dictionary<Layer, bool>();
		if (layers != null)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				Layer layer = layers[i];
				if (layer != null && (Object)(object)layer.gameObject != (Object)null)
				{
					dictionary[layer] = layer.gameObject.activeSelf;
				}
			}
		}
		if (flag)
		{
			ClearSpawnedInstances();
		}
		BuilderUtil.CreateRenderData(this, _inEditMode, dictionary);
		RenderMeshBuilder.Build(this, _inEditMode, flag);
		if (!_inEditMode)
		{
			tk2dSpriteDefinition firstValidDefinition = SpriteCollectionInst.FirstValidDefinition;
			if (firstValidDefinition != null && firstValidDefinition.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D)
			{
				ColliderBuilder2D.Build(this, flag);
			}
			else
			{
				ColliderBuilder3D.Build(this, flag);
			}
			BuilderUtil.SpawnPrefabs(this, flag);
		}
		Layer[] array = layers;
		foreach (Layer layer2 in array)
		{
			layer2.ClearDirtyFlag();
		}
		if (colorChannel != null)
		{
			colorChannel.ClearDirtyFlag();
		}
		if (Object.op_Implicit((Object)(object)SpriteCollectionInst))
		{
			spriteCollectionKey = SpriteCollectionInst.buildKey;
		}
	}

	public bool GetTileAtPosition(Vector3 position, out int x, out int y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float x2;
		float y2;
		bool tileFracAtPosition = GetTileFracAtPosition(position, out x2, out y2);
		x = (int)x2;
		y = (int)y2;
		return tileFracAtPosition;
	}

	public bool GetTileFracAtPosition(Vector3 position, out float x, out float y)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		switch (data.tileType)
		{
		case tk2dTileMapData.TileType.Rectangular:
		{
			Matrix4x4 worldToLocalMatrix2 = ((Component)this).transform.worldToLocalMatrix;
			Vector3 val2 = ((Matrix4x4)(ref worldToLocalMatrix2)).MultiplyPoint(position);
			x = (val2.x - data.tileOrigin.x) / data.tileSize.x;
			y = (val2.y - data.tileOrigin.y) / data.tileSize.y;
			return x >= 0f && x < (float)width && y >= 0f && y < (float)height;
		}
		case tk2dTileMapData.TileType.Isometric:
		{
			if (data.tileSize.x == 0f)
			{
				break;
			}
			float num = Mathf.Atan2(data.tileSize.y, data.tileSize.x / 2f);
			Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
			Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(position);
			x = (val.x - data.tileOrigin.x) / data.tileSize.x;
			y = (val.y - data.tileOrigin.y) / data.tileSize.y;
			float num2 = y * 0.5f;
			int num3 = (int)num2;
			float num4 = num2 - (float)num3;
			float num5 = x % 1f;
			x = (int)x;
			y = num3 * 2;
			if (num5 > 0.5f)
			{
				if (num4 > 0.5f && Mathf.Atan2(1f - num4, (num5 - 0.5f) * 2f) < num)
				{
					y += 1f;
				}
				else if (num4 < 0.5f && Mathf.Atan2(num4, (num5 - 0.5f) * 2f) < num)
				{
					y -= 1f;
				}
			}
			else if (num5 < 0.5f)
			{
				if (num4 > 0.5f && Mathf.Atan2(num4 - 0.5f, num5 * 2f) > num)
				{
					y += 1f;
					x -= 1f;
				}
				if (num4 < 0.5f && Mathf.Atan2(num4, (0.5f - num5) * 2f) < num)
				{
					y -= 1f;
					x -= 1f;
				}
			}
			return x >= 0f && x < (float)width && y >= 0f && y < (float)height;
		}
		}
		x = 0f;
		y = 0f;
		return false;
	}

	public Vector3 GetTilePosition(int x, int y)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		tk2dTileMapData.TileType tileType = data.tileType;
		if (tileType == tk2dTileMapData.TileType.Rectangular || tileType != tk2dTileMapData.TileType.Isometric)
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector((float)x * data.tileSize.x + data.tileOrigin.x, (float)y * data.tileSize.y + data.tileOrigin.y, 0f);
			Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
			return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(val);
		}
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(((float)x + ((((uint)y & (true ? 1u : 0u)) != 0) ? 0.5f : 0f)) * data.tileSize.x + data.tileOrigin.x, (float)y * data.tileSize.y + data.tileOrigin.y, 0f);
		Matrix4x4 localToWorldMatrix2 = ((Component)this).transform.localToWorldMatrix;
		return ((Matrix4x4)(ref localToWorldMatrix2)).MultiplyPoint(val2);
	}

	public int GetTileIdAtPosition(Vector3 position, int layer)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (layer < 0 || layer >= layers.Length)
		{
			return -1;
		}
		if (!GetTileAtPosition(position, out var x, out var y))
		{
			return -1;
		}
		return layers[layer].GetTile(x, y);
	}

	public TileInfo GetTileInfoForTileId(int tileId)
	{
		return data.GetTileInfoForSprite(tileId);
	}

	public Color GetInterpolatedColorAtPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
		Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(position);
		int num = (int)((val.x - data.tileOrigin.x) / data.tileSize.x);
		int num2 = (int)((val.y - data.tileOrigin.y) / data.tileSize.y);
		if (colorChannel == null || colorChannel.IsEmpty)
		{
			return Color.white;
		}
		if (num < 0 || num >= width || num2 < 0 || num2 >= height)
		{
			return colorChannel.clearColor;
		}
		int offset;
		ColorChunk colorChunk = colorChannel.FindChunkAndCoordinate(num, num2, out offset);
		if (colorChunk.Empty)
		{
			return colorChannel.clearColor;
		}
		int num3 = partitionSizeX + 1;
		Color val2 = Color32.op_Implicit(colorChunk.colors[offset]);
		Color val3 = Color32.op_Implicit(colorChunk.colors[offset + 1]);
		Color val4 = Color32.op_Implicit(colorChunk.colors[offset + num3]);
		Color val5 = Color32.op_Implicit(colorChunk.colors[offset + num3 + 1]);
		float num4 = (float)num * data.tileSize.x + data.tileOrigin.x;
		float num5 = (float)num2 * data.tileSize.y + data.tileOrigin.y;
		float num6 = (val.x - num4) / data.tileSize.x;
		float num7 = (val.y - num5) / data.tileSize.y;
		Color val6 = Color.Lerp(val2, val3, num6);
		Color val7 = Color.Lerp(val4, val5, num6);
		return Color.Lerp(val6, val7, num7);
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return (Object)(object)this.spriteCollection != (Object)null && ((Object)(object)spriteCollection == (Object)(object)this.spriteCollection || (Object)(object)spriteCollection == (Object)(object)this.spriteCollection.inst);
	}

	public void EndEditMode()
	{
		_inEditMode = false;
		SetPrefabsRootActive(active: true);
		Build(BuildFlags.ForceBuild);
		if ((Object)(object)prefabsRoot != (Object)null)
		{
			tk2dUtil.DestroyImmediate((Object)(object)prefabsRoot);
			prefabsRoot = null;
		}
	}

	public void TouchMesh(Mesh mesh)
	{
	}

	public void DestroyMesh(Mesh mesh)
	{
		tk2dUtil.DestroyImmediate((Object)(object)mesh);
	}

	public int GetTilePrefabsListCount()
	{
		return tilePrefabsList.Count;
	}

	public void GetTilePrefabsListItem(int index, out int x, out int y, out int layer, out GameObject instance)
	{
		TilemapPrefabInstance tilemapPrefabInstance = tilePrefabsList[index];
		x = tilemapPrefabInstance.x;
		y = tilemapPrefabInstance.y;
		layer = tilemapPrefabInstance.layer;
		instance = tilemapPrefabInstance.instance;
	}

	public void SetTilePrefabsList(List<int> xs, List<int> ys, List<int> layers, List<GameObject> instances)
	{
		int count = instances.Count;
		tilePrefabsList = new List<TilemapPrefabInstance>(count);
		for (int i = 0; i < count; i++)
		{
			TilemapPrefabInstance tilemapPrefabInstance = new TilemapPrefabInstance();
			tilemapPrefabInstance.x = xs[i];
			tilemapPrefabInstance.y = ys[i];
			tilemapPrefabInstance.layer = layers[i];
			tilemapPrefabInstance.instance = instances[i];
			tilePrefabsList.Add(tilemapPrefabInstance);
		}
	}

	public int GetTile(int x, int y, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
		{
			return -1;
		}
		return layers[layer].GetTile(x, y);
	}

	public tk2dTileFlags GetTileFlags(int x, int y, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
		{
			return tk2dTileFlags.None;
		}
		return layers[layer].GetTileFlags(x, y);
	}

	public void SetTile(int x, int y, int layer, int tile)
	{
		if (layer >= 0 && layer < layers.Length)
		{
			layers[layer].SetTile(x, y, tile);
		}
	}

	public void SetTileFlags(int x, int y, int layer, tk2dTileFlags flags)
	{
		if (layer >= 0 && layer < layers.Length)
		{
			layers[layer].SetTileFlags(x, y, flags);
		}
	}

	public void ClearTile(int x, int y, int layer)
	{
		if (layer >= 0 && layer < layers.Length)
		{
			layers[layer].ClearTile(x, y);
		}
	}
}
