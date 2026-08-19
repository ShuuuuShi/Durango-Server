using System;
using System.Reflection;
using UnityEngine;

public static class PresetColor
{
	public static Color UIYellow = new Color(1f, 72f / 85f, 0.35686275f);

	public static Color UIGreen = new Color(1f / 15f, 38f / 85f, 0.24313726f);

	public static Color UILightGreen = new Color(10f / 51f, 0.7058824f, 14f / 51f);

	public static Color UIRed = new Color(0.61960787f, 0.043137256f, 1f / 17f);

	public static Color UILightRed = new Color(76f / 85f, 2f / 15f, 2f / 15f);

	public static Color UIDarkRed = new Color(0.5254902f, 0.1764706f, 0.1764706f);

	public static Color UIBlue = new Color(0.2f, 0.2f, 1f);

	public static Color UISkyBlue = new Color(0.16862746f, 43f / 85f, 67f / 85f);

	public static Color UIGray = new Color(19f / 85f, 19f / 85f, 0.20784314f);

	public static Color UIDarkGray = new Color(0.29411766f, 0.29411766f, 14f / 51f);

	public static Color UILightGray = new Color(0.44313726f, 0.44313726f, 0.41960785f);

	public static Color UIMoreLightGray = new Color(44f / 85f, 44f / 85f, 25f / 51f);

	public static Color UIWhite = new Color(0.9098039f, 0.8980392f, 0.8745098f);

	public static Color UIBlack = new Color(0f, 0f, 0f);

	public static Color UIBlackAlpha50 = new Color(0f, 0f, 0f, 20f / 51f);

	public static Color UIButtonNormal = new Color(0.56078434f, 0.56078434f, 0.52156866f);

	public static Color UIDarkOrange = new Color(0.8862745f, 0.42745098f, 0.2f);

	public static Color UIPurple = Color32.op_Implicit(new Color32((byte)142, (byte)28, (byte)69, byte.MaxValue));

	public static Color UIBrown = Color32.op_Implicit(new Color32((byte)162, (byte)145, (byte)102, byte.MaxValue));

	public static Color UILightBrown = Color32.op_Implicit(new Color32((byte)216, (byte)212, (byte)202, byte.MaxValue));

	public static Color UIDarkBrown = Color32.op_Implicit(new Color32((byte)132, (byte)124, (byte)102, byte.MaxValue));

	public static Color UIDarkBrownGray = Color32.op_Implicit(new Color32((byte)75, (byte)66, (byte)43, byte.MaxValue));

	public static Color UIDeepDarkBrown = Color32.op_Implicit(new Color32((byte)30, (byte)28, (byte)22, byte.MaxValue));

	public static Color UIGrayBrown = Color32.op_Implicit(new Color32((byte)154, (byte)150, (byte)142, byte.MaxValue));

	public static Color UIRedBrown = Color32.op_Implicit(new Color32((byte)201, (byte)173, (byte)105, byte.MaxValue));

	public static Color UIMoreLightBrown = Color32.op_Implicit(new Color32((byte)124, (byte)113, (byte)88, byte.MaxValue));

	public static Color LoadingColor = Color32.op_Implicit(new Color32((byte)76, (byte)68, (byte)59, byte.MaxValue));

	public static Color UIBuff = Color32.op_Implicit(new Color32((byte)61, (byte)163, (byte)192, byte.MaxValue));

	public static Color UIDebuff = Color32.op_Implicit(new Color32((byte)211, (byte)54, (byte)41, byte.MaxValue));

	[HideInInspector]
	public static Color TryConnectColor = Color32.op_Implicit(new Color32((byte)181, (byte)33, (byte)39, byte.MaxValue));

	[HideInInspector]
	public static Color ConnectingColor = Color32.op_Implicit(new Color32((byte)217, (byte)121, (byte)50, byte.MaxValue));

	[HideInInspector]
	public static Color ConnectedColor = Color32.op_Implicit(new Color32((byte)47, (byte)174, (byte)39, byte.MaxValue));

	[HideInInspector]
	public static Color StableColor = Color32.op_Implicit(new Color32((byte)24, (byte)117, (byte)46, byte.MaxValue));

	[HideInInspector]
	public static Color UnstableColor = Color32.op_Implicit(new Color32((byte)157, (byte)42, (byte)46, byte.MaxValue));

	public static bool TryGet(string key, out Color color)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Type typeFromHandle = typeof(PresetColor);
		FieldInfo field = typeFromHandle.GetField(key, BindingFlags.Static | BindingFlags.Public);
		if ((object)field != null)
		{
			color = (Color)field.GetValue(null);
			return true;
		}
		Debug.LogWarning((object)$"Not Found Preset Color - {key}");
		color = Color.white;
		return false;
	}
}
