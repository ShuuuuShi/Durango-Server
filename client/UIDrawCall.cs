using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Internal/Draw Call")]
public class UIDrawCall : MonoBehaviour
{
	public enum Clipping
	{
		None = 0,
		TextureMask = 1,
		SoftClip = 3,
		ConstrainButDontClip = 4
	}

	public delegate void OnRenderCallback(Material mat);

	public class ExtentionUvs
	{
		private BetterList<KeyValuePair<int, List<Vector2>>> _vector2Uvs;

		private BetterList<KeyValuePair<int, List<Vector3>>> _vector3Uvs;

		private BetterList<KeyValuePair<int, List<Vector4>>> _vector4Uvs;

		private static List<T> GetExtensionUv<T>(ref BetterList<KeyValuePair<int, List<T>>> list, int index)
		{
			if (list == null)
			{
				list = new BetterList<KeyValuePair<int, List<T>>>();
			}
			for (int i = 0; i < list.size; i++)
			{
				if (list[i].Key == index)
				{
					return list[i].Value;
				}
			}
			List<T> list2 = null;
			if (list.buffer != null && list.size < list.buffer.Length)
			{
				list2 = list.buffer[list.size].Value;
			}
			if (list2 == null)
			{
				list2 = new List<T>();
			}
			list2.Clear();
			list.Add(new KeyValuePair<int, List<T>>(index, list2));
			return list2;
		}

		private static void Append<T>(ref BetterList<KeyValuePair<int, List<T>>> list, BetterList<KeyValuePair<int, List<T>>> append)
		{
			if (append != null)
			{
				for (int i = 0; i < append.size; i++)
				{
					Append(ref list, append[i]);
				}
			}
		}

		private static void Append<T>(ref BetterList<KeyValuePair<int, List<T>>> list, KeyValuePair<int, List<T>> append)
		{
			if (append.Value == null || append.Value.Count == 0)
			{
				return;
			}
			if (list == null)
			{
				list = new BetterList<KeyValuePair<int, List<T>>>();
			}
			List<T> list2 = null;
			for (int i = 0; i < list.size; i++)
			{
				if (list[i].Key == append.Key)
				{
					list2 = list[i].Value;
					break;
				}
			}
			if (list2 == null)
			{
				list.Add(new KeyValuePair<int, List<T>>(append.Key, new List<T>(append.Value)));
			}
			else
			{
				list2.AddRange(append.Value);
			}
		}

		public List<Vector2> GetVector2Uvs(int index)
		{
			return GetExtensionUv(ref _vector2Uvs, index);
		}

		public List<Vector3> GetVector3Uvs(int index)
		{
			return GetExtensionUv(ref _vector3Uvs, index);
		}

		public List<Vector4> GetVector4Uvs(int index)
		{
			return GetExtensionUv(ref _vector4Uvs, index);
		}

		public void Fill(ExtentionUvs other)
		{
			Append(ref _vector2Uvs, other._vector2Uvs);
			Append(ref _vector3Uvs, other._vector3Uvs);
			Append(ref _vector4Uvs, other._vector4Uvs);
		}

		public void Clear()
		{
			if (_vector2Uvs != null)
			{
				_vector2Uvs.Clear();
			}
			if (_vector3Uvs != null)
			{
				_vector3Uvs.Clear();
			}
			if (_vector4Uvs != null)
			{
				_vector4Uvs.Clear();
			}
		}

