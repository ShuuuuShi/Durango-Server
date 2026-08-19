using System;
using System.Collections;
using UnityEngine;

public class DamageLabelIndicator : MonoBehaviour
{
	public Action<DamageLabelIndicator> OnFinished;

	public Vector3 Dir { get; set; }

	public void Begin(string damage, Color color)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).StartCoroutine(coBegin(damage, color));
	}

	private IEnumerator coBegin(string damage, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		UISpriteLabel damageLabel = ((Component)this).GetComponent<UISpriteLabel>();
		damageLabel.text = string.Format("[{1}]{0}[-]", damage, NGUIText.EncodeColor(color));
		damageLabel.alpha = 0f;
		Transform t = ((Component)this).transform;
		Vector3 initPos = t.position;
		float[] stateTime = DamageIndicator.StateTime;
		float timer = 0f;
		int state = 0;
		float scale = 1f;
		Vector3 dir = Dir;
		if (dir == Vector3.zero)
		{
			dir = new Vector3(Random.value * 2f - 1f, 0f, Random.value * 2f - 1f);
		}
		((Vector3)(ref dir)).Normalize();
		dir.y = DamageIndicator.ThrowRatio;
		dir *= DamageIndicator.PopPower;
		while (state < stateTime.Length)
		{
			float stateDuration = stateTime[state];
			float ratio = timer / stateDuration;
			float sqrtRatio = Mathf.Sqrt(ratio);
			switch (state)
			{
			case 0:
				scale = 0.5f + 0.5f * sqrtRatio;
				damageLabel.alpha = sqrtRatio;
				break;
			case 1:
				scale = 1f;
				break;
			case 2:
				damageLabel.alpha = 1f - sqrtRatio;
				break;
			}
			t.position += dir * Time.deltaTime;
			dir.y -= DamageIndicator.Gravity * Time.deltaTime;
			Vector3 delta = t.position - initPos;
			delta.y = 0f;
			float angle = Mathf.Atan2(delta.z, delta.x);
			float depth = Mathf.Cos(angle - (float)Math.PI / 4f) * ((Vector3)(ref delta)).magnitude;
			t.localScale = Vector3.one * scale * (1f - depth / DamageIndicator.FowDepth);
			timer += Time.deltaTime;
			if (timer > stateDuration)
			{
				timer = 0f;
				state++;
			}
			yield return null;
		}
		if (OnFinished != null)
		{
			OnFinished(this);
		}
	}
}
