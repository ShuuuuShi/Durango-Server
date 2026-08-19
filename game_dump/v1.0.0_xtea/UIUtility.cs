using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class UIUtility
{
	private static Stack<Transform> mStack = new Stack<Transform>();

	public static UIWidget SetScrollViewInvisibleBox(UIScrollView scrollView, UIWidget box = null)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)scrollView == (Object)null)
		{
			return null;
		}
		UIPanel uIPanel = scrollView.panel;
		if ((Object)(object)uIPanel == (Object)null)
		{
			uIPanel = ((Component)scrollView).GetComponent<UIPanel>();
		}
		if ((Object)(object)uIPanel == (Object)null)
		{
			return null;
		}
		if ((Object)(object)box != (Object)null)
		{
			((Component)box).gameObject.SetActive(false);
		}
		bool enabled = ((Behaviour)scrollView).enabled;
		((Behaviour)scrollView).enabled = true;
		scrollView.ResetPosition();
		((Behaviour)scrollView).enabled = enabled;
		if ((Object)(object)box == (Object)null)
		{
			box = ((Component)scrollView).gameObject.AddChild<UIWidget>();
		}
		((Component)box).gameObject.SetActive(true);
		Vector4 finalClipRegion = uIPanel.finalClipRegion;
		((Component)box).transform.localPosition = new Vector3(finalClipRegion.x, finalClipRegion.y);
		Vector2 val = PanelInnerSize(uIPanel);
		box.width = (int)val.x;
		box.height = (int)val.y;
		box.depth = 0;
		return box;
	}

	public static Vector2 PanelInnerSize(UIPanel panel)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
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
		float num4 = (float)length / (float)num3;
		sprite.width = (int)((float)num * num4);
		sprite.height = (int)((float)num2 * num4);
	}

	public static string GetSpriteName(UISprite sprite)
	{
		return (!((Object)(object)sprite != (Object)null)) ? "icon_question" : sprite.spriteName;
	}

	public static bool SetSpriteName(UISprite sprite, string spriteName)
	{
		if ((Object)(object)sprite != (Object)null)
		{
			if (sprite.atlas.GetSprite(spriteName) != null)
			{
				sprite.spriteName = spriteName;
				return true;
			}
			sprite.spriteName = "icon_question";
		}
		return false;
	}

	public static string GetLabelText(UILabel label)
	{
		return (!((Object)(object)label != (Object)null)) ? string.Empty : label.text;
	}

	public static void SetLabelText(UILabel label, string text)
	{
		if ((Object)(object)label != (Object)null)
		{
			label.text = text;
		}
	}

	public static void SetLabelText(UISpriteLabel label, string text)
	{
		if ((Object)(object)label != (Object)null)
		{
			label.text = text;
		}
	}

	public static void AlignRightByLabel(UIWidget widget, UILabel label, int gap)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)widget != (Object)null && (Object)(object)label != (Object)null)
		{
			Vector3 localPosition = ((Component)label).transform.localPosition;
			localPosition.x += label.printedSize.x + (float)gap;
			((Component)widget).transform.localPosition = localPosition;
		}
	}

	public static void UpdateAnchors(Transform target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		mStack.Clear();
		Stack<Transform> stack = mStack;
		stack.Push(target);
		while (stack.Count > 0)
		{
			Transform val = stack.Pop();
			UIRect component = ((Component)val).GetComponent<UIRect>();
			if ((Object)(object)component != (Object)null)
			{
				component.UpdateAnchors();
			}
			int i = 0;
			for (int childCount = val.childCount; i < childCount; i++)
			{
				stack.Push(val.GetChild(i));
			}
		}
	}

	public static void ResetAnUpdateAnchors(Transform target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		mStack.Clear();
		Stack<Transform> stack = mStack;
		stack.Push(target);
		while (stack.Count > 0)
		{
			Transform val = stack.Pop();
			UIRect component = ((Component)val).GetComponent<UIRect>();
			if ((Object)(object)component != (Object)null)
			{
				component.ResetAndUpdateAnchors();
			}
			else
			{
				UIAnchor component2 = ((Component)val).GetComponent<UIAnchor>();
				if ((Object)(object)component2 != (Object)null)
				{
					((Behaviour)component2).enabled = true;
				}
			}
			int i = 0;
			for (int childCount = val.childCount; i < childCount; i++)
			{
				stack.Push(val.GetChild(i));
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

	public unsafe static float WidgetsReposition(IList<GameObject> list, UIWidget container, Vector3 vector, float margin = 0f, bool instant = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (list == null)
		{
			return 0f;
		}
		Vector3 localCenter = container.localCenter;
		Vector3 zero = Vector3.zero;
		zero.x = localCenter.x - (float)container.width * vector.x * 0.5f;
		zero.y = localCenter.y - (float)container.height * vector.y * 0.5f;
		return WidgetsReposition(new Func<int, Object>(list, (nint)(delegate*<IList<GameObject>, int, GameObject, GameObject>)(&ListExtensions.Get)), list.Count, vector, zero, margin, instant);
	}

	public static float WidgetsReposition(Func<int, Object> getFunc, int count, Vector3 vector, UIWidget first, float margin = 0f, bool instant = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (count == 0)
		{
			return 0f;
		}
		Vector3 val = first.localCenter + ((Component)first).transform.localPosition;
		Vector3 zero = Vector3.zero;
		zero.x = val.x - (float)first.width * vector.x * 0.5f;
		zero.y = val.y - (float)first.height * vector.y * 0.5f;
		return WidgetsReposition(getFunc, count, vector, zero, margin, instant);
	}

	public static float WidgetsReposition(Func<int, Object> getFunc, int count, Vector3 vector, Vector3 basePos, float margin = 0f, bool instant = true)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		if (count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			Object val = getFunc(i);
			UIWidget uIWidget = val as UIWidget;
			if ((Object)(object)uIWidget == (Object)null)
			{
				GameObject val2 = (GameObject)(object)((val is GameObject) ? val : null);
				uIWidget = ((!((Object)(object)val2 == (Object)null)) ? val2.GetComponent<UIWidget>() : null);
			}
			if (!((Object)(object)uIWidget == (Object)null) && IsVisibleWidget(uIWidget))
			{
				Vector2 pivotOffset = uIWidget.pivotOffset;
				Vector3 localPosition = ((Component)uIWidget).transform.localPosition;
				if (Math.Abs(vector.x) > 0f)
				{
					localPosition.x = basePos.x + num * vector.x;
					localPosition.x += (float)uIWidget.width * ((!(vector.x > 0f)) ? (pivotOffset.x - 1f) : pivotOffset.x);
					num += (float)uIWidget.width + margin;
				}
				if (Math.Abs(vector.y) > 0f)
				{
					localPosition.y = basePos.y + num * vector.y;
					localPosition.y += (float)uIWidget.height * ((!(vector.y > 0f)) ? (pivotOffset.y - 1f) : pivotOffset.y);
					num += (float)uIWidget.height + margin;
				}
				if (instant || !((Component)uIWidget).gameObject.activeInHierarchy)
				{
					((Component)(object)uIWidget).SetEnable<TweenPosition>(enable: false);
					((Component)uIWidget).transform.localPosition = localPosition;
				}
				else
				{
					TweenPosition.Begin(((Component)uIWidget).gameObject, 0.2f, localPosition);
				}
			}
		}
		return num;
	}

	public static float GetSize(Vector2 size, Vector2 vc)
	{
		return Mathf.Abs(size.x * vc.x + size.y * vc.y);
	}

	public static float GetSize(UIWidget widget, Vector2 vc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetSize(widget.localSize, vc);
	}

	public static float GetSize(UIPanel panel, Vector2 vc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GetSize(panel.GetViewSize(), vc);
	}

	public static float CalcGridSize(ListObjectPool nodes, Vector2 vector, Vector2 size, float rowMargin, float colMargin, out int rowItemCount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(vector);
		Vector3 val2 = ((val.x != 0f) ? Vector3.down : Vector3.right);
		Vector3 val3 = val;
		float size2 = GetSize(size, Vector2.op_Implicit(val2));
		float num = 0f;
		float num2 = 0f;
		UIWidget component = nodes.BaseObject.GetComponent<UIWidget>();
		float num3 = GetSize(component, Vector2.op_Implicit(val2)) + rowMargin;
		float num4 = GetSize(component, Vector2.op_Implicit(val3)) + colMargin;
		rowItemCount = -1;
		int num5 = 0;
		int i = 0;
		for (int count = nodes.Count; i < count; i++)
		{
			UIWidget component2 = nodes[i].GetComponent<UIWidget>();
			if (!IsVisibleWidget(component2))
			{
				continue;
			}
			if (num > 0f && num + num3 - rowMargin > size2)
			{
				num = 0f;
				num2 += num4;
				if (rowItemCount == -1)
				{
					rowItemCount = num5;
				}
			}
			num += num3;
			num5++;
		}
		if (rowItemCount == -1)
		{
			rowItemCount = num5;
		}
		return num2 + num4 - colMargin;
	}

	public static void WidgetsGridReposition(ListObjectPool nodes, ListObjectPool spliter, Vector2 vector, Vector3 basePos, Vector2 size, float rowMargin, float colMargin, bool instant = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(vector);
		Vector3 val2 = ((val.x != 0f) ? Vector3.down : Vector3.right);
		Vector3 val3 = val;
		int num;
		if (val3.x == 1f)
		{
			num = 1;
		}
		else if (val3.x == -1f)
		{
			num = 2;
		}
		else if (val3.y == 1f)
		{
			num = 0;
		}
		else
		{
			if (val3.y != -1f)
			{
				return;
			}
			num = 1;
		}
		CalcGridSize(nodes, vector, size, rowMargin, colMargin, out var rowItemCount);
		UIWidget component = nodes.BaseObject.GetComponent<UIWidget>();
		float num2 = GetSize(component, Vector2.op_Implicit(val2)) + rowMargin;
		float num3 = GetSize(component, Vector2.op_Implicit(val3)) + colMargin;
		int num4 = 0;
		int i = 0;
		for (int count = nodes.Count; i < count; i++)
		{
			UIWidget component2 = nodes[i].GetComponent<UIWidget>();
			if (IsVisibleWidget(component2))
			{
				Vector3 val4 = component2.localCorners[num];
				Vector3 val5 = basePos + val2 * num2 * (float)(num4 % rowItemCount) + val3 * num3 * (float)(num4 / rowItemCount) - val4;
				if (instant)
				{
					((Component)component2).transform.localPosition = val5;
				}
				else
				{
					TweenPosition.Begin(((Component)component2).gameObject, 0.2f, val5);
				}
				num4++;
			}
		}
		if (spliter != null && (Object)(object)spliter.BaseObject != (Object)null)
		{
			int num5 = Mathf.Max(Mathf.CeilToInt((float)num4 / (float)rowItemCount), 0);
			spliter.Set(Mathf.Max(0, num5 - 1));
			UIWidget component3 = spliter.BaseObject.GetComponent<UIWidget>();
			float size2 = GetSize(size, Vector2.op_Implicit(val2));
			float size3 = GetSize(component3, Vector2.op_Implicit(val3));
			int width = (int)((val3.x != 0f) ? size3 : size2);
			int height = (int)((val3.x != 0f) ? size2 : size3);
			int j = 0;
			for (int count2 = spliter.Count; j < count2; j++)
			{
				UIWidget component4 = spliter[j].GetComponent<UIWidget>();
				component4.width = width;
				component4.height = height;
				Vector3 val6 = component4.localCorners[num];
				((Component)component4).transform.localPosition = basePos + val3 * ((float)(j + 1) * num3 - colMargin * 0.5f) - val6;
			}
		}
	}

	public static bool IsVisibleWidget(UIWidget widget)
	{
		TweenAlpha component = ((Component)widget).GetComponent<TweenAlpha>();
		float num = ((!((Object)(object)component != (Object)null) || !((Behaviour)component).enabled) ? widget.alpha : component.to);
		return ((Component)widget).gameObject.activeSelf && num > 0f;
	}

	private static Color32 GetTextureAreaColor(Texture2D tex, Rect rect)
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		int i = Mathf.FloorToInt(((Rect)(ref rect)).xMin);
		for (int num7 = Mathf.CeilToInt(((Rect)(ref rect)).xMax); i < num7; i++)
		{
			int j = Mathf.FloorToInt(((Rect)(ref rect)).yMin);
			for (int num8 = Mathf.CeilToInt(((Rect)(ref rect)).yMax); j < num8; j++)
			{
				float num9 = 1f;
				num9 *= Mathf.Min((float)(i + 1), ((Rect)(ref rect)).xMax) - Mathf.Max((float)i, ((Rect)(ref rect)).xMin);
				num9 *= Mathf.Min((float)(j + 1), ((Rect)(ref rect)).yMax) - Mathf.Max((float)j, ((Rect)(ref rect)).yMin);
				Color val = tex.GetPixel(i, j) * num9;
				if (val.a > 0f)
				{
					num5 += num9;
					num += val.r;
					num2 += val.g;
					num3 += val.b;
				}
				num6 += num9;
				num4 += val.a;
			}
		}
		num /= num5;
		num2 /= num5;
		num3 /= num5;
		num4 /= num6;
		return Color32.op_Implicit(new Color(num, num2, num3, num4));
	}

	public static Color32[] ResizeTexturePixels(Texture2D texture, int width, int height)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return ResizeTexturePixels(texture, new Rect(0f, 0f, 1f, 1f), width, height);
	}

	public static Color32[] ResizeTexturePixels(Texture2D texture, Rect uv, int width, int height)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		TextureWrapMode wrapMode = ((Texture)texture).wrapMode;
		((Texture)texture).wrapMode = (TextureWrapMode)1;
		Color32[] array = (Color32[])(object)new Color32[width * height];
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(((Rect)(ref uv)).x * (float)((Texture)texture).width, ((Rect)(ref uv)).y * (float)((Texture)texture).height, ((Rect)(ref uv)).width * (float)((Texture)texture).width, ((Rect)(ref uv)).height * (float)((Texture)texture).height);
		float width2 = ((Rect)(ref val)).width;
		float height2 = ((Rect)(ref val)).height;
		float num = Mathf.Max(width2 / (float)width, height2 / (float)height);
		float num2 = ((float)width * num - width2) * 0.5f;
		float num3 = ((float)height * num - height2) * 0.5f;
		((Rect)(ref val)).x = ((Rect)(ref val)).x - num2;
		((Rect)(ref val)).width = ((Rect)(ref val)).width + num2;
		((Rect)(ref val)).y = ((Rect)(ref val)).y - num3;
		((Rect)(ref val)).height = ((Rect)(ref val)).height + num3;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Color val2 = Color32.op_Implicit(GetTextureAreaColor(texture, new Rect(((Rect)(ref val)).xMin + (float)i * num, ((Rect)(ref val)).yMin + (float)j * num, 1f, 1f)));
				ref Color32 reference = ref array[i + j * width];
				reference = Color32.op_Implicit(val2);
			}
		}
		((Texture)texture).wrapMode = wrapMode;
		return array;
	}

	public static Color32[] RemoveSpace(Color32[] pixels, ref int width, ref int height, out Rect rect)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		Rect nonespaceArea = GetNonespaceArea(pixels, width, height);
		int num = (int)((Rect)(ref nonespaceArea)).width + 1;
		int num2 = (int)((Rect)(ref nonespaceArea)).height + 1;
		if (num < 0 || num2 < 0)
		{
			((Rect)(ref rect))._002Ector(0f, 0f, 1f / (float)width, 1f / (float)height);
			width = 1;
			height = 1;
			return (Color32[])(object)new Color32[1]
			{
				new Color32((byte)0, (byte)0, (byte)0, (byte)0)
			};
		}
		Color32[] array = (Color32[])(object)new Color32[num * num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				int num4 = ((int)((Rect)(ref nonespaceArea)).yMin + i) * width + ((int)((Rect)(ref nonespaceArea)).xMin + j);
				ref Color32 reference = ref array[num3];
				reference = pixels[num4];
			}
		}
		((Rect)(ref rect))._002Ector(((Rect)(ref nonespaceArea)).xMin, ((Rect)(ref nonespaceArea)).yMin, (float)num, (float)num2);
		rect = DivideRect(rect, width, height);
		width = num;
		height = num2;
		return array;
	}

	public static Rect GetNonespaceArea(Color32[] pixels, int width, int height)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		int num = width;
		int num2 = 0;
		int num3 = height;
		int num4 = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				Color32 val = pixels[i * width + j];
				if (val.a > 0)
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
		return new Rect((float)num, (float)num3, (float)(num2 - num), (float)(num4 - num3));
	}

	public static Rect DivideRect(Rect rect, float x, float y)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((Rect)(ref rect)).x = ((Rect)(ref rect)).x / x;
		((Rect)(ref rect)).y = ((Rect)(ref rect)).y / y;
		((Rect)(ref rect)).width = ((Rect)(ref rect)).width / x;
		((Rect)(ref rect)).height = ((Rect)(ref rect)).height / y;
		return rect;
	}

	public static bool IsUrl(string text)
	{
		Match match = Regex.Match(text, "^https?://", RegexOptions.IgnoreCase);
		return match.Success;
	}

	public static float[] RGBtoHSV(Color col)
	{
		float r = col.r;
		float g = col.g;
		float b = col.b;
		float[] array = new float[3];
		float num = KMathUtil.Min(r, g, b);
		float num2 = (array[2] = KMathUtil.Max(r, g, b));
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (color1 == color2)
		{
			return 0;
		}
		float num = ColorSortValue(color1);
		float num2 = ColorSortValue(color2);
		return (num2 > num) ? 1 : (-1);
	}

	public static float ColorSortValue(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		float[] array = RGBtoHSV(c);
		array[0] /= 360f;
		array[1] = Mathf.Min(array[1], array[2]);
		if (array[1] > 0.5f)
		{
			return array[0] + 1f;
		}
		return array[2];
	}

	public static void SetPosition(this UIWidget widget, Vector3 pos, float pivotX, float pivotY)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		widget.SetPosition(pos, new Vector2(pivotX, pivotY));
	}

	public static void SetPosition(this UIWidget widget, Vector3 pos, Vector2 pivot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = pivot - widget.pivotOffset;
		((Component)widget).transform.localPosition = pos - Vector3.Scale(Vector2.op_Implicit(val), Vector2.op_Implicit(widget.localSize));
	}

	public static Vector3 GetPosition(this UIWidget widget, float pivotX, float pivotY)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return widget.GetPosition(new Vector2(pivotX, pivotY));
	}

	public static Vector3 GetPosition(this UIWidget widget, Vector2 pivot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)widget).transform.localPosition;
		Vector2 val = pivot - widget.pivotOffset;
		return localPosition + Vector3.Scale(Vector2.op_Implicit(val), Vector2.op_Implicit(widget.localSize));
	}

	public static void Resize(this UIWidget widget, Point2 size, Vector2 pivot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = widget.GetPosition(pivot);
		widget.width = size.x;
		widget.height = size.y;
		widget.SetPosition(position, pivot);
	}

	public static void SetEnable<T>(this Component comp, bool enable) where T : Behaviour
	{
		T component = comp.GetComponent<T>();
		if (!((Object)(object)component == (Object)null))
		{
			((Behaviour)component).enabled = enable;
		}
	}

	public static void SetText(this UILabel label, SyncString syncStr)
	{
		LabelUpdater.Set(label, syncStr);
	}

	public static void SetText(this UISpriteLabel label, SyncString syncStr)
	{
		LabelUpdater.Set(label, syncStr);
	}
}
