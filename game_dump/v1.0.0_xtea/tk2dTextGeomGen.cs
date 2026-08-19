using UnityEngine;

public static class tk2dTextGeomGen
{
	public class GeomData
	{
		internal tk2dTextMeshData textMeshData;

		internal tk2dFontData fontInst;

		internal string formattedText = string.Empty;
	}

	private static GeomData tmpData = new GeomData();

	private static readonly Color32[] channelSelectColors = (Color32[])(object)new Color32[4]
	{
		new Color32((byte)0, (byte)0, byte.MaxValue, (byte)0),
		Color32.op_Implicit(new Color(0f, 255f, 0f, 0f)),
		Color32.op_Implicit(new Color(255f, 0f, 0f, 0f)),
		Color32.op_Implicit(new Color(0f, 0f, 0f, 255f))
	};

	private static Color32 meshTopColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private static Color32 meshBottomColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private static float meshGradientTexU = 0f;

	private static int curGradientCount = 1;

	private static Color32 errorColor = new Color32(byte.MaxValue, (byte)0, byte.MaxValue, byte.MaxValue);

	public static GeomData Data(tk2dTextMeshData textMeshData, tk2dFontData fontData, string formattedText)
	{
		tmpData.textMeshData = textMeshData;
		tmpData.fontInst = fontData;
		tmpData.formattedText = formattedText;
		return tmpData;
	}

	public static Vector2 GetMeshDimensionsForString(string str, GeomData geomData)
	{
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		bool flag = false;
		int num4 = 0;
		for (int i = 0; i < str.Length && num4 < textMeshData.maxChars; i++)
		{
			if (flag)
			{
				flag = false;
				continue;
			}
			int num5 = str[i];
			if (num5 == 10)
			{
				num = Mathf.Max(num2, num);
				num2 = 0f;
				num3 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
				continue;
			}
			if (textMeshData.inlineStyling && num5 == 94 && i + 1 < str.Length)
			{
				if (str[i + 1] != '^')
				{
					int num6 = 0;
					switch (str[i + 1])
					{
					case 'c':
						num6 = 5;
						break;
					case 'C':
						num6 = 9;
						break;
					case 'g':
						num6 = 9;
						break;
					case 'G':
						num6 = 17;
						break;
					}
					i += num6;
					continue;
				}
				flag = true;
			}
			bool flag2 = num5 == 94;
			tk2dFontChar tk2dFontChar2;
			if (fontInst.useDictionary)
			{
				if (!fontInst.charDict.ContainsKey(num5))
				{
					num5 = 0;
				}
				tk2dFontChar2 = fontInst.charDict[num5];
			}
			else
			{
				if (num5 >= fontInst.chars.Length)
				{
					num5 = 0;
				}
				tk2dFontChar2 = fontInst.chars[num5];
			}
			if (flag2)
			{
				num5 = 94;
			}
			num2 += (tk2dFontChar2.advance + textMeshData.spacing) * textMeshData.scale.x;
			if (textMeshData.kerning && i < str.Length - 1)
			{
				tk2dFontKerning[] kerning = fontInst.kerning;
				foreach (tk2dFontKerning tk2dFontKerning2 in kerning)
				{
					if (tk2dFontKerning2.c0 == str[i] && tk2dFontKerning2.c1 == str[i + 1])
					{
						num2 += tk2dFontKerning2.amount * textMeshData.scale.x;
						break;
					}
				}
			}
			num4++;
		}
		num = Mathf.Max(num2, num);
		num3 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
		return new Vector2(num, num3);
	}

