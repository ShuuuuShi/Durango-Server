using System;
using UnityEngine;

public class RotateColor : UISprite
{
	[SerializeField]
	private float _speed = 100f;

	[SerializeField]
	private float _spotSize = 100f;

	[SerializeField]
	private Color _color1 = Color.white;

	[SerializeField]
	private Color _color2 = Color.black;

	protected override void LateUpdate()
	{
		base.LateUpdate();
		MarkAsChanged();
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		base.OnFill(arguments);
		Vector2 size = localSize;
		Vector3 vector = base.localCenter;
		float num = ((!Application.isPlaying) ? Time.realtimeSinceStartup : Time.time);
		float num2 = num * _speed % ((size.x + size.y) * 2f);
		float num3 = SideToRadian(size, num2);
		float r = SideToRadian(size, num2 - _spotSize * 0.5f);
		float r2 = SideToRadian(size, num2 + _spotSize * 0.5f);
		BetterList<Color> cols = arguments.cols;
		for (int i = 0; i < cols.size; i++)
		{
			Vector3 vector2 = geometry.verts[i] - vector;
			float r3 = Mathf.Atan2(vector2.y, vector2.x);
			float num4 = RadianDiff(r3, num3);
			float t = ((!(num4 < 0f)) ? (RadianDiff(r3, r2) / RadianDiff(num3, r2)) : (RadianDiff(r3, r) / RadianDiff(num3, r)));
			float a = cols[i].a;
			Color value = Color.Lerp(_color1, _color2, t);
			value.a *= a;
			cols[i] = value;
		}
	}

	private static float SideToRadian(Vector2 size, float len)
	{
		float x = size.x;
		float y = size.y;
		float num = x * 0.5f;
		float num2 = y * 0.5f;
		float num3 = (x + y) * 2f;
		while (true)
		{
			if (len < 0f)
			{
				len += num3;
				continue;
			}
			if (len >= num3)
			{
				len -= num3;
				continue;
			}
			break;
		}
		Vector2 vector = default(Vector2);
		if (len < x + y)
		{
			if (len < x)
			{
				vector.x = len - num;
				vector.y = num2;
			}
			else
			{
				vector.x = num;
				vector.y = num2 - (len - x);
			}
		}
		else
		{
			len -= x + y;
			if (len < x)
			{
				vector.x = num - len;
				vector.y = 0f - num2;
			}
			else
			{
				vector.x = 0f - num;
				vector.y = len - x - num2;
			}
		}
		return Mathf.Atan2(vector.y, vector.x);
	}

	private static float RadianDiff(float r1, float r2)
	{
		float num = r2 - r1;
		if (num < -(float)Math.PI)
		{
			num += (float)Math.PI * 2f;
		}
		else if (num > (float)Math.PI)
		{
			num -= (float)Math.PI * 2f;
		}
		return num;
	}
}
