using Durango.Render.Particle;
using UnityEngine;

namespace Durango.UI;

public class PlayerLevelUpRewardWidget : TweenerRewardWidget
{
	[SerializeField]
	private ParticleType _particle;

	[SerializeField]
	private Vector3 _particleOffset;

	protected override void OnInit()
	{
		base.OnInit();
		ParticleManager.Cache(_particle);
	}

	protected override void Play()
	{
		base.Play();
		ParticleManager.EmitFollow(_particle, _particleOffset, Quaternion.identity, base.transform);
	}
}