		public void FillMesh(Mesh mesh)
		{
			int vertexCount = mesh.vertexCount;
			if (_vector2Uvs != null)
			{
				foreach (KeyValuePair<int, List<Vector2>> vector2Uv in _vector2Uvs)
				{
					if (vector2Uv.Value.Count != 0)
					{
						while (vector2Uv.Value.Count < vertexCount)
						{
							vector2Uv.Value.Add(Vector2.zero);
						}
						mesh.SetUVs(vector2Uv.Key, vector2Uv.Value);
					}
				}
			}
			if (_vector3Uvs != null)
			{
				foreach (KeyValuePair<int, List<Vector3>> vector3Uv in _vector3Uvs)
				{
					if (vector3Uv.Value.Count != 0)
					{
						while (vector3Uv.Value.Count < vertexCount)
						{
							vector3Uv.Value.Add(Vector3.zero);
						}
						mesh.SetUVs(vector3Uv.Key, vector3Uv.Value);
					}
				}
			}
			if (_vector4Uvs == null)
			{
				return;
			}
			foreach (KeyValuePair<int, List<Vector4>> vector4Uv in _vector4Uvs)
			{
				if (vector4Uv.Value.Count != 0)
				{
					while (vector4Uv.Value.Count < vertexCount)
					{
						vector4Uv.Value.Add(Vector4.zero);
					}
					mesh.SetUVs(vector4Uv.Key, vector4Uv.Value);
				}
			}
		}
	}

	private static BetterList<UIDrawCall> mActiveList = new BetterList<UIDrawCall>();

	private static BetterList<UIDrawCall> mInactiveList = new BetterList<UIDrawCall>();

	[NonSerialized]
	[HideInInspector]
	public int widgetCount;

	[NonSerialized]
	[HideInInspector]
	public int depthStart = int.MaxValue;

	[NonSerialized]
	[HideInInspector]
	public int depthEnd = int.MinValue;

	[NonSerialized]
	[HideInInspector]
	public UIPanel manager;

	[NonSerialized]
	[HideInInspector]
	public UIPanel panel;

	[NonSerialized]
	[HideInInspector]
	public Texture2D clipTexture;

	[NonSerialized]
	[HideInInspector]
	public bool alwaysOnScreen;

	[NonSerialized]
	[HideInInspector]
	public BetterList<Vector3> verts = new BetterList<Vector3>();

	[NonSerialized]
	[HideInInspector]
	public BetterList<Vector3> norms = new BetterList<Vector3>();

	[NonSerialized]
	[HideInInspector]
	public BetterList<Vector4> tans = new BetterList<Vector4>();

	[NonSerialized]
	[HideInInspector]
	public BetterList<Vector2> uvs = new BetterList<Vector2>();

	[NonSerialized]
	[HideInInspector]
	public BetterList<Color> cols = new BetterList<Color>();

	[NonSerialized]
	[HideInInspector]
	public ExtentionUvs extentionUvs = new ExtentionUvs();

	private Material mMaterial;

	private Texture mTexture;

	private Shader mShader;

	private int mClipCount;

	private Transform mTrans;

	private Mesh mMesh;

	private MeshFilter mFilter;

	private MeshRenderer mRenderer;

	private Material mDynamicMat;

	private int[] mIndices;

	private bool mRebuildMat = true;

	private int mRenderQueue = 3000;

	private int mTriangles;

	[NonSerialized]
	public bool isDirty;

	[NonSerialized]
	private bool mTextureClip;

	public OnRenderCallback onRender;

	private const int maxIndexBufferCache = 10;

	private static List<int[]> mCache = new List<int[]>(10);

	private static int[] ClipRange = null;

	private static int[] ClipArgs = null;

	[Obsolete("Use UIDrawCall.activeList")]
	public static BetterList<UIDrawCall> list => mActiveList;

	public static BetterList<UIDrawCall> activeList => mActiveList;

	public static BetterList<UIDrawCall> inactiveList => mInactiveList;

	public int renderQueue
	{
		get
		{
			return mRenderQueue;
		}
		set
		{
			if (mRenderQueue != value)
			{
				mRenderQueue = value;
				if (mDynamicMat != null)
				{
					mDynamicMat.renderQueue = value;
				}
			}
		}
	}

	public int sortingOrder
	{
		get
		{
			return (mRenderer != null) ? mRenderer.sortingOrder : 0;
		}
		set
		{
			if (mRenderer != null && mRenderer.sortingOrder != value)
			{
				mRenderer.sortingOrder = value;
			}
		}
	}

