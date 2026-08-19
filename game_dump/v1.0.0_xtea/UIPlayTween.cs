using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Play Tween")]
public class UIPlayTween : MonoBehaviour
{
	public static UIPlayTween current;

	public GameObject tweenTarget;

	public int tweenGroup;

	public Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public bool resetIfDisabled;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public bool includeChildren;

	public List<EventDelegate> onFinished = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string callWhenFinished;

	private UITweener[] mTweens;

	private bool mStarted;

	private int mActive;

	private bool mActivated;

	private void Awake()
	{
		if ((Object)(object)eventReceiver != (Object)null && EventDelegate.IsValid(onFinished))
		{
			eventReceiver = null;
			callWhenFinished = null;
		}
	}

	private void Start()
	{
		mStarted = true;
		if ((Object)(object)tweenTarget == (Object)null)
		{
			tweenTarget = ((Component)this).gameObject;
		}
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			OnHover(UICamera.IsHighlighted(((Component)this).gameObject));
		}
		if (UICamera.currentTouch != null)
		{
			if (trigger == Trigger.OnPress || trigger == Trigger.OnPressTrue)
			{
				mActivated = (Object)(object)UICamera.currentTouch.pressed == (Object)(object)((Component)this).gameObject;
			}
			if (trigger == Trigger.OnHover || trigger == Trigger.OnHoverTrue)
			{
				mActivated = (Object)(object)UICamera.currentTouch.current == (Object)(object)((Component)this).gameObject;
			}
		}
		UIToggle component = ((Component)this).GetComponent<UIToggle>();
		if ((Object)(object)component != (Object)null)
		{
			EventDelegate.Add(component.onChange, OnToggle);
		}
	}

	private void OnDisable()
	{
		UIToggle component = ((Component)this).GetComponent<UIToggle>();
		if ((Object)(object)component != (Object)null)
		{
			EventDelegate.Remove(component.onChange, OnToggle);
		}
	}

	private void OnDragOver()
	{
		if (trigger == Trigger.OnHover)
		{
			OnHover(isOver: true);
		}
	}

	private void OnHover(bool isOver)
	{
		if (((Behaviour)this).enabled && (trigger == Trigger.OnHover || (trigger == Trigger.OnHoverTrue && isOver) || (trigger == Trigger.OnHoverFalse && !isOver)))
		{
			mActivated = isOver && trigger == Trigger.OnHover;
			Play(isOver);
		}
	}

	private void OnDragOut()
	{
		if (((Behaviour)this).enabled && mActivated)
		{
			mActivated = false;
			Play(forward: false);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (((Behaviour)this).enabled && (trigger == Trigger.OnPress || (trigger == Trigger.OnPressTrue && isPressed) || (trigger == Trigger.OnPressFalse && !isPressed)))
		{
			mActivated = isPressed && trigger == Trigger.OnPress;
			Play(isPressed);
		}
	}

	private void OnClick()
	{
		if (((Behaviour)this).enabled && trigger == Trigger.OnClick)
		{
			Play(forward: true);
		}
	}

	private void OnDoubleClick()
	{
		if (((Behaviour)this).enabled && trigger == Trigger.OnDoubleClick)
		{
			Play(forward: true);
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (((Behaviour)this).enabled && (trigger == Trigger.OnSelect || (trigger == Trigger.OnSelectTrue && isSelected) || (trigger == Trigger.OnSelectFalse && !isSelected)))
		{
			mActivated = isSelected && trigger == Trigger.OnSelect;
			Play(isSelected);
		}
	}

	private void OnToggle()
	{
		if (((Behaviour)this).enabled && !((Object)(object)UIToggle.current == (Object)null) && (trigger == Trigger.OnActivate || (trigger == Trigger.OnActivateTrue && UIToggle.current.value) || (trigger == Trigger.OnActivateFalse && !UIToggle.current.value)))
		{
			Play(UIToggle.current.value);
		}
	}

	private void Update()
	{
		if (disableWhenFinished == DisableCondition.DoNotDisable || mTweens == null)
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup == tweenGroup)
			{
				if (((Behaviour)uITweener).enabled)
				{
					flag = false;
					break;
				}
				if (uITweener.direction != (Direction)disableWhenFinished)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			if (flag2)
			{
				NGUITools.SetActive(tweenTarget, state: false);
			}
			mTweens = null;
		}
	}

	public void Play(bool forward)
	{
		mActive = 0;
		GameObject val = ((!((Object)(object)tweenTarget == (Object)null)) ? tweenTarget : ((Component)this).gameObject);
		if (!NGUITools.GetActive(val))
		{
			if (ifDisabledOnPlay != EnableCondition.EnableThenPlay)
			{
				return;
			}
			NGUITools.SetActive(val, state: true);
		}
		mTweens = ((!includeChildren) ? val.GetComponents<UITweener>() : val.GetComponentsInChildren<UITweener>());
		if (mTweens.Length == 0)
		{
			if (disableWhenFinished != 0)
			{
				NGUITools.SetActive(tweenTarget, state: false);
			}
			return;
		}
		bool flag = false;
		if (playDirection == Direction.Reverse)
		{
			forward = !forward;
		}
		int i = 0;
		for (int num = mTweens.Length; i < num; i++)
		{
			UITweener uITweener = mTweens[i];
			if (uITweener.tweenGroup != tweenGroup)
			{
				continue;
			}
			if (!flag && !NGUITools.GetActive(val))
			{
				flag = true;
				NGUITools.SetActive(val, state: true);
			}
			mActive++;
			if (playDirection == Direction.Toggle)
			{
				EventDelegate.Add(uITweener.onFinished, OnFinished, oneShot: true);
				uITweener.Toggle();
				continue;
			}
			if (resetOnPlay || (resetIfDisabled && !((Behaviour)uITweener).enabled))
			{
				uITweener.Play(forward);
				uITweener.ResetToBeginning();
			}
			EventDelegate.Add(uITweener.onFinished, OnFinished, oneShot: true);
			uITweener.Play(forward);
		}
	}

	private void OnFinished()
	{
		if (--mActive == 0 && (Object)(object)current == (Object)null)
		{
			current = this;
			EventDelegate.Execute(onFinished);
			if ((Object)(object)eventReceiver != (Object)null && !string.IsNullOrEmpty(callWhenFinished))
			{
				eventReceiver.SendMessage(callWhenFinished, (SendMessageOptions)1);
			}
			eventReceiver = null;
			current = null;
		}
	}
}
