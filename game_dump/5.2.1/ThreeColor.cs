using UnityEngine;

public struct ThreeColor
{
	public Color C1;

	public Color C2;

	public Color C3;

	public static ThreeColor gray = new ThreeColor(Color.gray, Color.gray, Color.gray);

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

	public ThreeColor(Color c1, Color c2, Color c3)
	{
		C1 = c1;
		C2 = c2;
		C3 = c3;
	}

	public static bool operator ==(ThreeColor x, ThreeColor y)
	{
		return Compare(x, y);
	}

	public static bool operator !=(ThreeColor x, ThreeColor y)
	{
		return !Compare(x, y);
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		ThreeColor rhs = (ThreeColor)o;
		return Compare(this, rhs);
	}

	public override int GetHashCode()
	{
		int num = this[0].GetHashCode();
		for (int i = 1; i < 3; i++)
		{
			num ^= this[i].GetHashCode();
		}
		return num;
	}

	private static bool Compare(ThreeColor lhs, ThreeColor rhs)
	{
		for (int i = 0; i < 3; i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public Color GetColor(int index)
	{
		index %= 3;
		return index switch
		{
			0 => C1, 
			1 => C2, 
			_ => C3, 
		};
	}

	public void SetColor(int index, Color col)
	{
		index %= 3;
		switch (index)
		{
		case 0:
			C1 = col;
			break;
		case 1:
			C2 = col;
			break;
		default:
			C3 = col;
			break;
		}
	}
}
