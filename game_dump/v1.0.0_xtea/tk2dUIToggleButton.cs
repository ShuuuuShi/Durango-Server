using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIToggleButton")]
public class tk2dUIToggleButton : tk2dUIBaseItemControl
{
	public GameObject offStateGO;

	public GameObject onStateGO;

	public bool activateOnPress;

	[SerializeField]
	private bool isOn = true;

	private bool isInToggleGroup;

	public string SendMessageOnToggleMethodName = string.Empty;

	public bool IsOn
	{
		get
		{
			return isOn;
		}
		set
		{
			if (isOn != value)
			{
				isOn = value;
				SetState();
				if (this.OnToggle != null)
				{
					this.OnToggle(this);
				}
			}
		}
	}

	public bool IsInToggleGroup
	{
		get
		{
			return isInToggleGroup;
		}
		set
		{
			isInToggleGroup = value;
		}
	}

	public event Action<tk2dUIToggleButton> OnToggle;

	private void Start()
	{
		SetState();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnClick += ButtonClick;
			uiItem.OnDown += ButtonDown;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnClick -= ButtonClick;
			uiItem.OnDown -= ButtonDown;
		}
	}

	private void ButtonClick()
	{
		if (!activateOnPress)
		{
			ButtonToggle();
		}
	}

	private void ButtonDown()
	{
		if (activateOnPress)
		{
			ButtonToggle();
		}
	}

	private void ButtonToggle()
	{
		if (!isOn || !isInToggleGroup)
		{
			isOn = !isOn;
			SetState();
			if (this.OnToggle != null)
			{
				this.OnToggle(this);
			}
			DoSendMessage(SendMessageOnToggleMethodName, this);
		}
	}

	private void SetState()
	{
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(offStateGO, !isOn);
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(onStateGO, isOn);
	}
}
