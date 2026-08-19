using System.IO;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.Utils;

public class ColorTable
{
	private readonly string _fileName;

	private Color[] _colors;

	public ColorTable(string fileName)
	{
		_fileName = fileName;
	}

	private void CheckLoaded()
	{
		if (_colors == null || _colors.Length == 0)
		{
			_colors = ReadColorTable(_fileName);
		}
	}

	public Color[] GetAll()
	{
		CheckLoaded();
		return _colors;
	}

	public Color GetRandom()
	{
		CheckLoaded();
		return _colors[Random.Range(0, _colors.Length)];
	}

	public Color GetRandom(int hashKey)
	{
		CheckLoaded();
		return _colors[KUtility.GetRandomHashRange(0, _colors.Length, hashKey)];
	}

	public Color GetColor(float ratio)
	{
		CheckLoaded();
		return _colors[(int)((float)_colors.Length * ratio) % _colors.Length];
	}

	private static Color[] ReadColorTable(string name)
	{
		if (!name.ToLower().Contains(".raw"))
		{
			return new Color[1] { name.ToColor() };
		}
		string path = "ColorTable/" + name;
		TextAsset textAsset = Resources.Load(path) as TextAsset;
		if (textAsset == null)
		{
			return new Color[1] { Color.magenta };
		}
		using Stream stream = new MemoryStream(textAsset.bytes);
		int num = (int)stream.Length / 3;
		Color[] array = new Color[num];
		BinaryReader binaryReader = new BinaryReader(stream);
		for (int i = 0; i < num; i++)
		{
			float r = (float)(int)binaryReader.ReadByte() / 255f;
			float g = (float)(int)binaryReader.ReadByte() / 255f;
			float b = (float)(int)binaryReader.ReadByte() / 255f;
			ref Color reference = ref array[i];
			reference = new Color(r, g, b, 1f);
		}
		Resources.UnloadAsset(textAsset);
		return array;
	}
}
