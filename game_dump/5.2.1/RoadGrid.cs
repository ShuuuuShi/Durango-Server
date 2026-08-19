using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

public class RoadGrid : MonoBehaviour
{
	public class RoadTile
	{
		public string Sprite;

		public Vector2 Pivot;

		public Vector2[] Vectors = new Vector2[4];

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

	[CompilerGenerated]
	private sealed class _003CCoDelayUpdateRoad_003Ed__21 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RoadGrid _003C_003E4__this;

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
		public _003CCoDelayUpdateRoad_003Ed__21(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			RoadGrid roadGrid = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				if (!(roadGrid._roadMeshObject == null))
				{
					roadGrid.UpdateRoadMesh();
					roadGrid._isUpdatingRoad = false;
					return false;
				}
			}
			else
			{
				_003C_003E1__state = -1;
			}
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
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

	public static readonly Point2[] Directions = new Point2[4]
	{
		Point2.right,
		Point2.up,
		Point2.left,
		Point2.down
	};

	private static readonly List<Vector3> Verts = new List<Vector3>();

	private static readonly List<Vector2> Uvs = new List<Vector2>();

	private static readonly List<Vector2> Uv2s = new List<Vector2>();

	private static readonly List<Color> Colors = new List<Color>();

	private static readonly List<int> Tris = new List<int>();

	private readonly RoadTile[] _roads = new RoadTile[256];

	private Mesh _roadMesh;

	private GameObject _roadMeshObject;

	private TerrainChunkBase _chunk;

	private bool _isUpdatingRoad;

	public void Init(TerrainChunkBase chunk)
	{
		_chunk = chunk;
		_roadMesh = new Mesh();
		PrepareMeshObject();
		Singleton<GameManager>.Instance().PostReconnect += PrepareMeshObject;
	}

	private void PrepareMeshObject()
	{
		if (_roadMeshObject != null)
		{
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(RoadManager.RoadObjectPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			GameObject gameObject = asset as GameObject;
			if (!(gameObject == null))
			{
				_roadMeshObject = _chunk.StaticObjectChunk.gameObject.AddChild(gameObject);
				UpdateRoad();
			}
		});
	}

	public RoadTile GetRoad(Point2 localTile)
	{
		if (localTile.x < 0 || localTile.y < 0 || localTile.x >= 16 || localTile.y >= 16)
		{
			return RoadManager.GetRoad(_chunk.ToWorldTile(localTile));
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
			return RoadManager.HasRoad(_chunk.ToWorldTile(localTile));
		}
		int num = localTile.x + localTile.y * 16;
		return _roads[num] != null;
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
		if (_roads[num] != null)
		{
			_roads[num] = null;
			for (int i = 0; i < Directions.Length; i++)
			{
				UpdateRoadPivot(localTile + Directions[i]);
			}
			UpdateRoad();
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
			GetRoad(localTile + point).SetDirty();
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
		Vector2 pivot = Vector2.one * 0.5f + point2.ToVector2();
		road.LinkCount = num;
		if (num > 1)
		{
			pivot += zero.ToVector2() * RoadManager.PivotRatio;
		}
		else
		{
			pivot -= zero.ToVector2() * RoadManager.PivotRatio;
		}
		if (RoadManager.RandomOffset > 0f)
		{
			float hashValue = _chunk.GetHashValue(ChunkHash.Category.RoadGrid, localTile.x, localTile.y);
			pivot += Vector2.one * (hashValue * 2f - 1f) * RoadManager.RandomOffset;
		}
		road.Pivot = pivot;
		road.SetDirty();
	}

	private void UpdateRoad()
	{
		if (!_isUpdatingRoad)
		{
			_isUpdatingRoad = true;
			StartCoroutine(CoDelayUpdateRoad());
		}
	}

	private IEnumerator CoDelayUpdateRoad()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoDelayUpdateRoad_003Ed__21(0)
		{
			_003C_003E4__this = this
		};
	}

	private void UpdateRoadMesh()
	{
		Verts.Clear();
		Uvs.Clear();
		Uv2s.Clear();
		Colors.Clear();
		Tris.Clear();
		int i = 0;
		Point2 localTile = default(Point2);
		for (int num = _roads.Length; i < num; i++)
		{
			if (_roads[i] != null)
			{
				localTile.x = i % 16;
				localTile.y = i / 16;
				DrawRoad(localTile);
			}
		}
		Mesh roadMesh = _roadMesh;
		roadMesh.Clear();
		roadMesh.SetVertices(Verts);
		roadMesh.SetUVs(0, Uvs);
		roadMesh.SetUVs(1, Uv2s);
		roadMesh.SetColors(Colors);
		roadMesh.SetTriangles(Tris, 0);
		_roadMeshObject.GetComponent<MeshFilter>().sharedMesh = roadMesh;
	}

	private void UpdateRoadVectors(Point2 localTile)
	{
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
		Vector2 zero = Vector2.zero;
		for (int j = 0; j < Directions.Length; j++)
		{
			Point2 point = Directions[j];
			if (HasRoad(localTile + point))
			{
				RoadTile road2 = GetRoad(localTile + point);
				zero += (road.Pivot - road2.Pivot).normalized;
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
				reference4 = zero + (road3.Pivot - road.Pivot).normalized * 2f;
				road.Vectors[k].Normalize();
			}
		}
		road.IsValidVectors = true;
	}

