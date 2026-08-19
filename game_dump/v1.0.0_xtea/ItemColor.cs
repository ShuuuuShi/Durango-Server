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
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return GetColor(index);
		}
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			SetColor(index, value);
		}
	}

	public ItemColor(params Color[] cols)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		_color0 = Color.clear;
		_color1 = Color.clear;
		_color2 = Color.clear;
		_count = KUtility.GetSize(cols);
		for (int i = 0; i < _count; i++)
		{
			this[i] = cols[i];
		}
	}

	public ItemColor(int colorCount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_color0 = Color.clear;
		_color1 = Color.clear;
		_color2 = Color.clear;
		_count = colorCount;
	}

	public override bool Equals(object o)
	{
		ItemColor rhs = (ItemColor)o;
		return Compare(this, rhs);
	}

	public override int GetHashCode()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!HasValue)
		{
			return -1;
		}
		Color val = this[0];
		int num = ((Color)(ref val)).GetHashCode();
		for (int i = 1; i < _count; i++)
		{
			int num2 = num;
			Color val2 = this[i];
			num = num2 ^ ((Color)(ref val2)).GetHashCode();
		}
		return num;
	}

	private static bool Compare(ItemColor lhs, ItemColor rhs)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		index = Mathf.Clamp(index, 0, _count - 1);
		Color val = _color0;
		switch (index)
		{
		case 0:
			val = _color0;
			break;
		case 1:
			val = _color1;
			break;
		case 2:
			val = _color2;
			break;
		}
		if (!origin)
		{
			float a = val.a;
			val = Color.white - (Color.white - val) * a;
			val.a = 1f;
		}
		return val;
	}

	public void SetColor(int index, Color col)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Color color = GetColor(index, origin: true);
		Color col2 = Color.white - ((Color.white - color) * (1f - ratio) + (Color.white - col) * ratio);
		SetColor(index, col2);
	}

	public void Bleaching(int index, float ratio)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Color color = GetColor(index, origin: true);
		color.a = 0f;
		Dyeing(index, color, ratio);
	}

	public static bool operator ==(ItemColor x, ItemColor y)
	{
		return Compare(x, y);
	}

	public static bool operator !=(ItemColor x, ItemColor y)
	{
		return !Compare(x, y);
	}
}
