using UnityEngine;

public class River : KSingleton<River>
{
	public const int WaterChunkSizeX = 4;

	public const int WaterChunkSizeY = 4;

	public const int WaterTileSizeX = 4;

	public const int WaterTileSizeY = 4;

	private int _flowMapOffsetId;

	private int _eyePosRiverId;

	private int _lightDirId;

	[SerializeField]
	public Material SharedMaterial;

	[SerializeField]
	private Transform _eyeOffset;

	[SerializeField]
	private Transform _lightDir;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private float _phaseLength;

	private int _maxWaterChunks;

	private RiverChunk[] _riverChunks;

	private float _delta1;

	private float _delta2;

	private void Start()
	{
		_flowMapOffsetId = Shader.PropertyToID("_FlowMapOffset");
		_eyePosRiverId = Shader.PropertyToID("_EyePosRiver");
		_lightDirId = Shader.PropertyToID("_LightDir");
	}

	private void Update()
	{
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SharedMaterial == (Object)null)
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
		Vector3 val = default(Vector3);
		val.x = _delta1;
		val.y = _delta2;
		float num = _phaseLength * 0.5f;
		val.z = Mathf.Abs(num - Mathf.Abs(_delta1)) / num;
		SharedMaterial.SetVector(_flowMapOffsetId, Vector4.op_Implicit(val));
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		SharedMaterial.SetVector(_eyePosRiverId, Vector4.op_Implicit(currentPosition + _eyeOffset.localPosition));
		SharedMaterial.SetVector(_lightDirId, Vector4.op_Implicit(_lightDir.forward));
	}

	public void Init(int maxWaterChunks)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		_maxWaterChunks = maxWaterChunks;
		_riverChunks = new RiverChunk[_maxWaterChunks];
		for (int i = 0; i < _maxWaterChunks; i++)
		{
			GameObject val = new GameObject();
			((Object)val).name = "RiverChunk";
			val.transform.parent = ((Component)this).gameObject.transform;
			_riverChunks[i] = val.AddComponent<RiverChunk>();
			val.SetActive(false);
		}
	}

	public RiverChunk GetWaterChunk(int chunkIndex)
	{
		return _riverChunks[chunkIndex];
	}
}
