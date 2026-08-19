using System.Collections.Generic;
using Durango.Utils;
using Shared.Battle;
using UnityEngine;

namespace Durango.Render.Particle;

public class InjuryEffectEmitter
{
	private struct InjuryParticleInfo
	{
		public readonly BodyPart BodyPart;

		public readonly string ParticlePath;

		public readonly string SoundEvent;

		public InjuryParticleInfo(BodyPart bodyPart, string particlePath, string soundEvent)
		{
			BodyPart = bodyPart;
			ParticlePath = particlePath;
			SoundEvent = soundEvent;
		}
	}

	private const string HeadInjuryParticle = "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Teeth_01.prefab";

	private const string BodyInjuryParticle = "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Viscus_01.prefab";

	private const string LegInjuryParticle = "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Leg_01.prefab";

	private const string TailInjuryParticle = "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Tail_01.prefab";

	private readonly Dictionary<string, InjuryParticleInfo> _particleInfo = new Dictionary<string, InjuryParticleInfo>
	{
		{
			"head_injury",
			new InjuryParticleInfo(BodyPart.Head, "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Teeth_01.prefab", "hit_parts_broken_teeth")
		},
		{
			"body_injury",
			new InjuryParticleInfo(BodyPart.Body, "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Viscus_01.prefab", "hit_parts_broken_viscus")
		},
		{
			"leg_injury",
			new InjuryParticleInfo(BodyPart.Leg, "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Leg_01.prefab", "hit_parts_broken_leg")
		},
		{
			"tail_injury",
			new InjuryParticleInfo(BodyPart.Tail, "Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Tail_01.prefab", "hit_parts_broken_tail")
		}
	};

	private AnimalBehavior _curTargetAnimal;

	public InjuryEffectEmitter()
	{
		ParticleManager.Cache("Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Teeth_01.prefab");
		ParticleManager.Cache("Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Viscus_01.prefab");
		ParticleManager.Cache("Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Leg_01.prefab");
		ParticleManager.Cache("Particle/Prefabs/Durango_01/05_UIEffect/FX_UIEffect_PartsBroken_Tail_01.prefab");
		GameSystem<StatusEffectSystem>.Instance().StatusEffectAdded += StatusEffectAdded;
	}

	public void SetTarget(string targetId)
	{
		GameObject gameObject = Singleton<ObjectManager>.Instance().FindObject(targetId);
		AnimalBehavior curTargetAnimal = ((!(gameObject != null)) ? null : gameObject.GetComponent<AnimalBehavior>());
		_curTargetAnimal = curTargetAnimal;
	}

	private void StatusEffectAdded(string entityId, string effectId)
	{
		if (!(_curTargetAnimal == null) && !(entityId != _curTargetAnimal.EntityId) && _particleInfo.TryGetValue(effectId, out var value))
		{
			Transform transform = _curTargetAnimal.GetBodyPartTransform(value.BodyPart);
			if (transform == null)
			{
				transform = _curTargetAnimal.Bip001Transform;
			}
			Vector3 pos = transform.position + Vector3.up * 150f;
			ParticleManager.Emit(value.ParticlePath, pos, Quaternion.identity);
			SoundManager.PlayEvent(value.SoundEvent);
		}
	}
}
