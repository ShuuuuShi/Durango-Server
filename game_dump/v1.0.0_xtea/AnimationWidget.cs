using System;
using System.Collections.Generic;
using UnityEngine;

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
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
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
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localPosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			SetPosition(value);
		}
	}

	public Vector3 Scale
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localScale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			SetScale(value);
		}
	}

	public Color Color
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Widget.color;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
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
		((Behaviour)tweener).enabled = false;
		Widget.alpha = alpha;
		if (_deactiveWhenFadeout && alpha == 0f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void SetPosition(Vector3 pos, bool useTween = true)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		TweenPosition tweener = GetTweener<TweenPosition>();
		if (useTween && tweener.duration > 0f)
		{
			tweener.from = ((Component)this).transform.localPosition;
			tweener.to = pos;
			tweener.tweenFactor = 0f;
			tweener.PlayForward();
		}
		else
		{
			((Behaviour)tweener).enabled = false;
			((Component)this).transform.localPosition = pos;
		}
	}

	public void SetScale(Vector3 scale, bool useTween = true)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		TweenScale tweener = GetTweener<TweenScale>();
		if (useTween && tweener.duration > 0f)
		{
			tweener.from = ((Component)this).transform.localScale;
			tweener.to = scale;
			tweener.tweenFactor = 0f;
			tweener.PlayForward();
		}
		else
		{
			((Behaviour)tweener).enabled = false;
			((Component)this).transform.localScale = scale;
		}
	}

	public void SetColor(Color color, bool useTween = true)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
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
			((Behaviour)tweener).enabled = false;
			Widget.color = color;
		}
	}

	private void OnFinishedTweenAlpha()
	{
		if (_deactiveWhenFadeout && Widget.alpha == 0f)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public T GetTweener<T>() where T : UITweener
	{
		Type typeFromHandle = typeof(T);
		if (!TweenDict.TryGetValue(typeFromHandle, out var value))
		{
			value = ((Component)this).gameObject.AddMissingComponent<T>();
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
		if ((Object)(object)obj.GetComponent<UIWidget>() == (Object)null)
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
