using System;
using System.Collections.Generic;
using EffectData;
using TerrainData;
using UnityEngine;

public class IntegratedEffect : MonoBehaviour
{
	public enum EffectType
	{
		Random,
		Biome
	}

	[Serializable]
	public class BiomeEffects
	{
		[Serializable]
		public class WeightedRandomEffect : WeightedCandidate
		{
			public EffectSet Effect;
		}

		public Biome Biome;

		[SerializeField]
		public List<WeightedRandomEffect> RandomEffects = new List<WeightedRandomEffect>();
	}

	[SerializeField]
	private EffectType _effectType;

	[SerializeField]
	private List<BiomeEffects> _effectsByBiome = new List<BiomeEffects>();

	[SerializeField]
	private BiomeEffects _defaultEffects = new BiomeEffects();

	private static readonly Dictionary<string, IntegratedEffect> CachedEffects = new Dictionary<string, IntegratedEffect>();

	public EffectType CurEffectType
	{
		get
		{
			return _effectType;
		}
		set
		{
			_effectType = value;
		}
	}

	public List<BiomeEffects> EffectsByBiome => _effectsByBiome;

	public BiomeEffects DefaultEffects => _defaultEffects;

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

	private static bool IsCached(string fullPath)
	{
		return CachedEffects.ContainsKey(fullPath) && (Object)(object)CachedEffects[fullPath] != (Object)null;
	}

	private void PrecacheIntrenal()
	{
		int count = _effectsByBiome.Count;
		for (int i = 0; i < count; i++)
		{
			PrecacheEffects(_effectsByBiome[i]);
		}
		PrecacheEffects(_defaultEffects);
	}

	private static void PrecacheEffects(BiomeEffects effects)
	{
		if (effects != null)
		{
			int count = effects.RandomEffects.Count;
			for (int i = 0; i < count; i++)
			{
				ParticleManager.Cache(effects.RandomEffects[i].Effect.Particle);
				SoundManager.Cache(effects.RandomEffects[i].Effect.Sound);
			}
		}
	}

	private EffectSet SelectEffectSet(Biome biome = Biome.Unspecified)
	{
		BiomeEffects biomeEffects = null;
		switch (_effectType)
		{
		case EffectType.Random:
			biomeEffects = _defaultEffects;
			break;
		case EffectType.Biome:
			biomeEffects = _defaultEffects;
			biomeEffects = GetBiomeEffects(biome, biomeEffects);
			break;
		}
		if (biomeEffects == null)
		{
			return null;
		}
		return WeightedCandidate.Select(biomeEffects.RandomEffects)?.Effect;
	}

	public BiomeEffects GetBiomeEffects(Biome biome, BiomeEffects defaultEffects = null)
	{
		if (biome == Biome.Unspecified)
		{
			return defaultEffects;
		}
		int count = _effectsByBiome.Count;
		for (int i = 0; i < count; i++)
		{
			if (biome == _effectsByBiome[i].Biome)
			{
				return _effectsByBiome[i];
			}
		}
		return defaultEffects;
	}

	public static void RequestIntegratedEffect(string assetPath, Action<IntegratedEffect> onLoaded)
	{
		if (IsCached(assetPath))
		{
			if (onLoaded != null)
			{
				onLoaded(CachedEffects[assetPath]);
			}
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(Object asset)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			if (!(asset == (Object)null))
			{
				GameObject val = (GameObject)asset;
				IntegratedEffect component = val.GetComponent<IntegratedEffect>();
				if (!((Object)(object)component == (Object)null) && onLoaded != null)
				{
					onLoaded(component);
				}
				CachedEffects[assetPath] = component;
			}
		});
	}

	public static void Emit(string assetPath, Biome biome, Vector3 pos, Quaternion rotation, Transform followingParent = null, bool useLocalPosition = true, bool comeForwardToCamera = false, bool groundDecal = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		RequestIntegratedEffect(assetPath, delegate(IntegratedEffect integratedEffect)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			EffectSet effectSet = integratedEffect.SelectEffectSet(biome);
			if (effectSet.Particle.Path != null)
			{
				ParticleManager.Emit(effectSet.Particle, pos, rotation, followingParent, useLocalPosition: false, comeForwardToCamera, groundDecal);
			}
			if (effectSet.Sound.Path != null)
			{
				SoundManager.Play((string)effectSet.Sound, pos, (Transform)null, loop: false, default(SoundManager.PitchRange));
			}
		});
	}
}
