using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Debug")]
public class NGUIDebug : MonoBehaviour
{
	private static bool mRayDebug = false;

	private static List<string> mLines = new List<string>();

	private static NGUIDebug mInstance = null;

	public static bool debugRaycast
	{
		get
		{
			return mRayDebug;
		}
		set
		{
			mRayDebug = value;
			if (value && Application.isPlaying)
			{
				CreateInstance();
			}
		}
	}

	public static void CreateInstance()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if ((Object)(object)mInstance == (Object)null)
		{
			GameObject val = new GameObject("_NGUI Debug");
			mInstance = val.AddComponent<NGUIDebug>();
			Object.DontDestroyOnLoad((Object)(object)val);
		}
	}

	private static void LogString(string text)
	{
		if (Application.isPlaying)
		{
			if (mLines.Count > 20)
			{
				mLines.RemoveAt(0);
			}
			mLines.Add(text);
			CreateInstance();
		}
	}

	public static void Log(params object[] objs)
	{
		string text = string.Empty;
		for (int i = 0; i < objs.Length; i++)
		{
			text = ((i != 0) ? (text + ", " + objs[i].ToString()) : (text + objs[i].ToString()));
		}
		LogString(text);
	}

	public static void Log(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			string[] array = s.Split('\n');
			string[] array2 = array;
			foreach (string text in array2)
			{
				LogString(text);
			}
		}
	}

	public static void Clear()
	{
		mLines.Clear();
	}

	public static void DrawBounds(Bounds b)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = ((Bounds)(ref b)).center;
		Vector3 val = ((Bounds)(ref b)).center - ((Bounds)(ref b)).extents;
		Vector3 val2 = ((Bounds)(ref b)).center + ((Bounds)(ref b)).extents;
		Debug.DrawLine(new Vector3(val.x, val.y, center.z), new Vector3(val2.x, val.y, center.z), Color.red);
		Debug.DrawLine(new Vector3(val.x, val.y, center.z), new Vector3(val.x, val2.y, center.z), Color.red);
		Debug.DrawLine(new Vector3(val2.x, val.y, center.z), new Vector3(val2.x, val2.y, center.z), Color.red);
		Debug.DrawLine(new Vector3(val.x, val2.y, center.z), new Vector3(val2.x, val2.y, center.z), Color.red);
	}

	private void OnGUI()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(5f, 5f, 1000f, 22f);
		if (mRayDebug)
		{
			UICamera.ControlScheme currentScheme = UICamera.currentScheme;
			string text = "Scheme: " + currentScheme;
			GUI.color = Color.black;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y - 1f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x - 1f;
			GUI.color = Color.white;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y + 18f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x + 1f;
			text = "Hover: " + NGUITools.GetHierarchy(UICamera.hoveredObject).Replace("\"", string.Empty);
			GUI.color = Color.black;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y - 1f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x - 1f;
			GUI.color = Color.white;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y + 18f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x + 1f;
			text = "Selection: " + NGUITools.GetHierarchy(UICamera.selectedObject).Replace("\"", string.Empty);
			GUI.color = Color.black;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y - 1f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x - 1f;
			GUI.color = Color.white;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y + 18f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x + 1f;
			text = "Controller: " + NGUITools.GetHierarchy(UICamera.controllerNavigationObject).Replace("\"", string.Empty);
			GUI.color = Color.black;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y - 1f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x - 1f;
			GUI.color = Color.white;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y + 18f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x + 1f;
			text = "Active events: " + UICamera.CountInputSources();
			if (UICamera.disableController)
			{
				text += ", disabled controller";
			}
			if (UICamera.ignoreControllerInput)
			{
				text += ", ignore controller";
			}
			if (UICamera.inputHasFocus)
			{
				text += ", input focus";
			}
			GUI.color = Color.black;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y - 1f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x - 1f;
			GUI.color = Color.white;
			GUI.Label(val, text);
			((Rect)(ref val)).y = ((Rect)(ref val)).y + 18f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x + 1f;
		}
		int i = 0;
		for (int count = mLines.Count; i < count; i++)
		{
			GUI.color = Color.black;
			GUI.Label(val, mLines[i]);
			((Rect)(ref val)).y = ((Rect)(ref val)).y - 1f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x - 1f;
			GUI.color = Color.white;
			GUI.Label(val, mLines[i]);
			((Rect)(ref val)).y = ((Rect)(ref val)).y + 18f;
			((Rect)(ref val)).x = ((Rect)(ref val)).x + 1f;
		}
	}
}
