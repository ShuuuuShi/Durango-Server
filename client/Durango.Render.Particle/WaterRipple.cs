using Durango.Terrain;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Render.Particle;

public class WaterRipple
{
	private readonly string _particlePath;

	private readonly bool _isRiver;

	private int _waterParticleId;

	public WaterRipple(string particlePath, bool isRiver = false)
	{
		_isRiver = isRiver;
		_particlePath = particlePath;
		ParticleManager.Cache(_particlePath);
	}

	public void Process(Biome biome, TerrainWater.WaterDepthLevel depthLevel, Vector3 position)
	{
		if (IsPlayable(biome, depthLevel))
		{
			CheckEmit();
			Update(position);
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

	private void Update(Vector3 position)
	{
		if (_waterParticleId == 0)
		{
			return;
		}
		GameObject particleIfLoaded = Singleton<ParticleManager>.Instance().GetParticleIfLoaded(_waterParticleId);
		if (particleIfLoaded != null)
		{
			Vector3 localPosition = position;
			localPosition.y = 0f;
			particleIfLoaded.transform.localPosition = localPosition;
			if (_isRiver)
			{
				Vector2 waterFlow = Singleton<TerrainBase>.Instance().GetWaterFlow(Util.ClientPositionToWorldPosition(position));
				float num = (0f - Mathf.Atan2(waterFlow.y, waterFlow.x)) * 57.29578f;
				Quaternion localRotation = Quaternion.Euler(new Vector3(90f, num - 90f, 0f));
				particleIfLoaded.transform.localRotation = localRotation;
			}
		}
	}

	private void CheckEmit()
	{
		if (_waterParticleId == 0)
		{
			_waterParticleId = ParticleManager.Emit(_particlePath, Vector3.zero, Quaternion.identity);
		}
	}

	public void Stop()
	{
		if (_waterParticleId != 0)
		{
			ParticleManager.Stop(_waterParticleId, immediately: false);
			_waterParticleId = 0;
		}
	}
}