	public static float GetYAnchorForHeight(float textHeight, GeomData geomData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected I4, but got Unknown
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		int num = textMeshData.anchor / 3;
		float num2 = (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
		switch (num)
		{
		case 0:
			return 0f - num2;
		case 1:
		{
			float num3 = (0f - textHeight) / 2f - num2;
			if (fontInst.version >= 2)
			{
				float num4 = fontInst.texelSize.y * textMeshData.scale.y;
				return Mathf.Floor(num3 / num4) * num4;
			}
			return num3;
		}
		case 2:
			return 0f - textHeight - num2;
		default:
			return 0f - num2;
		}
	}

	public static float GetXAnchorForWidth(float lineWidth, GeomData geomData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected I4, but got Unknown
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		switch (textMeshData.anchor % 3)
		{
		case 0:
			return 0f;
		case 1:
		{
			float num = (0f - lineWidth) / 2f;
			if (fontInst.version >= 2)
			{
				float num2 = fontInst.texelSize.x * textMeshData.scale.x;
				return Mathf.Floor(num / num2) * num2;
			}
			return num;
		}
		case 2:
			return 0f - lineWidth;
		default:
			return 0f;
		}
	}

	private static void PostAlignTextData(Vector3[] pos, int offset, int targetStart, int targetEnd, float offsetX)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		for (int i = targetStart * 4; i < targetEnd * 4; i++)
		{
			Vector3 val = pos[offset + i];
			val.x += offsetX;
			pos[offset + i] = val;
		}
	}

	private static int GetFullHexColorComponent(int c1, int c2)
	{
		int num = 0;
		if (c1 >= 48 && c1 <= 57)
		{
			num += (c1 - 48) * 16;
		}
		else if (c1 >= 97 && c1 <= 102)
		{
			num += (10 + c1 - 97) * 16;
		}
		else
		{
			if (c1 < 65 || c1 > 70)
			{
				return -1;
			}
			num += (10 + c1 - 65) * 16;
		}
		if (c2 >= 48 && c2 <= 57)
		{
			return num + (c2 - 48);
		}
		if (c2 >= 97 && c2 <= 102)
		{
			return num + (10 + c2 - 97);
		}
		if (c2 >= 65 && c2 <= 70)
		{
			return num + (10 + c2 - 65);
		}
		return -1;
	}

	private static int GetCompactHexColorComponent(int c)
	{
		if (c >= 48 && c <= 57)
		{
			return (c - 48) * 17;
		}
		if (c >= 97 && c <= 102)
		{
			return (10 + c - 97) * 17;
		}
		if (c >= 65 && c <= 70)
		{
			return (10 + c - 65) * 17;
		}
		return -1;
	}

	private static int GetStyleHexColor(string str, bool fullHex, ref Color32 color)
	{
		int num;
		int num2;
		int num3;
		int num4;
		if (fullHex)
		{
			if (str.Length < 8)
			{
				return 1;
			}
			num = GetFullHexColorComponent(str[0], str[1]);
			num2 = GetFullHexColorComponent(str[2], str[3]);
			num3 = GetFullHexColorComponent(str[4], str[5]);
			num4 = GetFullHexColorComponent(str[6], str[7]);
		}
		else
		{
			if (str.Length < 4)
			{
				return 1;
			}
			num = GetCompactHexColorComponent(str[0]);
			num2 = GetCompactHexColorComponent(str[1]);
			num3 = GetCompactHexColorComponent(str[2]);
			num4 = GetCompactHexColorComponent(str[3]);
		}
		if (num == -1 || num2 == -1 || num3 == -1 || num4 == -1)
		{
			return 1;
		}
		((Color32)(ref color))._002Ector((byte)num, (byte)num2, (byte)num3, (byte)num4);
		return 0;
	}

	private static int SetColorsFromStyleCommand(string args, bool twoColors, bool fullHex)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		int num = ((!twoColors) ? 1 : 2) * ((!fullHex) ? 4 : 8);
		bool flag = false;
		if (args.Length >= num)
		{
			if (GetStyleHexColor(args, fullHex, ref meshTopColor) != 0)
			{
				flag = true;
			}
			if (twoColors)
			{
				string str = args.Substring((!fullHex) ? 4 : 8);
				if (GetStyleHexColor(str, fullHex, ref meshBottomColor) != 0)
				{
					flag = true;
				}
			}
			else
			{
				meshBottomColor = meshTopColor;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			meshTopColor = (meshBottomColor = errorColor);
		}
		return num;
	}

