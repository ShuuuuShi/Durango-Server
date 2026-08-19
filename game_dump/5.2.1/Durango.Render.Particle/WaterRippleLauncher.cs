using Durango.Terrain;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Render.Particle;

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
		_attachedTransforms = new Transform[num];
		for (int i = 0; i < num; i++)
		{
			_attachedTransforms[i] = KUtility.FindTransformByName(base.gameObject, _attachedPartNames[i]);
			_ = _attachedTransforms[i] == null;
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
		for (int i = 0; i < _attachedPartNames.Length; i++)
		{
			Transform transform = _attachedTransforms[i];
			if (!(transform == null))
			{
				Vector3 position = transform.position;
				Vector3 vector = Util.ClientPositionToWorldPosition(position);
				Biome tileBiome = Singleton<TerrainBase>.Instance().GetTileBiome(vector);
				TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.GetWaterDepthLevel(Singleton<TerrainBase>.Instance().GetWaterDepth(vector));
				_waterRipples[i].Process(tileBiome, waterDepthLevel, position);
				if (_canBeRiver)
				{
					_riverRipples[i].Process(tileBiome, waterDepthLevel, position);
				}
			}
		}
	}
}
