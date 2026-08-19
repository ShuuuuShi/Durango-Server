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
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			string text = NGUITools.KeyToCaption(keyCode);
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
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = mList.Count; i < count; i++)
		{
			UIKeyBinding uIKeyBinding = mList[i];
			if ((Object)(object)uIKeyBinding != (Object)null && uIKeyBinding.keyCode == key)
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
		UIInput component = ((Component)this).GetComponent<UIInput>();
		mIsInput = (Object)(object)component != (Object)null;
		if ((Object)(object)component != (Object)null)
		{
			EventDelegate.Add(component.onSubmit, OnSubmit);
		}
	}

	protected virtual void OnSubmit()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
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
			if (UICamera.GetKey((KeyCode)308) || UICamera.GetKey((KeyCode)307))
			{
				return true;
			}
		}
		else if (modifier == Modifier.Control)
		{
			if (UICamera.GetKey((KeyCode)306) || UICamera.GetKey((KeyCode)305))
			{
				return true;
			}
		}
		else if (modifier == Modifier.Shift)
		{
			if (UICamera.GetKey((KeyCode)304) || UICamera.GetKey((KeyCode)303))
			{
				return true;
			}
		}
		else if (modifier == Modifier.None)
		{
			return !UICamera.GetKey((KeyCode)308) && !UICamera.GetKey((KeyCode)307) && !UICamera.GetKey((KeyCode)306) && !UICamera.GetKey((KeyCode)305) && !UICamera.GetKey((KeyCode)304) && !UICamera.GetKey((KeyCode)303);
		}
		return false;
	}

	protected virtual void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (UICamera.inputHasFocus || (int)keyCode == 0 || !IsModifierActive())
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
					UICamera.selectedObject = ((Component)this).gameObject;
				}
				mIgnoreUp = false;
			}
			else if (mPress)
			{
				UICamera.hoveredObject = ((Component)this).gameObject;
			}
		}
		if (flag2)
		{
			mPress = false;
		}
	}

	protected virtual void OnBindingPress(bool pressed)
	{
		UICamera.Notify(((Component)this).gameObject, "OnPress", pressed);
	}

	protected virtual void OnBindingClick()
	{
		UICamera.Notify(((Component)this).gameObject, "OnClick", null);
	}

	public override string ToString()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return (modifier == Modifier.None) ? ((Enum)keyCode).ToString() : string.Concat(modifier, "+", keyCode);
	}

	public static bool GetKeyCode(string text, out KeyCode key, out Modifier modifier)
	{
		key = (KeyCode)0;
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
				modifier = (Modifier)(int)Enum.Parse(typeof(Modifier), array[0]);
				key = (KeyCode)(int)Enum.Parse(typeof(KeyCode), array[1]);
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
				key = (KeyCode)(int)Enum.Parse(typeof(KeyCode), text);
			}
			catch (Exception)
			{
				return false;
			}
		}
		return true;
	}
}
