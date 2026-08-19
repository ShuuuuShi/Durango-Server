using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGrid : MonoBehaviour
{
	public class RoadTile
	{
		public string Sprite;

		public Vector2 Pivot;

		public Vector2[] Vectors = (Vector2[])(object)new Vector2[4];

		public bool IsValidVectors;

		public int LinkCount;

		public RoadGrid Grid { get; private set; }

		public RoadTile(RoadGrid grid)
		{
			Grid = grid;
		}

		public void SetDirty()
		{
			IsValidVectors = false;
			Grid.UpdateRoad();
		}
	}

	public static readonly Point2[] Directions = new Point2[4]
	{
		Point2.right,
		Point2.up,
		Point2.left,
		Point2.down
	};

	private static readonly BetterList<Vector3> Verts = new BetterList<Vector3>();

	private static readonly BetterList<Vector2> Uvs = new BetterList<Vector2>();

	private static readonly BetterList<Vector2> Uv2s = new BetterList<Vector2>();

	private static readonly BetterList<int> Tris = new BetterList<int>();

	private readonly RoadTile[] _roads = new RoadTile[256];

	private Mesh _roadMesh;

	private GameObject _roadMeshObject;

	private TerrainChunkA6 _chunk;

	private bool _isUpdatingRoad;

	private List<LineRenderer> _debugLines;

	public void Init(TerrainChunkA6 chunk)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		_chunk = chunk;
		_roadMesh = new Mesh();
		PrepareMeshObject();
		KSingleton<GameManager>.Instance().PostReconnect += PrepareMeshObject;
	}

	private void PrepareMeshObject()
	{
		if ((Object)(object)_roadMeshObject != (Object)null)
		{
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset("Models/Prop/road/road_grid.prefab", typeof(GameObject), delegate(Object asset)
		{
			GameObject val = (GameObject)(object)((asset is GameObject) ? asset : null);
			if (!((Object)(object)val == (Object)null))
			{
				_roadMeshObject = ((Component)_chunk.StaticObjectChunk).gameObject.AddChild(val);
			}
		});
	}

	public RoadTile GetRoad(Point2 localTile)
	{
		if (localTile.x < 0 || localTile.y < 0 || localTile.x >= 16 || localTile.y >= 16)
		{
			Point2 tile = _chunk.ToWorldTile(localTile);
			return RoadManager.GetRoad(tile);
		}
		int num = localTile.x + localTile.y * 16;
		RoadTile roadTile = _roads[num];
		if (roadTile == null)
		{
			roadTile = new RoadTile(this);
			_roads[num] = roadTile;
		}
		return roadTile;
	}

	public bool HasRoad(Point2 localTile)
	{
		if (localTile.x < 0 || localTile.y < 0 || localTile.x >= 16 || localTile.y >= 16)
		{
			Point2 tile = _chunk.ToWorldTile(localTile);
			return RoadManager.HasRoad(tile);
		}
		int num = localTile.x + localTile.y * 16;
		RoadTile roadTile = _roads[num];
		return roadTile != null;
	}

	public void AddRoad(Point2 localTile, string sprite)
	{
		RoadTile road = GetRoad(localTile);
		road.Sprite = sprite;
		road.SetDirty();
		UpdateRoadPivot(localTile);
		for (int i = 0; i < Directions.Length; i++)
		{
			UpdateRoadPivot(localTile + Directions[i]);
		}
	}

	public void RemoveRoad(Point2 localTile)
	{
		int num = localTile.x + localTile.y * 16;
		if (num < 0 || num >= _roads.Length)
		{
			Point2 tile = _chunk.ToWorldTile(localTile);
			RoadManager.RemoveRoad(tile);
			return;
		}
		RoadTile roadTile = _roads[num];
		if (roadTile != null)
		{
			_roads[num] = null;
			for (int i = 0; i < Directions.Length; i++)
			{
				UpdateRoadPivot(localTile + Directions[i]);
			}
		}
	}

	public void ClearRoad()
	{
		int i = 0;
		for (int num = _roads.Length; i < num; i++)
		{
			_roads[i] = null;
		}
		UpdateRoad();
	}

	private void UpdateRoadPivot(Point2 localTile)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		if (!HasRoad(localTile))
		{
			return;
		}
		RoadTile road = GetRoad(localTile);
		if (RoadManager.IsTileRoad)
		{
			road.Pivot = Vector2.one * 0.5f + _chunk.ToWorldTile(localTile).ToVector2();
			road.SetDirty();
			return;
		}
		Point2 zero = Point2.zero;
		int num = 0;
		for (int i = 0; i < Directions.Length; i++)
		{
			Point2 point = Directions[i];
			if (!HasRoad(localTile + point))
			{
				continue;
			}
			RoadTile road2 = GetRoad(localTile + point);
			road2.SetDirty();
			for (int j = 0; j < Directions.Length; j++)
			{
				if (HasRoad(localTile + point + Directions[j]))
				{
					GetRoad(localTile + point + Directions[j]).SetDirty();
				}
			}
			zero += point;
			num++;
		}
		Point2 point2 = _chunk.ToWorldTile(localTile);
		Vector2 val = Vector2.one * 0.5f + point2.ToVector2();
		road.LinkCount = num;
		val = ((num <= 1) ? (val - zero.ToVector2() * RoadManager.PivotRatio) : (val + zero.ToVector2() * RoadManager.PivotRatio));
		if (RoadManager.RandomOffset > 0f)
		{
			float hashValue = _chunk.GetHashValue(ChunkHash.Category.RoadGrid, localTile.x, localTile.y);
			val += Vector2.one * (hashValue * 2f - 1f) * RoadManager.RandomOffset;
		}
		road.Pivot = val;
		road.SetDirty();
	}

	private void UpdateRoad()
	{
		if (!_isUpdatingRoad)
		{
			_isUpdatingRoad = true;
			((MonoBehaviour)this).StartCoroutine(CoDelayUpdateRoad());
		}
	}

	private IEnumerator CoDelayUpdateRoad()
	{
		do
		{
			yield return null;
		}
		while (KSingleton<TerrainA6>.Instance().IsChunkLoading || (Object)(object)_roadMeshObject == (Object)null);
		UpdateRoadMesh();
		_isUpdatingRoad = false;
	}

	private void UpdateRoadMesh()
	{
		Verts.Clear();
		Uvs.Clear();
		Uv2s.Clear();
		Tris.Clear();
		int i = 0;
		Point2 localTile = default(Point2);
		for (int num = _roads.Length; i < num; i++)
		{
			RoadTile roadTile = _roads[i];
			if (roadTile != null)
			{
				localTile.x = i % 16;
				localTile.y = i / 16;
				DrawRoad(localTile);
			}
		}
		Mesh roadMesh = _roadMesh;
		roadMesh.Clear();
		roadMesh.vertices = Verts.ToArray();
		roadMesh.uv = Uvs.ToArray();
		roadMesh.uv2 = Uv2s.ToArray();
		roadMesh.triangles = Tris.ToArray();
		MeshFilter component = _roadMeshObject.GetComponent<MeshFilter>();
		component.sharedMesh = roadMesh;
	}

	private void UpdateRoadVectors(Point2 localTile)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		RoadTile road = GetRoad(localTile);
		if (road.IsValidVectors)
		{
			return;
		}
		for (int i = 0; i < road.Vectors.Length; i++)
		{
			ref Vector2 reference = ref road.Vectors[i];
			reference = Vector2.zero;
		}
		Vector2 val = Vector2.zero;
		for (int j = 0; j < Directions.Length; j++)
		{
			Point2 point = Directions[j];
			if (HasRoad(localTile + point))
			{
				RoadTile road2 = GetRoad(localTile + point);
				Vector2 val2 = val;
				Vector2 val3 = road.Pivot - road2.Pivot;
				val = val2 + ((Vector2)(ref val3)).normalized;
				ref Vector2 reference2 = ref road.Vectors[j];
				reference2 = point.ToVector2();
			}
			else
			{
				ref Vector2 reference3 = ref road.Vectors[j];
				reference3 = Vector2.zero;
			}
		}
		for (int k = 0; k < Directions.Length; k++)
		{
			Point2 point2 = Directions[k];
			if (!(road.Vectors[k] == Vector2.zero))
			{
				RoadTile road3 = GetRoad(localTile + point2);
				ref Vector2 reference4 = ref road.Vectors[k];
				Vector2 val4 = val;
				Vector2 val5 = road3.Pivot - road.Pivot;
				reference4 = val4 + ((Vector2)(ref val5)).normalized * 2f;
				((Vector2)(ref road.Vectors[k])).Normalize();
			}
		}
		road.IsValidVectors = true;
	}

	private void DrawRoad(Point2 localTile)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		RoadTile road = GetRoad(localTile);
		UpdateRoadVectors(localTile);
		Vector2 val = road.Grid._chunk.ChunkTileOffset.ToVector2();
		Vector2 val2 = road.Pivot - val;
		Rect roadRect = RoadManager.GetRoadRect(road.Sprite);
		Rect maskingRect = RoadManager.GetMaskingRect(road.LinkCount);
		bool flag = true;
		if (!RoadManager.IsTileRoad)
		{
			int i = 0;
			for (int num = Directions.Length; i < num; i++)
			{
				Point2 point = Directions[i];
				if (HasRoad(localTile + point))
				{
					flag = false;
					RoadTile road2 = GetRoad(localTile + point);
					UpdateRoadVectors(localTile + point);
					Vector2 p = val2;
					Vector2 v = road.Vectors[i];
					Vector2 p2 = road2.Pivot - val;
					Vector2 v2 = road2.Vectors[(i + 2) % num];
					int count = Mathf.Max(RoadManager.CurveLineCount, 1);
					FillMesh(p, p2, new Vector2(0f - v.y, v.x), new Vector2(v2.y, 0f - v2.x), new Vector2(v.y, 0f - v.x), new Vector2(0f - v2.y, v2.x), v, v2, count, roadRect, maskingRect, road.LinkCount == 1 || (localTile.x + localTile.y) % 2 == 0);
				}
			}
		}
		if (flag)
		{
			Vector3 pos = new Vector3(val2.x, 0f, val2.y) * 200f;
			FillDefaultMesh(pos, roadRect, maskingRect);
		}
	}

	private void FillMesh(Vector2 p1, Vector2 p2, Vector2 l1, Vector2 l2, Vector2 r1, Vector2 r2, Vector2 v1, Vector2 v2, int count, Rect roadUv, Rect maskUv, bool reverseUv)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		float num = RoadManager.RoadWidth * 0.5f;
		KMathUtil.BezierCurve4 bezierCurve = KMathUtil.MakeBezierCurve4(p1 + r1 * num / 200f, p2 + r2 * num / 200f, v1, v2);
		KMathUtil.BezierCurve4 bezierCurve2 = KMathUtil.MakeBezierCurve4(p1 + l1 * num / 200f, p2 + l2 * num / 200f, v1, v2);
		int size = Verts.size;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < count + 1; i++)
		{
			float num4 = 0.5f / (float)count * (float)i;
			Vector2 val = bezierCurve.Get(num4);
			Vector2 val2 = bezierCurve2.Get(num4);
			for (int j = 0; j < 2; j++)
			{
				Vector3 val3 = Vector3.zero;
				Vector2 zero = Vector2.zero;
				switch (j)
				{
				case 0:
					val3 = new Vector3(val.x, 0f, val.y) * 200f;
					((Vector2)(ref zero))._002Ector(0f, num4);
					break;
				case 1:
					val3 = new Vector3(val2.x, 0f, val2.y) * 200f;
					((Vector2)(ref zero))._002Ector(1f, num4);
					break;
				}
				if (i > 0)
				{
					switch (j)
					{
					case 0:
					{
						float num6 = num2;
						Vector3 val5 = val3 - Verts[Verts.size - 2];
						num2 = num6 + ((Vector3)(ref val5)).magnitude;
						break;
					}
					case 1:
					{
						float num5 = num3;
						Vector3 val4 = val3 - Verts[Verts.size - 2];
						num3 = num5 + ((Vector3)(ref val4)).magnitude;
						break;
					}
					}
				}
				Verts.Add(val3);
				Uvs.Add(zero);
			}
		}
		float num7 = Mathf.Min(2f, num2 / num);
		float num8 = Mathf.Min(2f, num3 / num);
		Vector2 value = default(Vector2);
		Vector2 item = default(Vector2);
		for (int k = size; k < Uvs.size; k++)
		{
			Vector2 val6 = Uvs[k];
			val6.y *= ((k % 2 != 0) ? num8 : num7);
			if (reverseUv)
			{
				val6.y = 1f - val6.y;
			}
			value.x = ((Rect)(ref roadUv)).x + ((Rect)(ref roadUv)).width * val6.x;
			value.y = ((Rect)(ref roadUv)).y + ((Rect)(ref roadUv)).height * val6.y;
			item.x = ((Rect)(ref maskUv)).x + ((Rect)(ref maskUv)).width * val6.x;
			item.y = ((Rect)(ref maskUv)).y + ((Rect)(ref maskUv)).height * val6.y;
			Uvs[k] = value;
			Uv2s.Add(item);
		}
		for (int m = 0; m < count; m++)
		{
			Tris.Add(size + m * 2);
			Tris.Add(size + 1 + m * 2);
			Tris.Add(size + 2 + m * 2);
			Tris.Add(size + 2 + m * 2);
			Tris.Add(size + 1 + m * 2);
			Tris.Add(size + 3 + m * 2);
		}
	}

	private void FillDefaultMesh(Vector3 pos, Rect roadUv, Rect maskUv)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		float num = ((!RoadManager.IsTileRoad) ? (RoadManager.RoadWidth * 0.5f) : 140f);
		int size = Verts.size;
		Verts.Add(pos + new Vector3(0f - num, 0f, 0f - num));
		Verts.Add(pos + new Vector3(0f - num, 0f, num));
		Verts.Add(pos + new Vector3(num, 0f, 0f - num));
		Verts.Add(pos + new Vector3(num, 0f, num));
		Uvs.Add(new Vector2(((Rect)(ref roadUv)).xMin, ((Rect)(ref roadUv)).yMin));
		Uvs.Add(new Vector2(((Rect)(ref roadUv)).xMin, ((Rect)(ref roadUv)).yMax));
		Uvs.Add(new Vector2(((Rect)(ref roadUv)).xMax, ((Rect)(ref roadUv)).yMin));
		Uvs.Add(new Vector2(((Rect)(ref roadUv)).xMax, ((Rect)(ref roadUv)).yMax));
		Uv2s.Add(new Vector2(((Rect)(ref maskUv)).xMin, ((Rect)(ref maskUv)).yMin));
		Uv2s.Add(new Vector2(((Rect)(ref maskUv)).xMin, ((Rect)(ref maskUv)).yMax));
		Uv2s.Add(new Vector2(((Rect)(ref maskUv)).xMax, ((Rect)(ref maskUv)).yMin));
		Uv2s.Add(new Vector2(((Rect)(ref maskUv)).xMax, ((Rect)(ref maskUv)).yMax));
		Tris.Add(size);
		Tris.Add(size + 1);
		Tris.Add(size + 2);
		Tris.Add(size + 2);
		Tris.Add(size + 1);
		Tris.Add(size + 3);
	}

	public void ForceUpdateRoads()
	{
		int i = 0;
		Point2 localTile = default(Point2);
		for (int num = _roads.Length; i < num; i++)
		{
			RoadTile roadTile = _roads[i];
			if (roadTile != null)
			{
				localTile.x = i % 16;
				localTile.y = i / 16;
				UpdateRoadPivot(localTile);
			}
		}
	}

	private void ClearDebugLines()
	{
		if (_debugLines != null)
		{
			int i = 0;
			for (int count = _debugLines.Count; i < count; i++)
			{
				Object.Destroy((Object)(object)((Component)_debugLines[i]).gameObject);
			}
			_debugLines.Clear();
			if (!RoadManager.ShowDebugLine)
			{
				_debugLines = null;
			}
		}
	}

	private void DrawLine(IList<Vector3> points)
	{
		LineRenderer val = ((Component)_chunk.StaticObjectChunk).gameObject.AddChild<LineRenderer>();
		((Renderer)val).sharedMaterial = RoadManager.DebugLineMaterial;
		val.useWorldSpace = false;
		val.SetVertexCount(points.Count);
		val.SetWidth(4f, 4f);
		Vector3[] array = (Vector3[])(object)new Vector3[points.Count];
		points.CopyTo(array, 0);
		val.SetPositions(array);
		if (_debugLines == null)
		{
			_debugLines = new List<LineRenderer>();
		}
		_debugLines.Add(val);
	}
}