	public string sortingLayerName
	{
		get
		{
			return (!(mRenderer != null)) ? null : mRenderer.sortingLayerName;
		}
		set
		{
			if (mRenderer != null && mRenderer.sortingLayerName != value)
			{
				mRenderer.sortingLayerName = value;
			}
		}
	}

	public int finalRenderQueue => (!(mDynamicMat != null)) ? mRenderQueue : mDynamicMat.renderQueue;

	public Transform cachedTransform
	{
		get
		{
			if (mTrans == null)
			{
				mTrans = base.transform;
			}
			return mTrans;
		}
	}

	public Material baseMaterial
	{
		get
		{
			return mMaterial;
		}
		set
		{
			if (mMaterial != value)
			{
				mMaterial = value;
				mRebuildMat = true;
			}
		}
	}

	public Material dynamicMaterial => mDynamicMat;

	public Texture mainTexture
	{
		get
		{
			return mTexture;
		}
		set
		{
			mTexture = value;
			if (mDynamicMat != null)
			{
				mDynamicMat.mainTexture = value;
			}
		}
	}

	public Shader shader
	{
		get
		{
			return mShader;
		}
		set
		{
			if (mShader != value)
			{
				mShader = value;
				mRebuildMat = true;
			}
		}
	}

	public int triangles => (mMesh != null) ? mTriangles : 0;

	public bool isClipped => mClipCount != 0;

	private void CreateMaterial()
	{
		mTextureClip = false;
		mClipCount = ((panel != null) ? panel.clipCount : 0);
		string text = ((mShader != null) ? mShader.name : ((!(mMaterial != null)) ? "Durango/NGUI/Transparent" : mMaterial.shader.name));
		text = text.Replace("GUI/Text Shader", "Durango/NGUI/Text");
		if (panel != null && panel.clipping == Clipping.TextureMask)
		{
			mTextureClip = true;
		}
		// [4 ก.ย. 2026] มือถือ (APK build เอง): shader ในโปรเจกต์เป็น dummy — ใช้ของแท้จาก preload bundle ก่อน (MobileShaderBootstrap)
		shader = MobileShaderBootstrap.Find(text) ?? Shader.Find(text);
		if (shader == null)
		{
			shader = NGUITools.defaultShader;
		}
		if (mMaterial != null)
		{
			mDynamicMat = new Material(mMaterial);
			mDynamicMat.name = "[NGUI] " + mMaterial.name;
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			mDynamicMat.CopyPropertiesFromMaterial(mMaterial);
			string[] shaderKeywords = mMaterial.shaderKeywords;
			for (int i = 0; i < shaderKeywords.Length; i++)
			{
				mDynamicMat.EnableKeyword(shaderKeywords[i]);
			}
			if (shader != null)
			{
				mDynamicMat.shader = shader;
			}
			else if (mClipCount != 0)
			{
				Debug.LogError(text + " shader doesn't have a clipped shader version for " + mClipCount + " clip regions");
			}
		}
		else
		{
			mDynamicMat = new Material(shader);
			mDynamicMat.name = "[NGUI] " + shader.name;
			mDynamicMat.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		}
		if (mTextureClip)
		{
			mDynamicMat.EnableKeyword("CLIP_TEX");
			return;
		}
		switch (mClipCount)
		{
		case 1:
			mDynamicMat.EnableKeyword("CLIP1");
			break;
		case 2:
			mDynamicMat.EnableKeyword("CLIP2");
			break;
		case 3:
			mDynamicMat.EnableKeyword("CLIP3");
			break;
		}
	}

	public void MaterialChanged()
	{
		mRebuildMat = true;
	}

	private Material RebuildMaterial()
	{
		NGUITools.DestroyImmediate(mDynamicMat);
		CreateMaterial();
		mDynamicMat.renderQueue = mRenderQueue;
		if (mTexture != null)
		{
			mDynamicMat.mainTexture = mTexture;
		}
		if (mRenderer != null)
		{
			mRenderer.sharedMaterials = new Material[1] { mDynamicMat };
		}
		return mDynamicMat;
	}

