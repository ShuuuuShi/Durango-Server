using System;
using System.Collections.Generic;
using UnityEngine;
using tk2dRuntime;

[AddComponentMenu("2D Toolkit/Backend/tk2dBaseSprite")]
public abstract class tk2dBaseSprite : MonoBehaviour, ISpriteCollectionForceBuild
{
	public enum Anchor
	{
		LowerLeft,
		LowerCenter,
		LowerRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		UpperLeft,
		UpperCenter,
		UpperRight
	}

	[SerializeField]
	private tk2dSpriteCollectionData collection;

	protected tk2dSpriteCollectionData collectionInst;

	[SerializeField]
	protected Color _color = Color.white;

	[SerializeField]
	protected Vector3 _scale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	protected int _spriteId;

	public BoxCollider2D boxCollider2D;

	public List<PolygonCollider2D> polygonCollider2D = new List<PolygonCollider2D>(1);

	public List<EdgeCollider2D> edgeCollider2D = new List<EdgeCollider2D>(1);

	public BoxCollider boxCollider;

	public MeshCollider meshCollider;

	public Vector3[] meshColliderPositions;

	public Mesh meshColliderMesh;

	private Renderer _cachedRenderer;

	[SerializeField]
	protected int renderLayer;

	public tk2dSpriteCollectionData Collection
	{
		get
		{
			return collection;
		}
		set
		{
			collection = value;
			collectionInst = collection.inst;
		}
	}

