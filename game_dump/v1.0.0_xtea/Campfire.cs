using System.Collections;
using UnityEngine;

public class Campfire : MonoBehaviour
{
	public GameObject _fireEffectTemplate;

	public tk2dSprite _bodySprite;

	private ParticleSystem _fireParticle;

	private IEnumerator coFire()
	{
		yield return (object)new WaitForSeconds(10f);
		float alpha2 = 2f;
		while (true)
		{
			alpha2 -= Time.deltaTime;
			Color color2 = _fireParticle.startColor;
			color2.a = alpha2 * 0.5f;
			_fireParticle.startColor = color2;
			((Component)this).GetComponent<AudioSource>().volume = alpha2 * 0.5f;
			if (alpha2 <= 0f)
			{
				break;
			}
			yield return null;
		}
		Object.Destroy((Object)(object)((Component)_fireParticle).gameObject);
		_bodySprite.spriteId = _bodySprite.GetSpriteIdByName("campfire_burnout");
		((Component)this).GetComponent<AudioSource>().Stop();
		yield return (object)new WaitForSeconds(2f);
		alpha2 = 1f;
		while (true)
		{
			alpha2 -= Time.deltaTime;
			Color color = _bodySprite.color;
			color.a = alpha2;
			_bodySprite.color = color;
			if (alpha2 <= 0f)
			{
				break;
			}
			yield return null;
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void StartFire()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(_fireEffectTemplate);
		val.transform.parent = ((Component)this).transform;
		val.transform.localPosition = Vector3.zero;
		_fireParticle = val.GetComponent<ParticleSystem>();
		_bodySprite.spriteId = _bodySprite.GetSpriteIdByName("campfire_burning");
		((Component)this).GetComponent<AudioSource>().Play();
		((MonoBehaviour)this).StartCoroutine("coFire");
	}
}
