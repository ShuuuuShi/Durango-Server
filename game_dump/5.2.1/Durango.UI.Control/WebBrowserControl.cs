using System;
using System.IO;
using System.Text;
using Durango.Render.Camera;
using Durango.System;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Control;

[RequireComponent(typeof(UITexture))]
public class WebBrowserControl : MonoBehaviour
{
	[SerializeField]
	private bool _allowKeyInput;

	private static DwarfPluginHelper _dwarfPlugin;

	private int _browserId = -1;

	private Texture2D _webBrowserTexture;

	private Texture _originalTexture;

	private bool _originalTextureSet;

	private Vector3 _textureLeftTopPosision;

	private Point2 _textureSize;

	private bool _focused;

	public Action<string> UrlChanged;

	private string _currentUrl;

	public static bool HasFocus { get; private set; }

	private bool Opened => _browserId != -1;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Initialize()
	{
		if ((Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor) || !Platform.Instance.UsePCUI || _dwarfPlugin != null)
		{
			return;
		}
		try
		{
			_dwarfPlugin = new DwarfPluginHelper();
			string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Dwarf"));
			_dwarfPlugin.Invoke_InitializeDwarf(fullPath);
			GameManager.Quitted += CleanUp;
		}
		catch (Exception)
		{
			_dwarfPlugin = null;
		}
	}

	private static void CleanUp()
	{
		if (_dwarfPlugin != null)
		{
			_dwarfPlugin.Invoke_CleanupDwarf();
			_dwarfPlugin = null;
		}
	}

	public void OnScreenResized()
	{
		UITexture component = GetComponent<UITexture>();
		_textureSize = new Point2(component.width, component.height);
		Vector3 textureLeftTopPosision = UIUtility.ToRootPosition(base.gameObject);
		textureLeftTopPosision.x += (float)(UIManager.ScreenWidth - _textureSize.x) * 0.5f;
		textureLeftTopPosision.y += (float)(UIManager.ScreenHeight - _textureSize.y) * 0.5f;
		_textureLeftTopPosision = textureLeftTopPosision;
	}

	public void OpenUrl(string url)
	{
		if (_dwarfPlugin == null)
		{
			return;
		}
		_currentUrl = url;
		if (!Opened)
		{
			UITexture component = GetComponent<UITexture>();
			_browserId = _dwarfPlugin.Invoke_StartBrowser(_textureSize.x, _textureSize.y, url);
			if (!Opened)
			{
				return;
			}
			IntPtr textureView = IntPtr.Zero;
			if (!_dwarfPlugin.Invoke_GetTextureView(_browserId, ref textureView))
			{
				StopBrowsing();
				CleanUp();
				return;
			}
			if (_webBrowserTexture == null)
			{
				_webBrowserTexture = Texture2D.CreateExternalTexture(_textureSize.x, _textureSize.y, TextureFormat.RGBA32, mipmap: false, linear: false, textureView);
			}
			if (!_originalTextureSet)
			{
				_originalTexture = component.mainTexture;
				_originalTextureSet = true;
			}
			component.mainTexture = _webBrowserTexture;
			UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, (UICamera.BoolDelegate)delegate(GameObject obj, bool pressed)
			{
				bool focused = _focused;
				if (obj != null && obj == base.gameObject && pressed && _allowKeyInput)
				{
					_focused = true;
				}
				else if (obj == null || obj != base.gameObject)
				{
					_focused = false;
				}
				if (focused != _focused)
				{
					HasFocus = _focused;
				}
			});
		}
		else
		{
			_dwarfPlugin.Invoke_SetCurrentUrl(_browserId, url);
		}
	}

	public void StopBrowsing(bool cleanUpTexture = true)
	{
		if (Opened)
		{
			if (_dwarfPlugin != null)
			{
				_dwarfPlugin.Invoke_StopBrowser(_browserId);
			}
			if (cleanUpTexture && _webBrowserTexture != null)
			{
				UnityEngine.Object.Destroy(_webBrowserTexture);
				_webBrowserTexture = null;
			}
			UITexture component = GetComponent<UITexture>();
			if (_originalTextureSet && component != null)
			{
				component.mainTexture = _originalTexture;
			}
			HasFocus = false;
			_browserId = -1;
			_currentUrl = string.Empty;
		}
	}

	private void Update()
	{
		if (!Opened)
		{
			return;
		}
		Point2 mousePosition = GetMousePosition();
		if (mousePosition.x >= 0 && mousePosition.y >= 0 && mousePosition.x <= _textureSize.x && mousePosition.y <= _textureSize.y)
		{
			_dwarfPlugin.Invoke_SendMouseMove(_browserId, mousePosition.x, mousePosition.y);
		}
		if (UrlChanged == null)
		{
			return;
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		if (value.Capacity < 1024)
		{
			value.Capacity = 1024;
		}
		_dwarfPlugin.Invoke_GetCurrentUrl(_browserId, value, 1024);
		string text = value.ToString();
		if (!(text == _currentUrl))
		{
			_currentUrl = text;
			UrlChanged(_currentUrl);
		}
	}

	private void OnGUI()
	{
		if (Opened && _focused && _allowKeyInput)
		{
			Event current = Event.current;
			if (current.isKey)
			{
				bool keyDown = current.type == EventType.KeyDown;
				_dwarfPlugin.Invoke_SendKeyboardEvent(_browserId, (int)current.keyCode, keyDown);
			}
		}
	}

	private void OnPress(bool pressed)
	{
		if (Opened)
		{
			Point2 mousePosition = GetMousePosition();
			_dwarfPlugin.Invoke_SendMouseClick(_browserId, DWARF_MOUSE_BTN_TYPE.BTN_LEFT, mousePosition.x, mousePosition.y, pressed);
		}
	}

	private void OnScroll(float delta)
	{
		if (Opened)
		{
			Point2 mousePosition = GetMousePosition();
			_dwarfPlugin.Invoke_SendMouseWheel(_browserId, mousePosition.x, mousePosition.y, delta);
		}
	}

	private Point2 GetMousePosition()
	{
		Vector3 vector = Input.mousePosition * MainCamera.NGUIScale();
		vector -= _textureLeftTopPosision;
		return new Point2((int)vector.x, _textureSize.y - (int)vector.y);
	}
}
