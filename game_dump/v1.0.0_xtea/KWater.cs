using UnityEngine;

public abstract class KWater : MonoBehaviour
{
	public const int TileSizeX = 2;

	public const int TileSizeY = 2;

	public const int ChunkSizeX = 4;

	public const int ChunkSizeY = 4;

	protected int WorldLightDirId;

	[SerializeField]
	protected Transform _specularLight;

	[SerializeField]
	private Material _sharedMaterial;

	private WaterChunk[] _waterChunks;

	private Vector4 _bumpTiling;

	private Vector4 _bumpDirection;

	private bool _tilingEnabled;

	protected Material SharedMaterial
	{
		get
		{
			return _sharedMaterial;
		}
		set
		{
			_sharedMaterial = value;
			InitBumpTiling();
			if (_waterChunks != null)
			{
				for (int i = 0; i < _waterChunks.Length; i++)
				{
					_waterChunks[i].SetMaterial(_sharedMaterial);
				}
			}
		}
	}

	public void Init(int maxWaterChunks)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		InitMaterial();
		_waterChunks = new WaterChunk[maxWaterChunks];
		for (int i = 0; i < maxWaterChunks; i++)
		{
			GameObject val = new GameObject();
			((Object)val).name = "WaterChunk";
			val.transform.parent = ((Component)this).gameObject.transform;
			WaterChunk waterChunk = val.AddComponent<WaterChunk>();
			waterChunk.Init(SharedMaterial);
			_waterChunks[i] = waterChunk;
			val.SetActive(false);
		}
		WorldLightDirId = Shader.PropertyToID("_WorldLightDir");
	}

	protected abstract void InitMaterial();

	public void InitBumpTiling()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Material sharedMaterial = SharedMaterial;
		if (sharedMaterial.HasProperty("_BumpTiling"))
		{
			_bumpTiling = sharedMaterial.GetVector("_BumpTiling");
			_bumpDirection = sharedMaterial.GetVector("_BumpDirection");
			_tilingEnabled = true;
		}
		else
		{
			_tilingEnabled = false;
		}
	}

	protected void UpdateTilingPeriod(int propertyId)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (_tilingEnabled)
		{
			float num = Time.realtimeSinceStartup / 20f;
			Vector4 val = default(Vector4);
			((Vector4)(ref val))._002Ector(num % (1f / (_bumpDirection.x * _bumpTiling.x)), num % (1f / (_bumpDirection.y * _bumpTiling.y)), num % (1f / (_bumpDirection.z * _bumpTiling.z)), num % (1f / (_bumpDirection.w * _bumpTiling.w)));
			SharedMaterial.SetVector(propertyId, val);
		}
	}

	public WaterChunk GetWaterChunk(int chunkIndex)
	{
		return _waterChunks[chunkIndex];
	}
}
