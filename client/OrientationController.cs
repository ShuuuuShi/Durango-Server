using System;
using System.Collections;
using Durango.Utils;
using UnityEngine;

public static class OrientationController
{
	public enum Orientation
	{
		Landscape,
		Portrait,
		AutoRotation
	}

	[Flags]
	public enum RotationLock
	{
		None = 0,
		Loading = 1,
		UI = 2,
		Battle = 4
	}

	private static RotationLock _rotationLock;

	private static Orientation _orientation;

	private static Orientation _targetOrientation;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InstallEvent()
	{
		GameManager.Started += delegate
		{
			_rotationLock = RotationLock.None;
			SetOrientation(_targetOrientation);
		};
	}

	public static void LockRotation(RotationLock l)
	{
		_rotationLock |= l;
		OnRotationLockChanged();
	}

	public static void UnlockRotation(RotationLock l)
	{
		_rotationLock &= ~l;
		OnRotationLockChanged();
	}

	public static void SetTargetOrientation(Orientation orientation, bool update)
	{
		_targetOrientation = orientation;
		if (update)
		{
			SetOrientation(_targetOrientation);
		}
	}

	private static void OnRotationLockChanged()
	{
		if (!GameManager.IsPrologueMode && _orientation == Orientation.AutoRotation)
		{
			if (_rotationLock != 0)
			{
				SetAutorotateProperty((Screen.width <= Screen.height) ? Orientation.Portrait : Orientation.Landscape);
			}
			else
			{
				SetAutorotateProperty(Orientation.AutoRotation);
			}
		}
	}

	public static void SetOrientation(Orientation orientation, ScreenOrientation screen = ScreenOrientation.Unknown)
	{
		if (_orientation == orientation)
		{
			return;
		}
		_orientation = orientation;
		if (screen == ScreenOrientation.Unknown)
		{
			DeviceOrientation deviceOrientation = Input.deviceOrientation;
			switch (deviceOrientation)
			{
			case DeviceOrientation.Portrait:
			case DeviceOrientation.PortraitUpsideDown:
			case DeviceOrientation.LandscapeLeft:
			case DeviceOrientation.LandscapeRight:
				screen = (ScreenOrientation)deviceOrientation;
				break;
			case DeviceOrientation.FaceUp:
			case DeviceOrientation.FaceDown:
				screen = Screen.orientation;
				break;
			}
		}
		switch (orientation)
		{
		case Orientation.Portrait:
			if (screen != ScreenOrientation.Portrait && screen != ScreenOrientation.PortraitUpsideDown)
			{
				screen = ScreenOrientation.Portrait;
			}
			break;
		case Orientation.Landscape:
			if (screen != ScreenOrientation.LandscapeLeft && screen != ScreenOrientation.LandscapeRight)
			{
				screen = ScreenOrientation.LandscapeLeft;
			}
			break;
		}
		Singleton<GameManager>.Instance().StartCoroutine(CoSetOrientation(orientation, screen));
	}

	private static IEnumerator CoSetOrientation(Orientation orientation, ScreenOrientation screenOrientation)
	{
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.IPhonePlayer)
		{
			yield return new WaitForEndOfFrame();
		}
		if (screenOrientation != 0)
		{
			Screen.orientation = screenOrientation;
		}
		if (platform == RuntimePlatform.IPhonePlayer)
		{
			yield return new WaitForEndOfFrame();
		}
		SetAutorotateProperty(orientation);
	}

	private static void SetAutorotateProperty(Orientation orienatation)
	{
		Screen.orientation = ScreenOrientation.AutoRotation;
		switch (orienatation)
		{
		case Orientation.AutoRotation:
			Screen.autorotateToPortrait = true;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortraitUpsideDown = true;
			break;
		case Orientation.Landscape:
			Screen.autorotateToPortrait = false;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortraitUpsideDown = false;
			break;
		case Orientation.Portrait:
			Screen.autorotateToPortrait = true;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToPortraitUpsideDown = true;
			break;
		}
	}
}
