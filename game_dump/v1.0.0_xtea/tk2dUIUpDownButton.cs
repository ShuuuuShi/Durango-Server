using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIUpDownButton")]
public class tk2dUIUpDownButton : tk2dUIBaseItemControl
{
	public GameObject upStateGO;

	public GameObject downStateGO;

	[SerializeField]
	private bool useOnReleaseInsteadOfOnUp;

	private bool isDown;

	public bool UseOnReleaseInsteadOfOnUp => useOnReleaseInsteadOfOnUp;

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
		}
	}

	private void ButtonUp()
	{
		isDown = false;
		SetState();
	}

	private void ButtonDown()
	{
		isDown = true;
		SetState();
	}

	private void SetState()
	{
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(upStateGO, !isDown);
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(downStateGO, isDown);
	}

	public void InternalSetUseOnReleaseInsteadOfOnUp(bool state)
	{
		useOnReleaseInsteadOfOnUp = state;
	}
}
