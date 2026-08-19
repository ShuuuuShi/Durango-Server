using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIMask")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class tk2dUIMask : MonoBehaviour
{
	public tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleCenter;

	public Vector2 size = new Vector2(1f, 1f);

	public float depth = 1f;

	public bool createBoxCollider = true;

	private MeshFilter _thisMeshFilter;

	private BoxCollider _thisBoxCollider;

	private static readonly Vector2[] uv = (Vector2[])(object)new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private static readonly int[] indices = new int[6] { 0, 3, 1, 2, 3, 0 };

	private MeshFilter ThisMeshFilter
	{
		get
		{
			if ((Object)(object)_thisMeshFilter == (Object)null)
			{
				_thisMeshFilter = ((Component)this).GetComponent<MeshFilter>();
			}
			return _thisMeshFilter;
		}
	}

	private BoxCollider ThisBoxCollider
	{
		get
		{
			if ((Object)(object)_thisBoxCollider == (Object)null)
			{
				_thisBoxCollider = ((Component)this).GetComponent<BoxCollider>();
			}
			return _thisBoxCollider;
		}
	}

	private void Awake()
	{
		Build();
	}

	private void OnDestroy()
	{
		if ((Object)(object)ThisMeshFilter.sharedMesh != (Object)null)
		{
			Object.Destroy((Object)(object)ThisMeshFilter.sharedMesh);
		}
	}

	private Mesh FillMesh(Mesh mesh)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.UpperLeft:
			((Vector3)(ref zero))._002Ector(0f, 0f - size.y, 0f);
			break;
		case tk2dBaseSprite.Anchor.UpperCenter:
			((Vector3)(ref zero))._002Ector((0f - size.x) / 2f, 0f - size.y, 0f);
			break;
		case tk2dBaseSprite.Anchor.UpperRight:
			((Vector3)(ref zero))._002Ector(0f - size.x, 0f - size.y, 0f);
			break;
		case tk2dBaseSprite.Anchor.MiddleLeft:
			((Vector3)(ref zero))._002Ector(0f, (0f - size.y) / 2f, 0f);
			break;
		case tk2dBaseSprite.Anchor.MiddleCenter:
			((Vector3)(ref zero))._002Ector((0f - size.x) / 2f, (0f - size.y) / 2f, 0f);
			break;
		case tk2dBaseSprite.Anchor.MiddleRight:
			((Vector3)(ref zero))._002Ector(0f - size.x, (0f - size.y) / 2f, 0f);
			break;
		case tk2dBaseSprite.Anchor.LowerLeft:
			((Vector3)(ref zero))._002Ector(0f, 0f, 0f);
			break;
		case tk2dBaseSprite.Anchor.LowerCenter:
			((Vector3)(ref zero))._002Ector((0f - size.x) / 2f, 0f, 0f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
			((Vector3)(ref zero))._002Ector(0f - size.x, 0f, 0f);
			break;
		}
		Vector3[] vertices = (Vector3[])(object)new Vector3[4]
		{
			zero + new Vector3(0f, 0f, 0f - depth),
			zero + new Vector3(size.x, 0f, 0f - depth),
			zero + new Vector3(0f, size.y, 0f - depth),
			zero + new Vector3(size.x, size.y, 0f - depth)
		};
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = indices;
		Bounds bounds = default(Bounds);
		((Bounds)(ref bounds)).SetMinMax(zero, zero + new Vector3(size.x, size.y, 0f));
		mesh.bounds = bounds;
		return mesh;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		Mesh sharedMesh = ThisMeshFilter.sharedMesh;
		if ((Object)(object)sharedMesh != (Object)null)
		{
			Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
			Bounds bounds = sharedMesh.bounds;
			Gizmos.color = Color32.op_Implicit(new Color32((byte)56, (byte)146, (byte)227, (byte)96));
			float num = (0f - depth) * 1.001f;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(((Bounds)(ref bounds)).center.x, ((Bounds)(ref bounds)).center.y, num * 0.5f);
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(((Bounds)(ref bounds)).extents.x * 2f, ((Bounds)(ref bounds)).extents.y * 2f, Mathf.Abs(num));
			Gizmos.DrawCube(val, val2);
			Gizmos.color = Color32.op_Implicit(new Color32((byte)22, (byte)145, byte.MaxValue, byte.MaxValue));
			Gizmos.DrawWireCube(val, val2);
		}
	}

	public void Build()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ThisMeshFilter.sharedMesh == (Object)null)
		{
			Mesh val = new Mesh();
			val.MarkDynamic();
			((Object)val).hideFlags = (HideFlags)52;
			ThisMeshFilter.mesh = FillMesh(val);
		}
		else
		{
			FillMesh(ThisMeshFilter.sharedMesh);
		}
		if (createBoxCollider)
		{
			if ((Object)(object)ThisBoxCollider == (Object)null)
			{
				_thisBoxCollider = ((Component)this).gameObject.AddComponent<BoxCollider>();
			}
			Bounds bounds = ThisMeshFilter.sharedMesh.bounds;
			ThisBoxCollider.center = new Vector3(((Bounds)(ref bounds)).center.x, ((Bounds)(ref bounds)).center.y, 0f - depth);
			ThisBoxCollider.size = new Vector3(((Bounds)(ref bounds)).size.x, ((Bounds)(ref bounds)).size.y, 0.0002f);
		}
		else if ((Object)(object)ThisBoxCollider != (Object)null)
		{
			Object.Destroy((Object)(object)ThisBoxCollider);
		}
	}

	public void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(size.x, size.y);
		Vector3 val2 = Vector3.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerLeft:
			((Vector3)(ref val2)).Set(0f, 0f, 0f);
			break;
		case tk2dBaseSprite.Anchor.LowerCenter:
			((Vector3)(ref val2)).Set(0.5f, 0f, 0f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
			((Vector3)(ref val2)).Set(1f, 0f, 0f);
			break;
		case tk2dBaseSprite.Anchor.MiddleLeft:
			((Vector3)(ref val2)).Set(0f, 0.5f, 0f);
			break;
		case tk2dBaseSprite.Anchor.MiddleCenter:
			((Vector3)(ref val2)).Set(0.5f, 0.5f, 0f);
			break;
		case tk2dBaseSprite.Anchor.MiddleRight:
			((Vector3)(ref val2)).Set(1f, 0.5f, 0f);
			break;
		case tk2dBaseSprite.Anchor.UpperLeft:
			((Vector3)(ref val2)).Set(0f, 1f, 0f);
			break;
		case tk2dBaseSprite.Anchor.UpperCenter:
			((Vector3)(ref val2)).Set(0.5f, 1f, 0f);
			break;
		case tk2dBaseSprite.Anchor.UpperRight:
			((Vector3)(ref val2)).Set(1f, 1f, 0f);
			break;
		}
		val2 = Vector3.Scale(val2, val) * -1f;
		Vector3 val3 = val + dMax - dMin;
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector((!Mathf.Approximately(val.x, 0f)) ? (val2.x * val3.x / val.x) : 0f, (!Mathf.Approximately(val.y, 0f)) ? (val2.y * val3.y / val.y) : 0f);
		Vector3 val5 = val2 + dMin - val4;
		val5.z = 0f;
		((Component)this).transform.position = ((Component)this).transform.TransformPoint(val5);
		size = new Vector2(val3.x, val3.y);
		Build();
	}
}
