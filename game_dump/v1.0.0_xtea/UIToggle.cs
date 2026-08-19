using System;
using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Toggle")]
public class UIToggle : UIWidgetContainer
{
	public delegate bool Validate(bool choice);

	public static BetterList<UIToggle> list = new BetterList<UIToggle>();

	public static UIToggle current;

	public int group;

	public UIWidget activeSprite;

	public bool invertSpriteState;

	public Animation activeAnimation;

	public Animator animator;

	public UITweener tween;

	public bool startsActive;

	public bool instantTween;

	public bool optionCanBeNone;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	public Validate validator;

	[HideInInspector]
	[SerializeField]
	private UISprite checkSprite;

	[HideInInspector]
	[SerializeField]
	private Animation checkAnimation;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string functionName = "OnActivate";

	[HideInInspector]
	[SerializeField]
	private bool startsChecked;

	private bool mIsActive = true;

	private bool mStarted;

	public bool value
	{
		get
		{
			return (!mStarted) ? startsActive : mIsActive;
		}
		set
		{
			if (!mStarted)
			{
				startsActive = value;
			}
			else if (group == 0 || value || optionCanBeNone || !mStarted)
			{
				Set(value);
			}
		}
	}

	public bool isColliderEnabled
	{
		get
		{
			Collider component = ((Component)this).GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				return component.enabled;
			}
			Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
			return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
		}
	}

	[Obsolete("Use 'value' instead")]
	public bool isChecked
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public static UIToggle GetActiveToggle(int group)
	{
		for (int i = 0; i < list.size; i++)
		{
			UIToggle uIToggle = list[i];
			if ((Object)(object)uIToggle != (Object)null && uIToggle.group == group && uIToggle.mIsActive)
			{
				return uIToggle;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		list.Add(this);
	}

	private void OnDisable()
	{
		list.Remove(this);
	}

	public void Start()
	{
		if (mStarted)
		{
			return;
		}
		if (startsChecked)
		{
			startsChecked = false;
			startsActive = true;
		}
		if (!Application.isPlaying)
		{
			if ((Object)(object)checkSprite != (Object)null && (Object)(object)activeSprite == (Object)null)
			{
				activeSprite = checkSprite;
				checkSprite = null;
			}
			if ((Object)(object)checkAnimation != (Object)null && (Object)(object)activeAnimation == (Object)null)
			{
				activeAnimation = checkAnimation;
				checkAnimation = null;
			}
			if (Application.isPlaying && (Object)(object)activeSprite != (Object)null)
			{
				activeSprite.alpha = (invertSpriteState ? ((!startsActive) ? 1f : 0f) : ((!startsActive) ? 0f : 1f));
			}
			if (EventDelegate.IsValid(onChange))
			{
				eventReceiver = null;
				functionName = null;
			}
		}
		else
		{
			mIsActive = !startsActive;
			mStarted = true;
			bool flag = instantTween;
			instantTween = true;
			Set(startsActive);
			instantTween = flag;
		}
	}

	private void OnClick()
	{
		if (((Behaviour)this).enabled && isColliderEnabled && UICamera.currentTouchID != -2)
		{
			value = !value;
		}
	}

	public void Set(bool state, bool notify = true)
	{
		if (validator != null && !validator(state))
		{
			return;
		}
		if (!mStarted)
		{
			mIsActive = state;
			startsActive = state;
			if ((Object)(object)activeSprite != (Object)null)
			{
				activeSprite.alpha = (invertSpriteState ? ((!state) ? 1f : 0f) : ((!state) ? 0f : 1f));
			}
		}
		else
		{
			if (mIsActive == state)
			{
				return;
			}
			if (group != 0 && state)
			{
				int num = 0;
				int size = list.size;
				while (num < size)
				{
					UIToggle uIToggle = list[num];
					if ((Object)(object)uIToggle != (Object)(object)this && uIToggle.group == group)
					{
						uIToggle.Set(state: false);
					}
					if (list.size != size)
					{
						size = list.size;
						num = 0;
					}
					else
					{
						num++;
					}
				}
			}
			mIsActive = state;
			if ((Object)(object)activeSprite != (Object)null)
			{
				if (instantTween || !NGUITools.GetActive((Behaviour)(object)this))
				{
					activeSprite.alpha = (invertSpriteState ? ((!mIsActive) ? 1f : 0f) : ((!mIsActive) ? 0f : 1f));
				}
				else
				{
					TweenAlpha.Begin(((Component)activeSprite).gameObject, 0.15f, invertSpriteState ? ((!mIsActive) ? 1f : 0f) : ((!mIsActive) ? 0f : 1f));
				}
			}
			if (notify && (Object)(object)current == (Object)null)
			{
				UIToggle uIToggle2 = current;
				current = this;
				if (EventDelegate.IsValid(onChange))
				{
					EventDelegate.Execute(onChange);
				}
				else if ((Object)(object)eventReceiver != (Object)null && !string.IsNullOrEmpty(functionName))
				{
					eventReceiver.SendMessage(functionName, (object)mIsActive, (SendMessageOptions)1);
				}
				current = uIToggle2;
			}
			if ((Object)(object)animator != (Object)null)
			{
				ActiveAnimation activeAnimation = ActiveAnimation.Play(animator, null, state ? Direction.Forward : Direction.Reverse, EnableCondition.IgnoreDisabledState, DisableCondition.DoNotDisable);
				if ((Object)(object)activeAnimation != (Object)null && (instantTween || !NGUITools.GetActive((Behaviour)(object)this)))
				{
					activeAnimation.Finish();
				}
			}
			else if ((Object)(object)this.activeAnimation != (Object)null)
			{
				ActiveAnimation activeAnimation2 = ActiveAnimation.Play(this.activeAnimation, null, state ? Direction.Forward : Direction.Reverse, EnableCondition.IgnoreDisabledState, DisableCondition.DoNotDisable);
				if ((Object)(object)activeAnimation2 != (Object)null && (instantTween || !NGUITools.GetActive((Behaviour)(object)this)))
				{
					activeAnimation2.Finish();
				}
			}
			else
			{
				if (!((Object)(object)tween != (Object)null))
				{
					return;
				}
				bool active = NGUITools.GetActive((Behaviour)(object)this);
				if (tween.tweenGroup != 0)
				{
					UITweener[] componentsInChildren = ((Component)tween).GetComponentsInChildren<UITweener>(true);
					int i = 0;
					for (int num2 = componentsInChildren.Length; i < num2; i++)
					{
						UITweener uITweener = componentsInChildren[i];
						if (uITweener.tweenGroup == tween.tweenGroup)
						{
							uITweener.Play(state);
							if (instantTween || !active)
							{
								uITweener.tweenFactor = ((!state) ? 0f : 1f);
							}
						}
					}
				}
				else
				{
					tween.Play(state);
					if (instantTween || !active)
					{
						tween.tweenFactor = ((!state) ? 0f : 1f);
					}
				}
			}
		}
	}
}
