using System;
using System.Collections;
using UnityEngine;

public class ScreenOrientationController : KSingleton<ScreenOrientationController>
{
	[Flags]
	public enum PortraitLock
	{
		None = 0,
		UI = 1,
		Loading = 2,
		Combat = 4
	}

	public enum PortraitModeUseType
	{
		None,
		Manual,
		Auto
	}

	private enum Orientation
	{
		AutoRotation,
		Portrait,
		Landscape
	}

	private static bool _prevLock;

	private static PortraitLock _portraitModeLock;

	private static PortraitModeUseType _portraitModeType;

	private static ScreenOrientation _prevScreen;

	private static DeviceOrientation _prevInput;

	public static PortraitModeUseType PortraitModeType
	{
		get
		{
			return _portraitModeType;
		}
		set
		{
			_portraitModeType = value;
			OnChangePortraitModeType();
		}
	}

	public event Action<bool> PortraitModeChanged;

	public event Action<bool> ReadyToChange;

	private void OnEnable()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		_prevScreen = Screen.orientation;
	}

	public static void SetPortraitLock(PortraitLock l)
	{
		_portraitModeLock |= l;
		OnChangePortraitModeType();
	}

	public static void SetPortraitUnlock(PortraitLock l)
	{
		_portraitModeLock &= ~l;
		OnChangePortraitModeType();
	}

	private static void OnChangePortraitModeType()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		bool flag = _portraitModeLock != PortraitLock.None;
		if (flag && _prevLock)
		{
			return;
		}
		_prevLock = flag;
		if (flag)
		{
			if (_portraitModeType == PortraitModeUseType.Auto)
			{
				SetOrientation(((int)Screen.orientation == 1) ? Orientation.Portrait : Orientation.Landscape);
			}
			return;
		}
		switch (_portraitModeType)
		{
		case PortraitModeUseType.Auto:
			SetOrientation(Orientation.AutoRotation);
			break;
		case PortraitModeUseType.Manual:
			break;
		default:
			SetOrientation(Orientation.Landscape);
			break;
		}
	}

	private static void SetOrientation(Orientation orientation)
	{
		if (KSingleton<ScreenOrientationController>.Exist())
		{
			((MonoBehaviour)KSingleton<ScreenOrientationController>.Instance()).StartCoroutine(CoSetOrientation(orientation));
		}
	}

	private static IEnumerator CoSetOrientation(Orientation orientation)
	{
		switch (orientation)
		{
		case Orientation.AutoRotation:
			Screen.autorotateToPortrait = true;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.orientation = (ScreenOrientation)5;
			break;
		case Orientation.Portrait:
			Screen.autorotateToPortrait = true;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.orientation = (ScreenOrientation)1;
			break;
		default:
			Screen.autorotateToPortrait = false;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.orientation = (ScreenOrientation)5;
			break;
		}
		yield return null;
	}

	public static void SetManualPortraitMode(bool isPortrait)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		if (_portraitModeType == PortraitModeUseType.Manual)
		{
			int x = DeviceInfo.CurrentScreenSize.x;
			int y = DeviceInfo.CurrentScreenSize.y;
			if (isPortrait)
			{
				SetOrientation(Orientation.Portrait);
				Screen.SetResolution(Mathf.Min(x, y), Mathf.Max(x, y), true);
			}
			else
			{
				Screen.orientation = (ScreenOrientation)(((int)_prevInput != 3) ? 4 : 3);
				Screen.SetResolution(Mathf.Max(x, y), Mathf.Min(x, y), true);
				SetOrientation(Orientation.Landscape);
			}
		}
	}

	private void Update()
	{
		CheckPortraitModeChanged();
		CheckManualPortraitMode();
	}

	private void CheckPortraitModeChanged()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		ScreenOrientation orientation = Screen.orientation;
		bool flag = (int)_prevScreen == 1 || (int)_prevScreen == 2;
		bool flag2 = (int)orientation == 1 || (int)orientation == 2;
		if (flag != flag2 && this.PortraitModeChanged != null)
		{
			this.PortraitModeChanged(flag2);
		}
		_prevScreen = orientation;
	}

	private void CheckManualPortraitMode()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected I4, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		if (_portraitModeType != PortraitModeUseType.Manual || _portraitModeLock != 0)
		{
			return;
		}
		DeviceOrientation deviceOrientation = Input.deviceOrientation;
		if (_prevInput == deviceOrientation)
		{
			return;
		}
		_prevInput = deviceOrientation;
		DeviceOrientation val = deviceOrientation;
		switch (val - 1)
		{
		case 2:
		case 3:
			if ((int)_prevScreen == 1 && this.ReadyToChange != null)
			{
				this.ReadyToChange(obj: false);
			}
			break;
		case 0:
			if ((int)_prevScreen != 1 && this.ReadyToChange != null)
			{
				this.ReadyToChange(obj: true);
			}
			break;
		case 1:
			break;
		}
	}
}
