using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class NewCheckUtil
{
	private static HashSet<string> _checkerKeys;

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

	public static void Save(NewChecker obj)
	{
		if (TryUpdate(obj))
		{
			Save();
		}
	}

	private static bool TryUpdate(NewChecker obj)
	{
		if (string.IsNullOrEmpty(obj.Key))
		{
			return false;
		}
		return (!obj.IsNew) ? CheckerKeys.Remove(obj.Key) : CheckerKeys.Add(obj.Key);
	}

	private static void Refresh(NewChecker obj)
	{
		if (!string.IsNullOrEmpty(obj.Key))
		{
			obj.IsNew = CheckerKeys.Contains(obj.Key);
		}
		int i = 0;
		for (int size = KUtility.GetSize(obj.Childs); i < size; i++)
		{
			Refresh(obj.Childs[i]);
		}
	}

	public static void Refresh(INewCheckerable obj)
	{
		Refresh(obj.NewChecker);
	}

	public static void Refresh<T>(IList<T> list)
	{
		int i = 0;
		for (int size = KUtility.GetSize(list); i < size; i++)
		{
			if (list[i] is INewCheckerable obj)
			{
				Refresh(obj);
			}
		}
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
