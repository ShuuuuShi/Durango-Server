using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{
	private enum ImageType
	{
		IMAGE,
		SCREENSHOT
	}

	[CompilerGenerated]
	private sealed class _003CGrabScreenshot_003Ed__15 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Rect screenArea;

		public string fileType;

		public string fileName;

		public string albumName;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGrabScreenshot_003Ed__15(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForEndOfFrame();
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				Texture2D texture2D = new Texture2D((int)screenArea.width, (int)screenArea.height, TextureFormat.RGB24, mipmap: false);
				texture2D.ReadPixels(screenArea, 0, 0);
				texture2D.Apply();
				byte[] bytes;
				string text;
				if (fileType == "png")
				{
					bytes = texture2D.EncodeToPNG();
					text = ".png";
				}
				else
				{
					bytes = texture2D.EncodeToJPG();
					text = ".jpeg";
				}
				if (ScreenshotManager.OnScreenshotTaken != null)
				{
					ScreenshotManager.OnScreenshotTaken(texture2D);
				}
				else
				{
					UnityEngine.Object.Destroy(texture2D);
				}
				string path = GetPath(fileName, albumName) + text;
				Instance.StartCoroutine(Instance.Save(bytes, path, ImageType.SCREENSHOT));
				return false;
			}
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSave_003Ed__18 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string path;

		public byte[] bytes;

		public ImageType imageType;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSave_003Ed__18(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
				{
					using (FileStream fileStream = AppData.OpenFile(path, FileMode.Create))
					{
						fileStream?.Write(bytes, 0, bytes.Length);
					}
					_003C_003E2__current = Instance.StartCoroutine(Instance.Wait(0.5f));
					_003C_003E1__state = 1;
					return true;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			switch (imageType)
			{
			case ImageType.SCREENSHOT:
				if (OnScreenshotSaved != null)
				{
					OnScreenshotSaved(path);
				}
				break;
			case ImageType.IMAGE:
				if (ScreenshotManager.OnImageSaved != null)
				{
					ScreenshotManager.OnImageSaved(path);
				}
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSave_003Ed__19 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string path;

		public MemoryStream memoryStream;

		public ImageType imageType;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSave_003Ed__19(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			bool flag;
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				flag = false;
				if (Application.isEditor)
				{
					using (FileStream fileStream = AppData.OpenFile(path, FileMode.Create))
					{
						if (fileStream != null)
						{
							memoryStream.WriteTo(fileStream);
						}
					}
					_003C_003E2__current = Instance.StartCoroutine(Instance.Wait(0.5f));
					_003C_003E1__state = 1;
					return true;
				}
				if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
				{
					break;
				}
				goto IL_00b0;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				{
					_003C_003E1__state = -1;
					flag = true;
					goto IL_00b0;
				}
				IL_00b0:
				if (!flag)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				break;
			}
			switch (imageType)
			{
			case ImageType.SCREENSHOT:
				if (OnScreenshotSaved != null)
				{
					OnScreenshotSaved(path);
				}
				break;
			case ImageType.IMAGE:
				if (ScreenshotManager.OnImageSaved != null)
				{
					ScreenshotManager.OnImageSaved(path);
				}
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWait_003Ed__20 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delay;

		private float _003CpauseTarget_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CWait_003Ed__20(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CpauseTarget_003E5__2 = Time.realtimeSinceStartup + delay;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.realtimeSinceStartup < _003CpauseTarget_003E5__2)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static Action<string> OnScreenshotSaved;

	private static ScreenshotManager instance;

	private static GameObject go;

	public static ScreenshotManager Instance
	{
		get
		{
			if (instance == null)
			{
				go = new GameObject();
				go.name = "ScreenshotManager";
				instance = go.AddComponent<ScreenshotManager>();
			}
			return instance;
		}
	}

	public static event Action<Texture2D> OnScreenshotTaken;

	public static event Action<string> OnImageSaved;

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private static string GetPath(string fileName, string albumName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			fileName = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
		}
		string result = null;
		if (Application.isEditor)
		{
			result = albumName + "/" + fileName;
		}
		else if (Application.platform != RuntimePlatform.Android)
		{
			result = AppData.CombinePath(fileName);
		}
		return result;
	}

	public static void SaveScreenshot(string fileName = null, string albumName = "Screenshots", string fileType = "jpeg", Rect screenArea = default(Rect))
	{
		if (screenArea == default(Rect))
		{
			screenArea = new Rect(0f, 0f, Screen.width, Screen.height);
		}
		Instance.StartCoroutine(Instance.GrabScreenshot(fileName, albumName, fileType, screenArea));
	}

	private IEnumerator GrabScreenshot(string fileName, string albumName, string fileType, Rect screenArea)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGrabScreenshot_003Ed__15(0)
		{
			fileName = fileName,
			albumName = albumName,
			fileType = fileType,
			screenArea = screenArea
		};
	}

	public static void SaveImage(Texture2D texture, string fileName = null, string albumName = "Screenshots", string fileType = "png", int quality = 90)
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
			bytes = texture.EncodeToJPG(quality);
			text = ".jpeg";
		}
		string path = GetPath(fileName, albumName) + text;
		Instance.StartCoroutine(Instance.Save(bytes, path, ImageType.IMAGE));
	}

	public static void SaveImage(MemoryStream memoryStream, string fileName = null, string albumName = "Screenshots", string fileExt = ".jpeg")
	{
		Instance.Awake();
		string path = GetPath(fileName, albumName) + fileExt;
		Instance.StartCoroutine(Instance.Save(memoryStream, path, ImageType.IMAGE));
	}

	private IEnumerator Save(byte[] bytes, string path, ImageType imageType)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSave_003Ed__18(0)
		{
			bytes = bytes,
			path = path,
			imageType = imageType
		};
	}

	private IEnumerator Save(MemoryStream memoryStream, string path, ImageType imageType)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSave_003Ed__19(0)
		{
			memoryStream = memoryStream,
			path = path,
			imageType = imageType
		};
	}

	private IEnumerator Wait(float delay)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWait_003Ed__20(0)
		{
			delay = delay
		};
	}

	static ScreenshotManager()
	{
	}

	public static void SaveImage(MemoryStream memoryStream, string path, string fileExt = ".jpeg")
	{
		Instance.Awake();
		Instance.StartCoroutine(Instance.Save(memoryStream, path + fileExt, ImageType.IMAGE));
	}
}
