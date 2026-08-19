using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

public class CommunicationButton : MonoBehaviour
{
	[SerializeField]
	private UISprite _sprite;

	[SerializeField]
	private UISprite _fill;

	private bool _isPressed;

	private Action _clicked;

	private Action _longTouched;

	public void Initailize(Action clicked, Action longTouched)
	{
		_clicked = clicked;
		_longTouched = longTouched;
		UIEventListener.Get(((Component)this).gameObject).onPress = Pressed;
	}

	private void Pressed(GameObject go, bool press)
	{
		if (press)
		{
			((MonoBehaviour)this).Invoke("LongTouched", 0.5f);
			_isPressed = true;
		}
		else if (_isPressed)
		{
			((MonoBehaviour)this).CancelInvoke("LongTouched");
			if (_clicked != null)
			{
				_clicked();
			}
			_isPressed = false;
		}
	}

	[UsedImplicitly]
	private void LongTouched()
	{
		if (_isPressed)
		{
			_isPressed = false;
			if (_longTouched != null)
			{
				_longTouched();
			}
		}
	}

	public void Set(string spriteName)
	{
		_sprite.spriteName = spriteName;
		UIUtility.ResizeToSquare(_sprite);
	}

	public void StartFillAmount(float time, Func<bool> checkFunc, Action callback)
	{
		((MonoBehaviour)this).StartCoroutine(CoFillAmount(time, checkFunc, callback));
	}

	private IEnumerator CoFillAmount(float time, Func<bool> checkFunc, Action callback)
	{
		((Component)_fill).gameObject.SetActive(true);
		float beginTime = Time.time;
		while (true)
		{
			float timePassed = Time.time - beginTime;
			float ratio = 1f - Mathf.Min(1f, timePassed / time);
			_fill.fillAmount = ratio;
			bool isEnabled = checkFunc();
			if (ratio <= 0f || !isEnabled || !((Component)_fill).gameObject.activeSelf)
			{
				break;
			}
			yield return null;
		}
		callback();
		((Component)_fill).gameObject.SetActive(false);
	}
}
