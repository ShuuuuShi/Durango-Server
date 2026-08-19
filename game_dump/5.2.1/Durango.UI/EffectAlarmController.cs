using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class EffectAlarmController : MonoBehaviour
{
	[Serializable]
	private class Effect
	{
		public ParticleType Particle;

		public string ParticleBone;

		public bool ParticleFollow;

		public float ZoomInDelay;

		public float ZoomInRatio;

		public float ZoomInDuration;

		public float ZoomRestoreDuration;

		public float ShakeDelay;

		public float ShakePower;

		public float ShakeDuration;

		public float ShakeDumpRatio;
	}

	[Serializable]
	[EnumType(typeof(EffectType))]
	private class EffectList : EnumKeyList
	{
		[SerializeField]
		private List<Effect> _values;

		public Effect Get(EffectType state)
		{
			int num = IndexOf((int)state);
			if (num == -1)
			{
				return null;
			}
			return _values[num];
		}
	}

	public enum EffectType
	{
		SmallLevelUp,
		LevelUp,
		PetLevelUp
	}

	[SerializeField]
	private EffectList _effectList;

	[ExposedInEditor(null)]
	public void Play(EffectType type)
	{
		Play(PlayerBehavior.LocalPlayer.EntityId, type);
	}

	public void Play(string entityId, EffectType type)
	{
		Effect effect = _effectList.Get(type);
		if (effect != null)
		{
			if (!string.IsNullOrEmpty(effect.Particle.Path))
			{
				ObjectManager.PlayParticle(entityId, effect.Particle, effect.ParticleBone, effect.ParticleFollow);
			}
			if (effect.ZoomInDuration > 0f)
			{
				Singleton<CameraController>.Instance().Delay(effect.ZoomInDelay).ZoomRatio(effect.ZoomInRatio, effect.ZoomInDuration)
					.Next()
					.ZoomRatio(1f, effect.ZoomRestoreDuration);
			}
			if (effect.ShakeDuration > 0f)
			{
				Singleton<CameraShaker>.Instance().Shake(effect.ShakePower, effect.ShakePower, 0f, effect.ZoomInDuration, effect.ShakeDumpRatio, effect.ShakeDelay);
			}
		}
	}
}
