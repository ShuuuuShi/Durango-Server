using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("2D Toolkit/Sprite/tk2dSprite")]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class tk2dSprite : tk2dBaseSprite
{
	public Mesh mesh;

	private Vector3[] meshNormals;

	private Vector4[] meshTangents;

	private Color32[] meshColors;

	public Vector3[] meshVertices { get; private set; }

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
		}
	}

	protected void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)mesh))
		{
			Object.Destroy((Object)(object)mesh);
		}
		if (Object.op_Implicit((Object)(object)meshColliderMesh))
		{
			Object.Destroy((Object)(object)meshColliderMesh);
		}
	}

	public override void Build()
	{
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
		meshVertices = (Vector3[])(object)new Vector3[tk2dSpriteDefinition2.positions.Length];
		meshColors = (Color32[])(object)new Color32[tk2dSpriteDefinition2.positions.Length];
		meshNormals = (Vector3[])(object)new Vector3[0];
		meshTangents = (Vector4[])(object)new Vector4[0];
		if (tk2dSpriteDefinition2.normals != null && tk2dSpriteDefinition2.normals.Length > 0)
		{
			meshNormals = (Vector3[])(object)new Vector3[tk2dSpriteDefinition2.normals.Length];
		}
		if (tk2dSpriteDefinition2.tangents != null && tk2dSpriteDefinition2.tangents.Length > 0)
		{
			meshTangents = (Vector4[])(object)new Vector4[tk2dSpriteDefinition2.tangents.Length];
		}
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);
		if ((Object)(object)mesh == (Object)null)
		{
			mesh = new Mesh();
			mesh.MarkDynamic();
			((Object)mesh).hideFlags = (HideFlags)52;
			((Component)this).GetComponent<MeshFilter>().mesh = mesh;
		}
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors32 = meshColors;
		mesh.uv = tk2dSpriteDefinition2.uvs;
		mesh.triangles = tk2dSpriteDefinition2.indices;
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
		UpdateMaterial();
		CreateCollider();
	}

	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteId);
	}

	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, string spriteName)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteName);
	}

	public static GameObject CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return tk2dBaseSprite.CreateFromTexture<tk2dSprite>(texture, size, region, anchor);
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
		UpdateVerticesImpl();
	}

	protected void UpdateColorsImpl()
	{
		if (!((Object)(object)mesh == (Object)null) && meshColors != null && meshColors.Length != 0)
		{
			SetColors(meshColors);
			mesh.colors32 = meshColors;
		}
	}

	protected void UpdateVerticesImpl()
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
		if (!((Object)(object)mesh == (Object)null) && meshVertices != null && meshVertices.Length != 0)
		{
			if (tk2dSpriteDefinition2.normals.Length != meshNormals.Length)
			{
				meshNormals = (Vector3[])(object)((tk2dSpriteDefinition2.normals == null || tk2dSpriteDefinition2.normals.Length <= 0) ? new Vector3[0] : new Vector3[tk2dSpriteDefinition2.normals.Length]);
			}
			if (tk2dSpriteDefinition2.tangents.Length != meshTangents.Length)
			{
				meshTangents = (Vector4[])(object)((tk2dSpriteDefinition2.tangents == null || tk2dSpriteDefinition2.tangents.Length <= 0) ? new Vector4[0] : new Vector4[tk2dSpriteDefinition2.tangents.Length]);
			}
			SetPositions(meshVertices, meshNormals, meshTangents);
			mesh.vertices = meshVertices;
			mesh.normals = meshNormals;
			mesh.tangents = meshTangents;
			mesh.uv = tk2dSpriteDefinition2.uvs;
			mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
		}
	}

	protected void UpdateGeometryImpl()
	{
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mesh == (Object)null))
		{
			tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
			if (meshVertices == null || meshVertices.Length != tk2dSpriteDefinition2.positions.Length)
			{
				meshVertices = (Vector3[])(object)new Vector3[tk2dSpriteDefinition2.positions.Length];
				meshColors = (Color32[])(object)new Color32[tk2dSpriteDefinition2.positions.Length];
			}
			if (meshNormals == null || (tk2dSpriteDefinition2.normals != null && meshNormals.Length != tk2dSpriteDefinition2.normals.Length))
			{
				meshNormals = (Vector3[])(object)new Vector3[tk2dSpriteDefinition2.normals.Length];
			}
			else if (tk2dSpriteDefinition2.normals == null)
			{
				meshNormals = (Vector3[])(object)new Vector3[0];
			}
			if (meshTangents == null || (tk2dSpriteDefinition2.tangents != null && meshTangents.Length != tk2dSpriteDefinition2.tangents.Length))
			{
				meshTangents = (Vector4[])(object)new Vector4[tk2dSpriteDefinition2.tangents.Length];
			}
			else if (tk2dSpriteDefinition2.tangents == null)
			{
				meshTangents = (Vector4[])(object)new Vector4[0];
			}
			SetPositions(meshVertices, meshNormals, meshTangents);
			SetColors(meshColors);
			mesh.Clear();
			mesh.vertices = meshVertices;
			mesh.normals = meshNormals;
			mesh.tangents = meshTangents;
			mesh.colors32 = meshColors;
			mesh.uv = tk2dSpriteDefinition2.uvs;
			mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
			mesh.triangles = tk2dSpriteDefinition2.indices;
		}
	}

	public void UpdateBounds()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
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
		if (meshVertices == null)
		{
			return 0;
		}
		return meshVertices.Length;
	}

	public override void ForceBuild()
	{
		base.ForceBuild();
		((Component)this).GetComponent<MeshFilter>().mesh = mesh;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.1f;
		tk2dSpriteDefinition currentSprite = base.CurrentSprite;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(Mathf.Abs(_scale.x), Mathf.Abs(_scale.y), Mathf.Abs(_scale.z));
		Vector3 val2 = Vector3.Scale(currentSprite.untrimmedBoundsData[0], _scale) - 0.5f * Vector3.Scale(currentSprite.untrimmedBoundsData[1], val);
		Vector3 val3 = Vector3.Scale(currentSprite.untrimmedBoundsData[1], val);
		Vector3 val4 = val3 + dMax - dMin;
		val4.x /= currentSprite.untrimmedBoundsData[1].x;
		val4.y /= currentSprite.untrimmedBoundsData[1].y;
		if (currentSprite.untrimmedBoundsData[1].x * val4.x < currentSprite.texelSize.x * num && val4.x < val.x)
		{
			dMin.x = 0f;
			val4.x = val.x;
		}
		if (currentSprite.untrimmedBoundsData[1].y * val4.y < currentSprite.texelSize.y * num && val4.y < val.y)
		{
			dMin.y = 0f;
			val4.y = val.y;
		}
		Vector2 val5 = Vector2.op_Implicit(new Vector3((!Mathf.Approximately(val.x, 0f)) ? (val4.x / val.x) : 0f, (!Mathf.Approximately(val.y, 0f)) ? (val4.y / val.y) : 0f));
		Vector3 val6 = default(Vector3);
		((Vector3)(ref val6))._002Ector(val2.x * val5.x, val2.y * val5.y);
		Vector3 val7 = dMin + val2 - val6;
		val7.z = 0f;
		((Component)this).transform.position = ((Component)this).transform.TransformPoint(val7);
		base.scale = new Vector3(_scale.x * val5.x, _scale.y * val5.y, _scale.z);
	}
}
