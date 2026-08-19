using System;
using System.Collections.Generic;
using EffectData;
using TerrainData;
using UnityEngine;

public class LandingEffectManager : KSingleton<LandingEffectManager>
{
	[Serializable]
	public class LandingEffect
	{
		public EffectSet[] EffectSets;
	}

	public enum ParticleSize
	{
		Small,
		Medium,
		Large,
		Count
	}

	[SerializeField]
	public List<LandingEffect> LandingEffects;

	protected override void OnAwake()
	{
		int count = LandingEffects.Count;
		for (int i = 0; i < count; i++)
		{
			EffectSet[] effectSets = LandingEffects[i].EffectSets;
			foreach (EffectSet effectSet in effectSets)
			{
				SoundManager.Cache(effectSet.Sound);
				ParticleManager.Cache(effectSet.Particle);
			}
		}
	}

	public EffectSet GetEffectSet(Biome biome, int particleSize)
	{
		if (LandingEffects == null || LandingEffects.Count != 15)
		{
			return null;
		}
		return (biome != Biome.Unspecified) ? LandingEffects[(int)biome].EffectSets[particleSize] : null;
	}
}
