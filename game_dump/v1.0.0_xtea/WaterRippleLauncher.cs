using TerrainData;
using UnityEngine;

public class WaterRippleLauncher : MonoBehaviour
{
	[SerializeField]
	private bool _isStatic;

	[SerializeField]
	private ParticleType _particleType;

	[SerializeField]
	private bool _canBeRiver;

	[SerializeField]
	private ParticleType _riverParticleType;

	[SerializeField]
	private string[] _attachedPartNames;

	private Transform[] _attachedTransforms;

	private WaterRipple[] _waterRipples;

	private WaterRipple[] _riverRipples;

	private void Start()
	{
		InitWaterRipples();
		ProcessWaterRipple();
	}

	private void InitWaterRipples()
	{
		int num = _attachedPartNames.Length;
		_attachedTransforms = (Transform[])(object)new Transform[num];
		for (int i = 0; i < num; i++)
		{
			_attachedTransforms[i] = KUtility.FindTransformByName(((Component)this).gameObject, _attachedPartNames[i]);
			if ((Object)(object)_attachedTransforms[i] == (Object)null)
			{
			}
		}
		_waterRipples = new WaterRipple[num];
		for (int j = 0; j < num; j++)
		{
			_waterRipples[j] = new WaterRipple(_particleType);
		}
		if (_canBeRiver)
		{
			_riverRipples = new WaterRipple[num];
			for (int k = 0; k < num; k++)
			{
				_riverRipples[k] = new WaterRipple(_riverParticleType, isRiver: true);
			}
		}
	}

	private void LateUpdate()
	{
		if (!_isStatic)
		{
			ProcessWaterRipple();
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < _attachedPartNames.Length; i++)
		{
			_waterRipples[i].Stop();
			if (_canBeRiver)
			{
				_riverRipples[i].Stop();
			}
		}
	}

	private void ProcessWaterRipple()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _attachedPartNames.Length; i++)
		{
			Transform val = _attachedTransforms[i];
			if (!((Object)(object)val == (Object)null))
			{
				Vector3 position = val.position;
				Vector3 val2 = TerrainA6.ClientPositionToWorldPosition(position);
				Biome tileBiome = TerrainA6.GetTileBiome(val2);
				float waterDepth = TerrainA6.GetWaterDepth(val2);
				TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.GetWaterDepthLevel(waterDepth);
				_waterRipples[i].Process(tileBiome, waterDepthLevel, position);
				if (_canBeRiver)
				{
					_riverRipples[i].Process(tileBiome, waterDepthLevel, position);
				}
			}
		}
	}
}
