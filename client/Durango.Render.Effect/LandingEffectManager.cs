using System;
using Durango.Render.Particle;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Render.Effect;

public class LandingEffectManager : Singleton<LandingEffectManager>
{
	[Serializable]
	public class LandingEffect
	{
		[EnumList(typeof(AnimationEventInfo.LandingEffectSize), false, 0, -1)]
		public EffectSet[] EffectSets;
	}

	public enum ParticleSize
	{
		Small,
		Medium,
		Large,
		Count
	}

	[EnumList(typeof(Biome), true, 0, -1)]
	[SerializeField]
	public LandingEffect[] LandingEffects;

	protected override void OnAwake()
	{
		int num = LandingEffects.Length;
		for (int i = 0; i < num; i++)
		{
			EffectSet[] effectSets = LandingEffects[i].EffectSets;
			foreach (EffectSet effectSet in effectSets)
			{
				SoundManager.PrepareEvent(effectSet.Sound);
				ParticleManager.Cache(effectSet.Particle);
			}
		}
	}

	public EffectSet GetEffectSet(Biome biome, AnimationEventInfo.LandingEffectSize particleSize)
	{
		if (LandingEffects == null || LandingEffects.Length <= (int)biome || biome < Biome.TemperateForest)
		{
			return null;
		}
		return LandingEffects[(int)biome].EffectSets[(int)particleSize];
	}
}