	private void DrawRoad(Point2 localTile)
	{
		RoadTile road = GetRoad(localTile);
		UpdateRoadVectors(localTile);
		Vector2 vector = road.Grid._chunk.ChunkTileOffset.ToVector2();
		Vector2 vector2 = road.Pivot - vector;
		Rect roadRect = RoadManager.GetRoadRect(road.Sprite);
		bool flag = true;
		if (!RoadManager.IsTileRoad)
		{
			int i = 0;
			for (int num = Directions.Length; i < num; i++)
			{
				Point2 point = Directions[i];
				if (!HasRoad(localTile + point))
				{
					continue;
				}
				flag = false;
				RoadTile road2 = GetRoad(localTile + point);
				bool flag2 = road.Sprite != road2.Sprite;
				UpdateRoadVectors(localTile + point);
				Vector2 p = vector2;
				Vector2 v = road.Vectors[i];
				Vector2 p2 = road2.Pivot - vector;
				Vector2 v2 = road2.Vectors[(i + 2) % num];
				int num2 = Mathf.Max(RoadManager.CurveLineCount, 1);
				if (flag2)
				{
					num2 *= 2;
				}
				int count = Verts.Count;
				FillMesh(p, p2, new Vector2(0f - v.y, v.x), new Vector2(v2.y, 0f - v2.x), new Vector2(v.y, 0f - v.x), new Vector2(0f - v2.y, v2.x), v, v2, num2, (!flag2) ? 0.5f : 1f);
				int count2 = Verts.Count;
				if (flag2)
				{
					for (int j = count; j < count2; j++)
					{
						float y = Uvs[j].y;
						Colors.Add(new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, y * y)));
					}
				}
				else
				{
					for (int k = count; k < count2; k++)
					{
						Colors.Add(Color.white);
					}
				}
				for (int l = count; l < count2; l++)
				{
					Uv2s.Add(roadRect.position);
				}
				bool flag3 = false;
				Rect rect;
				if (road.LinkCount <= 1)
				{
					flag3 = true;
					rect = RoadManager.GetEdgeMask();
				}
				else
				{
					flag3 = (localTile.x + localTile.y) % 2 == 0;
					rect = RoadManager.GetLinkMask();
				}
				for (int m = count; m < count2; m++)
				{
					Vector2 value = Uvs[m];
					value.x = Mathf.Lerp(rect.xMin, rect.xMax, value.x);
					value.y = ((!flag3) ? (rect.yMin + rect.height * 0.5f * value.y) : (rect.yMax - rect.height * 0.5f * value.y));
					Uvs[m] = value;
				}
			}
		}
		if (flag)
		{
			Vector3 pos = new Vector3(vector2.x, 0f, vector2.y) * 200f;
			FillDefaultMesh(pos, roadRect, RoadManager.GetAloneMask());
		}
	}

	private void FillMesh(Vector2 p1, Vector2 p2, Vector2 l1, Vector2 l2, Vector2 r1, Vector2 r2, Vector2 v1, Vector2 v2, int count, float drawRatio)
	{
		float num = RoadManager.RoadWidth * 0.5f;
		Maths.BezierCurve4 bezierCurve = Maths.MakeBezierCurve4(p1 + r1 * num / 200f, p2 + r2 * num / 200f, v1, v2);
		Maths.BezierCurve4 bezierCurve2 = Maths.MakeBezierCurve4(p1 + l1 * num / 200f, p2 + l2 * num / 200f, v1, v2);
		int count2 = Verts.Count;
		for (int i = 0; i < count + 1; i++)
		{
			float num2 = (float)i / (float)count;
			Vector2 vector = bezierCurve.Get(num2 * drawRatio);
			Vector2 vector2 = bezierCurve2.Get(num2 * drawRatio);
			Verts.Add(new Vector3(vector.x, 0f, vector.y) * 200f);
			Uvs.Add(new Vector2(0f, num2));
			Verts.Add(new Vector3(vector2.x, 0f, vector2.y) * 200f);
			Uvs.Add(new Vector2(1f, num2));
		}
		for (int j = 0; j < count; j++)
		{
			Tris.Add(count2 + j * 2);
			Tris.Add(count2 + 1 + j * 2);
			Tris.Add(count2 + 2 + j * 2);
			Tris.Add(count2 + 2 + j * 2);
			Tris.Add(count2 + 1 + j * 2);
			Tris.Add(count2 + 3 + j * 2);
		}
	}

	private void FillDefaultMesh(Vector3 pos, Rect roadUv, Rect maskUv)
	{
		float num = ((!RoadManager.IsTileRoad) ? (RoadManager.RoadWidth * 0.5f) : 140f);
		int count = Verts.Count;
		Verts.Add(pos + new Vector3(0f - num, 0f, 0f - num));
		Verts.Add(pos + new Vector3(0f - num, 0f, num));
		Verts.Add(pos + new Vector3(num, 0f, 0f - num));
		Verts.Add(pos + new Vector3(num, 0f, num));
		Colors.Add(Color.white);
		Colors.Add(Color.white);
		Colors.Add(Color.white);
		Colors.Add(Color.white);
		Uvs.Add(new Vector2(maskUv.xMin, maskUv.yMin));
		Uvs.Add(new Vector2(maskUv.xMin, maskUv.yMax));
		Uvs.Add(new Vector2(maskUv.xMax, maskUv.yMin));
		Uvs.Add(new Vector2(maskUv.xMax, maskUv.yMax));
		Uv2s.Add(roadUv.position);
		Uv2s.Add(roadUv.position);
		Uv2s.Add(roadUv.position);
		Uv2s.Add(roadUv.position);
		Tris.Add(count);
		Tris.Add(count + 1);
		Tris.Add(count + 2);
		Tris.Add(count + 2);
		Tris.Add(count + 1);
		Tris.Add(count + 3);
	}

	public void ForceUpdateRoads()
	{
		int i = 0;
		Point2 localTile = default(Point2);
		for (int num = _roads.Length; i < num; i++)
		{
			if (_roads[i] != null)
			{
				localTile.x = i % 16;
				localTile.y = i / 16;
				UpdateRoadPivot(localTile);
			}
		}
	}
}
