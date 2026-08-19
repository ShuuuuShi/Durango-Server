using UnityEngine;

public struct MaxLineHeight
{
	private float _value;

	private float _last;

	public MaxLineHeight(float val)
	{
		_value = 0f;
		_last = val;
	}

	public void Reset()
	{
		_value = 0f;
	}

	public void Set(float val)
	{
		_value = Mathf.Max(_value, val);
		_last = val;
	}

	public float Get()
	{
		return (!(_value > 0f)) ? _last : _value;
	}

	public static float operator +(float v1, MaxLineHeight v2)
	{
		return v1 + v2.Get();
	}

	public static float operator -(float v1, MaxLineHeight v2)
	{
		return v1 - v2.Get();
	}
}
