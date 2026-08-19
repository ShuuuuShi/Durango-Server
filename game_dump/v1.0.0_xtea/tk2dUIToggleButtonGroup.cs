using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIToggleButtonGroup")]
public class tk2dUIToggleButtonGroup : MonoBehaviour
{
	[SerializeField]
	private tk2dUIToggleButton[] toggleBtns;

	public GameObject sendMessageTarget;

	[SerializeField]
	private int selectedIndex;

	private tk2dUIToggleButton selectedToggleButton;

	public string SendMessageOnChangeMethodName = string.Empty;

	public tk2dUIToggleButton[] ToggleBtns => toggleBtns;

	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			if (selectedIndex != value)
			{
				selectedIndex = value;
				SetToggleButtonUsingSelectedIndex();
			}
		}
	}

	public tk2dUIToggleButton SelectedToggleButton
	{
		get
		{
			return selectedToggleButton;
		}
		set
		{
			ButtonToggle(value);
		}
	}

	public event Action<tk2dUIToggleButtonGroup> OnChange;

	protected virtual void Awake()
	{
		Setup();
	}

	protected void Setup()
	{
		tk2dUIToggleButton[] array = toggleBtns;
		foreach (tk2dUIToggleButton tk2dUIToggleButton2 in array)
		{
			if ((Object)(object)tk2dUIToggleButton2 != (Object)null)
			{
				tk2dUIToggleButton2.IsInToggleGroup = true;
				tk2dUIToggleButton2.IsOn = false;
				tk2dUIToggleButton2.OnToggle += ButtonToggle;
			}
		}
		SetToggleButtonUsingSelectedIndex();
	}

	public void AddNewToggleButtons(tk2dUIToggleButton[] newToggleBtns)
	{
		ClearExistingToggleBtns();
		toggleBtns = newToggleBtns;
		Setup();
	}

	private void ClearExistingToggleBtns()
	{
		if (toggleBtns != null && toggleBtns.Length > 0)
		{
			tk2dUIToggleButton[] array = toggleBtns;
			foreach (tk2dUIToggleButton tk2dUIToggleButton2 in array)
			{
				tk2dUIToggleButton2.IsInToggleGroup = false;
				tk2dUIToggleButton2.OnToggle -= ButtonToggle;
				tk2dUIToggleButton2.IsOn = false;
			}
		}
	}

	private void SetToggleButtonUsingSelectedIndex()
	{
		tk2dUIToggleButton tk2dUIToggleButton2 = null;
		if (selectedIndex >= 0 && selectedIndex < toggleBtns.Length)
		{
			tk2dUIToggleButton2 = toggleBtns[selectedIndex];
			tk2dUIToggleButton2.IsOn = true;
		}
		else
		{
			tk2dUIToggleButton2 = null;
			selectedIndex = -1;
			ButtonToggle(tk2dUIToggleButton2);
		}
	}

	private void ButtonToggle(tk2dUIToggleButton toggleButton)
	{
		if (!((Object)(object)toggleButton == (Object)null) && !toggleButton.IsOn)
		{
			return;
		}
		tk2dUIToggleButton[] array = toggleBtns;
		foreach (tk2dUIToggleButton tk2dUIToggleButton2 in array)
		{
			if ((Object)(object)tk2dUIToggleButton2 != (Object)(object)toggleButton)
			{
				tk2dUIToggleButton2.IsOn = false;
			}
		}
		if ((Object)(object)toggleButton != (Object)(object)selectedToggleButton)
		{
			selectedToggleButton = toggleButton;
			SetSelectedIndexFromSelectedToggleButton();
			if (this.OnChange != null)
			{
				this.OnChange(this);
			}
			if ((Object)(object)sendMessageTarget != (Object)null && SendMessageOnChangeMethodName.Length > 0)
			{
				sendMessageTarget.SendMessage(SendMessageOnChangeMethodName, (object)this, (SendMessageOptions)0);
			}
		}
	}

	private void SetSelectedIndexFromSelectedToggleButton()
	{
		selectedIndex = -1;
		for (int i = 0; i < toggleBtns.Length; i++)
		{
			tk2dUIToggleButton tk2dUIToggleButton2 = toggleBtns[i];
			if ((Object)(object)tk2dUIToggleButton2 == (Object)(object)selectedToggleButton)
			{
				selectedIndex = i;
				break;
			}
		}
	}
}
