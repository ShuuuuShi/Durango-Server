using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIHoverItem")]
public class tk2dUIHoverItem : tk2dUIBaseItemControl
{
	public GameObject outStateGO;

	public GameObject overStateGO;

	private bool isOver;

	public string SendMessageOnToggleHoverMethodName = string.Empty;

	public bool IsOver
	{
		get
		{
			return isOver;
		}
		set
		{
			if (isOver != value)
			{
				isOver = value;
				SetState();
				if (this.OnToggleHover != null)
				{
					this.OnToggleHover(this);
				}
				DoSendMessage(SendMessageOnToggleHoverMethodName, this);
			}
		}
	}

	public event Action<tk2dUIHoverItem> OnToggleHover;

	private void Start()
	{
		SetState();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnHoverOver += HoverOver;
			uiItem.OnHoverOut += HoverOut;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnHoverOver -= HoverOver;
			uiItem.OnHoverOut -= HoverOut;
		}
	}

	private void HoverOver()
	{
		IsOver = true;
	}

	private void HoverOut()
	{
		IsOver = false;
	}

	public void SetState()
	{
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(overStateGO, isOver);
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(outStateGO, !isOver);
	}
}
