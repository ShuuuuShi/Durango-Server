using UnityEngine;

public class ScreenRotateButton : TooltipBase
{
	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void FillData()
	{
	}

	protected override void OnClickWidget()
	{
		Hide(instant: true);
		ScreenOrientationController.SetManualPortraitMode(!UIManager.IsPortraitMode);
	}

	protected override void UpdateLayout()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		DeviceOrientation deviceOrientation = Input.deviceOrientation;
		ScreenOrientation orientation = Screen.orientation;
		if ((int)orientation == 1)
		{
			if ((int)deviceOrientation == 3)
			{
				flag = false;
			}
		}
		else if ((int)orientation == 4)
		{
			flag = false;
		}
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		((Component)component).transform.localEulerAngles = Vector3.forward * ((!flag) ? 180f : 0f);
		component.bottomAnchor.relative = ((!flag) ? 0f : 1f);
		component.bottomAnchor.absolute = (flag ? (-component.height) : 0);
		component.topAnchor.relative = ((!flag) ? 0f : 1f);
		component.topAnchor.absolute = ((!flag) ? component.height : 0);
		((Component)component).gameObject.SetActive(true);
		component.UpdateAnchors();
	}
}
