using System.Collections;
using UnityEngine;

public class HitEffectPanel : MonoBehaviour
{
	private UISprite[] _sprite;

	public float duration = 1f;

	public Color color = Color.white;

	private void Start()
	{
		int childCount = ((Component)this).transform.childCount;
		_sprite = new UISprite[childCount];
		for (int i = 0; i < childCount; i++)
		{
			_sprite[i] = ((Component)((Component)this).transform.GetChild(i)).GetComponent<UISprite>();
		}
		for (int j = 0; j < _sprite.Length; j++)
		{
			((Component)_sprite[j]).gameObject.SetActive(false);
			_sprite[j].alpha = 0f;
		}
	}

	public void StartHitEffect()
	{
		((MonoBehaviour)this).StopCoroutine("coStartHitEffect");
		((MonoBehaviour)this).StartCoroutine("coStartHitEffect");
	}

	private IEnumerator coStartHitEffect()
	{
		for (int k = 0; k < _sprite.Length; k++)
		{
			((Component)_sprite[k]).gameObject.SetActive(true);
			_sprite[k].alpha = 1f;
			_sprite[k].color = color;
		}
		float beginTime = Time.time;
		while (Time.time - beginTime < duration)
		{
			float dt = Time.time - beginTime;
			float t = dt / duration;
			for (int j = 0; j < _sprite.Length; j++)
			{
				_sprite[j].alpha = Mathf.Lerp(1f, 0f, t);
			}
			yield return (object)new WaitForSeconds(0.1f);
		}
		for (int i = 0; i < _sprite.Length; i++)
		{
			((Component)_sprite[i]).gameObject.SetActive(false);
			_sprite[i].alpha = 0f;
		}
	}
}
