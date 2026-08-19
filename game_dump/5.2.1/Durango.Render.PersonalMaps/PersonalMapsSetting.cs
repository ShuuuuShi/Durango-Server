using System;
using Durango.Render.Camera;
using Durango.Render.Screen;
using Durango.Render.Water;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.PersonalMaps;

public static class PersonalMapsSetting
{
	private struct BackupSettings
	{
		public GameObject IngameUIGameObject;

		public GameObject LightMaskGameObject;

		public float Zoom;

		public float OrthographicSize;

		public float TimeScale;
	}

	private const string _lightMaskGameObjectPath = "PlayerController/LightMask";

	private const string _ingameUIGameObjectPath = "InGameUI";

	private const int _orthographicSize = 1000;

	private static BackupSettings _backup;

	public static void ApplyCaptureSettings(UnityEngine.Camera camera, bool captureMode)
	{
		try
		{
			if (captureMode)
			{
				CreateBackup(camera);
			}
			Singleton<PlayerController>.Instance().enabled = !captureMode;
			Singleton<CustomColorCorrectionEffect>.Instance().UseOverlayAndDirtTexture = !captureMode;
			if (_backup.IngameUIGameObject != null)
			{
				_backup.IngameUIGameObject.SetActive(!captureMode);
			}
			if (_backup.LightMaskGameObject != null)
			{
				_backup.LightMaskGameObject.SetActive(!captureMode);
			}
			Singleton<CameraController>.Instance().Zoom((!captureMode) ? _backup.Zoom : 0.1f, 0f);
			camera.orthographic = captureMode;
			camera.orthographicSize = ((!captureMode) ? _backup.OrthographicSize : 1000f);
			Time.timeScale = ((!captureMode) ? _backup.TimeScale : 0f);
			Singleton<River>.Instance().enabled = !captureMode;
			Lake.FindLake().enabled = !captureMode;
			Ocean.FindOcean().enabled = !captureMode;
			Singleton<CustomColorCorrectionEffect>.Instance().DisableIndoorEffect = captureMode;
		}
		catch (Exception ex)
		{
			Debug.LogError("PersonalMaps ApplyCaptureSettings() failed: " + ex.Message);
		}
	}

	private static void CreateBackup(UnityEngine.Camera camera)
	{
		if (_backup.IngameUIGameObject == null)
		{
			_backup.IngameUIGameObject = GameObject.Find("InGameUI");
		}
		if (_backup.LightMaskGameObject == null)
		{
			_backup.LightMaskGameObject = GameObject.Find("PlayerController/LightMask");
		}
		_backup.Zoom = Singleton<MainCamera>.Instance().Zoom;
		_backup.OrthographicSize = camera.orthographicSize;
		_backup.TimeScale = Time.timeScale;
	}
}
