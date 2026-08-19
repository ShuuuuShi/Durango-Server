using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class AnimationWidget : MonoBehaviour
{
	[SerializeField]
	private float _duration;

	[SerializeField]
	private float _delay;

	[SerializeField]
	private bool _deactiveWhenFadeout;

	private Dictionary<Type, UITweener> _tweenDict;

	private UIWidget _widget;

	public float Duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = value;
			foreach (UITweener value2 in TweenDict.Values)
			{
				value2.duration = value;
			}
		}
	}

	public float Delay
	{
		get
		{
			return _delay;
		}
		set
		{
			_delay = value;
			foreach (UITweener value2 in TweenDict.Values)
			{
				value2.delay = value;
			}
		}
	}

	public bool DeactiveWhenFadeout
	{
		get
		{
			return _deactiveWhenFadeout;
		}
		set
		{
			_deactiveWhenFadeout = value;
		}
	}

	private Dictionary<Type, UITweener> TweenDict
	{
		get
		{
			if (_tweenDict == null)
			{
				_tweenDict = new Dictionary<Type, UITweener>();
			}
			return _tweenDict;
		}
	}

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public float Alpha
	{
		get
		{
			return Widget.alpha;
		}
		set
		{
			SetAlpha(value);
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			SetPosition(value);
		}
	}

	public Vector3 Scale
	{
		get
		{
			return base.transform.localScale;
		}
		set
		{
			SetScale(value);
		}
	}

	public Color Color
	{
		get
		{
			return Widget.color;
		}
		set
		{
			SetColor(value);
		}
	}

	public void SetAlpha(float alpha, bool useTween = true)
	{
		TweenAlpha tweener = GetTweener<TweenAlpha>();
		if (useTween && tweener.duration > 0f)
		{
			tweener.from = Widget.alpha;
			tweener.to = alpha;
			tweener.tweenFactor = 0f;
			tweener.PlayForward();
			return;
		}
		tweener.enabled = false;
		Widget.alpha = alpha;
		if (_deactiveWhenFadeout && alpha == 0f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetPosition(Vector3 pos, bool useTween = true)
	{
		TweenPosition tweener = GetTweener<TweenPosition>();
		if (useTween && tweener.duration > 0f)
		{
			tweener.from = base.transform.localPosition;
			tweener.to = pos;
			tweener.tweenFactor = 0f;
			tweener.PlayForward();
		}
		else
		{
			tweener.enabled = false;
			base.transform.localPosition = pos;
		}
	}

	public void SetScale(Vector3 scale, bool useTween = true)
	{
		TweenScale tweener = GetTweener<TweenScale>();
		if (useTween && tweener.duration > 0f)
		{
			tweener.from = base.transform.localScale;
			tweener.to = scale;
			tweener.tweenFactor = 0f;
			tweener.PlayForward();
		}
		else
		{
			tweener.enabled = false;
			base.transform.localScale = scale;
		}
	}

	public void SetColor(Color color, bool useTween = true)
	{
		TweenColor tweener = GetTweener<TweenColor>();
		if (useTween && tweener.duration > 0f)
		{
			tweener.from = Widget.color;
			tweener.to = color;
			tweener.tweenFactor = 0f;
			tweener.PlayForward();
		}
		else
		{
			tweener.enabled = false;
			Widget.color = color;
		}
	}

	private void OnFinishedTweenAlpha()
	{
		if (_deactiveWhenFadeout && Widget.alpha == 0f)
		{
			OnFadeOut();
			base.gameObject.SetActive(value: false);
		}
	}

	protected virtual void OnFadeOut()
	{
	}

	public T GetTweener<T>() where T : UITweener
	{
		Type typeFromHandle = typeof(T);
		if (!TweenDict.TryGetValue(typeFromHandle, out var value))
		{
			value = base.gameObject.AddMissingComponent<T>();
			value.duration = _duration;
			value.delay = _delay;
			TweenDict.Add(typeFromHandle, value);
			if (value is TweenAlpha)
			{
				value.AddOnFinished(OnFinishedTweenAlpha);
			}
		}
		return value as T;
	}

	public static AnimationWidget Get(GameObject obj, float duration, float delay = 0f, bool deactiveWhenFadeout = false)
	{
		if (obj.GetComponent<UIWidget>() == null)
		{
			return null;
		}
		AnimationWidget animationWidget = obj.AddMissingComponent<AnimationWidget>();
		animationWidget.Duration = duration;
		animationWidget.Delay = delay;
		animationWidget.DeactiveWhenFadeout = deactiveWhenFadeout;
		return animationWidget;
	}
}
