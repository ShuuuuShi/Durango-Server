using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIUpDownHoverButton")]
public class tk2dUIUpDownHoverButton : tk2dUIBaseItemControl
{
	public GameObject upStateGO;

	public GameObject downStateGO;

	public GameObject hoverOverStateGO;

	[SerializeField]
	private bool useOnReleaseInsteadOfOnUp;

	private bool isDown;

	private bool isHover;

	public string SendMessageOnToggleOverMethodName = string.Empty;

	public bool UseOnReleaseInsteadOfOnUp => useOnReleaseInsteadOfOnUp;

	public bool IsOver
	{
		get
		{
			return isDown || isHover;
		}
		set
		{
			if (value == isDown && !isHover)
			{
				return;
			}
			if (value)
			{
				isHover = true;
				SetState();
				if (this.OnToggleOver != null)
				{
					this.OnToggleOver(this);
				}
			}
			else if (isDown && isHover)
			{
				isDown = false;
				isHover = false;
				SetState();
				if (this.OnToggleOver != null)
				{
					this.OnToggleOver(this);
				}
			}
			else if (isDown)
			{
				isDown = false;
				SetState();
				if (this.OnToggleOver != null)
				{
					this.OnToggleOver(this);
				}
			}
			else
			{
				isHover = false;
				SetState();
				if (this.OnToggleOver != null)
				{
					this.OnToggleOver(this);
				}
			}
			DoSendMessage(SendMessageOnToggleOverMethodName, this);
		}
	}

	public event Action<tk2dUIUpDownHoverButton> OnToggleOver;

	private void Start()
	{
		SetState();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnDown += ButtonDown;
			if (useOnReleaseInsteadOfOnUp)
			{
				uiItem.OnRelease += ButtonUp;
			}
			else
			{
				uiItem.OnUp += ButtonUp;
			}
			uiItem.OnHoverOver += ButtonHoverOver;
			uiItem.OnHoverOut += ButtonHoverOut;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnDown -= ButtonDown;
			if (useOnReleaseInsteadOfOnUp)
			{
				uiItem.OnRelease -= ButtonUp;
			}
			else
			{
				uiItem.OnUp -= ButtonUp;
			}
			uiItem.OnHoverOver -= ButtonHoverOver;
			uiItem.OnHoverOut -= ButtonHoverOut;
		}
	}

	private void ButtonUp()
	{
		if (isDown)
		{
			isDown = false;
			SetState();
			if (!isHover && this.OnToggleOver != null)
			{
				this.OnToggleOver(this);
			}
		}
	}

	private void ButtonDown()
	{
		if (!isDown)
		{
			isDown = true;
			SetState();
			if (!isHover && this.OnToggleOver != null)
			{
				this.OnToggleOver(this);
			}
		}
	}

	private void ButtonHoverOver()
	{
		if (!isHover)
		{
			isHover = true;
			SetState();
			if (!isDown && this.OnToggleOver != null)
			{
				this.OnToggleOver(this);
			}
		}
	}

	private void ButtonHoverOut()
	{
		if (isHover)
		{
			isHover = false;
			SetState();
			if (!isDown && this.OnToggleOver != null)
			{
				this.OnToggleOver(this);
			}
		}
	}

	public void SetState()
	{
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(upStateGO, !isDown && !isHover);
		if ((Object)(object)downStateGO == (Object)(object)hoverOverStateGO)
		{
			tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(downStateGO, isDown || isHover);
			return;
		}
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(downStateGO, isDown);
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(hoverOverStateGO, isHover);
	}

	public void InternalSetUseOnReleaseInsteadOfOnUp(bool state)
	{
		useOnReleaseInsteadOfOnUp = state;
	}
}