	private void UpdateMaterials()
	{
		if (!(panel == null))
		{
			if (mRebuildMat || mDynamicMat == null || mClipCount != panel.clipCount || mTextureClip != (panel.clipping == Clipping.TextureMask))
			{
				RebuildMaterial();
				mRebuildMat = false;
			}
			else if (mRenderer.sharedMaterial != mDynamicMat)
			{
				mRenderer.sharedMaterials = new Material[1] { mDynamicMat };
			}
		}
	}

	public void UpdateGeometry(int widgetCount)
	{
		this.widgetCount = widgetCount;
		int size = verts.size;
		if (size > 0 && size == uvs.size && size == cols.size && size % 4 == 0)
		{
			if (mFilter == null)
			{
				mFilter = base.gameObject.GetComponent<MeshFilter>();
			}
			if (mFilter == null)
			{
				mFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			if (verts.size < 65000)
			{
				int num = (size >> 1) * 3;
				bool flag = mIndices == null || mIndices.Length != num;
				if (mMesh == null)
				{
					mMesh = new Mesh();
					mMesh.hideFlags = HideFlags.DontSave;
					mMesh.name = ((!(mMaterial != null)) ? "[NGUI] Mesh" : ("[NGUI] " + mMaterial.name));
					mMesh.MarkDynamic();
					flag = true;
				}
				bool flag2 = uvs.buffer.Length != verts.buffer.Length || cols.buffer.Length != verts.buffer.Length || (norms.buffer != null && norms.buffer.Length != verts.buffer.Length) || (tans.buffer != null && tans.buffer.Length != verts.buffer.Length);
				if (!flag2 && panel != null && panel.renderQueue != 0)
				{
					flag2 = mMesh == null || mMesh.vertexCount != verts.buffer.Length;
				}
				if (!flag2 && verts.size << 1 < verts.buffer.Length)
				{
					flag2 = true;
				}
				mTriangles = verts.size >> 1;
				if (flag2 || verts.buffer.Length > 65000)
				{
					if (flag2 || mMesh.vertexCount != verts.size)
					{
						mMesh.Clear();
						flag = true;
					}
					mMesh.vertices = verts.ToArray();
					mMesh.uv = uvs.ToArray();
					mMesh.colors = cols.ToArray();
					if (norms != null)
					{
						mMesh.normals = norms.ToArray();
					}
					if (tans != null)
					{
						mMesh.tangents = tans.ToArray();
					}
				}
				else
				{
					if (mMesh.vertexCount != verts.buffer.Length)
					{
						mMesh.Clear();
						flag = true;
					}
					mMesh.vertices = verts.buffer;
					mMesh.uv = uvs.buffer;
					mMesh.colors = cols.buffer;
					if (norms != null)
					{
						mMesh.normals = norms.buffer;
					}
					if (tans != null)
					{
						mMesh.tangents = tans.buffer;
					}
				}
				extentionUvs.FillMesh(mMesh);
				if (flag)
				{
					mIndices = GenerateCachedIndexBuffer(size, num);
					mMesh.triangles = mIndices;
				}
				if (flag2 || !alwaysOnScreen)
				{
					mMesh.RecalculateBounds();
				}
				mFilter.mesh = mMesh;
			}
			else
			{
				mTriangles = 0;
				if (mFilter.mesh != null)
				{
					mFilter.mesh.Clear();
				}
				Debug.LogError("Too many vertices on one panel: " + verts.size);
			}
			if (mRenderer == null)
			{
				mRenderer = base.gameObject.GetComponent<MeshRenderer>();
			}
			if (mRenderer == null)
			{
				mRenderer = base.gameObject.AddComponent<MeshRenderer>();
			}
			UpdateMaterials();
		}
		else
		{
			if (mFilter.mesh != null)
			{
				mFilter.mesh.Clear();
			}
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + size);
		}
		verts.Clear();
		uvs.Clear();
		cols.Clear();
		norms.Clear();
		tans.Clear();
		extentionUvs.Clear();
	}

