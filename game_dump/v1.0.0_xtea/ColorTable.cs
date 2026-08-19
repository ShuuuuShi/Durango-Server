using System.IO;
using UnityEngine;

public class ColorTable
{
	private readonly string _fileName;

	private Color[] _colorTable;

	private int _randomIndex;

	public ColorTable(string colorTableFileName)
	{
		_fileName = colorTableFileName;
	}

	private void CheckLoaded()
	{
		if (_colorTable == null || _colorTable.Length == 0)
		{
			_colorTable = ReadColorTable(_fileName);
		}
	}

	public Color GetNextColor()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		CheckLoaded();
		_randomIndex = (_randomIndex + 1) % _colorTable.Length;
		return _colorTable[_randomIndex];
	}

	public Color GetRandomColor()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		CheckLoaded();
		return _colorTable[Random.Range(0, _colorTable.Length)];
	}

	public static Color[] ReadColorTable(string name)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (!name.ToLower().Contains(".raw"))
		{
			return (Color[])(object)new Color[1] { KUtility.ToColor(name) };
		}
		string text = "ColorTable/" + name;
		Object obj = Resources.Load(text);
		TextAsset val = (TextAsset)(object)((obj is TextAsset) ? obj : null);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		using Stream stream = new MemoryStream(val.bytes);
		int num = (int)stream.Length / 3;
		Color[] array = (Color[])(object)new Color[num];
		BinaryReader binaryReader = new BinaryReader(stream);
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)(int)binaryReader.ReadByte() / 255f;
			float num3 = (float)(int)binaryReader.ReadByte() / 255f;
			float num4 = (float)(int)binaryReader.ReadByte() / 255f;
			ref Color reference = ref array[i];
			reference = new Color(num2, num3, num4, 1f);
		}
		Resources.UnloadAsset((Object)(object)val);
		return array;
	}
}
