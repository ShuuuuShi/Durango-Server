using System.Collections;
using UnityEngine;

namespace Durango.UI;

public class EventBuffStatusEffectIcon : StatusEffectIcon
{
	public override Vector3 Position
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			base.transform.localPosition = value;
		}
	}

	public override void PlayFadeIn(Vector3 targetPos)
	{
		if (!base.IsPlayingEffect)
		{
			base.transform.localPosition = targetPos;
			StartCoroutine(CoFadeIn());
		}
	}

	private IEnumerator CoFadeIn()
	{
		base.IsPlayingEffect = true;
		float startTime = Time.time;
		while (true)
		{
			float timer = Time.time - startTime;
			float ratio = timer / 1f;
			SetAlpha(Mathf.Pow(ratio, 3f));
			if (timer > 1f)
			{
				break;
			}
			yield return null;
		}
		base.IsPlayingEffect = false;
		if (base.Index < 0)
		{
			PlayFadeOut();
		}
		else
		{
			OnFinishFadeEffect();
		}
	}
}
