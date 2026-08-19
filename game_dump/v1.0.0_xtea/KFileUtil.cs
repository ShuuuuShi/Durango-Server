using System;
using System.IO;
using UnityEngine;

public static class KFileUtil
{
	public static readonly char sep = '/';

	public static string GetAppPath()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 11 || (int)Application.platform == 8)
		{
			return Application.persistentDataPath;
		}
		string dataPath = Application.dataPath;
		return dataPath.Substring(0, dataPath.LastIndexOf(sep));
	}

	public static string GetFileName(string path)
	{
		string[] array = path.Split(sep);
		int startIndex = array[array.Length - 1].LastIndexOf(".", StringComparison.Ordinal);
		return array[array.Length - 1].Remove(startIndex);
	}

	public static FileStream GetFileStream(string filename, FileMode mode = FileMode.OpenOrCreate)
	{
		string[] array = filename.Split(sep);
		int num = array.Length;
		if (num == 0)
		{
			return null;
		}
		string text = GetAppPath();
		for (int i = 0; i < num - 1; i++)
		{
			text = Path.Combine(text, array[i]);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
		}
		return File.Open(Path.Combine(text, array[num - 1]), mode);
	}

	public static string[] GetDirectoryFiles(string directoryPath, string searchPatten, SearchOption option)
	{
		string[] array = directoryPath.Split(sep);
		int num = array.Length;
		if (num == 0)
		{
			return null;
		}
		string text = GetAppPath();
		for (int i = 0; i < num; i++)
		{
			text = Path.Combine(text, array[i]);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
		}
		return Directory.GetFiles(text, searchPatten, option);
	}
}
