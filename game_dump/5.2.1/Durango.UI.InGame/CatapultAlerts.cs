using System.Collections.Generic;
using Durango.Render.Particle;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.InGame;

public class CatapultAlerts : AreaOfEffectAlert
{
	private struct AlartSturct
	{
		public int Id;

		public float HideAt;

		public int ParticleId;
	}

	[SerializeField]
	private ParticleType _shortParticle;

	[SerializeField]
	private float _shortParticleBaseSize;

	[SerializeField]
	private ParticleType _longParticle;

	[SerializeField]
	private float _longParticleBaseSize;

	private List<AlartSturct> _particleDeactiveAt = new List<AlartSturct>();

	public override int ShowCircle(Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt)
	{
		int num2;
		if (hideAt - showAt > 2f)
		{
			float num = ((!(_longParticleBaseSize > 0f)) ? 1f : (radius / _longParticleBaseSize));
			num2 = ParticleManager.Emit(_longParticle, position, Quaternion.identity, comeForwardToCamera: false, groundDecal: false, Vector3.one * num);
		}
		else
		{
			float num3 = ((!(_shortParticleBaseSize > 0f)) ? 1f : (radius / _shortParticleBaseSize));
			num2 = ParticleManager.Emit(_shortParticle, position, Quaternion.identity, comeForwardToCamera: false, groundDecal: false, Vector3.one * num3);
		}
		_particleDeactiveAt.Add(new AlartSturct
		{
			Id = AreaOfEffectVisualizer.GetNextId(),
			ParticleId = num2,
			HideAt = hideAt
		});
		base.enabled = true;
		return num2;
	}

	public override int ShowArc(Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt)
	{
		return -1;
	}

	public override int ShowRect(Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt)
	{
		return -1;
	}

	public override void Stop(int id, float delay)
	{
		for (int num = _particleDeactiveAt.Count - 1; num >= 0; num--)
		{
			if (id == _particleDeactiveAt[num].Id)
			{
				if (delay > 0f)
				{
					float num2 = AreaOfEffectVisualizer.Now();
					AlartSturct value = _particleDeactiveAt[num];
					value.HideAt = Mathf.Min(num2 + delay, value.HideAt);
					_particleDeactiveAt[num] = value;
				}
				else
				{
					_particleDeactiveAt.RemoveAt(num);
					ParticleManager.Stop(id);
				}
				break;
			}
		}
	}

	public override void Move(int id, Vector3 position)
	{
		GameObject particleIfLoaded = Singleton<ParticleManager>.Instance().GetParticleIfLoaded(id);
		if (!(particleIfLoaded == null))
		{
			particleIfLoaded.transform.position = position;
		}
	}

	private void Update()
	{
		float time = Time.time;
		for (int num = _particleDeactiveAt.Count - 1; num >= 0; num--)
		{
			if (_particleDeactiveAt[num].HideAt < time)
			{
				int particleId = _particleDeactiveAt[num].ParticleId;
				_particleDeactiveAt.RemoveAt(num);
				ParticleManager.Stop(particleId);
			}
		}
		if (_particleDeactiveAt.Count == 0)
		{
			base.enabled = false;
		}
	}
}
