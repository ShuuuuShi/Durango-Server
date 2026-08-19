using System.Collections.Generic;
using AnimationOrTween;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Play Animation")]
[ExecuteInEditMode]
public class UIPlayAnimation : MonoBehaviour
{
	public static UIPlayAnimation current;

	public Animation target;

	public Animator animator;

	public string clipName;

	public Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public bool clearSelection;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public List<EventDelegate> onFinished = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[SerializeField]
	[HideInInspector]
	private string callWhenFinished;

	private bool mStarted;

	private bool mActivated;

	private bool dragHighlight;

	private bool dualState => trigger == Trigger.OnPress || trigger == Trigger.OnHover;

	private void Awake()
	{
		UIButton component = ((Component)this).GetComponent<UIButton>();
		if ((Object)(object)component != (Object)null)
		{
			dragHighlight = component.dragHighlight;
		}
		if ((Object)(object)eventReceiver != (Object)null && EventDelegate.IsValid(onFinished))
		{
			eventReceiver = null;
			callWhenFinished = null;
		}
	}

	private void Start()
	{
		mStarted = true;
		if ((Object)(object)target == (Object)null && (Object)(object)animator == (Object)null)
		{
			animator = ((Component)this).GetComponentInChildren<Animator>();
		}
		if ((Object)(object)animator != (Object)null)
		{
			if (((Behaviour)animator).enabled)
			{
				((Behaviour)animator).enabled = false;
			}
			return;
		}
		if ((Object)(object)target == (Object)null)
		{
			target = ((Component)this).GetComponentInChildren<Animation>();
		}
		if ((Object)(object)target != (Object)null && ((Behaviour)target).enabled)
		{
			((Behaviour)target).enabled = false;
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

	private void OnHover(bool isOver)
	{
		if (((Behaviour)this).enabled && (trigger == Trigger.OnHover || (trigger == Trigger.OnHoverTrue && isOver) || (trigger == Trigger.OnHoverFalse && !isOver)))
		{
			Play(isOver, dualState);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (((Behaviour)this).enabled && UICamera.currentTouchID != -2 && UICamera.currentTouchID != -3 && (trigger == Trigger.OnPress || (trigger == Trigger.OnPressTrue && isPressed) || (trigger == Trigger.OnPressFalse && !isPressed)))
		{
			Play(isPressed, dualState);
		}
	}

	private void OnClick()
	{
		if (UICamera.currentTouchID != -2 && UICamera.currentTouchID != -3 && ((Behaviour)this).enabled && trigger == Trigger.OnClick)
		{
			Play(forward: true, onlyIfDifferent: false);
		}
	}

	private void OnDoubleClick()
	{
		if (UICamera.currentTouchID != -2 && UICamera.currentTouchID != -3 && ((Behaviour)this).enabled && trigger == Trigger.OnDoubleClick)
		{
			Play(forward: true, onlyIfDifferent: false);
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (((Behaviour)this).enabled && (trigger == Trigger.OnSelect || (trigger == Trigger.OnSelectTrue && isSelected) || (trigger == Trigger.OnSelectFalse && !isSelected)))
		{
			Play(isSelected, dualState);
		}
	}

	private void OnToggle()
	{
		if (((Behaviour)this).enabled && !((Object)(object)UIToggle.current == (Object)null) && (trigger == Trigger.OnActivate || (trigger == Trigger.OnActivateTrue && UIToggle.current.value) || (trigger == Trigger.OnActivateFalse && !UIToggle.current.value)))
		{
			Play(UIToggle.current.value, dualState);
		}
	}

	private void OnDragOver()
	{
		if (((Behaviour)this).enabled && dualState)
		{
			if ((Object)(object)UICamera.currentTouch.dragged == (Object)(object)((Component)this).gameObject)
			{
				Play(forward: true, onlyIfDifferent: true);
			}
			else if (dragHighlight && trigger == Trigger.OnPress)
			{
				Play(forward: true, onlyIfDifferent: true);
			}
		}
	}

	private void OnDragOut()
	{
		if (((Behaviour)this).enabled && dualState && (Object)(object)UICamera.hoveredObject != (Object)(object)((Component)this).gameObject)
		{
			Play(forward: false, onlyIfDifferent: true);
		}
	}

	private void OnDrop(GameObject go)
	{
		if (((Behaviour)this).enabled && trigger == Trigger.OnPress && (Object)(object)UICamera.currentTouch.dragged != (Object)(object)((Component)this).gameObject)
		{
			Play(forward: false, onlyIfDifferent: true);
		}
	}

	public void Play(bool forward)
	{
		Play(forward, onlyIfDifferent: true);
	}

	public void Play(bool forward, bool onlyIfDifferent)
	{
		if (!Object.op_Implicit((Object)(object)target) && !Object.op_Implicit((Object)(object)animator))
		{
			return;
		}
		if (onlyIfDifferent)
		{
			if (mActivated == forward)
			{
				return;
			}
			mActivated = forward;
		}
		if (clearSelection && (Object)(object)UICamera.selectedObject == (Object)(object)((Component)this).gameObject)
		{
			UICamera.selectedObject = null;
		}
		int num = 0 - playDirection;
		Direction direction = ((!forward) ? ((Direction)num) : playDirection);
		ActiveAnimation activeAnimation = ((!Object.op_Implicit((Object)(object)target)) ? ActiveAnimation.Play(animator, clipName, direction, ifDisabledOnPlay, disableWhenFinished) : ActiveAnimation.Play(target, clipName, direction, ifDisabledOnPlay, disableWhenFinished));
		if ((Object)(object)activeAnimation != (Object)null)
		{
			if (resetOnPlay)
			{
				activeAnimation.Reset();
			}
			for (int i = 0; i < onFinished.Count; i++)
			{
				EventDelegate.Add(activeAnimation.onFinished, OnFinished, oneShot: true);
			}
		}
	}

	public void PlayForward()
	{
		Play(forward: true);
	}

	public void PlayReverse()
	{
		Play(forward: false);
	}

	private void OnFinished()
	{
		if ((Object)(object)current == (Object)null)
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
