using UnityEngine;

namespace Durango.Render.Water;

public abstract class WaterBase : MonoBehaviour
{
	public const int TileSizeX = 2;

	public const int TileSizeY = 2;

	public const int ChunkSizeX = 4;

	public const int ChunkSizeY = 4;

	protected int WorldLightDirId;

	[SerializeField]
	protected Transform _specularLight;

	private Material _sharedMaterial;

	private WaterChunk[] _waterChunks;

	private Vector4 _bumpTiling;

	private Vector4 _bumpDirection;

	private bool _tilingEnabled;

	public Material SharedMaterial
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
		InitMaterial();
		_waterChunks = new WaterChunk[maxWaterChunks];
		for (int i = 0; i < maxWaterChunks; i++)
		{
			GameObject obj = new GameObject();
			obj.name = "WaterChunk";
			obj.transform.parent = base.gameObject.transform;
			WaterChunk waterChunk = obj.AddComponent<WaterChunk>();
			waterChunk.Init(SharedMaterial);
			_waterChunks[i] = waterChunk;
			obj.SetActive(value: false);
		}
		WorldLightDirId = Shader.PropertyToID("_WorldLightDir");
	}

	protected abstract void InitMaterial();

	public abstract void SetMaterialType(string lakeType);

	public void InitBumpTiling()
	{
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
		if (_tilingEnabled)
		{
			float num = Time.realtimeSinceStartup / 20f;
			Vector4 value = new Vector4(num % (1f / (_bumpDirection.x * _bumpTiling.x)), num % (1f / (_bumpDirection.y * _bumpTiling.y)), num % (1f / (_bumpDirection.z * _bumpTiling.z)), num % (1f / (_bumpDirection.w * _bumpTiling.w)));
			SharedMaterial.SetVector(propertyId, value);
		}
	}

	public WaterChunk GetWaterChunk(int chunkIndex)
	{
		return _waterChunks[chunkIndex];
	}
}
