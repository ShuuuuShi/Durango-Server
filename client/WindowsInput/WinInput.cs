using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WindowsInput;

public class WinInput
{
	private static Dictionary<int, bool> _isPressedVKDown;

	private static Dictionary<int, bool> _isPressedVKUp;

	static WinInput()
	{
		_isPressedVKDown = new Dictionary<int, bool>();
		_isPressedVKUp = new Dictionary<int, bool>();
		for (int i = 97; i <= 122; i++)
		{
			_isPressedVKDown.Add(KeyCodeToVkey((KeyCode)i), value: false);
			_isPressedVKUp.Add(KeyCodeToVkey((KeyCode)i), value: false);
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	protected static extern short GetAsyncKeyState(int keyCode);

	private static int KeyCodeToVkeyFullSet(KeyCode key)
	{
		int result = 0;
		switch (key)
		{
		case KeyCode.Backspace:
			result = 8;
			break;
		case KeyCode.Tab:
			result = 9;
			break;
		case KeyCode.Clear:
			result = 12;
			break;
		case KeyCode.Return:
			result = 13;
			break;
		case KeyCode.Pause:
			result = 19;
			break;
		case KeyCode.Escape:
			result = 27;
			break;
		case KeyCode.Space:
			result = 32;
			break;
		case KeyCode.Exclaim:
			result = 49;
			break;
		case KeyCode.DoubleQuote:
			result = 222;
			break;
		case KeyCode.Hash:
			result = 51;
			break;
		case KeyCode.Dollar:
			result = 52;
			break;
		case KeyCode.Ampersand:
			result = 55;
			break;
		case KeyCode.Quote:
			result = 222;
			break;
		case KeyCode.LeftParen:
			result = 57;
			break;
		case KeyCode.RightParen:
			result = 48;
			break;
		case KeyCode.Asterisk:
			result = 19;
			break;
		case KeyCode.Plus:
		case KeyCode.Equals:
			result = 187;
			break;
		case KeyCode.Comma:
		case KeyCode.Less:
			result = 188;
			break;
		case KeyCode.Minus:
		case KeyCode.Underscore:
			result = 189;
			break;
		case KeyCode.Period:
		case KeyCode.Greater:
			result = 190;
			break;
		case KeyCode.Slash:
		case KeyCode.Question:
			result = 191;
			break;
		case KeyCode.Alpha0:
		case KeyCode.Alpha1:
		case KeyCode.Alpha2:
		case KeyCode.Alpha3:
		case KeyCode.Alpha4:
		case KeyCode.Alpha5:
		case KeyCode.Alpha6:
		case KeyCode.Alpha7:
		case KeyCode.Alpha8:
		case KeyCode.Alpha9:
			result = (int)(48 + (key - 48));
			break;
		case KeyCode.Colon:
		case KeyCode.Semicolon:
			result = 186;
			break;
		case KeyCode.At:
			result = 50;
			break;
		case KeyCode.LeftBracket:
			result = 219;
			break;
		case KeyCode.Backslash:
			result = 220;
			break;
		case KeyCode.RightBracket:
			result = 221;
			break;
		case KeyCode.Caret:
			result = 54;
			break;
		case KeyCode.BackQuote:
			result = 192;
			break;
		case KeyCode.A:
		case KeyCode.B:
		case KeyCode.C:
		case KeyCode.D:
		case KeyCode.E:
		case KeyCode.F:
		case KeyCode.G:
		case KeyCode.H:
		case KeyCode.I:
		case KeyCode.J:
		case KeyCode.K:
		case KeyCode.L:
		case KeyCode.M:
		case KeyCode.N:
		case KeyCode.O:
		case KeyCode.P:
		case KeyCode.Q:
		case KeyCode.R:
		case KeyCode.S:
		case KeyCode.T:
		case KeyCode.U:
		case KeyCode.V:
		case KeyCode.W:
		case KeyCode.X:
		case KeyCode.Y:
		case KeyCode.Z:
			result = (int)(65 + (key - 97));
			break;
		case KeyCode.Delete:
			result = 46;
			break;
		case KeyCode.Keypad0:
		case KeyCode.Keypad1:
		case KeyCode.Keypad2:
		case KeyCode.Keypad3:
		case KeyCode.Keypad4:
		case KeyCode.Keypad5:
		case KeyCode.Keypad6:
		case KeyCode.Keypad7:
		case KeyCode.Keypad8:
		case KeyCode.Keypad9:
			result = (int)(96 + (key - 256));
			break;
		case KeyCode.KeypadPeriod:
			result = 110;
			break;
		case KeyCode.KeypadDivide:
			result = 111;
			break;
		case KeyCode.KeypadMultiply:
			result = 106;
			break;
		case KeyCode.KeypadMinus:
			result = 109;
			break;
		case KeyCode.KeypadPlus:
			result = 107;
			break;
		case KeyCode.KeypadEnter:
			result = 108;
			break;
		case KeyCode.UpArrow:
			result = 38;
			break;
		case KeyCode.DownArrow:
			result = 40;
			break;
		case KeyCode.RightArrow:
			result = 39;
			break;
		case KeyCode.LeftArrow:
			result = 37;
			break;
		case KeyCode.Insert:
			result = 45;
			break;
		case KeyCode.Home:
			result = 36;
			break;
		case KeyCode.End:
			result = 35;
			break;
		case KeyCode.PageUp:
			result = 33;
			break;
		case KeyCode.PageDown:
			result = 34;
			break;
		case KeyCode.F1:
		case KeyCode.F2:
		case KeyCode.F3:
		case KeyCode.F4:
		case KeyCode.F5:
		case KeyCode.F6:
		case KeyCode.F7:
		case KeyCode.F8:
		case KeyCode.F9:
		case KeyCode.F10:
		case KeyCode.F11:
		case KeyCode.F12:
		case KeyCode.F13:
		case KeyCode.F14:
		case KeyCode.F15:
			result = (int)(112 + (key - 282));
			break;
		case KeyCode.Numlock:
			result = 144;
			break;
		case KeyCode.CapsLock:
			result = 20;
			break;
		case KeyCode.ScrollLock:
			result = 145;
			break;
		case KeyCode.RightShift:
			result = 161;
			break;
		case KeyCode.LeftShift:
			result = 160;
			break;
		case KeyCode.RightControl:
			result = 163;
			break;
		case KeyCode.LeftControl:
			result = 162;
			break;
		case KeyCode.RightAlt:
			result = 165;
			break;
		case KeyCode.LeftAlt:
			result = 164;
			break;
		case KeyCode.Help:
			result = 227;
			break;
		case KeyCode.Print:
			result = 42;
			break;
		case KeyCode.SysReq:
			result = 44;
			break;
		case KeyCode.Break:
			result = 3;
			break;
		}
		return result;
	}

	private static int KeyCodeToVkey(KeyCode key)
	{
		int result = 0;
		switch (key)
		{
		case KeyCode.A:
		case KeyCode.B:
		case KeyCode.C:
		case KeyCode.D:
		case KeyCode.E:
		case KeyCode.F:
		case KeyCode.G:
		case KeyCode.H:
		case KeyCode.I:
		case KeyCode.J:
		case KeyCode.K:
		case KeyCode.L:
		case KeyCode.M:
		case KeyCode.N:
		case KeyCode.O:
		case KeyCode.P:
		case KeyCode.Q:
		case KeyCode.R:
		case KeyCode.S:
		case KeyCode.T:
		case KeyCode.U:
		case KeyCode.V:
		case KeyCode.W:
		case KeyCode.X:
		case KeyCode.Y:
		case KeyCode.Z:
			result = (int)(65 + (key - 97));
			break;
		}
		return result;
	}

	/// <summary>[4 ก.ย. 2026] user32.dll มีแค่บน Windows — มือถือ (APK Mono ที่ใช้ DLL ชุดเดียวกับ PC) ใช้ UnityEngine.Input แทน</summary>
	private static readonly bool UseNativeWin =
		Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;

	public static bool GetKey(KeyCode key)
	{
		if (!UseNativeWin) return Input.GetKey(key);
		int num = KeyCodeToVkey(key);
		if (num != 0)
		{
			return (GetAsyncKeyState(num) & 0x8000) != 0;
		}
		return Input.GetKey(key);
	}

	public static bool GetKeyDown(KeyCode key)
	{
		if (!UseNativeWin) return Input.GetKeyDown(key);
		int num = KeyCodeToVkey(key);
		if (num != 0)
		{
			bool flag = (GetAsyncKeyState(num) & 0x8000) != 0;
			bool flag2 = _isPressedVKDown[num];
			_isPressedVKDown[num] = flag;
			return !flag2 && flag;
		}
		return Input.GetKeyDown(key);
	}

	public static bool GetKeyUp(KeyCode key)
	{
		if (!UseNativeWin) return Input.GetKeyUp(key);
		int num = KeyCodeToVkey(key);
		if (num != 0)
		{
			bool flag = (GetAsyncKeyState(num) & 0x8000) != 0;
			bool flag2 = _isPressedVKUp[num];
			_isPressedVKUp[num] = flag;
			return flag2 && !flag;
		}
		return Input.GetKeyUp(key);
	}

	public static bool GetKeyFullCover(KeyCode key)
	{
		if (!UseNativeWin) return Input.GetKey(key);
		int num = KeyCodeToVkeyFullSet(key);
		if (num != 0)
		{
			return (GetAsyncKeyState(num) & 0x8000) != 0;
		}
		return Input.GetKey(key);
	}
}
