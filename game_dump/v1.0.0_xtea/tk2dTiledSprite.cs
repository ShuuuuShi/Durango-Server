using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("2D Toolkit/Sprite/tk2dTiledSprite")]
[RequireComponent(typeof(MeshRenderer))]
public class tk2dTiledSprite : tk2dBaseSprite
{
	private Mesh mesh;

	private Vector2[] meshUvs;

	private Vector3[] meshVertices;

	private Color32[] meshColors;

	private Vector3[] meshNormals;

	private Vector4[] meshTangents;

	private int[] meshIndices;

	[SerializeField]
	private Vector2 _dimensions = new Vector2(50f, 50f);

	[SerializeField]
	private Anchor _anchor;

	[SerializeField]
	protected bool _createBoxCollider;

	private Vector3 boundsCenter = Vector3.zero;

	private Vector3 boundsExtents = Vector3.zero;

	public Vector2 dimensions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _dimensions;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (value != _dimensions)
			{
				_dimensions = value;
				UpdateVertices();
				UpdateCollider();
			}
		}
	}

	public Anchor anchor
	{
		get
		{
			return _anchor;
		}
		set
		{
			if (value != _anchor)
			{
				_anchor = value;
				UpdateVertices();
				UpdateCollider();
			}
		}
	}

	public bool CreateBoxCollider
	{
		get
		{
			return _createBoxCollider;
		}
		set
		{
			if (_createBoxCollider != value)
			{
				_createBoxCollider = value;
				UpdateCollider();
			}
		}
	}

	private new void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		base.Awake();
		mesh = new Mesh();
		mesh.MarkDynamic();
		((Object)mesh).hideFlags = (HideFlags)52;
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
		if (Object.op_Implicit((Object)(object)base.Collection))
		{
			if (_spriteId < 0 || _spriteId >= base.Collection.Count)
			{
				_spriteId = 0;
			}
			Build();
			if ((Object)(object)boxCollider == (Object)null)
			{
				boxCollider = ((Component)this).GetComponent<BoxCollider>();
			}
			if ((Object)(object)boxCollider2D == (Object)null)
			{
				boxCollider2D = ((Component)this).GetComponent<BoxCollider2D>();
			}
		}
	}

	protected void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)mesh))
		{
			Object.Destroy((Object)(object)mesh);
		}
	}

	protected new void SetColors(Color32[] dest)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out var numVertices, out var _, base.CurrentSprite, dimensions);
		tk2dSpriteGeomGen.SetSpriteColors(dest, 0, numVertices, _color, collectionInst.premultipliedAlpha);
	}

	public override void Build()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Expected O, but got Unknown
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteDefinition currentSprite = base.CurrentSprite;
		tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out var numVertices, out var numIndices, currentSprite, dimensions);
		if (meshUvs == null || meshUvs.Length != numVertices)
		{
			meshUvs = (Vector2[])(object)new Vector2[numVertices];
			meshVertices = (Vector3[])(object)new Vector3[numVertices];
			meshColors = (Color32[])(object)new Color32[numVertices];
		}
		if (meshIndices == null || meshIndices.Length != numIndices)
		{
			meshIndices = new int[numIndices];
		}
		meshNormals = (Vector3[])(object)new Vector3[0];
		meshTangents = (Vector4[])(object)new Vector4[0];
		if (currentSprite.normals != null && currentSprite.normals.Length > 0)
		{
			meshNormals = (Vector3[])(object)new Vector3[numVertices];
		}
		if (currentSprite.tangents != null && currentSprite.tangents.Length > 0)
		{
			meshTangents = (Vector4[])(object)new Vector4[numVertices];
		}
		float colliderOffsetZ = ((!((Object)(object)boxCollider != (Object)null)) ? 0f : boxCollider.center.z);
		float colliderExtentZ = ((!((Object)(object)boxCollider != (Object)null)) ? 0.5f : (boxCollider.size.z * 0.5f));
		tk2dSpriteGeomGen.SetTiledSpriteGeom(meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, currentSprite, _scale, dimensions, anchor, colliderOffsetZ, colliderExtentZ);
		tk2dSpriteGeomGen.SetTiledSpriteIndices(meshIndices, 0, 0, currentSprite, dimensions);
		if (meshNormals.Length > 0 || meshTangents.Length > 0)
		{
			Vector3 pMin = default(Vector3);
			((Vector3)(ref pMin))._002Ector(currentSprite.positions[0].x * dimensions.x * currentSprite.texelSize.x * base.scale.x, currentSprite.positions[0].y * dimensions.y * currentSprite.texelSize.y * base.scale.y);
			Vector3 pMax = default(Vector3);
			((Vector3)(ref pMax))._002Ector(currentSprite.positions[3].x * dimensions.x * currentSprite.texelSize.x * base.scale.x, currentSprite.positions[3].y * dimensions.y * currentSprite.texelSize.y * base.scale.y);
			tk2dSpriteGeomGen.SetSpriteVertexNormals(meshVertices, pMin, pMax, currentSprite.normals, currentSprite.tangents, meshNormals, meshTangents);
		}
		SetColors(meshColors);
		if ((Object)(object)mesh == (Object)null)
		{
			mesh = new Mesh();
			mesh.MarkDynamic();
			((Object)mesh).hideFlags = (HideFlags)52;
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = meshVertices;
		mesh.colors32 = meshColors;
		mesh.uv = meshUvs;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.triangles = meshIndices;
		mesh.RecalculateBounds();
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(mesh.bounds, renderLayer);
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
		UpdateCollider();
		UpdateMaterial();
	}

	protected override void UpdateGeometry()
	{
		UpdateGeometryImpl();
	}

	protected override void UpdateColors()
	{
		UpdateColorsImpl();
	}

	protected override void UpdateVertices()
	{
		UpdateGeometryImpl();
	}

	protected void UpdateColorsImpl()
	{
		if (meshColors == null || meshColors.Length == 0)
		{
			Build();
			return;
		}
		SetColors(meshColors);
		mesh.colors32 = meshColors;
	}

	protected void UpdateGeometryImpl()
	{
		Build();
	}

	protected override void UpdateCollider()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (!CreateBoxCollider)
		{
			return;
		}
		if (base.CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D)
		{
			if ((Object)(object)boxCollider != (Object)null)
			{
				boxCollider.size = 2f * boundsExtents;
				boxCollider.center = boundsCenter;
			}
		}
		else if (base.CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D && (Object)(object)boxCollider2D != (Object)null)
		{
			boxCollider2D.size = Vector2.op_Implicit(2f * boundsExtents);
			((Collider2D)boxCollider2D).offset = Vector2.op_Implicit(boundsCenter);
		}
	}

	protected override void CreateCollider()
	{
		UpdateCollider();
	}

	protected override void UpdateMaterial()
	{
		Renderer component = ((Component)this).GetComponent<Renderer>();
		if ((Object)(object)component.sharedMaterial != (Object)(object)collectionInst.spriteDefinitions[base.spriteId].materialInst)
		{
			component.material = collectionInst.spriteDefinitions[base.spriteId].materialInst;
		}
	}

	protected override int GetCurrentVertexCount()
	{
		return 16;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.1f;
		tk2dSpriteDefinition currentSprite = base.CurrentSprite;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(_dimensions.x * currentSprite.texelSize.x, _dimensions.y * currentSprite.texelSize.y);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(val.x * _scale.x, val.y * _scale.y);
		Vector3 val3 = Vector3.zero;
		switch (_anchor)
		{
		case Anchor.LowerLeft:
			((Vector3)(ref val3)).Set(0f, 0f, 0f);
			break;
		case Anchor.LowerCenter:
			((Vector3)(ref val3)).Set(0.5f, 0f, 0f);
			break;
		case Anchor.LowerRight:
			((Vector3)(ref val3)).Set(1f, 0f, 0f);
			break;
		case Anchor.MiddleLeft:
			((Vector3)(ref val3)).Set(0f, 0.5f, 0f);
			break;
		case Anchor.MiddleCenter:
			((Vector3)(ref val3)).Set(0.5f, 0.5f, 0f);
			break;
		case Anchor.MiddleRight:
			((Vector3)(ref val3)).Set(1f, 0.5f, 0f);
			break;
		case Anchor.UpperLeft:
			((Vector3)(ref val3)).Set(0f, 1f, 0f);
			break;
		case Anchor.UpperCenter:
			((Vector3)(ref val3)).Set(0.5f, 1f, 0f);
			break;
		case Anchor.UpperRight:
			((Vector3)(ref val3)).Set(1f, 1f, 0f);
			break;
		}
		val3 = Vector3.Scale(val3, val2) * -1f;
		Vector3 val4 = val2 + dMax - dMin;
		val4.x /= val.x;
		val4.y /= val.y;
		if (Mathf.Abs(val.x * val4.x) < currentSprite.texelSize.x * num && Mathf.Abs(val4.x) < Mathf.Abs(_scale.x))
		{
			dMin.x = 0f;
			val4.x = _scale.x;
		}
		if (Mathf.Abs(val.y * val4.y) < currentSprite.texelSize.y * num && Mathf.Abs(val4.y) < Mathf.Abs(_scale.y))
		{
			dMin.y = 0f;
			val4.y = _scale.y;
		}
		Vector2 val5 = Vector2.op_Implicit(new Vector3((!Mathf.Approximately(_scale.x, 0f)) ? (val4.x / _scale.x) : 0f, (!Mathf.Approximately(_scale.y, 0f)) ? (val4.y / _scale.y) : 0f));
		Vector3 val6 = default(Vector3);
		((Vector3)(ref val6))._002Ector(val3.x * val5.x, val3.y * val5.y);
		Vector3 val7 = dMin + val3 - val6;
		val7.z = 0f;
		((Component)this).transform.position = ((Component)this).transform.TransformPoint(val7);
		dimensions = new Vector2(_dimensions.x * val5.x, _dimensions.y * val5.y);
	}
}
