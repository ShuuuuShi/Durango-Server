using System.Collections;
using UnityEngine;

public class Firefly : MonoBehaviour
{
	private const float StartTime = 5f / 6f;

	private const float EndTime = 5f / 24f;

	private const float RandomInterval = 1f / 24f;

	private GameObject _fireflyParicle;

	private bool _activated;

	private IEnumerator Start()
	{
		while (true)
		{
			float curNormalizedTime = TimeGauge.GetNormalizedTime();
			float r = Random.value * 2f - 1f;
			curNormalizedTime += r * (1f / 24f);
			if (IsActiveTime(curNormalizedTime))
			{
				ActiveParticle();
			}
			else
			{
				DeactiveParticle();
			}
			float interval = ((!_activated) ? (5f / 6f - curNormalizedTime) : (5f / 24f - curNormalizedTime));
			if (interval < 0f)
			{
				interval += 1f;
			}
			float sleepTime = TimeGauge.GetRealTimeFromNormalizedTime(interval);
			yield return (object)new WaitForSeconds(sleepTime);
		}
	}

	private void ActiveParticle()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_activated = true;
		if (!((Object)(object)_fireflyParicle != (Object)null))
		{
			float num = Random.value - 0.5f;
			float num2 = Random.value - 0.5f;
			Vector3 pos = default(Vector3);
			((Vector3)(ref pos))._002Ector(num * 200f, 0f, num2 * 200f);
			_fireflyParicle = ParticleManager.EmitSync("Particle/FX_Prop_FireFly_01.prefab", pos, Quaternion.identity, ((Component)this).transform);
		}
	}

	private void DeactiveParticle()
	{
		_activated = false;
		if (!((Object)(object)_fireflyParicle == (Object)null))
		{
			ParticleManager.Stop(_fireflyParicle, immediately: false);
			Object.Destroy((Object)(object)_fireflyParicle);
			_fireflyParicle = null;
		}
	}

	private static bool IsActiveTime(float normalizedTime)
	{
		return 5f / 6f < normalizedTime || normalizedTime < 5f / 24f;
	}
}
