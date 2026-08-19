using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Utils;

public static class AppData
{
	private static readonly char[] Seperator = new char[2] { '/', '\\' };

	private static string BasePath
	{
		get
		{
			string environmentVariable = global::System.Environment.GetEnvironmentVariable("DURANGO_APPDATA");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				return environmentVariable;
			}
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
			{
				return Application.persistentDataPath;
			}
			string directoryName = Path.GetDirectoryName(Application.dataPath);
			return Path.Combine(directoryName, "AppData");
		}
	}

	public static string CombinePath(string path)
	{
		return Path.Combine(BasePath, path);
	}

	public static void DeleteFolder(string path)
	{
		try
		{
			Directory.Delete(CombinePath(path), recursive: true);
		}
		catch (Exception)
		{
		}
	}

	public static void DeleteFile(string filename)
	{
		try
		{
			File.Delete(CombinePath(filename));
		}
		catch (Exception)
		{
		}
	}

	[CanBeNull]
	public static FileStream OpenFile(string filename, FileMode mode = FileMode.OpenOrCreate)
	{
		try
		{
			string[] array = filename.Split(Seperator);
			int num = array.Length;
			if (num == 0)
			{
				return null;
			}
			string text = BasePath;
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
		catch (Exception)
		{
			return null;
		}
	}

	public static void WriteAllBytes(string filename, byte[] bytes)
	{
		using FileStream fileStream = OpenFile(filename, FileMode.Create);
		try
		{
			fileStream?.Write(bytes, 0, bytes.Length);
		}
		catch (Exception)
		{
		}
	}

	public static byte[] ReadAllBytes(string filename)
	{
		byte[] array = null;
		using (FileStream fileStream = OpenFile(filename, FileMode.Open))
		{
			if (fileStream != null)
			{
				int num = (int)fileStream.Length;
				array = new byte[num];
				fileStream.Read(array, 0, num);
			}
		}
		return array;
	}

	public static string[] GetFiles(string directoryPath, string searchPatten, SearchOption option)
	{
		string text = CreateDirectory(directoryPath);
		return (!string.IsNullOrEmpty(text)) ? Directory.GetFiles(text, searchPatten, option) : null;
	}

	public static string[] GetDirectories(string directoryPath, string searchPatten, SearchOption option)
	{
		string text = CreateDirectory(directoryPath);
		return (!string.IsNullOrEmpty(text)) ? Directory.GetDirectories(text, searchPatten, option) : null;
	}

	public static string CreateDirectory(string directoryPath, bool deletePrev = false)
	{
		if (deletePrev)
		{
			string path = CombinePath(directoryPath);
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		string[] array = directoryPath.Split(Seperator);
		int num = array.Length;
		if (num == 0)
		{
			return null;
		}
		string text = BasePath;
		for (int i = 0; i < num; i++)
		{
			text = Path.Combine(text, array[i]);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
		}
		return text;
	}
}