	private static void SetGradientTexUFromStyleCommand(int arg)
	{
		meshGradientTexU = (float)(arg - 48) / (float)((curGradientCount <= 0) ? 1 : curGradientCount);
	}

	private static int HandleStyleCommand(string cmd)
	{
		if (cmd.Length == 0)
		{
			return 0;
		}
		int num = cmd[0];
		string args = cmd.Substring(1);
		int result = 0;
		switch (num)
		{
		case 99:
			result = 1 + SetColorsFromStyleCommand(args, twoColors: false, fullHex: false);
			break;
		case 67:
			result = 1 + SetColorsFromStyleCommand(args, twoColors: false, fullHex: true);
			break;
		case 103:
			result = 1 + SetColorsFromStyleCommand(args, twoColors: true, fullHex: false);
			break;
		case 71:
			result = 1 + SetColorsFromStyleCommand(args, twoColors: true, fullHex: true);
			break;
		}
		if (num >= 48 && num <= 57)
		{
			SetGradientTexUFromStyleCommand(num);
			result = 1;
		}
		return result;
	}

	public static void GetTextMeshGeomDesc(out int numVertices, out int numIndices, GeomData geomData)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		numVertices = textMeshData.maxChars * 4;
		numIndices = textMeshData.maxChars * 6;
	}

	public static int SetTextMeshGeom(Vector3[] pos, Vector2[] uv, Vector2[] uv2, Color32[] color, int offset, GeomData geomData)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_0844: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_084a: Unknown result type (might be due to invalid IL or missing references)
		//IL_084c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0851: Unknown result type (might be due to invalid IL or missing references)
		//IL_0853: Unknown result type (might be due to invalid IL or missing references)
		//IL_0854: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Unknown result type (might be due to invalid IL or missing references)
		//IL_085d: Unknown result type (might be due to invalid IL or missing references)
		//IL_085e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0860: Unknown result type (might be due to invalid IL or missing references)
		//IL_0865: Unknown result type (might be due to invalid IL or missing references)
		//IL_0867: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08be: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0985: Unknown result type (might be due to invalid IL or missing references)
		//IL_098a: Unknown result type (might be due to invalid IL or missing references)
		//IL_098f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0990: Unknown result type (might be due to invalid IL or missing references)
		//IL_0992: Unknown result type (might be due to invalid IL or missing references)
		//IL_0997: Unknown result type (might be due to invalid IL or missing references)
		//IL_0999: Unknown result type (might be due to invalid IL or missing references)
		//IL_099a: Unknown result type (might be due to invalid IL or missing references)
		//IL_099c: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		//IL_0906: Unknown result type (might be due to invalid IL or missing references)
		//IL_0908: Unknown result type (might be due to invalid IL or missing references)
		//IL_090d: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0932: Unknown result type (might be due to invalid IL or missing references)
		//IL_0937: Unknown result type (might be due to invalid IL or missing references)
		//IL_0938: Unknown result type (might be due to invalid IL or missing references)
		//IL_093a: Unknown result type (might be due to invalid IL or missing references)
		//IL_093f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0941: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0583: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_066b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0670: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0623: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		string formattedText = geomData.formattedText;
		meshTopColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		meshBottomColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		meshGradientTexU = (float)textMeshData.textureGradient / (float)((fontInst.gradientCount <= 0) ? 1 : fontInst.gradientCount);
		curGradientCount = fontInst.gradientCount;
		float yAnchorForHeight = GetYAnchorForHeight(GetMeshDimensionsForString(geomData.formattedText, geomData).y, geomData);
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < formattedText.Length && num3 < textMeshData.maxChars; i++)
		{
			int num5 = formattedText[i];
			bool flag = num5 == 94;
			tk2dFontChar tk2dFontChar2;
			if (fontInst.useDictionary)
			{
				if (!fontInst.charDict.ContainsKey(num5))
				{
					num5 = 0;
				}
				tk2dFontChar2 = fontInst.charDict[num5];
			}
			else
			{
				if (num5 >= fontInst.chars.Length)
				{
					num5 = 0;
				}
				tk2dFontChar2 = fontInst.chars[num5];
			}
			if (flag)
			{
				num5 = 94;
			}
			if (num5 == 10)
			{
				float lineWidth = num;
				int targetEnd = num3;
				if (num4 != num3)
				{
					float xAnchorForWidth = GetXAnchorForWidth(lineWidth, geomData);
					PostAlignTextData(pos, offset, num4, targetEnd, xAnchorForWidth);
				}
				num4 = num3;
				num = 0f;
				num2 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
				continue;
			}
			if (textMeshData.inlineStyling && num5 == 94)
			{
				if (i + 1 >= formattedText.Length || formattedText[i + 1] != '^')
				{
					i += HandleStyleCommand(formattedText.Substring(i + 1));
					continue;
				}
				i++;
			}
			ref Vector3 reference = ref pos[offset + num3 * 4];
			reference = new Vector3(num + tk2dFontChar2.p0.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p0.y * textMeshData.scale.y, 0f);
			ref Vector3 reference2 = ref pos[offset + num3 * 4 + 1];
			reference2 = new Vector3(num + tk2dFontChar2.p1.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p0.y * textMeshData.scale.y, 0f);
			ref Vector3 reference3 = ref pos[offset + num3 * 4 + 2];
			reference3 = new Vector3(num + tk2dFontChar2.p0.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p1.y * textMeshData.scale.y, 0f);
			ref Vector3 reference4 = ref pos[offset + num3 * 4 + 3];
			reference4 = new Vector3(num + tk2dFontChar2.p1.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p1.y * textMeshData.scale.y, 0f);
			if (tk2dFontChar2.flipped)
			{
				ref Vector2 reference5 = ref uv[offset + num3 * 4];
				reference5 = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv1.y);
				ref Vector2 reference6 = ref uv[offset + num3 * 4 + 1];
				reference6 = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv0.y);
				ref Vector2 reference7 = ref uv[offset + num3 * 4 + 2];
				reference7 = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv1.y);
				ref Vector2 reference8 = ref uv[offset + num3 * 4 + 3];
				reference8 = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv0.y);
			}
			else
			{
				ref Vector2 reference9 = ref uv[offset + num3 * 4];
				reference9 = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv0.y);
				ref Vector2 reference10 = ref uv[offset + num3 * 4 + 1];
				reference10 = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv0.y);
				ref Vector2 reference11 = ref uv[offset + num3 * 4 + 2];
				reference11 = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv1.y);
				ref Vector2 reference12 = ref uv[offset + num3 * 4 + 3];
				reference12 = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv1.y);
			}
			if (fontInst.textureGradients)
			{
				ref Vector2 reference13 = ref uv2[offset + num3 * 4];
				reference13 = tk2dFontChar2.gradientUv[0] + new Vector2(meshGradientTexU, 0f);
				ref Vector2 reference14 = ref uv2[offset + num3 * 4 + 1];
				reference14 = tk2dFontChar2.gradientUv[1] + new Vector2(meshGradientTexU, 0f);
				ref Vector2 reference15 = ref uv2[offset + num3 * 4 + 2];
				reference15 = tk2dFontChar2.gradientUv[2] + new Vector2(meshGradientTexU, 0f);
				ref Vector2 reference16 = ref uv2[offset + num3 * 4 + 3];
				reference16 = tk2dFontChar2.gradientUv[3] + new Vector2(meshGradientTexU, 0f);
			}
			if (fontInst.isPacked)
			{
				Color32 val = channelSelectColors[tk2dFontChar2.channel];
				color[offset + num3 * 4] = val;
				color[offset + num3 * 4 + 1] = val;
				color[offset + num3 * 4 + 2] = val;
				color[offset + num3 * 4 + 3] = val;
			}
			else
			{
				ref Color32 reference17 = ref color[offset + num3 * 4];
				reference17 = meshTopColor;
				ref Color32 reference18 = ref color[offset + num3 * 4 + 1];
				reference18 = meshTopColor;
				ref Color32 reference19 = ref color[offset + num3 * 4 + 2];
				reference19 = meshBottomColor;
				ref Color32 reference20 = ref color[offset + num3 * 4 + 3];
				reference20 = meshBottomColor;
			}
			num += (tk2dFontChar2.advance + textMeshData.spacing) * textMeshData.scale.x;
			if (textMeshData.kerning && i < formattedText.Length - 1)
			{
				tk2dFontKerning[] kerning = fontInst.kerning;
				foreach (tk2dFontKerning tk2dFontKerning2 in kerning)
				{
					if (tk2dFontKerning2.c0 == formattedText[i] && tk2dFontKerning2.c1 == formattedText[i + 1])
					{
						num += tk2dFontKerning2.amount * textMeshData.scale.x;
						break;
					}
				}
			}
			num3++;
		}
		if (num4 != num3)
		{
			float lineWidth2 = num;
			int targetEnd2 = num3;
			float xAnchorForWidth2 = GetXAnchorForWidth(lineWidth2, geomData);
			PostAlignTextData(pos, offset, num4, targetEnd2, xAnchorForWidth2);
		}
		for (int k = num3; k < textMeshData.maxChars; k++)
		{
			ref Vector3 reference21 = ref pos[offset + k * 4];
			ref Vector3 reference22 = ref pos[offset + k * 4 + 1];
			ref Vector3 reference23 = ref pos[offset + k * 4 + 2];
			ref Vector3 reference24 = ref pos[offset + k * 4 + 3];
			reference21 = (reference22 = (reference23 = (reference24 = Vector3.zero)));
			ref Vector2 reference25 = ref uv[offset + k * 4];
			ref Vector2 reference26 = ref uv[offset + k * 4 + 1];
			ref Vector2 reference27 = ref uv[offset + k * 4 + 2];
			ref Vector2 reference28 = ref uv[offset + k * 4 + 3];
			reference25 = (reference26 = (reference27 = (reference28 = Vector2.zero)));
			if (fontInst.textureGradients)
			{
				ref Vector2 reference29 = ref uv2[offset + k * 4];
				ref Vector2 reference30 = ref uv2[offset + k * 4 + 1];
				ref Vector2 reference31 = ref uv2[offset + k * 4 + 2];
				ref Vector2 reference32 = ref uv2[offset + k * 4 + 3];
				reference29 = (reference30 = (reference31 = (reference32 = Vector2.zero)));
			}
			if (!fontInst.isPacked)
			{
				ref Color32 reference33 = ref color[offset + k * 4];
				ref Color32 reference34 = ref color[offset + k * 4 + 1];
				reference33 = (reference34 = meshTopColor);
				ref Color32 reference35 = ref color[offset + k * 4 + 2];
				ref Color32 reference36 = ref color[offset + k * 4 + 3];
				reference35 = (reference36 = meshBottomColor);
			}
			else
			{
				ref Color32 reference37 = ref color[offset + k * 4];
				ref Color32 reference38 = ref color[offset + k * 4 + 1];
				ref Color32 reference39 = ref color[offset + k * 4 + 2];
				ref Color32 reference40 = ref color[offset + k * 4 + 3];
				reference37 = (reference38 = (reference39 = (reference40 = Color32.op_Implicit(Color.clear))));
			}
		}
		return num3;
	}

	public static void SetTextMeshIndices(int[] indices, int offset, int vStart, GeomData geomData, int target)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		for (int i = 0; i < textMeshData.maxChars; i++)
		{
			indices[offset + i * 6] = vStart + i * 4;
			indices[offset + i * 6 + 1] = vStart + i * 4 + 1;
			indices[offset + i * 6 + 2] = vStart + i * 4 + 3;
			indices[offset + i * 6 + 3] = vStart + i * 4 + 2;
			indices[offset + i * 6 + 4] = vStart + i * 4;
			indices[offset + i * 6 + 5] = vStart + i * 4 + 3;
		}
	}
}
