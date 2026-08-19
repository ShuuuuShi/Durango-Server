using TerrainData;
using UnityEngine;

public class WaterRipple
{
	private readonly string _particlePath;

	private readonly bool _isRiver;

	private ParticleSystem _waterParticle;

	public WaterRipple(string particlePath, bool isRiver = false)
	{
		_isRiver = isRiver;
		_particlePath = particlePath;
		ParticleManager.Cache(_particlePath);
	}

	public void Process(Biome biome, TerrainWater.WaterDepthLevel depthLevel, Vector3 position)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlayable(biome, depthLevel))
		{
			Play(position);
		}
		else
		{
			Stop();
		}
	}

	private bool IsPlayable(Biome biome, TerrainWater.WaterDepthLevel depthLevel)
	{
		if (depthLevel == TerrainWater.WaterDepthLevel.Land)
		{
			return false;
		}
		if (_isRiver)
		{
			return biome == Biome.River;
		}
		return biome == Biome.WarmOcean || biome == Biome.SandBeach || biome == Biome.Lake || biome == Biome.ColdOcean || biome == Biome.PebbleBeach;
	}

	private void Play(Vector3 position)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		CheckWaterParticle();
		if (!((Object)(object)_waterParticle == (Object)null))
		{
			Vector3 localPosition = position;
			localPosition.y = 0f;
			((Component)_waterParticle).transform.localPosition = localPosition;
			if (_isRiver)
			{
				Vector2 waterFlow = TerrainA6.GetWaterFlow(TerrainA6.ClientPositionToWorldPosition(position));
				float num = (0f - Mathf.Atan2(waterFlow.y, waterFlow.x)) * 57.29578f;
				Quaternion localRotation = Quaternion.Euler(new Vector3(90f, num - 90f, 0f));
				((Component)_waterParticle).transform.localRotation = localRotation;
			}
		}
	}

	private void CheckWaterParticle()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_waterParticle != (Object)null))
		{
			GameObject val = ParticleManager.EmitSync(_particlePath, Vector3.zero, Quaternion.identity);
			if ((Object)(object)val != (Object)null)
			{
				_waterParticle = val.GetComponent<ParticleSystem>();
			}
		}
	}

	public void Stop()
	{
		if (!((Object)(object)_waterParticle == (Object)null))
		{
			_waterParticle.Stop();
			_waterParticle.Clear();
			_waterParticle = null;
		}
	}
}
