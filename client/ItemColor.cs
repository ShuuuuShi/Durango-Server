using Durango.Utils.Extensions;
using UnityEngine;

public struct ItemColor
{
	private readonly int _count;

	private Color _color0;

	private Color _color1;

	private Color _color2;

	public int Count => _count;

	public bool HasValue => _count > 0;

	public bool IsMultiColor => _count > 1;

	public Color this[int index]
	{
		get
		{
			return GetColor(index);
		}
		set
		{
			SetColor(index, value);
		}
	}

	public ItemColor(string hex)
	{
		Color color = hex.ToColor();
		_color0 = color;
		_color1 = Color.clear;
		_color2 = Color.clear;
		_count = 1;
	}

	public ItemColor(string[] hexes)
	{
		_color0 = (_color1 = (_color2 = Color.clear));
		if (hexes == null)
		{
			_count = 0;
			return;
		}
		if (hexes.Length > 0)
		{
			_color0 = hexes[0].ToColor();
		}
		if (hexes.Length > 1)
		{
			_color1 = hexes[1].ToColor();
		}
		if (hexes.Length > 2)
		{
			_color2 = hexes[2].ToColor();
		}
		_count = hexes.Length;
	}

	public ItemColor(Color color)
	{
		_color0 = color;
		_color1 = Color.clear;
		_color2 = Color.clear;
		_count = 1;
	}

	public ItemColor(Color c1, Color c2, Color c3)
	{
		_color0 = c1;
		_color1 = c2;
		_color2 = c3;
		_count = 3;
	}

	public ItemColor(int colorCount)
	{
		_color0 = Color.clear;
		_color1 = Color.clear;
		_color2 = Color.clear;
		_count = colorCount;
	}

	public ItemColor(string r, string g, string b)
	{
		int num = ((!string.IsNullOrEmpty(r)) ? (string.IsNullOrEmpty(g) ? 1 : ((!string.IsNullOrEmpty(b)) ? 3 : 2)) : 0);
		if (num == 0)
		{
			_color0 = Color.clear;
			_color1 = Color.clear;
			_color2 = Color.clear;
			_count = 0;
		}
		else
		{
			_count = num;
			_color0 = r.ToColor();
			_color1 = g.ToColor();
			_color2 = b.ToColor();
		}
	}

	public ItemColor ToThreeColor()
	{
		return (_count != 3) ? new ItemColor(_color0, (_count != 2) ? _color0 : _color1, _color0) : this;
	}

	public static bool operator ==(ItemColor x, ItemColor y)
	{
		return Compare(x, y);
	}

	public static bool operator !=(ItemColor x, ItemColor y)
	{
		return !Compare(x, y);
	}

	public override bool Equals(object o)
	{
		ItemColor rhs = (ItemColor)o;
		return Compare(this, rhs);
	}

	public override int GetHashCode()
	{
		if (!HasValue)
		{
			return -1;
		}
		int num = this[0].GetHashCode();
		for (int i = 1; i < _count; i++)
		{
			num ^= this[i].GetHashCode();
		}
		return num;
	}

	private static bool Compare(ItemColor lhs, ItemColor rhs)
	{
		if (!lhs.HasValue && !rhs.HasValue)
		{
			return true;
		}
		if (lhs.HasValue != rhs.HasValue || lhs._count != rhs._count)
		{
			return false;
		}
		int count = lhs._count;
		for (int i = 0; i < count; i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public Color GetColor(int index, bool origin = false)
	{
		index = Mathf.Clamp(index, 0, _count - 1);
		Color color = _color0;
		switch (index)
		{
		case 0:
			color = _color0;
			break;
		case 1:
			color = _color1;
			break;
		case 2:
			color = _color2;
			break;
		}
		if (!origin)
		{
			color = Color.Lerp(Color.white, color, color.a);
			color.a = 1f;
		}
		return color;
	}

	public void SetColor(int index, Color col)
	{
		switch (index)
		{
		case 0:
			_color0 = col;
			break;
		case 1:
			_color1 = col;
			break;
		case 2:
			_color2 = col;
			break;
		default:
			_color0 = col;
			break;
		}
	}

	public void Dyeing(int index, Color col, float ratio)
	{
		Color color = GetColor(index, origin: true);
		Color col2 = Color.white - ((Color.white - color) * (1f - ratio) + (Color.white - col) * ratio);
		SetColor(index, col2);
	}

	public void Bleaching(int index, float ratio)
	{
		Color color = GetColor(index, origin: true);
		color.a = 0f;
		Dyeing(index, color, ratio);
	}

	public string[] ToHexes()
	{
		string[] array = new string[Count];
		if (array.Length > 0)
		{
			array[0] = _color0.ToHex();
		}
		if (array.Length > 1)
		{
			array[1] = _color1.ToHex();
		}
		if (array.Length > 2)
		{
			array[2] = _color2.ToHex();
		}
		return array;
	}
}
