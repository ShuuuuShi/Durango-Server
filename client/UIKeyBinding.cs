using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Key Binding")]
public class UIKeyBinding : MonoBehaviour
{
	public enum Action
	{
		PressAndClick,
		Select,
		All
	}

	public enum Modifier
	{
		Any,
		Shift,
		Control,
		Alt,
		None
	}

	private static List<UIKeyBinding> mList = new List<UIKeyBinding>();

	public KeyCode keyCode;

	public Modifier modifier;

	public Action action;

	[NonSerialized]
	private bool mIgnoreUp;

	[NonSerialized]
	private bool mIsInput;

	[NonSerialized]
	private bool mPress;

	public string captionText
	{
		get
		{
			string text = InputKeyboard.KeyToCaption(keyCode);
			if (modifier == Modifier.Alt)
			{
				return "Alt+" + text;
			}
			if (modifier == Modifier.Control)
			{
				return "Control+" + text;
			}
			if (modifier == Modifier.Shift)
			{
				return "Shift+" + text;
			}
			return text;
		}
	}

	public static bool IsBound(KeyCode key)
	{
		int i = 0;
		for (int count = mList.Count; i < count; i++)
		{
			UIKeyBinding uIKeyBinding = mList[i];
			if (uIKeyBinding != null && uIKeyBinding.keyCode == key)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void OnEnable()
	{
		mList.Add(this);
	}

	protected virtual void OnDisable()
	{
		mList.Remove(this);
	}

	protected virtual void Start()
	{
		UIInput component = GetComponent<UIInput>();
		mIsInput = component != null;
		if (component != null)
		{
			EventDelegate.Add(component.onSubmit, OnSubmit);
		}
	}

	protected virtual void OnSubmit()
	{
		if (UICamera.currentKey == keyCode && IsModifierActive())
		{
			mIgnoreUp = true;
		}
	}

	protected virtual bool IsModifierActive()
	{
		if (modifier == Modifier.Any)
		{
			return true;
		}
		if (modifier == Modifier.Alt)
		{
			if (UICamera.GetKey(KeyCode.LeftAlt) || UICamera.GetKey(KeyCode.RightAlt))
			{
				return true;
			}
		}
		else if (modifier == Modifier.Control)
		{
			if (UICamera.GetKey(KeyCode.LeftControl) || UICamera.GetKey(KeyCode.RightControl))
			{
				return true;
			}
		}
		else if (modifier == Modifier.Shift)
		{
			if (UICamera.GetKey(KeyCode.LeftShift) || UICamera.GetKey(KeyCode.RightShift))
			{
				return true;
			}
		}
		else if (modifier == Modifier.None)
		{
			return !UICamera.GetKey(KeyCode.LeftAlt) && !UICamera.GetKey(KeyCode.RightAlt) && !UICamera.GetKey(KeyCode.LeftControl) && !UICamera.GetKey(KeyCode.RightControl) && !UICamera.GetKey(KeyCode.LeftShift) && !UICamera.GetKey(KeyCode.RightShift);
		}
		return false;
	}

	protected virtual void Update()
	{
		if (UICamera.inputHasFocus || keyCode == KeyCode.None || !IsModifierActive())
		{
			return;
		}
		bool flag = UICamera.GetKeyDown(keyCode);
		bool flag2 = UICamera.GetKeyUp(keyCode);
		if (flag)
		{
			mPress = true;
		}
		if (action == Action.PressAndClick || action == Action.All)
		{
			if (flag)
			{
				UICamera.currentKey = keyCode;
				OnBindingPress(pressed: true);
			}
			if (mPress && flag2)
			{
				UICamera.currentKey = keyCode;
				OnBindingPress(pressed: false);
				OnBindingClick();
			}
		}
		if ((action == Action.Select || action == Action.All) && flag2)
		{
			if (mIsInput)
			{
				if (!mIgnoreUp && !UICamera.inputHasFocus && mPress)
				{
					UICamera.selectedObject = base.gameObject;
				}
				mIgnoreUp = false;
			}
			else if (mPress)
			{
				UICamera.hoveredObject = base.gameObject;
			}
		}
		if (flag2)
		{
			mPress = false;
		}
	}

	protected virtual void OnBindingPress(bool pressed)
	{
		UICamera.Notify(base.gameObject, "OnPress", pressed);
	}

	protected virtual void OnBindingClick()
	{
		UICamera.Notify(base.gameObject, "OnClick", null);
	}

	public override string ToString()
	{
		return (modifier == Modifier.None) ? keyCode.ToString() : string.Concat(modifier, "+", keyCode);
	}

	public static bool GetKeyCode(string text, out KeyCode key, out Modifier modifier)
	{
		key = KeyCode.None;
		modifier = Modifier.None;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.Contains("+"))
		{
			string[] array = text.Split('+');
			try
			{
				modifier = (Modifier)Enum.Parse(typeof(Modifier), array[0]);
				key = (KeyCode)Enum.Parse(typeof(KeyCode), array[1]);
			}
			catch (Exception)
			{
				return false;
			}
		}
		else
		{
			modifier = Modifier.None;
			try
			{
				key = (KeyCode)Enum.Parse(typeof(KeyCode), text);
			}
			catch (Exception)
			{
				return false;
			}
		}
		return true;
	}
}
