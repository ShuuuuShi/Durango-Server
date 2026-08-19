using System;
using System.Collections.Generic;
using System.Text;
using Durango.Utils;
using UnityEngine;

namespace Durango.Logic.Notification;

public sealed class Toggle : Notification
{
	private static readonly DelayedFunction SaveFunction = new DelayedFunction(Save);

	private static HashSet<string> _checkerKeys;

	private readonly string _key;

	private bool _on;

	public override bool On
	{
		get
		{
			return _on;
		}
		set
		{
			if (_on != value)
			{
				_on = value;
				if (TryUpdate())
				{
					SaveFunction.Call(Singleton<GameManager>.Instance());
				}
				OnChanged();
			}
		}
	}

	public override int Count
	{
		get
		{
			return On ? 1 : 0;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	private static HashSet<string> CheckerKeys
	{
		get
		{
			if (_checkerKeys == null)
			{
				_checkerKeys = new HashSet<string>();
				Load();
			}
			return _checkerKeys;
		}
	}

	public Toggle(Type type, string key = null)
	{
		Type = type;
		_key = key;
	}

	public override void Refresh()
	{
		if (!string.IsNullOrEmpty(_key))
		{
			On = CheckerKeys.Contains(_key);
		}
	}

	private bool TryUpdate()
	{
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		return (!On) ? CheckerKeys.Remove(_key) : CheckerKeys.Add(_key);
	}

	private static void Save()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (string checkerKey in CheckerKeys)
		{
			if (num > 0)
			{
				stringBuilder.Append(",");
			}
			if (!string.IsNullOrEmpty(checkerKey))
			{
				stringBuilder.Append(checkerKey);
				num++;
			}
		}
		PlayerPrefs.SetString("new_object", stringBuilder.ToString());
		PlayerPrefs.Save();
	}

	private static void Load()
	{
		string @string = PlayerPrefs.GetString("new_object");
		string[] array = @string.Split(',');
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			CheckerKeys.Add(array[i]);
		}
	}
}
