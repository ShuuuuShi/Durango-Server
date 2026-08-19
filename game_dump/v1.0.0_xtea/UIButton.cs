using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button")]
public class UIButton : UIButtonColor
{
	public static UIButton current;

	public bool dragHighlight;

	public string hoverSprite;

	public string pressedSprite;

	public string disabledSprite;

	public Sprite hoverSprite2D;

	public Sprite pressedSprite2D;

	public Sprite disabledSprite2D;

	public bool pixelSnap;

	public List<EventDelegate> onClick = new List<EventDelegate>();

	[NonSerialized]
	private UISprite mSprite;

	[NonSerialized]
	private UI2DSprite mSprite2D;

	[NonSerialized]
	private string mNormalSprite;

	[NonSerialized]
	private Sprite mNormalSprite2D;

	public override bool isEnabled
	{
		get
		{
			if (!((Behaviour)this).enabled)
			{
				return false;
			}
			Collider component = ((Component)this).gameObject.GetComponent<Collider>();
			if (Object.op_Implicit((Object)(object)component) && component.enabled)
			{
				return true;
			}
			Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
			return Object.op_Implicit((Object)(object)component2) && ((Behaviour)component2).enabled;
		}
		set
		{
			if (isEnabled == value)
			{
				return;
			}
			Collider component = ((Component)this).gameObject.GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				component.enabled = value;
				UIButton[] components = ((Component)this).GetComponents<UIButton>();
				UIButton[] array = components;
				foreach (UIButton uIButton in array)
				{
					uIButton.SetState((!value) ? State.Disabled : State.Normal, immediate: false);
				}
				return;
			}
			Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Behaviour)component2).enabled = value;
				UIButton[] components2 = ((Component)this).GetComponents<UIButton>();
				UIButton[] array2 = components2;
				foreach (UIButton uIButton2 in array2)
				{
					uIButton2.SetState((!value) ? State.Disabled : State.Normal, immediate: false);
				}
			}
			else
			{
				((Behaviour)this).enabled = value;
			}
		}
	}

	public string normalSprite
	{
		get
		{
			if (!mInitDone)
			{
				OnInit();
			}
			return mNormalSprite;
		}
		set
		{
			if (!mInitDone)
			{
				OnInit();
			}
			if ((Object)(object)mSprite != (Object)null && !string.IsNullOrEmpty(mNormalSprite) && mNormalSprite == mSprite.spriteName)
			{
				mNormalSprite = value;
				SetSprite(value);
				NGUITools.SetDirty((Object)(object)mSprite);
				return;
			}
			mNormalSprite = value;
			if (mState == State.Normal)
			{
				SetSprite(value);
			}
		}
	}

	public Sprite normalSprite2D
	{
		get
		{
			if (!mInitDone)
			{
				OnInit();
			}
			return mNormalSprite2D;
		}
		set
		{
			if (!mInitDone)
			{
				OnInit();
			}
			if ((Object)(object)mSprite2D != (Object)null && (Object)(object)mNormalSprite2D == (Object)(object)mSprite2D.sprite2D)
			{
				mNormalSprite2D = value;
				SetSprite(value);
				NGUITools.SetDirty((Object)(object)mSprite);
				return;
			}
			mNormalSprite2D = value;
			if (mState == State.Normal)
			{
				SetSprite(value);
			}
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		mSprite = mWidget as UISprite;
		mSprite2D = mWidget as UI2DSprite;
		if ((Object)(object)mSprite != (Object)null)
		{
			mNormalSprite = mSprite.spriteName;
		}
		if ((Object)(object)mSprite2D != (Object)null)
		{
			mNormalSprite2D = mSprite2D.sprite2D;
		}
	}

	protected override void OnEnable()
	{
		if (isEnabled)
		{
			if (mInitDone)
			{
				OnHover((Object)(object)UICamera.hoveredObject == (Object)(object)((Component)this).gameObject);
			}
		}
		else
		{
			SetState(State.Disabled, immediate: true);
		}
	}

	protected override void OnDragOver()
	{
		if (isEnabled && (dragHighlight || (Object)(object)UICamera.currentTouch.pressed == (Object)(object)((Component)this).gameObject))
		{
			base.OnDragOver();
		}
	}

	protected override void OnDragOut()
	{
		if (isEnabled && (dragHighlight || (Object)(object)UICamera.currentTouch.pressed == (Object)(object)((Component)this).gameObject))
		{
			base.OnDragOut();
		}
	}

	protected virtual void OnClick()
	{
		if ((Object)(object)current == (Object)null && isEnabled && UICamera.currentTouchID != -2 && UICamera.currentTouchID != -3)
		{
			current = this;
			EventDelegate.Execute(onClick);
			current = null;
		}
	}

	public override void SetState(State state, bool immediate)
	{
		base.SetState(state, immediate);
		if ((Object)(object)mSprite != (Object)null)
		{
			switch (state)
			{
			case State.Normal:
				SetSprite(mNormalSprite);
				break;
			case State.Hover:
				SetSprite((!string.IsNullOrEmpty(hoverSprite)) ? hoverSprite : mNormalSprite);
				break;
			case State.Pressed:
				SetSprite(pressedSprite);
				break;
			case State.Disabled:
				SetSprite(disabledSprite);
				break;
			}
		}
		else if ((Object)(object)mSprite2D != (Object)null)
		{
			switch (state)
			{
			case State.Normal:
				SetSprite(mNormalSprite2D);
				break;
			case State.Hover:
				SetSprite((!((Object)(object)hoverSprite2D == (Object)null)) ? hoverSprite2D : mNormalSprite2D);
				break;
			case State.Pressed:
				SetSprite(pressedSprite2D);
				break;
			case State.Disabled:
				SetSprite(disabledSprite2D);
				break;
			}
		}
	}

	protected void SetSprite(string sp)
	{
		if ((Object)(object)mSprite != (Object)null && !string.IsNullOrEmpty(sp) && mSprite.spriteName != sp)
		{
			mSprite.spriteName = sp;
			if (pixelSnap)
			{
				mSprite.MakePixelPerfect();
			}
		}
	}

	protected void SetSprite(Sprite sp)
	{
		if ((Object)(object)sp != (Object)null && (Object)(object)mSprite2D != (Object)null && (Object)(object)mSprite2D.sprite2D != (Object)(object)sp)
		{
			mSprite2D.sprite2D = sp;
			if (pixelSnap)
			{
				mSprite2D.MakePixelPerfect();
			}
		}
	}
}