	private int[] GenerateCachedIndexBuffer(int vertexCount, int indexCount)
	{
		int i = 0;
		for (int count = mCache.Count; i < count; i++)
		{
			int[] array = mCache[i];
			if (array != null && array.Length == indexCount)
			{
				return array;
			}
		}
		int[] array2 = new int[indexCount];
		int num = 0;
		for (int j = 0; j < vertexCount; j += 4)
		{
			array2[num++] = j;
			array2[num++] = j + 1;
			array2[num++] = j + 2;
			array2[num++] = j + 2;
			array2[num++] = j + 3;
			array2[num++] = j;
		}
		if (mCache.Count > 10)
		{
			mCache.RemoveAt(0);
		}
		mCache.Add(array2);
		return array2;
	}

	private void OnWillRenderObject()
	{
		UpdateMaterials();
		if (onRender != null)
		{
			onRender(mDynamicMat ?? mMaterial);
		}
		if (mDynamicMat == null || mClipCount == 0)
		{
			return;
		}
		if (mTextureClip)
		{
			Vector4 drawCallClipRange = panel.drawCallClipRange;
			mDynamicMat.SetVector(ClipRange[0], new Vector4((0f - drawCallClipRange.x) / drawCallClipRange.z, (0f - drawCallClipRange.y) / drawCallClipRange.w, 1f / drawCallClipRange.z, 1f / drawCallClipRange.w));
			mDynamicMat.SetTexture("_ClipTex", clipTexture);
			return;
		}
		UIPanel parentPanel = panel;
		int num = 0;
		while (parentPanel != null)
		{
			if (parentPanel.hasClipping)
			{
				float angle = 0f;
				Vector4 drawCallClipRange2 = parentPanel.drawCallClipRange;
				if (parentPanel != panel)
				{
					Vector3 vector = parentPanel.cachedTransform.InverseTransformPoint(panel.cachedTransform.position);
					drawCallClipRange2.x -= vector.x;
					drawCallClipRange2.y -= vector.y;
					Vector3 eulerAngles = panel.cachedTransform.rotation.eulerAngles;
					Vector3 eulerAngles2 = parentPanel.cachedTransform.rotation.eulerAngles;
					Vector3 vector2 = eulerAngles2 - eulerAngles;
					vector2.x = NGUIMath.WrapAngle(vector2.x);
					vector2.y = NGUIMath.WrapAngle(vector2.y);
					vector2.z = NGUIMath.WrapAngle(vector2.z);
					if (Mathf.Abs(vector2.x) > 0.001f || Mathf.Abs(vector2.y) > 0.001f)
					{
					}
					angle = vector2.z;
				}
				SetClipping(num++, drawCallClipRange2, parentPanel.clipSoftness, angle);
			}
			parentPanel = parentPanel.parentPanel;
		}
	}

	private void SetClipping(int index, Vector4 cr, Vector2 soft, float angle)
	{
		angle *= -(float)Math.PI / 180f;
		Vector2 vector = new Vector2(1000f, 1000f);
		if (soft.x > 0f)
		{
			vector.x = cr.z / soft.x;
		}
		if (soft.y > 0f)
		{
			vector.y = cr.w / soft.y;
		}
		if (index < ClipRange.Length)
		{
			mDynamicMat.SetVector(ClipRange[index], new Vector4((0f - cr.x) / cr.z, (0f - cr.y) / cr.w, 1f / cr.z, 1f / cr.w));
			mDynamicMat.SetVector(ClipArgs[index], new Vector4(vector.x, vector.y, Mathf.Sin(angle), Mathf.Cos(angle)));
		}
	}

	private void Awake()
	{
		if (ClipRange == null)
		{
			ClipRange = new int[4]
			{
				Shader.PropertyToID("_ClipRange0"),
				Shader.PropertyToID("_ClipRange1"),
				Shader.PropertyToID("_ClipRange2"),
				Shader.PropertyToID("_ClipRange4")
			};
		}
		if (ClipArgs == null)
		{
			ClipArgs = new int[4]
			{
				Shader.PropertyToID("_ClipArgs0"),
				Shader.PropertyToID("_ClipArgs1"),
				Shader.PropertyToID("_ClipArgs2"),
				Shader.PropertyToID("_ClipArgs3")
			};
		}
	}

