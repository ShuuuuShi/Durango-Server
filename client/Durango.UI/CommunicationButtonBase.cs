using System;
using System.Collections;
using UnityEngine;

namespace Durango.UI;

public class CommunicationButtonBase : UIWidget
{
	[SerializeField]
	protected UISprite _sprite;

	[SerializeField]
	private UISprite _fill;

	protected Action _clicked;

	private Action _longTouched;

	public virtual bool ToggleOn { get; set; }

	public void Initailize(Action clicked, Action longTouched)
	{
		_clicked = clicked;
		_longTouched = longTouched;
	}

	private void OnLongPress()
	{
		if (_longTouched != null)
		{
			_longTouched();
		}
	}

	public void Set(string spriteName)
	{
		_sprite.spriteName = spriteName;
		UIUtility.ResizeToSquare(_sprite);
	}

	public void StartFillAmount(float time, Func<bool> checkFunc, Action callback)
	{
		StartCoroutine(CoFillAmount(time, checkFunc, callback));
	}

	private IEnumerator CoFillAmount(float time, Func<bool> checkFunc, Action callback)
	{
		_fill.gameObject.SetActive(value: true);
		float beginTime = Time.time;
		while (true)
		{
			float timePassed = Time.time - beginTime;
			float ratio = 1f - Mathf.Min(1f, timePassed / time);
			_fill.fillAmount = ratio;
			bool isEnabled = checkFunc();
			if (ratio <= 0f || !isEnabled || !_fill.gameObject.activeSelf)
			{
				break;
			}
			yield return null;
		}
		callback();
		_fill.gameObject.SetActive(value: false);
	}
}
