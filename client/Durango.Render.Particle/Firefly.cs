using System.Collections;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Render.Particle;

public class Firefly : MonoBehaviour
{
	private const float StartTime = 5f / 6f;

	private const float EndTime = 5f / 24f;

	private const float RandomInterval = 1f / 24f;

	private int _particleId;

	private bool _isActiveTime;

	private static bool _particleAllowed;

	private IEnumerator Start()
	{
		while (true)
		{
			float curNormalizedTime = TimeGauge.GetNormalizedTime();
			float r = Random.value * 2f - 1f;
			curNormalizedTime += r * (1f / 24f);
			_isActiveTime = IsActiveTime(curNormalizedTime);
			UpdateParticle();
			float interval = ((!_isActiveTime) ? (5f / 6f - curNormalizedTime) : (5f / 24f - curNormalizedTime));
			if (interval < 0f)
			{
				interval += 1f;
			}
			float sleepTime = TimeGauge.GetRealTimeFromNormalizedTime(interval);
			yield return new WaitForSeconds(sleepTime);
		}
	}

	private void EmitParticle()
	{
		if (_particleId == 0)
		{
			float num = Random.value - 0.5f;
			float num2 = Random.value - 0.5f;
			Vector3 pos = new Vector3(num * 200f, 0f, num2 * 200f);
			_particleId = ParticleManager.EmitFollow("Particle/FX_Prop_FireFly_01.prefab", pos, Quaternion.identity, base.transform, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, default(Vector3), null, reusable: true, limit: false);
		}
	}

	private void StopParticle()
	{
		if (_particleId != 0)
		{
			ParticleManager.Stop(_particleId, immediately: false);
			_particleId = 0;
		}
	}

	private static bool IsActiveTime(float normalizedTime)
	{
		return 5f / 6f < normalizedTime || normalizedTime < 5f / 24f;
	}

	private void OnDisable()
	{
		_isActiveTime = false;
		StopParticle();
	}

	private void UpdateParticle()
	{
		if (_isActiveTime && _particleAllowed)
		{
			EmitParticle();
		}
		else
		{
			StopParticle();
		}
	}

	public static void ChangeFireflyOption(bool allow)
	{
		_particleAllowed = allow;
		if (Singleton<TerrainBase>.HasInstance())
		{
			Singleton<TerrainBase>.Instance().gameObject.BroadcastMessage("OnFireflyOptionChanged", SendMessageOptions.DontRequireReceiver);
		}
	}

	[UsedImplicitly]
	private void OnFireflyOptionChanged()
	{
		UpdateParticle();
	}
}
