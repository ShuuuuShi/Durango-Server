using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class VisibleController : MonoBehaviour
{
	private static readonly HashSet<VisibleController> Components;

	[SerializeField]
	private VisibleType _flag;

	private UIRect _uiRect;

	private readonly HashSet<string> _visibleKeys = new HashSet<string>();

	private float _animationBeginTime;

	private float _animationDuration;

	private float _animationStartValue;

	public bool Visible { get; private set; }

	public VisibleType Flag => _flag;

	public event Action<bool> Changed;

	static VisibleController()
	{
		Components = new HashSet<VisibleController>();
		GameManager.Reset += delegate
		{
			Components.Clear();
		};
	}

	public static void Hide(VisibleType flag, bool hide, string key = null, float duration = 0f)
	{
		foreach (VisibleController component in Components)
		{
			if ((component._flag & flag) != 0)
			{
				component.SetVisible(!hide, key, duration);
			}
		}
	}

	public static void HideExceptFor(VisibleType flag, bool hide, string key = null, float duration = 0f)
	{
		foreach (VisibleController component in Components)
		{
			if ((component._flag & flag) == 0)
			{
				component.SetVisible(!hide, key, duration);
			}
		}
	}

	public static void Hide([NotNull] Predicate<VisibleController> func, bool hide, string key = null, float duration = 0f)
	{
		foreach (VisibleController component in Components)
		{
			if (func(component))
			{
				component.SetVisible(!hide, key, duration);
			}
		}
	}

	public void HideExceptForMe(bool hide, string key = null, float duration = 0f)
	{
		foreach (VisibleController component in Components)
		{
			if (!(component == this))
			{
				component.SetVisible(!hide, key, duration);
			}
		}
	}

	private void Awake()
	{
		Visible = true;
		_uiRect = GetComponent<UIRect>();
		if (_uiRect == null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			Components.Add(this);
		}
	}

	private void OnDestroy()
	{
		Components.Remove(this);
	}

	private void Update()
	{
		float time = Time.time;
		float num = _animationBeginTime + _animationDuration - time;
		if (num <= 0f || _animationDuration <= 0f)
		{
			_uiRect.visible = Visible;
			base.enabled = false;
		}
		else
		{
			float t = num / _animationDuration;
			_uiRect.visibleRatio = Mathf.Lerp((!Visible) ? 0f : 1f, _animationStartValue, t);
		}
	}

	public void SetVisible(bool visible, string key, float duration = 0f)
	{
		if (visible)
		{
			if (!string.IsNullOrEmpty(key))
			{
				_visibleKeys.Remove(key);
			}
			if (_visibleKeys.Count > 0)
			{
				return;
			}
		}
		else if (!string.IsNullOrEmpty(key))
		{
			_visibleKeys.Add(key);
		}
		Visible = visible;
		if (duration > 0f)
		{
			_animationBeginTime = Time.time;
			_animationDuration = duration;
			_animationStartValue = _uiRect.visibleRatio;
			base.enabled = true;
		}
		else
		{
			_uiRect.visible = Visible;
			base.enabled = false;
		}
		if (this.Changed != null)
		{
			this.Changed(Visible);
		}
	}
}
