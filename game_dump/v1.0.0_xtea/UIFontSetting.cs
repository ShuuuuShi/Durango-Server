using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class UIFontSetting : MonoBehaviour
{
	public const string CustomFontSaveKey = "custom_font";

	[SerializeField]
	private string _mainFont;

	[SerializeField]
	private UIFont _target;

	[HideInInspector]
	[SerializeField]
	private List<string> _fontNames;

	private bool _needResetFontNames;

	[NonSerialized]
	private List<string> _avaiableFontList;

	public string MainFont => _mainFont;

	private UIFont Target
	{
		get
		{
			if ((Object)(object)_target == (Object)null)
			{
				_target = ((Component)this).GetComponent<UIFont>();
			}
			if ((Object)(object)_target.dynamicFont == (Object)null)
			{
				_target.dynamicFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
			}
			return _target;
		}
	}

	public Font Font
	{
		get
		{
			Font result = null;
			UIFont target = Target;
			if ((Object)(object)target != (Object)null)
			{
				result = target.dynamicFont;
			}
			return result;
		}
		set
		{
			Target.dynamicFont = value;
			ApplyFontNames();
		}
	}

	public List<string> FontNames
	{
		get
		{
			if (_fontNames == null)
			{
				_fontNames = new List<string>(Font.fontNames);
			}
			else if (_needResetFontNames)
			{
				_needResetFontNames = false;
				_fontNames.Clear();
				_fontNames.AddRange(Font.fontNames);
			}
			return _fontNames;
		}
	}

	public List<string> AvailableFontList
	{
		get
		{
			if (_avaiableFontList == null)
			{
				_avaiableFontList = MakeAvaiableFontList();
			}
			return _avaiableFontList;
		}
	}

	public void ResetFontNames()
	{
		_needResetFontNames = true;
	}

	public void ApplyFontNames()
	{
		List<string> fontNames = FontNames;
		for (int num = fontNames.Count - 1; num >= 0; num--)
		{
			if (string.IsNullOrEmpty(fontNames[num]))
			{
				fontNames.RemoveAt(num);
			}
		}
		string[] fontNames2 = Font.fontNames;
		string[] array = fontNames.ToArray();
		bool flag = true;
		if (fontNames2.Length == array.Length)
		{
			flag = false;
			for (int i = 0; i < fontNames2.Length; i++)
			{
				if (fontNames2[i] != array[i])
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Font.fontNames = array;
			RefreshChracterMatrial();
		}
	}

	public void Init()
	{
		string @string = PlayerPrefs.GetString("custom_font");
		SetCustomFont(@string);
	}

	public void SetCustomFont(string font)
	{
		if (FontNames[0] != font)
		{
			if (FontNames[0] == MainFont)
			{
				FontNames.Insert(0, font);
			}
			else
			{
				FontNames[0] = font;
			}
		}
		ApplyFontNames();
		PlayerPrefs.SetString("custom_font", font);
		PlayerPrefs.Save();
	}

	private void RefreshChracterMatrial()
	{
		foreach (UIRoot item in UIRoot.list)
		{
			Stack<Transform> stack = new Stack<Transform>();
			stack.Push(((Component)item).transform);
			while (stack.Count > 0)
			{
				Transform val = stack.Pop();
				UILabel component = ((Component)val).GetComponent<UILabel>();
				if ((Object)(object)component != (Object)null)
				{
					component.MarkAsChanged();
				}
				for (int i = 0; i < val.childCount; i++)
				{
					stack.Push(val.GetChild(i));
				}
			}
		}
	}

	private List<string> MakeAvaiableFontList()
	{
		List<string> list = new List<string>();
		string[] oSInstalledFontNames = Font.GetOSInstalledFontNames();
		foreach (string text in oSInstalledFontNames)
		{
			if (!text.EndsWith("Bold", ignoreCase: true, CultureInfo.CurrentCulture) && !text.EndsWith("Italic", ignoreCase: true, CultureInfo.CurrentCulture) && !text.EndsWith("Cond", ignoreCase: true, CultureInfo.CurrentCulture) && !text.EndsWith("Condensed", ignoreCase: true, CultureInfo.CurrentCulture))
			{
				list.Add(text);
			}
		}
		if (!list.Contains(MainFont))
		{
			list.Add(MainFont);
		}
		list.Sort();
		return list;
	}
}