	private void OnEnable()
	{
		mRebuildMat = true;
	}

	private void OnDisable()
	{
		depthStart = int.MaxValue;
		depthEnd = int.MinValue;
		panel = null;
		manager = null;
		mMaterial = null;
		mTexture = null;
		clipTexture = null;
		if (mRenderer != null)
		{
			mRenderer.sharedMaterials = new Material[0];
		}
		NGUITools.DestroyImmediate(mDynamicMat);
		mDynamicMat = null;
	}

	private void OnDestroy()
	{
		NGUITools.DestroyImmediate(mMesh);
		mMesh = null;
	}

	public static UIDrawCall Create(UIPanel panel, Material mat, Texture tex, Shader shader)
	{
		return Create(null, panel, mat, tex, shader);
	}

	private static UIDrawCall Create(string name, UIPanel pan, Material mat, Texture tex, Shader shader)
	{
		UIDrawCall uIDrawCall = Create(name);
		uIDrawCall.gameObject.layer = pan.cachedGameObject.layer;
		uIDrawCall.baseMaterial = mat;
		uIDrawCall.mainTexture = tex;
		uIDrawCall.shader = shader;
		uIDrawCall.renderQueue = pan.startingRenderQueue;
		uIDrawCall.sortingOrder = pan.sortingOrder;
		uIDrawCall.manager = pan;
		return uIDrawCall;
	}

	private static UIDrawCall Create(string name)
	{
		while (mInactiveList.size > 0)
		{
			UIDrawCall uIDrawCall = mInactiveList.Pop();
			if (uIDrawCall != null)
			{
				mActiveList.Add(uIDrawCall);
				if (name != null)
				{
					uIDrawCall.name = name;
				}
				NGUITools.SetActive(uIDrawCall.gameObject, state: true);
				return uIDrawCall;
			}
		}
		GameObject gameObject = new GameObject(name);
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		UIDrawCall uIDrawCall2 = gameObject.AddComponent<UIDrawCall>();
		mActiveList.Add(uIDrawCall2);
		return uIDrawCall2;
	}

	public static void ClearAll()
	{
		bool isPlaying = Application.isPlaying;
		int num = mActiveList.size;
		while (num > 0)
		{
			UIDrawCall uIDrawCall = mActiveList[--num];
			if ((bool)uIDrawCall)
			{
				if (isPlaying)
				{
					NGUITools.SetActive(uIDrawCall.gameObject, state: false);
				}
				else
				{
					NGUITools.DestroyImmediate(uIDrawCall.gameObject);
				}
			}
		}
		mActiveList.Clear();
	}

	public static void ReleaseAll()
	{
		ClearAll();
		ReleaseInactive();
	}

	public static void ReleaseInactive()
	{
		int num = mInactiveList.size;
		while (num > 0)
		{
			UIDrawCall uIDrawCall = mInactiveList[--num];
			if ((bool)uIDrawCall)
			{
				NGUITools.DestroyImmediate(uIDrawCall.gameObject);
			}
		}
		mInactiveList.Clear();
	}

	public static int Count(UIPanel panel)
	{
		int num = 0;
		for (int i = 0; i < mActiveList.size; i++)
		{
			if (mActiveList[i].manager == panel)
			{
				num++;
			}
		}
		return num;
	}

	public static void Destroy(UIDrawCall dc)
	{
		if (!dc)
		{
			return;
		}
		dc.onRender = null;
		if (Application.isPlaying)
		{
			if (mActiveList.Remove(dc))
			{
				NGUITools.SetActive(dc.gameObject, state: false);
				mInactiveList.Add(dc);
			}
		}
		else
		{
			mActiveList.Remove(dc);
			NGUITools.DestroyImmediate(dc.gameObject);
		}
	}
}
