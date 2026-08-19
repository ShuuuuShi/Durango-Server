using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
	private enum ImageType
	{
		IMAGE,
		SCREENSHOT
	}

	public static Action<string> OnScreenshotSaved;

	private static ScreenshotManager instance;

	private static GameObject go;

	private static AndroidJavaClass obj;

	private static AndroidJavaClass androidEnv;

	public static ScreenshotManager Instance
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Invalid comparison between Unknown and I4
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				go = new GameObject();
				((Object)go).name = "ScreenshotManager";
				instance = go.AddComponent<ScreenshotManager>();
				if ((int)Application.platform == 11)
				{
					obj = new AndroidJavaClass("com.secondfury.galleryscreenshot.MainActivity");
					androidEnv = new AndroidJavaClass("android.os.Environment");
				}
			}
			return instance;
		}
	}

	public static event Action<Texture2D> OnScreenshotTaken;

	public static event Action<string> OnImageSaved;

	private void Awake()
	{
		if ((Object)(object)instance != (Object)null && (Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private static string GetPath(string fileName, string albumName)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
		}
		string text = null;
		if ((int)Application.platform == 11)
		{
			string path = Application.persistentDataPath;
			AndroidJavaObject val = ((AndroidJavaObject)androidEnv).CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", new object[1] { "Pictures" });
			try
			{
				path = val.Call<string>("getAbsolutePath", new object[0]);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			string path2 = Path.Combine(albumName, fileName);
			text = Path.Combine(path, path2);
			string directoryName = Path.GetDirectoryName(text);
			Directory.CreateDirectory(directoryName);
		}
		return text;
	}

	public static void SaveScreenshot(string fileName = null, string albumName = "Screenshots", string fileType = "jpeg", [Optional] Rect screenArea)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (screenArea == default(Rect))
		{
			((Rect)(ref screenArea))._002Ector(0f, 0f, (float)Screen.width, (float)Screen.height);
		}
		((MonoBehaviour)Instance).StartCoroutine(Instance.GrabScreenshot(fileName, albumName, fileType, screenArea));
	}

	private IEnumerator GrabScreenshot(string fileName, string albumName, string fileType, Rect screenArea)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		yield return (object)new WaitForEndOfFrame();
		Texture2D texture = new Texture2D((int)((Rect)(ref screenArea)).width, (int)((Rect)(ref screenArea)).height, (TextureFormat)3, false);
		texture.ReadPixels(screenArea, 0, 0);
		texture.Apply();
		byte[] bytes;
		string fileExt;
		if (fileType == "png")
		{
			bytes = texture.EncodeToPNG();
			fileExt = ".png";
		}
		else
		{
			bytes = texture.EncodeToJPG();
			fileExt = ".jpeg";
		}
		if (ScreenshotManager.OnScreenshotTaken != null)
		{
			ScreenshotManager.OnScreenshotTaken(texture);
		}
		else
		{
			Object.Destroy((Object)(object)texture);
		}
		string path = GetPath(fileName, albumName) + fileExt;
		((MonoBehaviour)Instance).StartCoroutine(Instance.Save(bytes, fileName, path, ImageType.SCREENSHOT));
	}

	public static void SaveImage(Texture2D texture, string fileName = null, string albumName = "Screenshots", string fileType = "jpeg")
	{
		Instance.Awake();
		byte[] bytes;
		string text;
		if (fileType == "png")
		{
			bytes = texture.EncodeToPNG();
			text = ".png";
		}
		else
		{
			bytes = texture.EncodeToJPG();
			text = ".jpeg";
		}
		string path = GetPath(fileName, albumName) + text;
		((MonoBehaviour)Instance).StartCoroutine(Instance.Save(bytes, fileName, path, ImageType.IMAGE));
	}

	private IEnumerator Save(byte[] bytes, string fileName, string path, ImageType imageType)
	{
		bool saved = false;
		if ((int)Application.platform == 11)
		{
			File.WriteAllBytes(path, bytes);
			while (!saved)
			{
				saved = ((AndroidJavaObject)obj).CallStatic<bool>("addImageToGallery", new object[1] { path });
				yield return ((MonoBehaviour)Instance).StartCoroutine(Instance.Wait(0.5f));
			}
		}
		switch (imageType)
		{
		case ImageType.IMAGE:
			if (ScreenshotManager.OnImageSaved != null)
			{
				ScreenshotManager.OnImageSaved(path);
			}
			break;
		case ImageType.SCREENSHOT:
			if (OnScreenshotSaved != null)
			{
				OnScreenshotSaved(path);
			}
			break;
		}
	}

	private IEnumerator Wait(float delay)
	{
		float pauseTarget = Time.realtimeSinceStartup + delay;
		while (Time.realtimeSinceStartup < pauseTarget)
		{
			yield return null;
		}
	}
}