	public Color color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _color;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (value != _color)
			{
				_color = value;
				InitInstance();
				UpdateColors();
			}
		}
	}

	public Vector3 scale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _scale;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (value != _scale)
			{
				_scale = value;
				InitInstance();
				UpdateVertices();
				UpdateCollider();
				if (this.SpriteChanged != null)
				{
					this.SpriteChanged(this);
				}
			}
		}
	}

	private Renderer CachedRenderer
	{
		get
		{
			if ((Object)(object)_cachedRenderer == (Object)null)
			{
				_cachedRenderer = ((Component)this).GetComponent<Renderer>();
			}
			return _cachedRenderer;
		}
	}

	public int SortingOrder
	{
		get
		{
			return CachedRenderer.sortingOrder;
		}
		set
		{
			if (CachedRenderer.sortingOrder != value)
			{
				renderLayer = value;
				CachedRenderer.sortingOrder = value;
			}
		}
	}

	public bool FlipX
	{
		get
		{
			return _scale.x < 0f;
		}
		set
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			scale = new Vector3(Mathf.Abs(_scale.x) * (float)((!value) ? 1 : (-1)), _scale.y, _scale.z);
		}
	}

	public bool FlipY
	{
		get
		{
			return _scale.y < 0f;
		}
		set
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			scale = new Vector3(_scale.x, Mathf.Abs(_scale.y) * (float)((!value) ? 1 : (-1)), _scale.z);
		}
	}

	public int spriteId
	{
		get
		{
			return _spriteId;
		}
		set
		{
			if (value != _spriteId)
			{
				InitInstance();
				value = Mathf.Clamp(value, 0, collectionInst.spriteDefinitions.Length - 1);
				if (_spriteId < 0 || _spriteId >= collectionInst.spriteDefinitions.Length || GetCurrentVertexCount() != collectionInst.spriteDefinitions[value].positions.Length || collectionInst.spriteDefinitions[_spriteId].complexGeometry != collectionInst.spriteDefinitions[value].complexGeometry)
				{
					_spriteId = value;
					UpdateGeometry();
				}
				else
				{
					_spriteId = value;
					UpdateVertices();
				}
				UpdateMaterial();
				UpdateCollider();
				if (this.SpriteChanged != null)
				{
					this.SpriteChanged(this);
				}
			}
		}
	}

	public tk2dSpriteDefinition CurrentSprite
	{
		get
		{
			InitInstance();
			return (!((Object)(object)collectionInst == (Object)null)) ? collectionInst.spriteDefinitions[_spriteId] : null;
		}
	}

	public event Action<tk2dBaseSprite> SpriteChanged;

	private void InitInstance()
	{
		if ((Object)(object)collectionInst == (Object)null && (Object)(object)collection != (Object)null)
		{
			collectionInst = collection.inst;
		}
	}

	public void SetSprite(int newSpriteId)
	{
		spriteId = newSpriteId;
	}

	public bool SetSprite(string spriteName)
	{
		int spriteIdByName = collection.GetSpriteIdByName(spriteName, -1);
		if (spriteIdByName != -1)
		{
			SetSprite(spriteIdByName);
		}
		else
		{
			Debug.LogError((object)("SetSprite - Sprite not found in collection: " + spriteName));
		}
		return spriteIdByName != -1;
	}

	public void SetSprite(tk2dSpriteCollectionData newCollection, int newSpriteId)
	{
		bool flag = false;
		if ((Object)(object)Collection != (Object)(object)newCollection)
		{
			collection = newCollection;
			collectionInst = collection.inst;
			_spriteId = -1;
			flag = true;
		}
		spriteId = newSpriteId;
		if (flag)
		{
			UpdateMaterial();
		}
	}

	public bool SetSprite(tk2dSpriteCollectionData newCollection, string spriteName)
	{
		int spriteIdByName = newCollection.GetSpriteIdByName(spriteName, -1);
		if (spriteIdByName != -1)
		{
			SetSprite(newCollection, spriteIdByName);
		}
		else
		{
			Debug.LogError((object)("SetSprite - Sprite not found in collection: " + spriteName));
		}
		return spriteIdByName != -1;
	}

	public void MakePixelPerfect()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		tk2dCamera tk2dCamera2 = tk2dCamera.CameraForLayer(((Component)this).gameObject.layer);
		if ((Object)(object)tk2dCamera2 != (Object)null)
		{
			if (Collection.version < 2)
			{
				Debug.LogError((object)"Need to rebuild sprite collection.");
			}
			float distance = ((Component)this).transform.position.z - ((Component)tk2dCamera2).transform.position.z;
			float num2 = Collection.invOrthoSize * Collection.halfTargetHeight;
			num = tk2dCamera2.GetSizeAtDistance(distance) * num2;
		}
		else if (Object.op_Implicit((Object)(object)Camera.main))
		{
			if (Camera.main.orthographic)
			{
				num = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = ((Component)this).transform.position.z - ((Component)Camera.main).transform.position.z;
				num = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
			num *= Collection.invOrthoSize;
		}
		else
		{
			Debug.LogError((object)"Main camera not found.");
		}
		scale = new Vector3(Mathf.Sign(scale.x) * num, Mathf.Sign(scale.y) * num, Mathf.Sign(scale.z) * num);
	}

	protected abstract void UpdateMaterial();

	protected abstract void UpdateColors();

	protected abstract void UpdateVertices();

	protected abstract void UpdateGeometry();

	protected abstract int GetCurrentVertexCount();

	public abstract void Build();

	public int GetSpriteIdByName(string name)
	{
		InitInstance();
		return collectionInst.GetSpriteIdByName(name);
	}

	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId) where T : tk2dBaseSprite
	{
		T val = go.AddComponent<T>();
		val._spriteId = -1;
		val.SetSprite(spriteCollection, spriteId);
		val.Build();
		return val;
	}

	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, string spriteName) where T : tk2dBaseSprite
	{
		int spriteIdByName = spriteCollection.GetSpriteIdByName(spriteName, -1);
		if (spriteIdByName == -1)
		{
			Debug.LogError((object)$"Unable to find sprite named {spriteName} in sprite collection {spriteCollection.spriteCollectionName}");
			return (T)null;
		}
		return AddComponent<T>(go, spriteCollection, spriteIdByName);
	}

	protected int GetNumVertices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].positions.Length;
	}

	protected int GetNumIndices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].indices.Length;
	}

	protected void SetPositions(Vector3[] positions, Vector3[] normals, Vector4[] tangents)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[spriteId];
		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; i++)
		{
			positions[i].x = tk2dSpriteDefinition2.positions[i].x * _scale.x;
			positions[i].y = tk2dSpriteDefinition2.positions[i].y * _scale.y;
			positions[i].z = tk2dSpriteDefinition2.positions[i].z * _scale.z;
		}
		int num = tk2dSpriteDefinition2.normals.Length;
		if (normals.Length == num)
		{
			for (int j = 0; j < num; j++)
			{
				ref Vector3 reference = ref normals[j];
				reference = tk2dSpriteDefinition2.normals[j];
			}
		}
		int num2 = tk2dSpriteDefinition2.tangents.Length;
		if (tangents.Length == num2)
		{
			for (int k = 0; k < num2; k++)
			{
				ref Vector4 reference2 = ref tangents[k];
				reference2 = tk2dSpriteDefinition2.tangents[k];
			}
		}
	}

	protected void SetColors(Color32[] dest)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Color val = _color;
		if (collectionInst.premultipliedAlpha)
		{
			val.r *= val.a;
			val.g *= val.a;
			val.b *= val.a;
		}
		Color32 val2 = Color32.op_Implicit(val);
		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; i++)
		{
			dest[i] = val2;
		}
	}

	public Bounds GetBounds()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		InitInstance();
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(tk2dSpriteDefinition2.boundsData[0].x * _scale.x, tk2dSpriteDefinition2.boundsData[0].y * _scale.y, tk2dSpriteDefinition2.boundsData[0].z * _scale.z), new Vector3(tk2dSpriteDefinition2.boundsData[1].x * Mathf.Abs(_scale.x), tk2dSpriteDefinition2.boundsData[1].y * Mathf.Abs(_scale.y), tk2dSpriteDefinition2.boundsData[1].z * Mathf.Abs(_scale.z)));
	}

	public Bounds GetUntrimmedBounds()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		InitInstance();
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(tk2dSpriteDefinition2.untrimmedBoundsData[0].x * _scale.x, tk2dSpriteDefinition2.untrimmedBoundsData[0].y * _scale.y, tk2dSpriteDefinition2.untrimmedBoundsData[0].z * _scale.z), new Vector3(tk2dSpriteDefinition2.untrimmedBoundsData[1].x * Mathf.Abs(_scale.x), tk2dSpriteDefinition2.untrimmedBoundsData[1].y * Mathf.Abs(_scale.y), tk2dSpriteDefinition2.untrimmedBoundsData[1].z * Mathf.Abs(_scale.z)));
	}

	public static Bounds AdjustedMeshBounds(Bounds bounds, int renderLayer)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = default(Vector3);
		((Vector3)(ref center))._002Ector(((Bounds)(ref bounds)).center.x, 0f, (float)(-renderLayer) * 0.01f);
		((Bounds)(ref bounds)).center = center;
		Vector3 size = ((Bounds)(ref bounds)).size;
		((Bounds)(ref bounds)).size = new Vector3(size.x, size.y * 2f, size.z);
		return bounds;
	}

	public tk2dSpriteDefinition GetCurrentSpriteDef()
	{
		InitInstance();
		return (!((Object)(object)collectionInst == (Object)null)) ? collectionInst.spriteDefinitions[_spriteId] : null;
	}

	public virtual void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
	}

	protected virtual bool NeedBoxCollider()
	{
		return false;
	}

	protected virtual void UpdateCollider()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06df: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		if (tk2dSpriteDefinition2.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D)
		{
			if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Box && (Object)(object)boxCollider == (Object)null)
			{
				boxCollider = ((Component)this).gameObject.GetComponent<BoxCollider>();
				if ((Object)(object)boxCollider == (Object)null)
				{
					boxCollider = ((Component)this).gameObject.AddComponent<BoxCollider>();
				}
			}
			if ((Object)(object)boxCollider != (Object)null)
			{
				if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Box)
				{
					boxCollider.center = new Vector3(tk2dSpriteDefinition2.colliderVertices[0].x * _scale.x, tk2dSpriteDefinition2.colliderVertices[0].y * _scale.y, tk2dSpriteDefinition2.colliderVertices[0].z * _scale.z);
					boxCollider.size = new Vector3(2f * tk2dSpriteDefinition2.colliderVertices[1].x * _scale.x, 2f * tk2dSpriteDefinition2.colliderVertices[1].y * _scale.y, 2f * tk2dSpriteDefinition2.colliderVertices[1].z * _scale.z);
				}
				else if (tk2dSpriteDefinition2.colliderType != 0 && (Object)(object)boxCollider != (Object)null)
				{
					boxCollider.center = new Vector3(0f, 0f, -100000f);
					boxCollider.size = Vector3.zero;
				}
			}
		}
		else
		{
			if (tk2dSpriteDefinition2.physicsEngine != tk2dSpriteDefinition.PhysicsEngine.Physics2D)
			{
				return;
			}
			if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Box)
			{
				if ((Object)(object)boxCollider2D == (Object)null)
				{
					boxCollider2D = ((Component)this).gameObject.GetComponent<BoxCollider2D>();
					if ((Object)(object)boxCollider2D == (Object)null)
					{
						boxCollider2D = ((Component)this).gameObject.AddComponent<BoxCollider2D>();
					}
				}
				if (polygonCollider2D.Count > 0)
				{
					foreach (PolygonCollider2D item in polygonCollider2D)
					{
						if ((Object)(object)item != (Object)null && ((Behaviour)item).enabled)
						{
							((Behaviour)item).enabled = false;
						}
					}
				}
				if (edgeCollider2D.Count > 0)
				{
					foreach (EdgeCollider2D item2 in edgeCollider2D)
					{
						if ((Object)(object)item2 != (Object)null && ((Behaviour)item2).enabled)
						{
							((Behaviour)item2).enabled = false;
						}
					}
				}
				if (!((Behaviour)boxCollider2D).enabled)
				{
					((Behaviour)boxCollider2D).enabled = true;
				}
				((Collider2D)boxCollider2D).offset = new Vector2(tk2dSpriteDefinition2.colliderVertices[0].x * _scale.x, tk2dSpriteDefinition2.colliderVertices[0].y * _scale.y);
				boxCollider2D.size = new Vector2(Mathf.Abs(2f * tk2dSpriteDefinition2.colliderVertices[1].x * _scale.x), Mathf.Abs(2f * tk2dSpriteDefinition2.colliderVertices[1].y * _scale.y));
			}
			else if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
			{
				if ((Object)(object)boxCollider2D != (Object)null && ((Behaviour)boxCollider2D).enabled)
				{
					((Behaviour)boxCollider2D).enabled = false;
				}
				int num = tk2dSpriteDefinition2.polygonCollider2D.Length;
				for (int i = 0; i < polygonCollider2D.Count; i++)
				{
					if ((Object)(object)polygonCollider2D[i] == (Object)null)
					{
						polygonCollider2D[i] = ((Component)this).gameObject.AddComponent<PolygonCollider2D>();
					}
				}
				while (polygonCollider2D.Count < num)
				{
					polygonCollider2D.Add(((Component)this).gameObject.AddComponent<PolygonCollider2D>());
				}
				for (int j = 0; j < num; j++)
				{
					if (!((Behaviour)polygonCollider2D[j]).enabled)
					{
						((Behaviour)polygonCollider2D[j]).enabled = true;
					}
					if (_scale.x != 1f || _scale.y != 1f)
					{
						Vector2[] points = tk2dSpriteDefinition2.polygonCollider2D[j].points;
						Vector2[] array = (Vector2[])(object)new Vector2[points.Length];
						for (int k = 0; k < points.Length; k++)
						{
							ref Vector2 reference = ref array[k];
							reference = Vector2.Scale(points[k], Vector2.op_Implicit(_scale));
						}
						polygonCollider2D[j].points = array;
					}
					else
					{
						polygonCollider2D[j].points = tk2dSpriteDefinition2.polygonCollider2D[j].points;
					}
				}
				for (int l = num; l < polygonCollider2D.Count; l++)
				{
					if (((Behaviour)polygonCollider2D[l]).enabled)
					{
						((Behaviour)polygonCollider2D[l]).enabled = false;
					}
				}
				int num2 = tk2dSpriteDefinition2.edgeCollider2D.Length;
				for (int m = 0; m < edgeCollider2D.Count; m++)
				{
					if ((Object)(object)edgeCollider2D[m] == (Object)null)
					{
						edgeCollider2D[m] = ((Component)this).gameObject.AddComponent<EdgeCollider2D>();
					}
				}
				while (edgeCollider2D.Count < num2)
				{
					edgeCollider2D.Add(((Component)this).gameObject.AddComponent<EdgeCollider2D>());
				}
				for (int n = 0; n < num2; n++)
				{
					if (!((Behaviour)edgeCollider2D[n]).enabled)
					{
						((Behaviour)edgeCollider2D[n]).enabled = true;
					}
					if (_scale.x != 1f || _scale.y != 1f)
					{
						Vector2[] points2 = tk2dSpriteDefinition2.edgeCollider2D[n].points;
						Vector2[] array2 = (Vector2[])(object)new Vector2[points2.Length];
						for (int num3 = 0; num3 < points2.Length; num3++)
						{
							ref Vector2 reference2 = ref array2[num3];
							reference2 = Vector2.Scale(points2[num3], Vector2.op_Implicit(_scale));
						}
						edgeCollider2D[n].points = array2;
					}
					else
					{
						edgeCollider2D[n].points = tk2dSpriteDefinition2.edgeCollider2D[n].points;
					}
				}
				for (int num4 = num2; num4 < edgeCollider2D.Count; num4++)
				{
					if (((Behaviour)edgeCollider2D[num4]).enabled)
					{
						((Behaviour)edgeCollider2D[num4]).enabled = false;
					}
				}
			}
			else
			{
				if (tk2dSpriteDefinition2.colliderType != tk2dSpriteDefinition.ColliderType.None)
				{
					return;
				}
				if ((Object)(object)boxCollider2D != (Object)null && ((Behaviour)boxCollider2D).enabled)
				{
					((Behaviour)boxCollider2D).enabled = false;
				}
				if (polygonCollider2D.Count > 0)
				{
					foreach (PolygonCollider2D item3 in polygonCollider2D)
					{
						if ((Object)(object)item3 != (Object)null && ((Behaviour)item3).enabled)
						{
							((Behaviour)item3).enabled = false;
						}
					}
				}
				if (edgeCollider2D.Count <= 0)
				{
					return;
				}
				foreach (EdgeCollider2D item4 in edgeCollider2D)
				{
					if ((Object)(object)item4 != (Object)null && ((Behaviour)item4).enabled)
					{
						((Behaviour)item4).enabled = false;
					}
				}
			}
		}
	}

	protected virtual void CreateCollider()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Unset)
		{
			return;
		}
		if (tk2dSpriteDefinition2.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D)
		{
			if ((Object)(object)((Component)this).GetComponent<Collider>() != (Object)null)
			{
				boxCollider = ((Component)this).GetComponent<BoxCollider>();
				meshCollider = ((Component)this).GetComponent<MeshCollider>();
			}
			if ((NeedBoxCollider() || tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Box) && (Object)(object)meshCollider == (Object)null)
			{
				if ((Object)(object)boxCollider == (Object)null)
				{
					boxCollider = ((Component)this).gameObject.AddComponent<BoxCollider>();
				}
			}
			else if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Mesh && (Object)(object)boxCollider == (Object)null)
			{
				if ((Object)(object)meshCollider == (Object)null)
				{
					meshCollider = ((Component)this).gameObject.AddComponent<MeshCollider>();
				}
				if ((Object)(object)meshColliderMesh == (Object)null)
				{
					meshColliderMesh = new Mesh();
				}
				meshColliderMesh.Clear();
				meshColliderPositions = (Vector3[])(object)new Vector3[tk2dSpriteDefinition2.colliderVertices.Length];
				for (int i = 0; i < meshColliderPositions.Length; i++)
				{
					ref Vector3 reference = ref meshColliderPositions[i];
					reference = new Vector3(tk2dSpriteDefinition2.colliderVertices[i].x * _scale.x, tk2dSpriteDefinition2.colliderVertices[i].y * _scale.y, tk2dSpriteDefinition2.colliderVertices[i].z * _scale.z);
				}
				meshColliderMesh.vertices = meshColliderPositions;
				float num = _scale.x * _scale.y * _scale.z;
				meshColliderMesh.triangles = ((!(num >= 0f)) ? tk2dSpriteDefinition2.colliderIndicesBack : tk2dSpriteDefinition2.colliderIndicesFwd);
				meshCollider.sharedMesh = meshColliderMesh;
				meshCollider.convex = tk2dSpriteDefinition2.colliderConvex;
				if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<Rigidbody>()))
				{
					((Component)this).GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
				}
			}
			else if (tk2dSpriteDefinition2.colliderType != tk2dSpriteDefinition.ColliderType.None && Application.isPlaying)
			{
				Debug.LogError((object)("Invalid mesh collider on sprite '" + ((Object)this).name + "', please remove and try again."));
			}
			UpdateCollider();
		}
		else if (tk2dSpriteDefinition2.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D)
		{
			UpdateCollider();
		}
	}

	protected void Awake()
	{
		if ((Object)(object)collection != (Object)null)
		{
			collectionInst = collection.inst;
		}
		CachedRenderer.sortingOrder = renderLayer;
	}

	public void CreateSimpleBoxCollider()
	{
		if (CurrentSprite == null)
		{
			return;
		}
		if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D)
		{
			boxCollider2D = ((Component)this).GetComponent<BoxCollider2D>();
			if ((Object)(object)boxCollider2D != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)boxCollider2D, true);
			}
			boxCollider = ((Component)this).GetComponent<BoxCollider>();
			if ((Object)(object)boxCollider == (Object)null)
			{
				boxCollider = ((Component)this).gameObject.AddComponent<BoxCollider>();
			}
		}
		else if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D)
		{
			boxCollider = ((Component)this).GetComponent<BoxCollider>();
			if ((Object)(object)boxCollider != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)boxCollider, true);
			}
			boxCollider2D = ((Component)this).GetComponent<BoxCollider2D>();
			if ((Object)(object)boxCollider2D == (Object)null)
			{
				boxCollider2D = ((Component)this).gameObject.AddComponent<BoxCollider2D>();
			}
		}
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return (Object)(object)Collection == (Object)(object)spriteCollection;
	}

	public virtual void ForceBuild()
	{
		if (!((Object)(object)collection == (Object)null))
		{
			collectionInst = collection.inst;
			if (spriteId < 0 || spriteId >= collectionInst.spriteDefinitions.Length)
			{
				spriteId = 0;
			}
			Build();
			if (this.SpriteChanged != null)
			{
				this.SpriteChanged(this);
			}
		}
	}

	public static GameObject CreateFromTexture<T>(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor) where T : tk2dBaseSprite
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		tk2dSpriteCollectionData tk2dSpriteCollectionData2 = SpriteCollectionGenerator.CreateFromTexture(texture, size, region, anchor);
		if ((Object)(object)tk2dSpriteCollectionData2 == (Object)null)
		{
			return null;
		}
		GameObject val = new GameObject();
		AddComponent<T>(val, tk2dSpriteCollectionData2, 0);
		return val;
	}
}
