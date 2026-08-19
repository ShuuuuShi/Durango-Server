using System;
using System.Collections.Generic;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;
using UnityEngine;

namespace Durango.Render.Effect;

public class IntegratedEffect : MonoBehaviour
{
	public enum Liquid
	{
		Water,
		Lava
	}

	[Serializable]
	public class BiomeEffects
	{
		public Biome Biome;

		[SerializeField]
		public List<WeightedRandomEffect> RandomEffects = new List<WeightedRandomEffect>();
	}

	[Serializable]
	public class WaterDepthEffects
	{
		public TerrainWater.WaterDepthLevel WaterDepthLevel;

		[SerializeField]
		public List<WeightedRandomEffect> RandomEffects = new List<WeightedRandomEffect>();
	}

	[Serializable]
	public class WeightedRandomEffect : WeightedCandidate
	{
		public EffectSet Effect;
	}

	private static readonly Dictionary<string, IntegratedEffect> CachedEffects = new Dictionary<string, IntegratedEffect>();

	[SerializeField]
	private List<WeightedRandomEffect> _defaultEffects = new List<WeightedRandomEffect>();

	[SerializeField]
	private List<BiomeEffects> _effectsByBiome = new List<BiomeEffects>();

	[SerializeField]
	private List<WaterDepthEffects> _waterDepthEffects = new List<WaterDepthEffects>();

	[SerializeField]
	private List<WaterDepthEffects> _lavaDepthEffects = new List<WaterDepthEffects>();

	public static void Precache(string fullPath)
	{
		if (!string.IsNullOrEmpty(fullPath) && !IsCached(fullPath))
		{
			RequestIntegratedEffect(fullPath, delegate(IntegratedEffect integratedEffect)
			{
				integratedEffect.PrecacheIntrenal();
			});
		}
	}

	public static void Emit(string assetPath, Biome biome, Vector3 pos, Quaternion rotation, Transform followingParent = null, bool comeForwardToCamera = false, bool groundDecal = false, TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.WaterDepthLevel.Land, Vector3 scale = default(Vector3))
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return;
		}
		RequestIntegratedEffect(assetPath, delegate(IntegratedEffect integratedEffect)
		{
			EffectSet effectSet = integratedEffect.SelectEffectSet(biome, waterDepthLevel);
			if (effectSet.Particle.Path != null)
			{
				if (followingParent != null)
				{
					ParticleManager.EmitFollow(effectSet.Particle, pos, rotation, followingParent, useLocalPosition: false, comeForwardToCamera, groundDecal, scale);
				}
				else
				{
					ParticleManager.Emit(effectSet.Particle, pos, rotation, comeForwardToCamera, groundDecal, scale);
				}
			}
			SoundManager.PlayEvent(effectSet.Sound, SoundPosition.Fix(pos));
		});
	}

	private static bool IsCached(string fullPath)
	{
		return CachedEffects.Get(fullPath) != null;
	}

	private void PrecacheIntrenal()
	{
		PrecacheEffects(_defaultEffects);
		for (int i = 0; i < _effectsByBiome.Count; i++)
		{
			if (_effectsByBiome[i] != null)
			{
				PrecacheEffects(_effectsByBiome[i].RandomEffects);
			}
		}
		PrecacheLiquidEffects(_waterDepthEffects);
		PrecacheLiquidEffects(_lavaDepthEffects);
	}

	private void PrecacheLiquidEffects(List<WaterDepthEffects> liquidDepthEffects)
	{
		for (int i = 0; i < liquidDepthEffects.Count; i++)
		{
			if (liquidDepthEffects[i] != null)
			{
				PrecacheEffects(liquidDepthEffects[i].RandomEffects);
			}
		}
	}

	private static void PrecacheEffects(List<WeightedRandomEffect> randomEffects)
	{
		if (randomEffects != null)
		{
			for (int i = 0; i < randomEffects.Count; i++)
			{
				ParticleManager.Cache(randomEffects[i].Effect.Particle);
				SoundManager.PrepareEvent(randomEffects[i].Effect.Sound);
			}
		}
	}

	private EffectSet SelectEffectSet(Biome biome = Biome.Invalid, TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.WaterDepthLevel.Land)
	{
		List<WeightedRandomEffect> list = ResolveEffects(biome, waterDepthLevel);
		if (list == null)
		{
			return null;
		}
		return WeightedCandidate.Select(list)?.Effect;
	}

	private List<WeightedRandomEffect> ResolveEffects(Biome biome, TerrainWater.WaterDepthLevel waterDepthLevel)
	{
		if (waterDepthLevel != 0)
		{
			List<WaterDepthEffects> liquidDepthEffects = GetLiquidDepthEffects((biome == Biome.Lava) ? Liquid.Lava : Liquid.Water);
			for (int i = 0; i < liquidDepthEffects.Count; i++)
			{
				if (waterDepthLevel == liquidDepthEffects[i].WaterDepthLevel)
				{
					return liquidDepthEffects[i].RandomEffects;
				}
			}
		}
		for (int j = 0; j < _effectsByBiome.Count; j++)
		{
			if (biome == _effectsByBiome[j].Biome)
			{
				return _effectsByBiome[j].RandomEffects;
			}
		}
		return _defaultEffects;
	}

	[NotNull]
	private List<WaterDepthEffects> GetLiquidDepthEffects(Liquid type)
	{
		if (type == Liquid.Water)
		{
			return _waterDepthEffects;
		}
		return _lavaDepthEffects;
	}

	private static void RequestIntegratedEffect(string assetPath, Action<IntegratedEffect> onLoaded)
	{
		if (IsCached(assetPath))
		{
			if (onLoaded != null)
			{
				onLoaded(CachedEffects[assetPath]);
			}
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				IntegratedEffect component = ((GameObject)asset).GetComponent<IntegratedEffect>();
				if (!(component == null) && onLoaded != null)
				{
					onLoaded(component);
				}
				CachedEffects[assetPath] = component;
			}
		});
	}

	public static void RequestProperEffectSet(string assetPath, Biome biome, TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.WaterDepthLevel.Land, Action<EffectSet> onLoaded = null)
	{
		RequestIntegratedEffect(assetPath, delegate(IntegratedEffect ie)
		{
			EffectSet effectSet = ie.SelectEffectSet(biome, waterDepthLevel);
			if (effectSet != null && onLoaded != null)
			{
				onLoaded(effectSet);
			}
		});
	}
}
