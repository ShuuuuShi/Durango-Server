using System;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Water;

public class River : Singleton<River>
{
	[Serializable]
	public class RiverSet
	{
		public string RiverType;

		public Material Material;

		public float Speed;

		public float PhaseLength;
	}

	public const int WaterChunkSizeX = 4;

	public const int WaterChunkSizeY = 4;

	public const int WaterTileSizeX = 4;

	public const int WaterTileSizeY = 4;

	[SerializeField]
	private RiverSet[] _riverSets;

	private Material _sharedMaterial;

	private int _flowMapOffsetId;

	private int _eyePosRiverId;

	private int _lightDirId;

	private float _speed;

	private float _phaseLength;

	[SerializeField]
	private Transform _eyeOffset;

	[SerializeField]
	private Transform _lightDir;

	private int _maxWaterChunks;

	private RiverChunk[] _riverChunks;

	private float _delta1;

	private float _delta2;

	public Material SharedMaterial
	{
		get
		{
			return _sharedMaterial;
		}
		set
		{
			_sharedMaterial = value;
			if (_riverChunks != null)
			{
				for (int i = 0; i < _riverChunks.Length; i++)
				{
					_riverChunks[i].SetMaterial(_sharedMaterial);
				}
			}
		}
	}

	private void Start()
	{
		_flowMapOffsetId = Shader.PropertyToID("_FlowMapOffset");
		_eyePosRiverId = Shader.PropertyToID("_EyePosRiver");
		_lightDirId = Shader.PropertyToID("_LightDir");
	}

	private void Update()
	{
		if (SharedMaterial == null)
		{
			return;
		}
		if ((double)_speed > 0.0)
		{
			_delta1 += Time.deltaTime * _speed;
			if (_delta1 > _phaseLength || _delta1 < 0f - _phaseLength)
			{
				_delta1 = 0f;
			}
			_delta2 = _delta1 + _phaseLength * 0.5f;
			if (_delta2 > _phaseLength)
			{
				_delta2 -= _phaseLength;
			}
		}
		else
		{
			_delta2 += Time.deltaTime * _speed;
			if (_delta2 > _phaseLength || _delta2 < 0f - _phaseLength)
			{
				_delta2 = 0f;
			}
			_delta1 = _delta2 - _phaseLength * 0.5f;
			if (_delta1 < 0f - _phaseLength)
			{
				_delta1 += _phaseLength;
			}
		}
		Vector3 vector = default(Vector3);
		vector.x = _delta1;
		vector.y = _delta2;
		float num = _phaseLength * 0.5f;
		vector.z = Mathf.Abs(num - Mathf.Abs(_delta1)) / num;
		SharedMaterial.SetVector(_flowMapOffsetId, vector);
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		SharedMaterial.SetVector(_eyePosRiverId, currentPosition + _eyeOffset.localPosition);
		SharedMaterial.SetVector(_lightDirId, _lightDir.forward);
	}

	public void Init(int maxWaterChunks)
	{
		SetMaterialType(TerrainMeta.RiverType);
		_maxWaterChunks = maxWaterChunks;
		_riverChunks = new RiverChunk[_maxWaterChunks];
		for (int i = 0; i < _maxWaterChunks; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "RiverChunk";
			gameObject.transform.parent = base.gameObject.transform;
			_riverChunks[i] = gameObject.AddComponent<RiverChunk>();
			gameObject.SetActive(value: false);
		}
	}

	public RiverChunk GetWaterChunk(int chunkIndex)
	{
		return _riverChunks[chunkIndex];
	}

	public void SetMaterialType(string type)
	{
		for (int i = 0; i < _riverSets.Length; i++)
		{
			if (!(_riverSets[i].RiverType != type))
			{
				ApplyRiverSet(_riverSets[i]);
				return;
			}
		}
		Debug.LogError("River Type not found - " + type);
	}

	private void ApplyRiverSet(RiverSet set)
	{
		SharedMaterial = set.Material;
		_speed = set.Speed;
		_phaseLength = set.PhaseLength;
	}
}
