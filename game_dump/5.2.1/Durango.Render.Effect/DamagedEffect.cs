using System;
using System.Collections.Generic;
using Durango.Model;
using Durango.Render.Particle;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.Render.Effect;

public class DamagedEffect : MonoBehaviour
{
	[Serializable]
	private struct Effect
	{
		public string[] Animation;

		public Particle[] Particles;

		public Audio[] Audios;
	}

	[Serializable]
	private struct Audio
	{
		public SoundEventType Value;

		public float Delay;
	}

	[Serializable]
	private struct Particle
	{
		public ParticleType Value;

		public float Delay;
	}

	[SerializeField]
	private Effect _damagedEffects;

	[SerializeField]
	private Effect _destroyedEffects;

	private readonly List<KeyValuePair<float, ParticleType>> _particles = new List<KeyValuePair<float, ParticleType>>();

	private readonly List<KeyValuePair<float, SoundEventType>> _audios = new List<KeyValuePair<float, SoundEventType>>();

	private AnimatingModel _animationProp;

	private bool _isFoundAnimationProp;

	public void PlayDamaged(Damage damage, [CanBeNull] DamageableEntity attacker)
	{
		Play(_damagedEffects);
	}

	public void PlayDestoryed()
	{
		Play(_destroyedEffects);
	}

	private void Update()
	{
		float time = Time.time;
		for (int num = _particles.Count - 1; num >= 0; num--)
		{
			if (_particles[num].Key < time)
			{
				PlayParticle(_particles[num].Value);
				_particles.RemoveAt(num);
			}
		}
		for (int num2 = _audios.Count - 1; num2 >= 0; num2--)
		{
			if (_audios[num2].Key < time)
			{
				PlayAudio(_audios[num2].Value);
				_audios.RemoveAt(num2);
			}
		}
		if (_particles.Count == 0 && _audios.Count == 0)
		{
			base.enabled = false;
		}
	}

	private void Play(Effect effect)
	{
		int i = 0;
		for (int size = KUtility.GetSize(effect.Animation); i < size; i++)
		{
			PlayAnimation(effect.Animation[i]);
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(effect.Particles); j < size2; j++)
		{
			AddParticle(effect.Particles[j]);
		}
		int k = 0;
		for (int size3 = KUtility.GetSize(effect.Audios); k < size3; k++)
		{
			AddAudio(effect.Audios[k]);
		}
	}

	private void AddParticle(Particle type)
	{
		if (type.Delay > 0f)
		{
			_particles.Add(new KeyValuePair<float, ParticleType>(Time.time + type.Delay, type.Value));
			base.enabled = true;
		}
		else
		{
			PlayParticle(type.Value);
		}
	}

	private void AddAudio(Audio type)
	{
		if (type.Delay > 0f)
		{
			_audios.Add(new KeyValuePair<float, SoundEventType>(Time.time + type.Delay, type.Value));
			base.enabled = true;
		}
		else
		{
			PlayAudio(type.Value);
		}
	}

	private void PlayAnimation(string type)
	{
		if (!string.IsNullOrEmpty(type))
		{
			if (!_isFoundAnimationProp)
			{
				_isFoundAnimationProp = true;
				_animationProp = GetComponentInChildren<AnimatingModel>();
			}
			if (_animationProp != null)
			{
				_animationProp.Play(type, loop: false);
			}
		}
	}

	private void PlayAudio(SoundEventType type)
	{
		SoundManager.PlayEvent(type);
	}

	private void PlayParticle(ParticleType type)
	{
		ParticleManager.Emit(type, base.transform.position, Quaternion.identity);
	}
}
