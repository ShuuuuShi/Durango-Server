using System;
using UnityEngine;

public class RotateColor : MonoBehaviour
{
	[SerializeField]
	private float _speed = 100f;

	[SerializeField]
	private float _spotSize = 100f;

	[SerializeField]
	private Color _color1 = Color.white;

	[SerializeField]
	private Color _color2 = Color.black;

	private UIWidget _widget;

	private void OnEnable()
	{
		_widget = ((Component)this).GetComponent<UIWidget>();
		if ((Object)(object)_widget != (Object)null)
		{
			UIWidget widget = _widget;
			widget.onPostFill = (UIWidget.OnPostFillCallback)Delegate.Combine(widget.onPostFill, new UIWidget.OnPostFillCallback(OnPostFill));
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)_widget != (Object)null)
		{
			UIWidget widget = _widget;
			widget.onPostFill = (UIWidget.OnPostFillCallback)Delegate.Remove(widget.onPostFill, new UIWidget.OnPostFillCallback(OnPostFill));
		}
	}

	private void Update()
	{
		UpdateColor();
		_widget.SetDirty();
	}

	private void OnPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		UpdateColor();
	}

	private void UpdateColor()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_widget == (Object)null))
		{
			UIGeometry geometry = _widget.geometry;
			int width = _widget.width;
			int height = _widget.height;
			float num = Time.time * _speed % (float)((width + height) * 2);
			float num2 = SideToRadian(num);
			float r = SideToRadian(num - _spotSize * 0.5f);
			float r2 = SideToRadian(num + _spotSize * 0.5f);
			int size = geometry.cols.size;
			for (int i = 0; i < size; i++)
			{
				Vector3 val = geometry.verts[i];
				float r3 = Mathf.Atan2(val.y, val.x);
				float num3 = RadianDiff(r3, num2);
				float num4 = ((!(num3 < 0f)) ? (RadianDiff(r3, r2) / RadianDiff(num2, r2)) : (RadianDiff(r3, r) / RadianDiff(num2, r)));
				float a = geometry.cols[i].a;
				Color value = Color.Lerp(_color1, _color2, num4);
				value.a = a;
				geometry.cols[i] = value;
			}
		}
	}

	private float SideToRadian(float len)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		int width = _widget.width;
		int height = _widget.height;
		float num = (float)width * 0.5f;
		float num2 = (float)height * 0.5f;
		float num3 = (float)(width + height) * 2f;
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
		Vector2 val = default(Vector2);
		if (len < (float)(width + height))
		{
			if (len < (float)width)
			{
				val.x = len - num;
				val.y = num2;
			}
			else
			{
				val.x = num;
				val.y = num2 - (len - (float)width);
			}
		}
		else
		{
			len -= (float)(width + height);
			if (len < (float)width)
			{
				val.x = num - len;
				val.y = 0f - num2;
			}
			else
			{
				val.x = 0f - num;
				val.y = len - (float)width - num2;
			}
		}
		return Mathf.Atan2(val.y, val.x);
	}

	private float RadianDiff(float r1, float r2)
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
