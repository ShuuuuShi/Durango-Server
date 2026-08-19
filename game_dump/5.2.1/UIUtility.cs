using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Durango.System;
using Durango.UI;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public static class UIUtility
{
	public struct Separators
	{
		public ListObjectPool<UISprite> List;

		public int? Size;

		public bool Left;

		public bool Right;

		public bool Bottom;

		public bool Top;

		public static implicit operator Separators(ListObjectPool<UISprite> value)
		{
			Separators result = default(Separators);
			result.List = value;
			return result;
		}
	}

	private static Stack<Transform> mStack = new Stack<Transform>();

	public static UIWidget SetScrollViewInvisibleBox(UIScrollView scrollView, UIWidget box = null)
	{
		if (scrollView == null)
		{
			return null;
		}
		UIPanel uIPanel = scrollView.panel;
		if (uIPanel == null)
		{
			uIPanel = scrollView.GetComponent<UIPanel>();
		}
		if (uIPanel == null)
		{
			return null;
		}
		if (box != null)
		{
			box.gameObject.SetActive(value: false);
		}
		bool enabled = scrollView.enabled;
		scrollView.enabled = true;
		scrollView.ResetPosition();
		scrollView.enabled = enabled;
		if (box == null)
		{
			box = scrollView.gameObject.AddChild<UIWidget>();
		}
		box.gameObject.SetActive(value: true);
		Vector4 finalClipRegion = uIPanel.finalClipRegion;
		box.transform.localPosition = new Vector3(finalClipRegion.x, finalClipRegion.y);
		Vector2 vector = PanelInnerSize(uIPanel);
		box.width = (int)vector.x;
		box.height = (int)vector.y;
		box.depth = 0;
		return box;
	}

	public static Vector2 PanelInnerSize(UIPanel panel)
	{
		Vector2 result = default(Vector2);
		result.x = panel.width - ((!panel.softBorderPadding) ? 0f : (panel.clipSoftness.x * 2f));
		result.y = panel.height - ((!panel.softBorderPadding) ? 0f : (panel.clipSoftness.y * 2f));
		return result;
	}

	public static void ResizeToSquare(UISprite sprite)
	{
		ResizeToSquare(sprite, Mathf.Max(sprite.width, sprite.height));
	}

	public static void ResizeToSquare(UISprite sprite, int length)
	{
		UISpriteData atlasSprite = sprite.GetAtlasSprite();
		if (atlasSprite == null)
		{
			sprite.width = length;
			sprite.height = length;
			return;
		}
		int num = atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight;
		int num2 = atlasSprite.height + atlasSprite.paddingTop + atlasSprite.paddingBottom;
		int num3 = Mathf.Max(num, num2);
		float num4 = ((length != 0) ? ((float)length / (float)num3) : 1f);
		sprite.SetDimensions(Mathf.RoundToInt((float)num * num4), Mathf.RoundToInt((float)num2 * num4));
	}

	public static void ResizeWidth(UISprite sprite, int width)
	{
		UISpriteData atlasSprite = sprite.GetAtlasSprite();
		int h;
		if (atlasSprite == null)
		{
			h = width;
		}
		else
		{
			float num = (float)(atlasSprite.height + atlasSprite.paddingBottom + atlasSprite.paddingTop) / (float)(atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight);
			h = Mathf.RoundToInt((float)sprite.height * num);
		}
		sprite.SetDimensions(width, h);
	}

	public static void ResizeHeight(UISprite sprite, int height)
	{
		UISpriteData atlasSprite = sprite.GetAtlasSprite();
		int w;
		if (atlasSprite == null)
		{
			w = height;
		}
		else
		{
			float num = (float)(atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight) / (float)(atlasSprite.height + atlasSprite.paddingBottom + atlasSprite.paddingTop);
			w = Mathf.RoundToInt((float)sprite.height * num);
		}
		sprite.SetDimensions(w, height);
	}

	public static void UpdateAnchors(Transform target)
	{
		if (target == null)
		{
			return;
		}
		mStack.Clear();
		Stack<Transform> stack = mStack;
		stack.Push(target);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			UIRect component = transform.GetComponent<UIRect>();
			if (component != null)
			{
				component.UpdateAnchors();
			}
			for (int num = transform.childCount - 1; num >= 0; num--)
			{
				stack.Push(transform.GetChild(num));
			}
		}
	}

	public static void ResetAndUpdateAnchors(Transform target)
	{
		if (target == null)
		{
			return;
		}
		mStack.Clear();
		Stack<Transform> stack = mStack;
		stack.Push(target);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			UIRect component = transform.GetComponent<UIRect>();
			if (component != null)
			{
				component.ResetAndUpdateAnchors();
			}
			else
			{
				UIAnchor component2 = transform.GetComponent<UIAnchor>();
				if (component2 != null)
				{
					component2.enabled = true;
				}
			}
			for (int num = transform.childCount - 1; num >= 0; num--)
			{
				stack.Push(transform.GetChild(num));
			}
		}
	}

	public static T GetValueByPercentage<T>(int percentage, int[] percentages, T[] values)
	{
		int num = Math.Min(percentages.Length, values.Length);
		if (num <= 0)
		{
			throw new ArgumentException();
		}
		for (int i = 0; i < num; i++)
		{
			if (percentage >= percentages[i])
			{
				return values[i];
			}
		}
		return values[num - 1];
	}

	public static float WidgetsReposition<T>(IEnumerable<T> widgets, UIWidget container, Vector3 vector, float margin = 0f, float pivot = 0f, bool instant = true)
	{
		if (widgets == null)
		{
			return 0f;
		}
		Vector3 localCenter = container.localCenter;
		Vector3 zero = Vector3.zero;
		zero.x = localCenter.x - (float)container.width * vector.x * 0.5f;
		zero.y = localCenter.y - (float)container.height * vector.y * 0.5f;
		return WidgetsReposition(widgets, vector, zero, margin, pivot, instant);
	}

	public static float WidgetsReposition<T>(IEnumerable<T> widgets, Vector3 vector, float margin = 0f, bool instant = true)
	{
		if (widgets == null)
		{
			return 0f;
		}
		T val = default(T);
		bool flag = false;
		using (IEnumerator<T> enumerator = widgets.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				val = enumerator.Current;
				flag = true;
			}
		}
		if (!flag)
		{
			return 0f;
		}
		UIWidget widget = GetWidget(val);
		Vector3 vector2 = widget.localCenter + widget.transform.localPosition;
		Vector3 zero = Vector3.zero;
		zero.x = vector2.x - (float)widget.width * vector.x * 0.5f;
		zero.y = vector2.y - (float)widget.height * vector.y * 0.5f;
		return WidgetsReposition(widgets, vector, zero, margin, 0f, instant);
	}

	public static float WidgetsReposition<T>(IEnumerable<T> widgets, Vector3 vector, Vector3 basePos, float margin = 0f, float pivot = 0f, bool instant = true)
	{
		if (widgets == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (T widget3 in widgets)
		{
			UIWidget widget = GetWidget(widget3);
			if (!(widget == null) && IsVisibleWidget(widget))
			{
				Vector2 pivotOffset = widget.pivotOffset;
				Vector3 vector2 = basePos;
				if (Math.Abs(vector.x) > 0f)
				{
					vector2.x = basePos.x + num * vector.x;
					vector2.x += (float)widget.width * ((!(vector.x > 0f)) ? (pivotOffset.x - 1f) : pivotOffset.x);
					vector2.y += (pivotOffset.y - 0.5f) * (float)widget.height;
					num += (float)widget.width + margin;
				}
				if (Math.Abs(vector.y) > 0f)
				{
					vector2.y = basePos.y + num * vector.y;
					vector2.y += (float)widget.height * ((!(vector.y > 0f)) ? (pivotOffset.y - 1f) : pivotOffset.y);
					vector2.x += (pivotOffset.x - 0.5f) * (float)widget.width;
					num += (float)widget.height + margin;
				}
				if (instant || !widget.gameObject.activeInHierarchy)
				{
					widget.SetEnable<TweenPosition>(enable: false);
					widget.transform.localPosition = vector2;
				}
				else
				{
					TweenPosition.Begin(widget.gameObject, 0.2f, vector2);
				}
			}
		}
		float num2 = Mathf.Max(0f, num - margin);
		if (pivot != 0f)
		{
			foreach (T widget4 in widgets)
			{
				UIWidget widget2 = GetWidget(widget4);
				if (!(widget2 == null) && IsVisibleWidget(widget2))
				{
					widget2.transform.localPosition -= vector * num2 * pivot;
				}
			}
		}
		return num2;
	}

	public static UIWidget GetWidget(object obj)
	{
		UIWidget uIWidget = obj as UIWidget;
		if (uIWidget == null)
		{
			GameObject gameObject = obj as GameObject;
			if (gameObject != null)
			{
				uIWidget = gameObject.GetComponent<UIWidget>();
			}
			else
			{
				Component component = obj as Component;
				if (component != null)
				{
					uIWidget = component.GetComponent<UIWidget>();
				}
			}
		}
		return uIWidget;
	}

	public static float GetSize(Vector2 size, Vector2 vc)
	{
		return Mathf.Abs(size.x * vc.x + size.y * vc.y);
	}

	public static float GetBreadth(Vector2 size, Vector2 vc)
	{
		return GetSize(size, new Vector2(vc.y, vc.x));
	}

	public static Vector2 WidgetsGridReposition<T>(IEnumerable<T> nodes, ListObjectPool spliter, Vector2 dir, Vector3 basePos, float breadth, Vector2 baseNodeSize, float rowMargin, float colMargin, float rowPivot = 0f, Vector2? pivot = null, bool instant = true)
	{
		int rowItemCount;
		float rowSize;
		float colSize;
		return WidgetsGridReposition(nodes, spliter, dir, basePos, breadth, baseNodeSize, rowMargin, colMargin, out rowItemCount, out rowSize, out colSize, rowPivot, pivot, instant);
	}

	public static Vector2 WidgetsGridReposition<T>(IEnumerable<T> nodes, ListObjectPool spliter, Vector2 dir, Vector3 basePos, float breadth, Vector2 baseNodeSize, float rowMargin, float colMargin, out int rowItemCount, out float rowSize, out float colSize, float rowPivot = 0f, Vector2? pivot = null, bool instant = true)
	{
		if (nodes == null)
		{
			rowItemCount = 0;
			rowSize = 0f;
			colSize = 0f;
			return Vector2.zero;
		}
		Vector3 vector = ((dir.x != 0f) ? Vector3.down : Vector3.right);
		Vector3 vector2 = dir;
		if (Mathf.Abs(vector2.x) != 1f && Mathf.Abs(vector2.y) != 1f)
		{
			rowItemCount = 0;
			rowSize = 0f;
			colSize = 0f;
			return Vector2.zero;
		}
		Vector2 vector3;
		if (vector2.x == 1f)
		{
			vector3 = new Vector2(0f, 1f);
		}
		else if (vector2.x == -1f)
		{
			vector3 = new Vector2(1f, 1f);
		}
		else if (vector2.y == 1f)
		{
			vector3 = new Vector2(0f, 0f);
		}
		else
		{
			if (vector2.y != -1f)
			{
				rowItemCount = 0;
				rowSize = 0f;
				colSize = 0f;
				return Vector2.zero;
			}
			vector3 = new Vector2(0f, 1f);
		}
		int num = 0;
		foreach (T node in nodes)
		{
			if (IsVisibleWidget(GetWidget(node)))
			{
				num++;
			}
		}
		float size = GetSize(baseNodeSize, vector);
		float size2 = GetSize(baseNodeSize, vector2);
		rowItemCount = Mathf.Max(1, Mathf.RoundToInt((breadth + rowMargin) / (size + rowMargin)));
		float num2 = (breadth - rowMargin * (float)(rowItemCount - 1)) / ((float)rowItemCount * size);
		size *= num2;
		size2 *= num2;
		rowItemCount = Mathf.Min(rowItemCount, num);
		int num3 = ((rowItemCount > 0) ? Mathf.CeilToInt((float)num / (float)rowItemCount) : 0);
		rowSize = size;
		colSize = size2;
		float num4 = (float)num3 * colSize + (float)(num3 - 1) * colMargin;
		float num5 = (rowSize + rowMargin) * (float)rowItemCount - rowMargin;
		Vector2 result = ((dir.x != 0f) ? new Vector2(num4, num5) : new Vector2(num5, num4));
		if (pivot.HasValue)
		{
			Vector2 vector4 = vector3 - pivot.Value;
			basePos += new Vector3(result.x * vector4.x, result.y * vector4.y);
		}
		Vector2 vector5 = vector * rowSize + vector2 * colSize;
		vector5.x = Mathf.Abs(vector5.x);
		vector5.y = Mathf.Abs(vector5.y);
		int num6 = 0;
		foreach (T node2 in nodes)
		{
			UIWidget widget = GetWidget(node2);
			if (IsVisibleWidget(widget))
			{
				widget.gameObject.transform.localScale = Vector3.one * num2;
				int num7 = num6 % rowItemCount;
				int num8 = num6 / rowItemCount;
				Vector2 vector6 = widget.pivotOffset - vector3;
				Vector3 vector7 = basePos + vector * (rowSize + rowMargin) * num7 + vector2 * (colSize + colMargin) * num8;
				vector7.x += vector5.x * vector6.x;
				vector7.y += vector5.y * vector6.y;
				if (rowPivot != 0f)
				{
					int num9 = Mathf.Max(0, (num8 + 1) * rowItemCount - num);
					vector7 += vector * num9 * (rowSize + rowMargin) * rowPivot;
				}
				if (instant)
				{
					widget.transform.localPosition = vector7;
				}
				else
				{
					TweenPosition.Begin(widget.gameObject, 0.2f, vector7);
				}
				num6++;
			}
		}
		if (spliter != null && spliter.BaseObject != null)
		{
			int num10 = Mathf.Max(Mathf.CeilToInt((float)num / (float)rowItemCount), 0);
			spliter.Set(Mathf.Max(0, num10 - 1));
			float size3 = GetSize(spliter.BaseObject.GetComponent<UIWidget>().localSize, vector2);
			int num11 = (int)((vector2.x != 0f) ? size3 : breadth);
			int num12 = (int)((vector2.x != 0f) ? breadth : size3);
			int i = 0;
			for (int count = spliter.Count; i < count; i++)
			{
				UIWidget component = spliter[i].GetComponent<UIWidget>();
				component.width = num11;
				component.height = num12;
				Vector2 vector8 = component.pivotOffset - vector3;
				Vector3 localPosition = basePos + vector2 * ((float)(i + 1) * colSize - colMargin * 0.5f);
				localPosition.x += (float)num11 * vector8.x;
				localPosition.y += (float)num12 * vector8.y;
				component.transform.localPosition = localPosition;
			}
		}
		return result;
	}

	public static bool IsVisibleWidget(UIWidget widget)
	{
		if (widget.gameObject.activeSelf)
		{
			if (widget.alpha > 0f)
			{
				return true;
			}
			TweenAlpha component = widget.GetComponent<TweenAlpha>();
			if (component != null && component.enabled)
			{
				return true;
			}
			TweenerPlayer component2 = widget.GetComponent<TweenerPlayer>();
			if (component2 != null && component2.enabled)
			{
				return true;
			}
		}
		return false;
	}

	private static Color32 GetTextureAreaColor(Texture2D tex, Rect rect)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		int i = Mathf.FloorToInt(rect.xMin);
		for (int num7 = Mathf.CeilToInt(rect.xMax); i < num7; i++)
		{
			int j = Mathf.FloorToInt(rect.yMin);
			for (int num8 = Mathf.CeilToInt(rect.yMax); j < num8; j++)
			{
				float num9 = 1f;
				num9 *= Mathf.Min(i + 1, rect.xMax) - Mathf.Max(i, rect.xMin);
				num9 *= Mathf.Min(j + 1, rect.yMax) - Mathf.Max(j, rect.yMin);
				Color color = tex.GetPixel(i, j) * num9;
				if (color.a > 0f)
				{
					num5 += num9;
					num += color.r;
					num2 += color.g;
					num3 += color.b;
				}
				num6 += num9;
				num4 += color.a;
			}
		}
		num /= num5;
		num2 /= num5;
		num3 /= num5;
		num4 /= num6;
		return new Color(num, num2, num3, num4);
	}

	public static Color32[] ResizeTexturePixels(Texture2D texture, int width, int height)
	{
		return ResizeTexturePixels(texture, new Rect(0f, 0f, 1f, 1f), width, height);
	}

	public static Color32[] ResizeTexturePixels(Texture2D texture, Rect uv, int width, int height)
	{
		TextureWrapMode wrapMode = texture.wrapMode;
		texture.wrapMode = TextureWrapMode.Clamp;
		Color32[] array = new Color32[width * height];
		Rect rect = new Rect(uv.x * (float)texture.width, uv.y * (float)texture.height, uv.width * (float)texture.width, uv.height * (float)texture.height);
		float width2 = rect.width;
		float height2 = rect.height;
		float num = Mathf.Max(width2 / (float)width, height2 / (float)height);
		float num2 = ((float)width * num - width2) * 0.5f;
		float num3 = ((float)height * num - height2) * 0.5f;
		rect.x -= num2;
		rect.width += num2;
		rect.y -= num3;
		rect.height += num3;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Color color = GetTextureAreaColor(texture, new Rect(rect.xMin + (float)i * num, rect.yMin + (float)j * num, 1f, 1f));
				ref Color32 reference = ref array[i + j * width];
				reference = color;
			}
		}
		texture.wrapMode = wrapMode;
		return array;
	}

	public static Color32[] RemoveSpace(Color32[] pixels, ref int width, ref int height, out Rect rect)
	{
		Rect nonespaceArea = GetNonespaceArea(pixels, width, height);
		int num = (int)nonespaceArea.width + 1;
		int num2 = (int)nonespaceArea.height + 1;
		if (num < 0 || num2 < 0)
		{
			rect = new Rect(0f, 0f, 1f / (float)width, 1f / (float)height);
			width = 1;
			height = 1;
			return new Color32[1]
			{
				new Color32(0, 0, 0, 0)
			};
		}
		Color32[] array = new Color32[num * num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				int num4 = ((int)nonespaceArea.yMin + i) * width + ((int)nonespaceArea.xMin + j);
				ref Color32 reference = ref array[num3];
				reference = pixels[num4];
			}
		}
		rect = new Rect(nonespaceArea.xMin, nonespaceArea.yMin, num, num2);
		rect = DivideRect(rect, width, height);
		width = num;
		height = num2;
		return array;
	}

	public static Rect GetNonespaceArea(Color32[] pixels, int width, int height)
	{
		int num = width;
		int num2 = 0;
		int num3 = height;
		int num4 = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (pixels[i * width + j].a > 0)
				{
					if (i < num3)
					{
						num3 = i;
					}
					if (i > num4)
					{
						num4 = i;
					}
					if (j < num)
					{
						num = j;
					}
					if (j > num2)
					{
						num2 = j;
					}
				}
			}
		}
		return new Rect(num, num3, num2 - num, num4 - num3);
	}

	public static Rect DivideRect(Rect rect, float x, float y)
	{
		rect.x /= x;
		rect.y /= y;
		rect.width /= x;
		rect.height /= y;
		return rect;
	}

	public static bool IsUrl(string text)
	{
		return Regex.Match(text, "^https?://", RegexOptions.IgnoreCase).Success;
	}

	public static float[] RGBtoHSV(Color col)
	{
		float r = col.r;
		float g = col.g;
		float b = col.b;
		float[] array = new float[3];
		float num = Maths.Min(r, g, b);
		float num2 = (array[2] = Maths.Max(r, g, b));
		float num3 = num2 - num;
		if (num2 != 0f)
		{
			array[1] = num3 / num2;
			if (r == num2)
			{
				array[0] = (g - b) / num3;
			}
			else if (g == num2)
			{
				array[0] = 2f + (b - r) / num3;
			}
			else
			{
				array[0] = 4f + (r - g) / num3;
			}
			array[0] *= 60f;
			if (array[0] < 0f)
			{
				array[0] += 360f;
			}
			return array;
		}
		array[1] = 0f;
		array[0] = -1f;
		return array;
	}

	public static int ColorComparison(Color color1, Color color2)
	{
		if (color1 == color2)
		{
			return 0;
		}
		Vector3Int seperatedHsvColor = GetSeperatedHsvColor(color1);
		Vector3Int seperatedHsvColor2 = GetSeperatedHsvColor(color2);
		if (seperatedHsvColor.x - seperatedHsvColor2.x == 0)
		{
			if (seperatedHsvColor.y - seperatedHsvColor2.y == 0)
			{
				if (seperatedHsvColor.z - seperatedHsvColor2.z == 0)
				{
					return 0;
				}
				return seperatedHsvColor.z - seperatedHsvColor2.z;
			}
			return seperatedHsvColor.y - seperatedHsvColor2.y;
		}
		return seperatedHsvColor.x - seperatedHsvColor2.x;
	}

	private static Vector3Int GetSeperatedHsvColor(Color color)
	{
		float num = 0.299f * color.r * color.r + 0.587f * color.g * color.g + 0.144f * color.b * color.b;
		float[] array = RGBtoHSV(color);
		int num2 = (int)(array[0] * 8f);
		int num3 = (int)(num * 8f);
		int num4 = (int)(array[1] * 4f);
		if (num2 % 2 == 1)
		{
			num3 = 8 - num3;
			num4 = 4 - num4;
		}
		return new Vector3Int(num2, num3, num4);
	}

	public static void SetPosition(this UIWidget widget, Vector3 pos, float pivotX, float pivotY)
	{
		widget.SetPosition(pos, new Vector2(pivotX, pivotY));
	}

	public static void SetPosition(this UIWidget widget, Vector3 pos, Vector2 pivot)
	{
		Vector2 vector = pivot - widget.pivotOffset;
		widget.transform.localPosition = pos - Vector3.Scale(vector, widget.localSize);
	}

	public static Vector3 GetPosition(this UIWidget widget, float pivotX, float pivotY)
	{
		return widget.GetPosition(new Vector2(pivotX, pivotY));
	}

	public static Vector3 GetPosition(this UIWidget widget, Vector2 pivot)
	{
		Vector3 localPosition = widget.transform.localPosition;
		Vector2 vector = pivot - widget.pivotOffset;
		return localPosition + Vector3.Scale(vector, widget.localSize);
	}

	public static Vector3 GetLocalPosition(this UIWidget widget, float pivotX, float pivotY)
	{
		return Vector3.Scale(new Vector2(pivotX, pivotY) - widget.pivotOffset, widget.localSize);
	}

	public static void Resize(this UIWidget widget, Point2 size, Vector2 pivot)
	{
		Vector3 position = widget.GetPosition(pivot);
		widget.SetDimensions(size.x, size.y);
		widget.SetPosition(position, pivot);
	}

	public static T SetEnable<T>(this Component comp, bool enable) where T : Behaviour
	{
		T component = comp.GetComponent<T>();
		if (component == null)
		{
			return null;
		}
		component.enabled = enable;
		return component;
	}

	public static void MakeGridBackground(Vector3 pos, Vector2 pivot, float width, float height, Vector2 gridSize, [NotNull] UISprite sprite)
	{
		UISpriteData atlasSprite = sprite.GetAtlasSprite();
		Vector3 one = Vector3.one;
		one.x = gridSize.x / (float)atlasSprite.width;
		one.y = gridSize.y / (float)atlasSprite.height;
		sprite.SetDimensions((int)(width / one.x), (int)(height / one.y));
		sprite.transform.localScale = one;
		sprite.SetPosition(pos, pivot);
	}

	public static void MakeGridBackground(Vector3 pos, Vector2 pivot, float width, float height, Vector2 gridSize, Separators separators)
	{
		if (separators.List == null)
		{
			return;
		}
		int num = ((gridSize.x > 0f) ? Mathf.Max(0, Mathf.RoundToInt(width / gridSize.x) + 1) : 0);
		int num2 = ((gridSize.y > 0f) ? Mathf.Max(0, Mathf.RoundToInt(height / gridSize.y) + 1) : 0);
		if (!separators.Right)
		{
			num--;
		}
		if (!separators.Top)
		{
			num2--;
		}
		UISpriteData atlasSprite = separators.List.BaseObject.GetAtlasSprite();
		bool flag = atlasSprite.width < atlasSprite.height;
		int num3 = (separators.Size.HasValue ? separators.Size.Value : ((!flag) ? atlasSprite.height : atlasSprite.width));
		Vector3 vector = pos - new Vector3(pivot.x * width, pivot.y * height);
		separators.List.BeginLoad();
		for (int i = ((!separators.Left) ? 1 : 0); i < num; i++)
		{
			UISprite next = separators.List.GetNext();
			if (flag)
			{
				next.SetDimensions(num3, (int)height);
				next.pivot = UIWidget.Pivot.Bottom;
				next.transform.localEulerAngles = Vector3.zero;
			}
			else
			{
				next.SetDimensions((int)height, num3);
				next.pivot = UIWidget.Pivot.Left;
				next.transform.localEulerAngles = Vector3.forward * 90f;
			}
			next.transform.localPosition = vector + Vector3.right * i * gridSize.x;
		}
		for (int j = ((!separators.Bottom) ? 1 : 0); j < num2; j++)
		{
			UISprite next2 = separators.List.GetNext();
			if (flag)
			{
				next2.SetDimensions(num3, (int)width);
				next2.pivot = UIWidget.Pivot.Bottom;
				next2.transform.localEulerAngles = Vector3.back * 90f;
			}
			else
			{
				next2.SetDimensions((int)width, num3);
				next2.pivot = UIWidget.Pivot.Left;
				next2.transform.localEulerAngles = Vector3.zero;
			}
			next2.transform.localPosition = vector + Vector3.up * j * gridSize.y;
		}
		separators.List.EndLoad();
	}

	public static Vector3 ToRootPosition(GameObject obj)
	{
		Transform parent = obj.transform.parent;
		if (parent == null)
		{
			return Vector3.zero;
		}
		return ToRootPosition(parent.gameObject, obj.transform.localPosition);
	}

	public static Vector3 ToRootPosition(GameObject parent, Vector3 pos)
	{
		return NGUITools.GetRoot(parent).transform.InverseTransformPoint(parent.transform.TransformPoint(pos));
	}

	public static void DoPoolAsMethod<T, TU>(ref List<T> objs, IList<TU> dataList, Transform parent, Func<TU, T> selectPrefab, Action<T, TU, int> initalize) where T : Component
	{
		if (objs == null)
		{
			objs = new List<T>();
			T[] componentsInChildren = parent.GetComponentsInChildren<T>(parent);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				objs.Add(componentsInChildren[i]);
			}
		}
		DoPooling(objs, dataList, parent, selectPrefab, initalize);
		for (int j = dataList.Count; j < objs.Count; j++)
		{
			objs[j].gameObject.SetActive(value: false);
		}
		for (int k = 0; k < objs.Count; k++)
		{
			objs[k].transform.SetSiblingIndex(k);
		}
	}

	private static void DoPooling<T, TU>(IList<T> objs, IList<TU> data, Transform parent, Func<TU, T> prefabSelector, Action<T, TU, int> initalize) where T : Component
	{
		for (int i = 0; i < data.Count; i++)
		{
			TU arg = data[i];
			T val = prefabSelector(arg);
			if (i >= objs.Count)
			{
				T component = UnityEngine.Object.Instantiate(val, parent).GetComponent<T>();
				objs.Add(component);
			}
			else if (objs[i].GetType() != val.GetType())
			{
				int num = -1;
				for (int num2 = objs.Count - 1; num2 >= i + 1; num2--)
				{
					if (objs[num2].GetType() == val.GetType())
					{
						num = num2;
						break;
					}
				}
				if (num != -1)
				{
					T value = objs[i];
					objs[i] = objs[num];
					objs[num] = value;
				}
				else
				{
					objs.Add(objs[i]);
					int index = i;
					T val2 = UnityEngine.Object.Instantiate(val, parent);
					objs[index] = val2.GetComponent<T>();
				}
			}
			T val3 = objs[i];
			val3.gameObject.SetActive(value: true);
			initalize(val3, data[i], i);
		}
	}

	public static T FindComponentInParent<T>(GameObject obj)
	{
		if (obj == null)
		{
			return default(T);
		}
		Transform parent = obj.transform.parent;
		while ((bool)parent)
		{
			T component = parent.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			parent = parent.parent;
		}
		return default(T);
	}

	public static void SyncTweener(UITweener tweener, float offset = 0f)
	{
		if (tweener == null)
		{
			return;
		}
		float duration = tweener.duration;
		float num = Time.time + offset;
		switch (tweener.style)
		{
		case UITweener.Style.Once:
		case UITweener.Style.Loop:
		{
			float num3 = num % duration;
			tweener.tweenFactor = num3 / duration;
			tweener.PlayForward();
			break;
		}
		case UITweener.Style.PingPong:
		{
			float num2 = num % (duration * 2f);
			if (num2 > duration)
			{
				tweener.PlayReverse();
				tweener.tweenFactor = 1f - (num2 - duration) / duration;
			}
			else
			{
				tweener.PlayForward();
				tweener.tweenFactor = num2 / duration;
			}
			break;
		}
		}
	}

	public static void OpenUri(string title, string link)
	{
		if (IsUrl(link))
		{
			Platform.Instance.ShowWeb(title, link);
		}
		else if (link.StartsWith("ui://") && Singleton<UIManager>.HasInstance())
		{
			Singleton<UIManager>.Instance().OpenUri(link.Substring("ui://".Length));
		}
	}

	public static UIWidget GetChildSprite(UILabel label, string key)
	{
		if (!(label is UISpriteLabel uISpriteLabel))
		{
			return null;
		}
		return uISpriteLabel.GetChildWidget(key);
	}

	public static UIWidget GetChildSprite(UILabel label, int index)
	{
		if (!(label is UISpriteLabel uISpriteLabel))
		{
			return null;
		}
		return uISpriteLabel.GetChildWidget(index);
	}

	public static bool IsWidgetContainsMousePointer(UIWidget widget)
	{
		if (widget == null || !Singleton<UIManager>.HasInstance())
		{
			return false;
		}
		Rect rect = new Rect(widget.GetPosition(0f, 0f), widget.localSize);
		Vector3 point = NGUIMath.ScreenToPixels(Input.mousePosition, Singleton<UIManager>.Instance().UIRoot.transform);
		return rect.Contains(point);
	}
}
