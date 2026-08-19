using System.Diagnostics;
using UnityEngine;

public static class NGUIMath
{
	[DebuggerStepThrough]
	[DebuggerHidden]
	public static float Lerp(float from, float to, float factor)
	{
		return from * (1f - factor) + to * factor;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static int ClampIndex(int val, int max)
	{
		return (val >= 0) ? ((val >= max) ? (max - 1) : val) : 0;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static int RepeatIndex(int val, int max)
	{
		if (max < 1)
		{
			return 0;
		}
		while (val < 0)
		{
			val += max;
		}
		while (val >= max)
		{
			val -= max;
		}
		return val;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static float WrapAngle(float angle)
	{
		while (angle > 180f)
		{
			angle -= 360f;
		}
		while (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static float Wrap01(float val)
	{
		return val - (float)Mathf.FloorToInt(val);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static int HexToDecimal(char ch)
	{
		switch (ch)
		{
		case '0':
			return 0;
		case '1':
			return 1;
		case '2':
			return 2;
		case '3':
			return 3;
		case '4':
			return 4;
		case '5':
			return 5;
		case '6':
			return 6;
		case '7':
			return 7;
		case '8':
			return 8;
		case '9':
			return 9;
		case 'A':
		case 'a':
			return 10;
		case 'B':
		case 'b':
			return 11;
		case 'C':
		case 'c':
			return 12;
		case 'D':
		case 'd':
			return 13;
		case 'E':
		case 'e':
			return 14;
		case 'F':
		case 'f':
			return 15;
		default:
			return 15;
		}
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static char DecimalToHexChar(int num)
	{
		if (num > 15)
		{
			return 'F';
		}
		if (num < 10)
		{
			return (char)(48 + num);
		}
		return (char)(65 + num - 10);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string DecimalToHex8(int num)
	{
		num &= 0xFF;
		return num.ToString("X2");
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string DecimalToHex24(int num)
	{
		num &= 0xFFFFFF;
		return num.ToString("X6");
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string DecimalToHex32(int num)
	{
		return num.ToString("X8");
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static int ColorToInt(Color c)
	{
		int num = 0;
		num |= Mathf.RoundToInt(c.r * 255f) << 24;
		num |= Mathf.RoundToInt(c.g * 255f) << 16;
		num |= Mathf.RoundToInt(c.b * 255f) << 8;
		return num | Mathf.RoundToInt(c.a * 255f);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color IntToColor(int val)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.003921569f;
		Color black = Color.black;
		black.r = num * (float)((val >> 24) & 0xFF);
		black.g = num * (float)((val >> 16) & 0xFF);
		black.b = num * (float)((val >> 8) & 0xFF);
		black.a = num * (float)(val & 0xFF);
		return black;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string IntToBinary(int val, int bits)
	{
		string text = string.Empty;
		int num = bits;
		while (num > 0)
		{
			if (num == 8 || num == 16 || num == 24)
			{
				text += " ";
			}
			text += (((val & (1 << --num)) == 0) ? '0' : '1');
		}
		return text;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color HexToColor(uint val)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return IntToColor((int)val);
	}

	public static Rect ConvertToTexCoords(Rect rect, int width, int height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Rect result = rect;
		if ((float)width != 0f && (float)height != 0f)
		{
			((Rect)(ref result)).xMin = ((Rect)(ref rect)).xMin / (float)width;
			((Rect)(ref result)).xMax = ((Rect)(ref rect)).xMax / (float)width;
			((Rect)(ref result)).yMin = 1f - ((Rect)(ref rect)).yMax / (float)height;
			((Rect)(ref result)).yMax = 1f - ((Rect)(ref rect)).yMin / (float)height;
		}
		return result;
	}

	public static Rect ConvertToPixels(Rect rect, int width, int height, bool round)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Rect result = rect;
		if (round)
		{
			((Rect)(ref result)).xMin = Mathf.RoundToInt(((Rect)(ref rect)).xMin * (float)width);
			((Rect)(ref result)).xMax = Mathf.RoundToInt(((Rect)(ref rect)).xMax * (float)width);
			((Rect)(ref result)).yMin = Mathf.RoundToInt((1f - ((Rect)(ref rect)).yMax) * (float)height);
			((Rect)(ref result)).yMax = Mathf.RoundToInt((1f - ((Rect)(ref rect)).yMin) * (float)height);
		}
		else
		{
			((Rect)(ref result)).xMin = ((Rect)(ref rect)).xMin * (float)width;
			((Rect)(ref result)).xMax = ((Rect)(ref rect)).xMax * (float)width;
			((Rect)(ref result)).yMin = (1f - ((Rect)(ref rect)).yMax) * (float)height;
			((Rect)(ref result)).yMax = (1f - ((Rect)(ref rect)).yMin) * (float)height;
		}
		return result;
	}

	public static Rect MakePixelPerfect(Rect rect)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		((Rect)(ref rect)).xMin = Mathf.RoundToInt(((Rect)(ref rect)).xMin);
		((Rect)(ref rect)).yMin = Mathf.RoundToInt(((Rect)(ref rect)).yMin);
		((Rect)(ref rect)).xMax = Mathf.RoundToInt(((Rect)(ref rect)).xMax);
		((Rect)(ref rect)).yMax = Mathf.RoundToInt(((Rect)(ref rect)).yMax);
		return rect;
	}

	public static Rect MakePixelPerfect(Rect rect, int width, int height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		rect = ConvertToPixels(rect, width, height, round: true);
		((Rect)(ref rect)).xMin = Mathf.RoundToInt(((Rect)(ref rect)).xMin);
		((Rect)(ref rect)).yMin = Mathf.RoundToInt(((Rect)(ref rect)).yMin);
		((Rect)(ref rect)).xMax = Mathf.RoundToInt(((Rect)(ref rect)).xMax);
		((Rect)(ref rect)).yMax = Mathf.RoundToInt(((Rect)(ref rect)).yMax);
		return ConvertToTexCoords(rect, width, height);
	}

	public static Vector2 ConstrainRect(Vector2 minRect, Vector2 maxRect, Vector2 minArea, Vector2 maxArea)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		float num = maxRect.x - minRect.x;
		float num2 = maxRect.y - minRect.y;
		float num3 = maxArea.x - minArea.x;
		float num4 = maxArea.y - minArea.y;
		if (num > num3)
		{
			float num5 = num - num3;
			minArea.x -= num5;
			maxArea.x += num5;
		}
		if (num2 > num4)
		{
			float num6 = num2 - num4;
			minArea.y -= num6;
			maxArea.y += num6;
		}
		if (minRect.x < minArea.x)
		{
			zero.x += minArea.x - minRect.x;
		}
		if (maxRect.x > maxArea.x)
		{
			zero.x -= maxRect.x - maxArea.x;
		}
		if (minRect.y < minArea.y)
		{
			zero.y += minArea.y - minRect.y;
		}
		if (maxRect.y > maxArea.y)
		{
			zero.y -= maxRect.y - maxArea.y;
		}
		return zero;
	}

	public static Bounds CalculateAbsoluteWidgetBounds(Transform trans)
	{
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)trans != (Object)null)
		{
			UIWidget[] componentsInChildren = ((Component)trans).GetComponentsInChildren<UIWidget>();
			if (componentsInChildren.Length == 0)
			{
				return new Bounds(trans.position, Vector3.zero);
			}
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(float.MinValue, float.MinValue, float.MinValue);
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget uIWidget = componentsInChildren[i];
				if (!((Behaviour)uIWidget).enabled)
				{
					continue;
				}
				Vector3[] worldCorners = uIWidget.worldCorners;
				for (int j = 0; j < 4; j++)
				{
					Vector3 val3 = worldCorners[j];
					if (val3.x > val2.x)
					{
						val2.x = val3.x;
					}
					if (val3.y > val2.y)
					{
						val2.y = val3.y;
					}
					if (val3.z > val2.z)
					{
						val2.z = val3.z;
					}
					if (val3.x < val.x)
					{
						val.x = val3.x;
					}
					if (val3.y < val.y)
					{
						val.y = val3.y;
					}
					if (val3.z < val.z)
					{
						val.z = val3.z;
					}
				}
			}
			Bounds result = default(Bounds);
			((Bounds)(ref result))._002Ector(val, Vector3.zero);
			((Bounds)(ref result)).Encapsulate(val2);
			return result;
		}
		return new Bounds(Vector3.zero, Vector3.zero);
	}

	public static Bounds CalculateRelativeWidgetBounds(Transform trans)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return CalculateRelativeWidgetBounds(trans, trans, considerInactive: false);
	}

	public static Bounds CalculateRelativeWidgetBounds(Transform trans, bool considerInactive)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return CalculateRelativeWidgetBounds(trans, trans, considerInactive);
	}

	public static Bounds CalculateRelativeWidgetBounds(Transform relativeTo, Transform content)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return CalculateRelativeWidgetBounds(relativeTo, content, considerInactive: false);
	}

	public static Bounds CalculateRelativeWidgetBounds(Transform relativeTo, Transform content, bool considerInactive, bool considerChildren = true)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)content != (Object)null && (Object)(object)relativeTo != (Object)null)
		{
			bool isSet = false;
			Matrix4x4 toLocal = relativeTo.worldToLocalMatrix;
			Vector3 vMin = default(Vector3);
			((Vector3)(ref vMin))._002Ector(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vMax = default(Vector3);
			((Vector3)(ref vMax))._002Ector(float.MinValue, float.MinValue, float.MinValue);
			CalculateRelativeWidgetBounds(content, considerInactive, isRoot: true, ref toLocal, ref vMin, ref vMax, ref isSet, considerChildren);
			if (isSet)
			{
				Bounds result = default(Bounds);
				((Bounds)(ref result))._002Ector(vMin, Vector3.zero);
				((Bounds)(ref result)).Encapsulate(vMax);
				return result;
			}
		}
		return new Bounds(Vector3.zero, Vector3.zero);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	private static void CalculateRelativeWidgetBounds(Transform content, bool considerInactive, bool isRoot, ref Matrix4x4 toLocal, ref Vector3 vMin, ref Vector3 vMax, ref bool isSet, bool considerChildren)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)content == (Object)null || (!considerInactive && !NGUITools.GetActive(((Component)content).gameObject)))
		{
			return;
		}
		UIPanel uIPanel = ((!isRoot) ? ((Component)content).GetComponent<UIPanel>() : null);
		if ((Object)(object)uIPanel != (Object)null && !((Behaviour)uIPanel).enabled)
		{
			return;
		}
		if ((Object)(object)uIPanel != (Object)null && uIPanel.clipping != 0)
		{
			Vector3[] worldCorners = uIPanel.worldCorners;
			for (int i = 0; i < 4; i++)
			{
				Vector3 val = ((Matrix4x4)(ref toLocal)).MultiplyPoint3x4(worldCorners[i]);
				if (val.x > vMax.x)
				{
					vMax.x = val.x;
				}
				if (val.y > vMax.y)
				{
					vMax.y = val.y;
				}
				if (val.z > vMax.z)
				{
					vMax.z = val.z;
				}
				if (val.x < vMin.x)
				{
					vMin.x = val.x;
				}
				if (val.y < vMin.y)
				{
					vMin.y = val.y;
				}
				if (val.z < vMin.z)
				{
					vMin.z = val.z;
				}
				isSet = true;
			}
			return;
		}
		UIWidget component = ((Component)content).GetComponent<UIWidget>();
		if ((Object)(object)component != (Object)null && ((Behaviour)component).enabled)
		{
			Vector3[] worldCorners2 = component.worldCorners;
			for (int j = 0; j < 4; j++)
			{
				Vector3 val2 = ((Matrix4x4)(ref toLocal)).MultiplyPoint3x4(worldCorners2[j]);
				if (val2.x > vMax.x)
				{
					vMax.x = val2.x;
				}
				if (val2.y > vMax.y)
				{
					vMax.y = val2.y;
				}
				if (val2.z > vMax.z)
				{
					vMax.z = val2.z;
				}
				if (val2.x < vMin.x)
				{
					vMin.x = val2.x;
				}
				if (val2.y < vMin.y)
				{
					vMin.y = val2.y;
				}
				if (val2.z < vMin.z)
				{
					vMin.z = val2.z;
				}
				isSet = true;
			}
			if (!considerChildren)
			{
				return;
			}
		}
		int k = 0;
		for (int childCount = content.childCount; k < childCount; k++)
		{
			CalculateRelativeWidgetBounds(content.GetChild(k), considerInactive, isRoot: false, ref toLocal, ref vMin, ref vMax, ref isSet, considerChildren: true);
		}
	}

	public static Vector3 SpringDampen(ref Vector3 velocity, float strength, float deltaTime)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (deltaTime > 1f)
		{
			deltaTime = 1f;
		}
		float num = 1f - strength * 0.001f;
		int num2 = Mathf.RoundToInt(deltaTime * 1000f);
		float num3 = Mathf.Pow(num, (float)num2);
		Vector3 val = velocity * ((num3 - 1f) / Mathf.Log(num));
		velocity *= num3;
		return val * 0.06f;
	}

	public static Vector2 SpringDampen(ref Vector2 velocity, float strength, float deltaTime)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (deltaTime > 1f)
		{
			deltaTime = 1f;
		}
		float num = 1f - strength * 0.001f;
		int num2 = Mathf.RoundToInt(deltaTime * 1000f);
		float num3 = Mathf.Pow(num, (float)num2);
		Vector2 val = velocity * ((num3 - 1f) / Mathf.Log(num));
		velocity *= num3;
		return val * 0.06f;
	}

	public static float SpringLerp(float strength, float deltaTime)
	{
		if (deltaTime > 1f)
		{
			deltaTime = 1f;
		}
		int num = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 = Mathf.Lerp(num2, 1f, deltaTime);
		}
		return num2;
	}

	public static float SpringLerp(float from, float to, float strength, float deltaTime)
	{
		if (deltaTime > 1f)
		{
			deltaTime = 1f;
		}
		int num = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		for (int i = 0; i < num; i++)
		{
			from = Mathf.Lerp(from, to, deltaTime);
		}
		return from;
	}

	public static Vector2 SpringLerp(Vector2 from, Vector2 to, float strength, float deltaTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	public static Vector3 SpringLerp(Vector3 from, Vector3 to, float strength, float deltaTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	public static Quaternion SpringLerp(Quaternion from, Quaternion to, float strength, float deltaTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Slerp(from, to, SpringLerp(strength, deltaTime));
	}

	public static float RotateTowards(float from, float to, float maxAngle)
	{
		float num = WrapAngle(to - from);
		if (Mathf.Abs(num) > maxAngle)
		{
			num = maxAngle * Mathf.Sign(num);
		}
		return from + num;
	}

	private static float DistancePointToLineSegment(Vector2 point, Vector2 a, Vector2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = b - a;
		float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
		if (sqrMagnitude == 0f)
		{
			Vector2 val2 = point - a;
			return ((Vector2)(ref val2)).magnitude;
		}
		float num = Vector2.Dot(point - a, b - a) / sqrMagnitude;
		if (num < 0f)
		{
			Vector2 val3 = point - a;
			return ((Vector2)(ref val3)).magnitude;
		}
		if (num > 1f)
		{
			Vector2 val4 = point - b;
			return ((Vector2)(ref val4)).magnitude;
		}
		Vector2 val5 = a + num * (b - a);
		Vector2 val6 = point - val5;
		return ((Vector2)(ref val6)).magnitude;
	}

	public static float DistanceToRectangle(Vector2[] screenPoints, Vector2 mousePos)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int val = 4;
		for (int i = 0; i < 5; i++)
		{
			Vector3 val2 = Vector2.op_Implicit(screenPoints[RepeatIndex(i, 4)]);
			Vector3 val3 = Vector2.op_Implicit(screenPoints[RepeatIndex(val, 4)]);
			if (val2.y > mousePos.y != val3.y > mousePos.y && mousePos.x < (val3.x - val2.x) * (mousePos.y - val2.y) / (val3.y - val2.y) + val2.x)
			{
				flag = !flag;
			}
			val = i;
		}
		if (!flag)
		{
			float num = -1f;
			for (int j = 0; j < 4; j++)
			{
				Vector3 val4 = Vector2.op_Implicit(screenPoints[j]);
				Vector3 val5 = Vector2.op_Implicit(screenPoints[RepeatIndex(j + 1, 4)]);
				float num2 = DistancePointToLineSegment(mousePos, Vector2.op_Implicit(val4), Vector2.op_Implicit(val5));
				if (num2 < num || num < 0f)
				{
					num = num2;
				}
			}
			return num;
		}
		return 0f;
	}

	public static float DistanceToRectangle(Vector3[] worldPoints, Vector2 mousePos, Camera cam)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector2[] array = (Vector2[])(object)new Vector2[4];
		for (int i = 0; i < 4; i++)
		{
			ref Vector2 reference = ref array[i];
			reference = Vector2.op_Implicit(cam.WorldToScreenPoint(worldPoints[i]));
		}
		return DistanceToRectangle(array, mousePos);
	}

	public static Vector2 GetPivotOffset(UIWidget.Pivot pv)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		switch (pv)
		{
		case UIWidget.Pivot.Top:
		case UIWidget.Pivot.Center:
		case UIWidget.Pivot.Bottom:
			zero.x = 0.5f;
			break;
		case UIWidget.Pivot.TopRight:
		case UIWidget.Pivot.Right:
		case UIWidget.Pivot.BottomRight:
			zero.x = 1f;
			break;
		default:
			zero.x = 0f;
			break;
		}
		switch (pv)
		{
		case UIWidget.Pivot.Left:
		case UIWidget.Pivot.Center:
		case UIWidget.Pivot.Right:
			zero.y = 0.5f;
			break;
		case UIWidget.Pivot.TopLeft:
		case UIWidget.Pivot.Top:
		case UIWidget.Pivot.TopRight:
			zero.y = 1f;
			break;
		default:
			zero.y = 0f;
			break;
		}
		return zero;
	}

	public static UIWidget.Pivot GetPivot(Vector2 offset)
	{
		if (offset.x == 0f)
		{
			if (offset.y == 0f)
			{
				return UIWidget.Pivot.BottomLeft;
			}
			if (offset.y == 1f)
			{
				return UIWidget.Pivot.TopLeft;
			}
			return UIWidget.Pivot.Left;
		}
		if (offset.x == 1f)
		{
			if (offset.y == 0f)
			{
				return UIWidget.Pivot.BottomRight;
			}
			if (offset.y == 1f)
			{
				return UIWidget.Pivot.TopRight;
			}
			return UIWidget.Pivot.Right;
		}
		if (offset.y == 0f)
		{
			return UIWidget.Pivot.Bottom;
		}
		if (offset.y == 1f)
		{
			return UIWidget.Pivot.Top;
		}
		return UIWidget.Pivot.Center;
	}

	public static void MoveWidget(UIRect w, float x, float y)
	{
		MoveRect(w, x, y);
	}

	public static void MoveRect(UIRect rect, float x, float y)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(x + 0.5f);
		int num2 = Mathf.FloorToInt(y + 0.5f);
		Transform cachedTransform = rect.cachedTransform;
		cachedTransform.localPosition += new Vector3((float)num, (float)num2);
		int num3 = 0;
		if (Object.op_Implicit((Object)(object)rect.leftAnchor.target))
		{
			num3++;
			rect.leftAnchor.absolute += num;
		}
		if (Object.op_Implicit((Object)(object)rect.rightAnchor.target))
		{
			num3++;
			rect.rightAnchor.absolute += num;
		}
		if (Object.op_Implicit((Object)(object)rect.bottomAnchor.target))
		{
			num3++;
			rect.bottomAnchor.absolute += num2;
		}
		if (Object.op_Implicit((Object)(object)rect.topAnchor.target))
		{
			num3++;
			rect.topAnchor.absolute += num2;
		}
		if (num3 != 0)
		{
			rect.UpdateAnchors();
		}
	}

	public static void ResizeWidget(UIWidget w, UIWidget.Pivot pivot, float x, float y, int minWidth, int minHeight)
	{
		ResizeWidget(w, pivot, x, y, 2, 2, 100000, 100000);
	}

	public static void ResizeWidget(UIWidget w, UIWidget.Pivot pivot, float x, float y, int minWidth, int minHeight, int maxWidth, int maxHeight)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (pivot == UIWidget.Pivot.Center)
		{
			int num = Mathf.RoundToInt(x - (float)w.width);
			int num2 = Mathf.RoundToInt(y - (float)w.height);
			num -= num & 1;
			num2 -= num2 & 1;
			if ((num | num2) != 0)
			{
				num >>= 1;
				num2 >>= 1;
				AdjustWidget(w, -num, -num2, num, num2, minWidth, minHeight);
			}
			return;
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(x, y);
		val = Quaternion.Inverse(w.cachedTransform.localRotation) * val;
		switch (pivot)
		{
		case UIWidget.Pivot.BottomLeft:
			AdjustWidget(w, val.x, val.y, 0f, 0f, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.Left:
			AdjustWidget(w, val.x, 0f, 0f, 0f, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.TopLeft:
			AdjustWidget(w, val.x, 0f, 0f, val.y, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.Top:
			AdjustWidget(w, 0f, 0f, 0f, val.y, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.TopRight:
			AdjustWidget(w, 0f, 0f, val.x, val.y, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.Right:
			AdjustWidget(w, 0f, 0f, val.x, 0f, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.BottomRight:
			AdjustWidget(w, 0f, val.y, val.x, 0f, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.Bottom:
			AdjustWidget(w, 0f, val.y, 0f, 0f, minWidth, minHeight, maxWidth, maxHeight);
			break;
		case UIWidget.Pivot.Center:
			break;
		}
	}

	public static void AdjustWidget(UIWidget w, float left, float bottom, float right, float top)
	{
		AdjustWidget(w, left, bottom, right, top, 2, 2, 100000, 100000);
	}

	public static void AdjustWidget(UIWidget w, float left, float bottom, float right, float top, int minWidth, int minHeight)
	{
		AdjustWidget(w, left, bottom, right, top, minWidth, minHeight, 100000, 100000);
	}

	public static void AdjustWidget(UIWidget w, float left, float bottom, float right, float top, int minWidth, int minHeight, int maxWidth, int maxHeight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_0628: Unknown result type (might be due to invalid IL or missing references)
		Vector2 pivotOffset = w.pivotOffset;
		Transform cachedTransform = w.cachedTransform;
		Quaternion localRotation = cachedTransform.localRotation;
		int num = Mathf.FloorToInt(left + 0.5f);
		int num2 = Mathf.FloorToInt(bottom + 0.5f);
		int num3 = Mathf.FloorToInt(right + 0.5f);
		int num4 = Mathf.FloorToInt(top + 0.5f);
		if (pivotOffset.x == 0.5f && (num == 0 || num3 == 0))
		{
			num = num >> 1 << 1;
			num3 = num3 >> 1 << 1;
		}
		if (pivotOffset.y == 0.5f && (num2 == 0 || num4 == 0))
		{
			num2 = num2 >> 1 << 1;
			num4 = num4 >> 1 << 1;
		}
		Vector3 val = localRotation * new Vector3((float)num, (float)num4);
		Vector3 val2 = localRotation * new Vector3((float)num3, (float)num4);
		Vector3 val3 = localRotation * new Vector3((float)num, (float)num2);
		Vector3 val4 = localRotation * new Vector3((float)num3, (float)num2);
		Vector3 val5 = localRotation * new Vector3((float)num, 0f);
		Vector3 val6 = localRotation * new Vector3((float)num3, 0f);
		Vector3 val7 = localRotation * new Vector3(0f, (float)num4);
		Vector3 val8 = localRotation * new Vector3(0f, (float)num2);
		Vector3 zero = Vector3.zero;
		if (pivotOffset.x == 0f && pivotOffset.y == 1f)
		{
			zero.x = val.x;
			zero.y = val.y;
		}
		else if (pivotOffset.x == 1f && pivotOffset.y == 0f)
		{
			zero.x = val4.x;
			zero.y = val4.y;
		}
		else if (pivotOffset.x == 0f && pivotOffset.y == 0f)
		{
			zero.x = val3.x;
			zero.y = val3.y;
		}
		else if (pivotOffset.x == 1f && pivotOffset.y == 1f)
		{
			zero.x = val2.x;
			zero.y = val2.y;
		}
		else if (pivotOffset.x == 0f && pivotOffset.y == 0.5f)
		{
			zero.x = val5.x + (val7.x + val8.x) * 0.5f;
			zero.y = val5.y + (val7.y + val8.y) * 0.5f;
		}
		else if (pivotOffset.x == 1f && pivotOffset.y == 0.5f)
		{
			zero.x = val6.x + (val7.x + val8.x) * 0.5f;
			zero.y = val6.y + (val7.y + val8.y) * 0.5f;
		}
		else if (pivotOffset.x == 0.5f && pivotOffset.y == 1f)
		{
			zero.x = val7.x + (val5.x + val6.x) * 0.5f;
			zero.y = val7.y + (val5.y + val6.y) * 0.5f;
		}
		else if (pivotOffset.x == 0.5f && pivotOffset.y == 0f)
		{
			zero.x = val8.x + (val5.x + val6.x) * 0.5f;
			zero.y = val8.y + (val5.y + val6.y) * 0.5f;
		}
		else if (pivotOffset.x == 0.5f && pivotOffset.y == 0.5f)
		{
			zero.x = (val5.x + val6.x + val7.x + val8.x) * 0.5f;
			zero.y = (val7.y + val8.y + val5.y + val6.y) * 0.5f;
		}
		minWidth = Mathf.Max(minWidth, w.minWidth);
		minHeight = Mathf.Max(minHeight, w.minHeight);
		int num5 = w.width + num3 - num;
		int num6 = w.height + num4 - num2;
		Vector3 zero2 = Vector3.zero;
		int num7 = num5;
		if (num5 < minWidth)
		{
			num7 = minWidth;
		}
		else if (num5 > maxWidth)
		{
			num7 = maxWidth;
		}
		if (num5 != num7)
		{
			if (num != 0)
			{
				zero2.x -= Mathf.Lerp((float)(num7 - num5), 0f, pivotOffset.x);
			}
			else
			{
				zero2.x += Mathf.Lerp(0f, (float)(num7 - num5), pivotOffset.x);
			}
			num5 = num7;
		}
		int num8 = num6;
		if (num6 < minHeight)
		{
			num8 = minHeight;
		}
		else if (num6 > maxHeight)
		{
			num8 = maxHeight;
		}
		if (num6 != num8)
		{
			if (num2 != 0)
			{
				zero2.y -= Mathf.Lerp((float)(num8 - num6), 0f, pivotOffset.y);
			}
			else
			{
				zero2.y += Mathf.Lerp(0f, (float)(num8 - num6), pivotOffset.y);
			}
			num6 = num8;
		}
		if (pivotOffset.x == 0.5f)
		{
			num5 = num5 >> 1 << 1;
		}
		if (pivotOffset.y == 0.5f)
		{
			num6 = num6 >> 1 << 1;
		}
		Vector3 val10 = (cachedTransform.localPosition = cachedTransform.localPosition + zero + localRotation * zero2);
		w.SetDimensions(num5, num6);
		if (w.isAnchored)
		{
			cachedTransform = cachedTransform.parent;
			float num9 = val10.x - pivotOffset.x * (float)num5;
			float num10 = val10.y - pivotOffset.y * (float)num6;
			if (Object.op_Implicit((Object)(object)w.leftAnchor.target))
			{
				w.leftAnchor.SetHorizontal(cachedTransform, num9);
			}
			if (Object.op_Implicit((Object)(object)w.rightAnchor.target))
			{
				w.rightAnchor.SetHorizontal(cachedTransform, num9 + (float)num5);
			}
			if (Object.op_Implicit((Object)(object)w.bottomAnchor.target))
			{
				w.bottomAnchor.SetVertical(cachedTransform, num10);
			}
			if (Object.op_Implicit((Object)(object)w.topAnchor.target))
			{
				w.topAnchor.SetVertical(cachedTransform, num10 + (float)num6);
			}
		}
	}

	public static int AdjustByDPI(float height)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		float num = Screen.dpi;
		RuntimePlatform platform = Application.platform;
		if (num == 0f)
		{
			num = (((int)platform != 11 && (int)platform != 8) ? 96f : 160f);
		}
		int num2 = Mathf.RoundToInt(height * (96f / num));
		if ((num2 & 1) == 1)
		{
			num2++;
		}
		return num2;
	}

	public static Vector2 ScreenToPixels(Vector2 pos, Transform relativeTo)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		int layer = ((Component)relativeTo).gameObject.layer;
		Camera val = NGUITools.FindCameraForLayer(layer);
		if ((Object)(object)val == (Object)null)
		{
			return pos;
		}
		Vector3 val2 = val.ScreenToWorldPoint(Vector2.op_Implicit(pos));
		return Vector2.op_Implicit(relativeTo.InverseTransformPoint(val2));
	}

	public static Vector2 ScreenToParentPixels(Vector2 pos, Transform relativeTo)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		int layer = ((Component)relativeTo).gameObject.layer;
		if ((Object)(object)relativeTo.parent != (Object)null)
		{
			relativeTo = relativeTo.parent;
		}
		Camera val = NGUITools.FindCameraForLayer(layer);
		if ((Object)(object)val == (Object)null)
		{
			return pos;
		}
		Vector3 val2 = val.ScreenToWorldPoint(Vector2.op_Implicit(pos));
		return Vector2.op_Implicit((!((Object)(object)relativeTo != (Object)null)) ? val2 : relativeTo.InverseTransformPoint(val2));
	}

	public static Vector3 WorldToLocalPoint(Vector3 worldPos, Camera worldCam, Camera uiCam, Transform relativeTo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		worldPos = worldCam.WorldToViewportPoint(worldPos);
		worldPos = uiCam.ViewportToWorldPoint(worldPos);
		if ((Object)(object)relativeTo == (Object)null)
		{
			return worldPos;
		}
		relativeTo = relativeTo.parent;
		if ((Object)(object)relativeTo == (Object)null)
		{
			return worldPos;
		}
		return relativeTo.InverseTransformPoint(worldPos);
	}

	public static void OverlayPosition(this Transform trans, Vector3 worldPos, Camera worldCam, Camera myCam)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		worldPos = worldCam.WorldToViewportPoint(worldPos);
		worldPos = myCam.ViewportToWorldPoint(worldPos);
		Transform parent = trans.parent;
		trans.localPosition = ((!((Object)(object)parent != (Object)null)) ? worldPos : parent.InverseTransformPoint(worldPos));
	}

	public static void OverlayPosition(this Transform trans, Vector3 worldPos, Camera worldCam)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Camera val = NGUITools.FindCameraForLayer(((Component)trans).gameObject.layer);
		if ((Object)(object)val != (Object)null)
		{
			trans.OverlayPosition(worldPos, worldCam, val);
		}
	}

	public static void OverlayPosition(this Transform trans, Transform target)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Camera val = NGUITools.FindCameraForLayer(((Component)trans).gameObject.layer);
		Camera val2 = NGUITools.FindCameraForLayer(((Component)target).gameObject.layer);
		if ((Object)(object)val != (Object)null && (Object)(object)val2 != (Object)null)
		{
			trans.OverlayPosition(target.position, val2, val);
		}
	}
}
