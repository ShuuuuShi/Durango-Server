using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class NGUITools
{
	public delegate void OnInitFunc<T>(T w) where T : UIWidget;

	private static AudioListener mListener;

	private static bool mLoaded = false;

	private static float mGlobalVolume = 1f;

	private static float mLastTimestamp = 0f;

	private static AudioClip mLastClip;

	private static Dictionary<Type, string> mTypeNames = new Dictionary<Type, string>();

	private static Vector3[] mSides = (Vector3[])(object)new Vector3[4];

	public static KeyCode[] keys = (KeyCode[])(object)new KeyCode[145]
	{
		(KeyCode)8,
		(KeyCode)9,
		(KeyCode)12,
		(KeyCode)13,
		(KeyCode)19,
		(KeyCode)27,
		(KeyCode)32,
		(KeyCode)33,
		(KeyCode)34,
		(KeyCode)35,
		(KeyCode)36,
		(KeyCode)38,
		(KeyCode)39,
		(KeyCode)40,
		(KeyCode)41,
		(KeyCode)42,
		(KeyCode)43,
		(KeyCode)44,
		(KeyCode)45,
		(KeyCode)46,
		(KeyCode)47,
		(KeyCode)48,
		(KeyCode)49,
		(KeyCode)50,
		(KeyCode)51,
		(KeyCode)52,
		(KeyCode)53,
		(KeyCode)54,
		(KeyCode)55,
		(KeyCode)56,
		(KeyCode)57,
		(KeyCode)58,
		(KeyCode)59,
		(KeyCode)60,
		(KeyCode)61,
		(KeyCode)62,
		(KeyCode)63,
		(KeyCode)64,
		(KeyCode)91,
		(KeyCode)92,
		(KeyCode)93,
		(KeyCode)94,
		(KeyCode)95,
		(KeyCode)96,
		(KeyCode)97,
		(KeyCode)98,
		(KeyCode)99,
		(KeyCode)100,
		(KeyCode)101,
		(KeyCode)102,
		(KeyCode)103,
		(KeyCode)104,
		(KeyCode)105,
		(KeyCode)106,
		(KeyCode)107,
		(KeyCode)108,
		(KeyCode)109,
		(KeyCode)110,
		(KeyCode)111,
		(KeyCode)112,
		(KeyCode)113,
		(KeyCode)114,
		(KeyCode)115,
		(KeyCode)116,
		(KeyCode)117,
		(KeyCode)118,
		(KeyCode)119,
		(KeyCode)120,
		(KeyCode)121,
		(KeyCode)122,
		(KeyCode)127,
		(KeyCode)256,
		(KeyCode)257,
		(KeyCode)258,
		(KeyCode)259,
		(KeyCode)260,
		(KeyCode)261,
		(KeyCode)262,
		(KeyCode)263,
		(KeyCode)264,
		(KeyCode)265,
		(KeyCode)266,
		(KeyCode)267,
		(KeyCode)268,
		(KeyCode)269,
		(KeyCode)270,
		(KeyCode)271,
		(KeyCode)272,
		(KeyCode)273,
		(KeyCode)274,
		(KeyCode)275,
		(KeyCode)276,
		(KeyCode)277,
		(KeyCode)278,
		(KeyCode)279,
		(KeyCode)280,
		(KeyCode)281,
		(KeyCode)282,
		(KeyCode)283,
		(KeyCode)284,
		(KeyCode)285,
		(KeyCode)286,
		(KeyCode)287,
		(KeyCode)288,
		(KeyCode)289,
		(KeyCode)290,
		(KeyCode)291,
		(KeyCode)292,
		(KeyCode)293,
		(KeyCode)294,
		(KeyCode)295,
		(KeyCode)296,
		(KeyCode)300,
		(KeyCode)301,
		(KeyCode)302,
		(KeyCode)303,
		(KeyCode)304,
		(KeyCode)305,
		(KeyCode)306,
		(KeyCode)307,
		(KeyCode)308,
		(KeyCode)326,
		(KeyCode)327,
		(KeyCode)328,
		(KeyCode)329,
		(KeyCode)330,
		(KeyCode)331,
		(KeyCode)332,
		(KeyCode)333,
		(KeyCode)334,
		(KeyCode)335,
		(KeyCode)336,
		(KeyCode)337,
		(KeyCode)338,
		(KeyCode)339,
		(KeyCode)340,
		(KeyCode)341,
		(KeyCode)342,
		(KeyCode)343,
		(KeyCode)344,
		(KeyCode)345,
		(KeyCode)346,
		(KeyCode)347,
		(KeyCode)348,
		(KeyCode)349
	};

	private static Dictionary<string, UIWidget> mWidgets = new Dictionary<string, UIWidget>();

	private static UIPanel mRoot;

	private static GameObject mGo;

	private static ColorSpace mColorSpace = (ColorSpace)(-1);

	public static float soundVolume
	{
		get
		{
			if (!mLoaded)
			{
				mLoaded = true;
				mGlobalVolume = PlayerPrefs.GetFloat("Sound", 1f);
			}
			return mGlobalVolume;
		}
		set
		{
			if (mGlobalVolume != value)
			{
				mLoaded = true;
				mGlobalVolume = value;
				PlayerPrefs.SetFloat("Sound", value);
			}
		}
	}

	public static bool fileAccess => true;

	public static string clipboard
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			TextEditor val = new TextEditor();
			val.Paste();
			return val.text;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			TextEditor val = new TextEditor();
			val.text = value;
			val.OnFocus();
			val.Copy();
		}
	}

	public static Vector2 screenSize => new Vector2((float)Screen.width, (float)Screen.height);

	public static AudioSource PlaySound(AudioClip clip)
	{
		return PlaySound(clip, 1f, 1f);
	}

	public static AudioSource PlaySound(AudioClip clip, float volume)
	{
		return PlaySound(clip, volume, 1f);
	}

	public static AudioSource PlaySound(AudioClip clip, float volume, float pitch)
	{
		float time = RealTime.time;
		if ((Object)(object)mLastClip == (Object)(object)clip && mLastTimestamp + 0.1f > time)
		{
			return null;
		}
		mLastClip = clip;
		mLastTimestamp = time;
		volume *= soundVolume;
		if ((Object)(object)clip != (Object)null && volume > 0.01f)
		{
			if ((Object)(object)mListener == (Object)null || !GetActive((Behaviour)(object)mListener))
			{
				if (Object.FindObjectsOfType(typeof(AudioListener)) is AudioListener[] array)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (GetActive((Behaviour)(object)array[i]))
						{
							mListener = array[i];
							break;
						}
					}
				}
				if ((Object)(object)mListener == (Object)null)
				{
					Camera val = Camera.main;
					if ((Object)(object)val == (Object)null)
					{
						Object obj = Object.FindObjectOfType(typeof(Camera));
						val = (Camera)(object)((obj is Camera) ? obj : null);
					}
					if ((Object)(object)val != (Object)null)
					{
						mListener = ((Component)val).gameObject.AddComponent<AudioListener>();
					}
				}
			}
			if ((Object)(object)mListener != (Object)null && ((Behaviour)mListener).enabled && GetActive(((Component)mListener).gameObject))
			{
				AudioSource val2 = ((Component)mListener).GetComponent<AudioSource>();
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = ((Component)mListener).gameObject.AddComponent<AudioSource>();
				}
				val2.priority = 50;
				val2.pitch = pitch;
				val2.PlayOneShot(clip, volume);
				return val2;
			}
		}
		return null;
	}

	public static int RandomRange(int min, int max)
	{
		if (min == max)
		{
			return min;
		}
		return Random.Range(min, max + 1);
	}

	public static string GetHierarchy(GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return string.Empty;
		}
		string text = ((Object)obj).name;
		while ((Object)(object)obj.transform.parent != (Object)null)
		{
			obj = ((Component)obj.transform.parent).gameObject;
			text = ((Object)obj).name + "\\" + text;
		}
		return text;
	}

	public static T[] FindActive<T>() where T : Component
	{
		return Object.FindObjectsOfType(typeof(T)) as T[];
	}

	public static Camera FindCameraForLayer(int layer)
	{
		int num = 1 << layer;
		Camera cachedCamera;
		for (int i = 0; i < UICamera.list.size; i++)
		{
			cachedCamera = UICamera.list.buffer[i].cachedCamera;
			if (Object.op_Implicit((Object)(object)cachedCamera) && (cachedCamera.cullingMask & num) != 0)
			{
				return cachedCamera;
			}
		}
		cachedCamera = Camera.main;
		if (Object.op_Implicit((Object)(object)cachedCamera) && (cachedCamera.cullingMask & num) != 0)
		{
			return cachedCamera;
		}
		Camera[] array = (Camera[])(object)new Camera[Camera.allCamerasCount];
		int allCameras = Camera.GetAllCameras(array);
		for (int j = 0; j < allCameras; j++)
		{
			cachedCamera = array[j];
			if (Object.op_Implicit((Object)(object)cachedCamera) && ((Behaviour)cachedCamera).enabled && (cachedCamera.cullingMask & num) != 0)
			{
				return cachedCamera;
			}
		}
		return null;
	}

	public static void AddWidgetCollider(GameObject go)
	{
		AddWidgetCollider(go, considerInactive: false);
	}

	public static void AddWidgetCollider(GameObject go, bool considerInactive)
	{
		if (!((Object)(object)go != (Object)null))
		{
			return;
		}
		Collider component = go.GetComponent<Collider>();
		BoxCollider val = (BoxCollider)(object)((component is BoxCollider) ? component : null);
		if ((Object)(object)val != (Object)null)
		{
			UpdateWidgetCollider(val, considerInactive);
		}
		else
		{
			if ((Object)(object)component != (Object)null)
			{
				return;
			}
			BoxCollider2D component2 = go.GetComponent<BoxCollider2D>();
			if ((Object)(object)component2 != (Object)null)
			{
				UpdateWidgetCollider(component2, considerInactive);
				return;
			}
			UICamera uICamera = UICamera.FindCameraForLayer(go.layer);
			if ((Object)(object)uICamera != (Object)null && (uICamera.eventType == UICamera.EventType.World_2D || uICamera.eventType == UICamera.EventType.UI_2D))
			{
				component2 = go.AddComponent<BoxCollider2D>();
				((Collider2D)component2).isTrigger = true;
				UIWidget component3 = go.GetComponent<UIWidget>();
				if ((Object)(object)component3 != (Object)null)
				{
					component3.autoResizeBoxCollider = true;
				}
				UpdateWidgetCollider(component2, considerInactive);
			}
			else
			{
				val = go.AddComponent<BoxCollider>();
				((Collider)val).isTrigger = true;
				UIWidget component4 = go.GetComponent<UIWidget>();
				if ((Object)(object)component4 != (Object)null)
				{
					component4.autoResizeBoxCollider = true;
				}
				UpdateWidgetCollider(val, considerInactive);
			}
		}
	}

	public static void UpdateWidgetCollider(GameObject go)
	{
		UpdateWidgetCollider(go, considerInactive: false);
	}

	public static void UpdateWidgetCollider(GameObject go, bool considerInactive)
	{
		if (!((Object)(object)go != (Object)null))
		{
			return;
		}
		BoxCollider component = go.GetComponent<BoxCollider>();
		if ((Object)(object)component != (Object)null)
		{
			UpdateWidgetCollider(component, considerInactive);
			return;
		}
		BoxCollider2D component2 = go.GetComponent<BoxCollider2D>();
		if ((Object)(object)component2 != (Object)null)
		{
			UpdateWidgetCollider(component2, considerInactive);
		}
	}

	public static void UpdateWidgetCollider(BoxCollider box, bool considerInactive)
	{
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)box != (Object)null))
		{
			return;
		}
		GameObject gameObject = ((Component)box).gameObject;
		UIWidget component = gameObject.GetComponent<UIWidget>();
		if ((Object)(object)component != (Object)null)
		{
			Vector4 drawRegion = component.drawRegion;
			if (drawRegion.x != 0f || drawRegion.y != 0f || drawRegion.z != 1f || drawRegion.w != 1f)
			{
				Vector4 drawingDimensions = component.drawingDimensions;
				box.center = new Vector3((drawingDimensions.x + drawingDimensions.z) * 0.5f, (drawingDimensions.y + drawingDimensions.w) * 0.5f);
				box.size = new Vector3(drawingDimensions.z - drawingDimensions.x, drawingDimensions.w - drawingDimensions.y);
			}
			else
			{
				Vector3[] localCorners = component.localCorners;
				box.center = Vector3.Lerp(localCorners[0], localCorners[2], 0.5f);
				box.size = localCorners[2] - localCorners[0];
			}
		}
		else
		{
			Bounds val = NGUIMath.CalculateRelativeWidgetBounds(gameObject.transform, considerInactive);
			box.center = ((Bounds)(ref val)).center;
			box.size = new Vector3(((Bounds)(ref val)).size.x, ((Bounds)(ref val)).size.y, 0f);
		}
	}

	public static void UpdateWidgetCollider(BoxCollider2D box, bool considerInactive)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)box != (Object)null)
		{
			GameObject gameObject = ((Component)box).gameObject;
			UIWidget component = gameObject.GetComponent<UIWidget>();
			if ((Object)(object)component != (Object)null)
			{
				Vector3[] localCorners = component.localCorners;
				((Collider2D)box).offset = Vector2.op_Implicit(Vector3.Lerp(localCorners[0], localCorners[2], 0.5f));
				box.size = Vector2.op_Implicit(localCorners[2] - localCorners[0]);
			}
			else
			{
				Bounds val = NGUIMath.CalculateRelativeWidgetBounds(gameObject.transform, considerInactive);
				((Collider2D)box).offset = Vector2.op_Implicit(((Bounds)(ref val)).center);
				box.size = new Vector2(((Bounds)(ref val)).size.x, ((Bounds)(ref val)).size.y);
			}
		}
	}

	public static string GetTypeName<T>()
	{
		string text = typeof(T).ToString();
		if (text.StartsWith("UI"))
		{
			text = text.Substring(2);
		}
		else if (text.StartsWith("UnityEngine."))
		{
			text = text.Substring(12);
		}
		return text;
	}

	public static string GetTypeName(Object obj)
	{
		if (obj == (Object)null)
		{
			return "Null";
		}
		string text = ((object)obj).GetType().ToString();
		if (text.StartsWith("UI"))
		{
			text = text.Substring(2);
		}
		else if (text.StartsWith("UnityEngine."))
		{
			text = text.Substring(12);
		}
		return text;
	}

	public static void RegisterUndo(Object obj, string name)
	{
	}

	public static void SetDirty(Object obj)
	{
	}

	public static GameObject AddChild(this GameObject parent)
	{
		return parent.AddChild(undo: true);
	}

	public static GameObject AddChild(this GameObject parent, bool undo)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		if ((Object)(object)parent != (Object)null)
		{
			Transform transform = val.transform;
			transform.parent = parent.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			val.layer = parent.layer;
		}
		return val;
	}

	public static GameObject AddChild(this GameObject parent, GameObject prefab)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(prefab);
		if ((Object)(object)val != (Object)null && (Object)(object)parent != (Object)null)
		{
			Transform transform = val.transform;
			transform.parent = parent.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			val.layer = parent.layer;
		}
		return val;
	}

	public static int CalculateRaycastDepth(GameObject go)
	{
		UIWidget component = go.GetComponent<UIWidget>();
		if ((Object)(object)component != (Object)null)
		{
			return component.raycastDepth;
		}
		UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>();
		if (componentsInChildren.Length == 0)
		{
			return 0;
		}
		int num = int.MaxValue;
		int i = 0;
		for (int num2 = componentsInChildren.Length; i < num2; i++)
		{
			if (((Behaviour)componentsInChildren[i]).enabled)
			{
				num = Mathf.Min(num, componentsInChildren[i].raycastDepth);
			}
		}
		return num;
	}

	public static int CalculateNextDepth(GameObject go)
	{
		if (Object.op_Implicit((Object)(object)go))
		{
			int num = -1;
			UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num2 = componentsInChildren.Length; i < num2; i++)
			{
				num = Mathf.Max(num, componentsInChildren[i].depth);
			}
			return num + 1;
		}
		return 0;
	}

	public static int CalculateNextDepth(GameObject go, bool ignoreChildrenWithColliders)
	{
		if (Object.op_Implicit((Object)(object)go) && ignoreChildrenWithColliders)
		{
			int num = -1;
			UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num2 = componentsInChildren.Length; i < num2; i++)
			{
				UIWidget uIWidget = componentsInChildren[i];
				if (!((Object)(object)uIWidget.cachedGameObject != (Object)(object)go) || (!((Object)(object)((Component)uIWidget).GetComponent<Collider>() != (Object)null) && !((Object)(object)((Component)uIWidget).GetComponent<Collider2D>() != (Object)null)))
				{
					num = Mathf.Max(num, uIWidget.depth);
				}
			}
			return num + 1;
		}
		return CalculateNextDepth(go);
	}

	public static int AdjustDepth(GameObject go, int adjustment)
	{
		if ((Object)(object)go != (Object)null)
		{
			UIPanel component = go.GetComponent<UIPanel>();
			if ((Object)(object)component != (Object)null)
			{
				UIPanel[] componentsInChildren = go.GetComponentsInChildren<UIPanel>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].depth += adjustment;
				}
				return 1;
			}
			component = NGUITools.FindInParents<UIPanel>(go);
			if ((Object)(object)component == (Object)null)
			{
				return 0;
			}
			UIWidget[] componentsInChildren2 = go.GetComponentsInChildren<UIWidget>(true);
			int j = 0;
			for (int num = componentsInChildren2.Length; j < num; j++)
			{
				UIWidget uIWidget = componentsInChildren2[j];
				if (!((Object)(object)uIWidget.panel != (Object)(object)component))
				{
					uIWidget.depth += adjustment;
				}
			}
			return 2;
		}
		return 0;
	}

	public static void BringForward(GameObject go)
	{
		switch (AdjustDepth(go, 1000))
		{
		case 1:
			NormalizePanelDepths();
			break;
		case 2:
			NormalizeWidgetDepths();
			break;
		}
	}

	public static void PushBack(GameObject go)
	{
		switch (AdjustDepth(go, -1000))
		{
		case 1:
			NormalizePanelDepths();
			break;
		case 2:
			NormalizeWidgetDepths();
			break;
		}
	}

	public static void NormalizeDepths()
	{
		NormalizeWidgetDepths();
		NormalizePanelDepths();
	}

	public static void NormalizeWidgetDepths()
	{
		NormalizeWidgetDepths(NGUITools.FindActive<UIWidget>());
	}

	public static void NormalizeWidgetDepths(GameObject go)
	{
		NormalizeWidgetDepths(go.GetComponentsInChildren<UIWidget>());
	}

	public static void NormalizeWidgetDepths(UIWidget[] list)
	{
		int num = list.Length;
		if (num <= 0)
		{
			return;
		}
		Array.Sort(list, UIWidget.FullCompareFunc);
		int num2 = 0;
		int depth = list[0].depth;
		for (int i = 0; i < num; i++)
		{
			UIWidget uIWidget = list[i];
			if (uIWidget.depth == depth)
			{
				uIWidget.depth = num2;
				continue;
			}
			depth = uIWidget.depth;
			num2 = (uIWidget.depth = num2 + 1);
		}
	}

	public static void NormalizePanelDepths()
	{
		UIPanel[] array = NGUITools.FindActive<UIPanel>();
		int num = array.Length;
		if (num <= 0)
		{
			return;
		}
		Array.Sort(array, UIPanel.CompareFunc);
		int num2 = 0;
		int depth = array[0].depth;
		for (int i = 0; i < num; i++)
		{
			UIPanel uIPanel = array[i];
			if (uIPanel.depth == depth)
			{
				uIPanel.depth = num2;
				continue;
			}
			depth = uIPanel.depth;
			num2 = (uIPanel.depth = num2 + 1);
		}
	}

	public static UIPanel CreateUI(bool advanced3D)
	{
		return CreateUI(null, advanced3D, -1);
	}

	public static UIPanel CreateUI(bool advanced3D, int layer)
	{
		return CreateUI(null, advanced3D, layer);
	}

	public static UIPanel CreateUI(Transform trans, bool advanced3D, int layer)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Invalid comparison between Unknown and I4
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Invalid comparison between Unknown and I4
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		UIRoot uIRoot = ((!((Object)(object)trans != (Object)null)) ? null : NGUITools.FindInParents<UIRoot>(((Component)trans).gameObject));
		if ((Object)(object)uIRoot == (Object)null && UIRoot.list.Count > 0)
		{
			foreach (UIRoot item in UIRoot.list)
			{
				if (((Component)item).gameObject.layer == layer)
				{
					uIRoot = item;
					break;
				}
			}
		}
		if ((Object)(object)uIRoot == (Object)null)
		{
			int i = 0;
			for (int count = UIPanel.list.Count; i < count; i++)
			{
				UIPanel uIPanel = UIPanel.list[i];
				GameObject gameObject = ((Component)uIPanel).gameObject;
				if ((int)((Object)gameObject).hideFlags == 0 && gameObject.layer == layer)
				{
					trans.parent = ((Component)uIPanel).transform;
					trans.localScale = Vector3.one;
					return uIPanel;
				}
			}
		}
		if ((Object)(object)uIRoot != (Object)null)
		{
			UICamera componentInChildren = ((Component)uIRoot).GetComponentInChildren<UICamera>();
			if ((Object)(object)componentInChildren != (Object)null && ((Component)componentInChildren).GetComponent<Camera>().orthographic == advanced3D)
			{
				trans = null;
				uIRoot = null;
			}
		}
		if ((Object)(object)uIRoot == (Object)null)
		{
			GameObject val = ((GameObject)null).AddChild(undo: false);
			uIRoot = val.AddComponent<UIRoot>();
			if (layer == -1)
			{
				layer = LayerMask.NameToLayer("UI");
			}
			if (layer == -1)
			{
				layer = LayerMask.NameToLayer("2D UI");
			}
			val.layer = layer;
			if (advanced3D)
			{
				((Object)val).name = "UI Root (3D)";
				uIRoot.scalingStyle = UIRoot.Scaling.Constrained;
			}
			else
			{
				((Object)val).name = "UI Root";
				uIRoot.scalingStyle = UIRoot.Scaling.Flexible;
			}
		}
		UIPanel uIPanel2 = ((Component)uIRoot).GetComponentInChildren<UIPanel>();
		if ((Object)(object)uIPanel2 == (Object)null)
		{
			Camera[] array = NGUITools.FindActive<Camera>();
			float num = -1f;
			bool flag = false;
			int num2 = 1 << ((Component)uIRoot).gameObject.layer;
			foreach (Camera val2 in array)
			{
				if ((int)val2.clearFlags == 2 || (int)val2.clearFlags == 1)
				{
					flag = true;
				}
				num = Mathf.Max(num, val2.depth);
				val2.cullingMask &= ~num2;
			}
			Camera val3 = ((Component)uIRoot).gameObject.AddChild<Camera>(undo: false);
			((Component)val3).gameObject.AddComponent<UICamera>();
			val3.clearFlags = (CameraClearFlags)((!flag) ? 2 : 3);
			val3.backgroundColor = Color.grey;
			val3.cullingMask = num2;
			val3.depth = num + 1f;
			if (advanced3D)
			{
				val3.nearClipPlane = 0.1f;
				val3.farClipPlane = 4f;
				((Component)val3).transform.localPosition = new Vector3(0f, 0f, -700f);
			}
			else
			{
				val3.orthographic = true;
				val3.orthographicSize = 1f;
				val3.nearClipPlane = -10f;
				val3.farClipPlane = 10f;
			}
			AudioListener[] array2 = NGUITools.FindActive<AudioListener>();
			if (array2 == null || array2.Length == 0)
			{
				((Component)val3).gameObject.AddComponent<AudioListener>();
			}
			uIPanel2 = ((Component)uIRoot).gameObject.AddComponent<UIPanel>();
		}
		if ((Object)(object)trans != (Object)null)
		{
			while ((Object)(object)trans.parent != (Object)null)
			{
				trans = trans.parent;
			}
			if (IsChild(trans, ((Component)uIPanel2).transform))
			{
				uIPanel2 = ((Component)trans).gameObject.AddComponent<UIPanel>();
			}
			else
			{
				trans.parent = ((Component)uIPanel2).transform;
				trans.localScale = Vector3.one;
				trans.localPosition = Vector3.zero;
				uIPanel2.cachedTransform.SetChildLayer(uIPanel2.cachedGameObject.layer);
			}
		}
		return uIPanel2;
	}

	public static void SetChildLayer(this Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			((Component)child).gameObject.layer = layer;
			child.SetChildLayer(layer);
		}
	}

	public static T AddChild<T>(this GameObject parent) where T : Component
	{
		GameObject val = parent.AddChild();
		if (!mTypeNames.TryGetValue(typeof(T), out var value) || value == null)
		{
			value = GetTypeName<T>();
			mTypeNames[typeof(T)] = value;
		}
		((Object)val).name = value;
		return val.AddComponent<T>();
	}

	public static T AddChild<T>(this GameObject parent, bool undo) where T : Component
	{
		GameObject val = parent.AddChild(undo);
		if (!mTypeNames.TryGetValue(typeof(T), out var value) || value == null)
		{
			value = GetTypeName<T>();
			mTypeNames[typeof(T)] = value;
		}
		((Object)val).name = value;
		return val.AddComponent<T>();
	}

	public static T AddWidget<T>(this GameObject go, int depth = int.MaxValue) where T : UIWidget
	{
		if (depth == int.MaxValue)
		{
			depth = CalculateNextDepth(go);
		}
		T result = go.AddChild<T>();
		result.width = 100;
		result.height = 100;
		result.depth = depth;
		return result;
	}

	public static UISprite AddSprite(this GameObject go, UIAtlas atlas, string spriteName, int depth = int.MaxValue)
	{
		UISpriteData uISpriteData = ((!((Object)(object)atlas != (Object)null)) ? null : atlas.GetSprite(spriteName));
		UISprite uISprite = go.AddWidget<UISprite>(depth);
		uISprite.type = ((uISpriteData != null && uISpriteData.hasBorder) ? UIBasicSprite.Type.Sliced : UIBasicSprite.Type.Simple);
		uISprite.atlas = atlas;
		uISprite.spriteName = spriteName;
		return uISprite;
	}

	public static GameObject GetRoot(GameObject go)
	{
		Transform val = go.transform;
		while (true)
		{
			Transform parent = val.parent;
			if ((Object)(object)parent == (Object)null)
			{
				break;
			}
			val = parent;
		}
		return ((Component)val).gameObject;
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if ((Object)(object)go == (Object)null)
		{
			return (T)(object)null;
		}
		T component = go.GetComponent<T>();
		if ((Object)(object)component == (Object)null)
		{
			Transform parent = go.transform.parent;
			while ((Object)(object)parent != (Object)null && (Object)(object)component == (Object)null)
			{
				component = ((Component)parent).gameObject.GetComponent<T>();
				parent = parent.parent;
			}
		}
		return component;
	}

	public static T FindInParents<T>(Transform trans) where T : Component
	{
		if ((Object)(object)trans == (Object)null)
		{
			return (T)(object)null;
		}
		return ((Component)trans).GetComponentInParent<T>();
	}

	public static void Destroy(Object obj)
	{
		if (!Object.op_Implicit(obj))
		{
			return;
		}
		if (obj is Transform)
		{
			Transform val = (Transform)(object)((obj is Transform) ? obj : null);
			GameObject gameObject = ((Component)val).gameObject;
			if (Application.isPlaying)
			{
				val.parent = null;
				Object.Destroy((Object)(object)gameObject);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)gameObject);
			}
		}
		else if (obj is GameObject)
		{
			GameObject val2 = (GameObject)(object)((obj is GameObject) ? obj : null);
			Transform transform = val2.transform;
			if (Application.isPlaying)
			{
				transform.parent = null;
				Object.Destroy((Object)(object)val2);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)val2);
			}
		}
		else if (Application.isPlaying)
		{
			Object.Destroy(obj);
		}
		else
		{
			Object.DestroyImmediate(obj);
		}
	}

	public static void DestroyChildren(this Transform t)
	{
		bool isPlaying = Application.isPlaying;
		while (t.childCount != 0)
		{
			Transform child = t.GetChild(0);
			if (isPlaying)
			{
				child.parent = null;
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)((Component)child).gameObject);
			}
		}
	}

	public static void DestroyImmediate(Object obj)
	{
		if (obj != (Object)null)
		{
			if (Application.isEditor)
			{
				Object.DestroyImmediate(obj);
			}
			else
			{
				Object.Destroy(obj);
			}
		}
	}

	public static void Broadcast(string funcName)
	{
		GameObject[] array = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i].SendMessage(funcName, (SendMessageOptions)1);
		}
	}

	public static void Broadcast(string funcName, object param)
	{
		GameObject[] array = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i].SendMessage(funcName, param, (SendMessageOptions)1);
		}
	}

	public static bool IsChild(Transform parent, Transform child)
	{
		if ((Object)(object)parent == (Object)null || (Object)(object)child == (Object)null)
		{
			return false;
		}
		while ((Object)(object)child != (Object)null)
		{
			if ((Object)(object)child == (Object)(object)parent)
			{
				return true;
			}
			child = child.parent;
		}
		return false;
	}

	private static void Activate(Transform t)
	{
		Activate(t, compatibilityMode: false);
	}

	private static void Activate(Transform t, bool compatibilityMode)
	{
		SetActiveSelf(((Component)t).gameObject, state: true);
		if (!compatibilityMode)
		{
			return;
		}
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			if (((Component)child).gameObject.activeSelf)
			{
				return;
			}
		}
		int j = 0;
		for (int childCount2 = t.childCount; j < childCount2; j++)
		{
			Transform child2 = t.GetChild(j);
			Activate(child2, compatibilityMode: true);
		}
	}

	private static void Deactivate(Transform t)
	{
		SetActiveSelf(((Component)t).gameObject, state: false);
	}

	public static void SetActive(GameObject go, bool state)
	{
		SetActive(go, state, compatibilityMode: true);
	}

	public static void SetActive(GameObject go, bool state, bool compatibilityMode)
	{
		if (Object.op_Implicit((Object)(object)go))
		{
			if (state)
			{
				Activate(go.transform, compatibilityMode);
				CallCreatePanel(go.transform);
			}
			else
			{
				Deactivate(go.transform);
			}
		}
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	private static void CallCreatePanel(Transform t)
	{
		UIWidget component = ((Component)t).GetComponent<UIWidget>();
		if ((Object)(object)component != (Object)null)
		{
			component.CreatePanel();
		}
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			CallCreatePanel(t.GetChild(i));
		}
	}

	public static void SetActiveChildren(GameObject go, bool state)
	{
		Transform transform = go.transform;
		if (state)
		{
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				Activate(child);
			}
		}
		else
		{
			int j = 0;
			for (int childCount2 = transform.childCount; j < childCount2; j++)
			{
				Transform child2 = transform.GetChild(j);
				Deactivate(child2);
			}
		}
	}

	[Obsolete("Use NGUITools.GetActive instead")]
	public static bool IsActive(Behaviour mb)
	{
		return (Object)(object)mb != (Object)null && mb.enabled && ((Component)mb).gameObject.activeInHierarchy;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static bool GetActive(Behaviour mb)
	{
		return Object.op_Implicit((Object)(object)mb) && mb.enabled && ((Component)mb).gameObject.activeInHierarchy;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static bool GetActive(GameObject go)
	{
		return Object.op_Implicit((Object)(object)go) && go.activeInHierarchy;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static void SetActiveSelf(GameObject go, bool state)
	{
		go.SetActive(state);
	}

	public static void SetLayer(GameObject go, int layer)
	{
		go.layer = layer;
		Transform transform = go.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			SetLayer(((Component)child).gameObject, layer);
		}
	}

	public static Vector3 Round(Vector3 v)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		v.x = Mathf.Round(v.x);
		v.y = Mathf.Round(v.y);
		v.z = Mathf.Round(v.z);
		return v;
	}

	public static void MakePixelPerfect(Transform t)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)t).GetComponent<UIWidget>();
		if ((Object)(object)component != (Object)null)
		{
			component.MakePixelPerfect();
		}
		if ((Object)(object)((Component)t).GetComponent<UIAnchor>() == (Object)null && (Object)(object)((Component)t).GetComponent<UIRoot>() == (Object)null)
		{
			t.localPosition = Round(t.localPosition);
			t.localScale = Round(t.localScale);
		}
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			MakePixelPerfect(t.GetChild(i));
		}
	}

	public static void FitOnScreen(this Camera cam, Transform transform, Vector3 pos)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		cam.FitOnScreen(transform, transform, pos);
	}

	public static void FitOnScreen(this Camera cam, Transform transform, Transform content, Vector3 pos)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		cam.FitOnScreen(transform, content, pos, out var _);
	}

	public static void FitOnScreen(this Camera cam, Transform transform, Transform content, Vector3 pos, out Bounds bounds)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		bounds = NGUIMath.CalculateRelativeWidgetBounds(transform, content);
		Vector3 min = ((Bounds)(ref bounds)).min;
		Vector3 max = ((Bounds)(ref bounds)).max;
		Vector3 size = ((Bounds)(ref bounds)).size;
		size.x += min.x;
		size.y -= max.y;
		if ((Object)(object)cam != (Object)null)
		{
			pos.x = Mathf.Clamp01(pos.x / (float)Screen.width);
			pos.y = Mathf.Clamp01(pos.y / (float)Screen.height);
			float num = cam.orthographicSize / transform.parent.lossyScale.y;
			float num2 = (float)Screen.height * 0.5f / num;
			max = Vector2.op_Implicit(new Vector2(num2 * size.x / (float)Screen.width, num2 * size.y / (float)Screen.height));
			pos.x = Mathf.Min(pos.x, 1f - max.x);
			pos.y = Mathf.Max(pos.y, max.y);
			transform.position = cam.ViewportToWorldPoint(pos);
			pos = transform.localPosition;
			pos.x = Mathf.Round(pos.x);
			pos.y = Mathf.Round(pos.y);
		}
		else
		{
			if (pos.x + size.x > (float)Screen.width)
			{
				pos.x = (float)Screen.width - size.x;
			}
			if (pos.y - size.y < 0f)
			{
				pos.y = size.y;
			}
			pos.x -= (float)Screen.width * 0.5f;
			pos.y -= (float)Screen.height * 0.5f;
		}
		transform.localPosition = pos;
	}

	public static bool Save(string fileName, byte[] bytes)
	{
		if (!fileAccess)
		{
			return false;
		}
		string path = Application.persistentDataPath + "/" + fileName;
		if (bytes == null)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			return true;
		}
		FileStream fileStream = null;
		try
		{
			fileStream = File.Create(path);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex.Message);
			return false;
		}
		fileStream.Write(bytes, 0, bytes.Length);
		fileStream.Close();
		return true;
	}

	public static byte[] Load(string fileName)
	{
		if (!fileAccess)
		{
			return null;
		}
		string path = Application.persistentDataPath + "/" + fileName;
		if (File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}
		return null;
	}

	public static Color ApplyPMA(Color c)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (c.a != 1f)
		{
			c.r *= c.a;
			c.g *= c.a;
			c.b *= c.a;
		}
		return c;
	}

	public static void MarkParentAsChanged(GameObject go)
	{
		UIRect[] componentsInChildren = go.GetComponentsInChildren<UIRect>();
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			componentsInChildren[i].ParentHasChanged();
		}
	}

	[Obsolete("Use NGUIText.EncodeColor instead")]
	public static string EncodeColor(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return NGUIText.EncodeColor24(c);
	}

	[Obsolete("Use NGUIText.ParseColor instead")]
	public static Color ParseColor(string text, int offset)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return NGUIText.ParseColor24(text, offset);
	}

	[Obsolete("Use NGUIText.StripSymbols instead")]
	public static string StripSymbols(string text)
	{
		return NGUIText.StripSymbols(text);
	}

	public static T AddMissingComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static Vector3[] GetSides(this Camera cam)
	{
		return cam.GetSides(Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, 0.5f), null);
	}

	public static Vector3[] GetSides(this Camera cam, float depth)
	{
		return cam.GetSides(depth, null);
	}

	public static Vector3[] GetSides(this Camera cam, Transform relativeTo)
	{
		return cam.GetSides(Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, 0.5f), relativeTo);
	}

	public static Vector3[] GetSides(this Camera cam, float depth, Transform relativeTo)
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if (cam.orthographic)
		{
			float orthographicSize = cam.orthographicSize;
			float num = 0f - orthographicSize;
			float num2 = orthographicSize;
			float num3 = 0f - orthographicSize;
			float num4 = orthographicSize;
			Rect rect = cam.rect;
			Vector2 val = screenSize;
			float num5 = val.x / val.y;
			num5 *= ((Rect)(ref rect)).width / ((Rect)(ref rect)).height;
			num *= num5;
			num2 *= num5;
			Transform transform = ((Component)cam).transform;
			Quaternion rotation = transform.rotation;
			Vector3 position = transform.position;
			ref Vector3 reference = ref mSides[0];
			reference = rotation * new Vector3(num, 0f, depth) + position;
			ref Vector3 reference2 = ref mSides[1];
			reference2 = rotation * new Vector3(0f, num4, depth) + position;
			ref Vector3 reference3 = ref mSides[2];
			reference3 = rotation * new Vector3(num2, 0f, depth) + position;
			ref Vector3 reference4 = ref mSides[3];
			reference4 = rotation * new Vector3(0f, num3, depth) + position;
		}
		else
		{
			ref Vector3 reference5 = ref mSides[0];
			reference5 = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth));
			ref Vector3 reference6 = ref mSides[1];
			reference6 = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
			ref Vector3 reference7 = ref mSides[2];
			reference7 = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));
			ref Vector3 reference8 = ref mSides[3];
			reference8 = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth));
		}
		if ((Object)(object)relativeTo != (Object)null)
		{
			for (int i = 0; i < 4; i++)
			{
				ref Vector3 reference9 = ref mSides[i];
				reference9 = relativeTo.InverseTransformPoint(mSides[i]);
			}
		}
		return mSides;
	}

	public static Vector3[] GetWorldCorners(this Camera cam)
	{
		float depth = Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, 0.5f);
		return cam.GetWorldCorners(depth, null);
	}

	public static Vector3[] GetWorldCorners(this Camera cam, float depth)
	{
		return cam.GetWorldCorners(depth, null);
	}

	public static Vector3[] GetWorldCorners(this Camera cam, Transform relativeTo)
	{
		return cam.GetWorldCorners(Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, 0.5f), relativeTo);
	}

	public static Vector3[] GetWorldCorners(this Camera cam, float depth, Transform relativeTo)
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		if (cam.orthographic)
		{
			float orthographicSize = cam.orthographicSize;
			float num = 0f - orthographicSize;
			float num2 = orthographicSize;
			float num3 = 0f - orthographicSize;
			float num4 = orthographicSize;
			Rect rect = cam.rect;
			Vector2 val = screenSize;
			float num5 = val.x / val.y;
			num5 *= ((Rect)(ref rect)).width / ((Rect)(ref rect)).height;
			num *= num5;
			num2 *= num5;
			Transform transform = ((Component)cam).transform;
			Quaternion rotation = transform.rotation;
			Vector3 position = transform.position;
			ref Vector3 reference = ref mSides[0];
			reference = rotation * new Vector3(num, num3, depth) + position;
			ref Vector3 reference2 = ref mSides[1];
			reference2 = rotation * new Vector3(num, num4, depth) + position;
			ref Vector3 reference3 = ref mSides[2];
			reference3 = rotation * new Vector3(num2, num4, depth) + position;
			ref Vector3 reference4 = ref mSides[3];
			reference4 = rotation * new Vector3(num2, num3, depth) + position;
		}
		else
		{
			ref Vector3 reference5 = ref mSides[0];
			reference5 = cam.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
			ref Vector3 reference6 = ref mSides[1];
			reference6 = cam.ViewportToWorldPoint(new Vector3(0f, 1f, depth));
			ref Vector3 reference7 = ref mSides[2];
			reference7 = cam.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
			ref Vector3 reference8 = ref mSides[3];
			reference8 = cam.ViewportToWorldPoint(new Vector3(1f, 0f, depth));
		}
		if ((Object)(object)relativeTo != (Object)null)
		{
			for (int i = 0; i < 4; i++)
			{
				ref Vector3 reference9 = ref mSides[i];
				reference9 = relativeTo.InverseTransformPoint(mSides[i]);
			}
		}
		return mSides;
	}

	public static string GetFuncName(object obj, string method)
	{
		if (obj == null)
		{
			return "<null>";
		}
		string text = obj.GetType().ToString();
		int num = text.LastIndexOf('/');
		if (num > 0)
		{
			text = text.Substring(num + 1);
		}
		return (!string.IsNullOrEmpty(method)) ? (text + "/" + method) : text;
	}

	public static void Execute<T>(GameObject go, string funcName) where T : Component
	{
		T[] components = go.GetComponents<T>();
		T[] array = components;
		for (int i = 0; i < array.Length; i++)
		{
			T val = array[i];
			((object)val).GetType().GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(val, null);
		}
	}

	public static void ExecuteAll<T>(GameObject root, string funcName) where T : Component
	{
		Execute<T>(root, funcName);
		Transform transform = root.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			ExecuteAll<T>(((Component)transform.GetChild(i)).gameObject, funcName);
		}
	}

	public static void ImmediatelyCreateDrawCalls(GameObject root)
	{
		NGUITools.ExecuteAll<UIWidget>(root, "Start");
		NGUITools.ExecuteAll<UIPanel>(root, "Start");
		NGUITools.ExecuteAll<UIWidget>(root, "Update");
		NGUITools.ExecuteAll<UIPanel>(root, "Update");
		NGUITools.ExecuteAll<UIPanel>(root, "LateUpdate");
	}

	public static string KeyToCaption(KeyCode key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected I4, but got Unknown
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected I4, but got Unknown
		return (int)key switch
		{
			0 => null, 
			8 => "BS", 
			9 => "Tab", 
			12 => "Clr", 
			13 => "NT", 
			19 => "PS", 
			27 => "Esc", 
			32 => "SP", 
			33 => "!", 
			34 => "\"", 
			35 => "#", 
			36 => "$", 
			38 => "&", 
			39 => "'", 
			40 => "(", 
			41 => ")", 
			42 => "*", 
			43 => "+", 
			44 => ",", 
			45 => "-", 
			46 => ".", 
			47 => "/", 
			48 => "0", 
			49 => "1", 
			50 => "2", 
			51 => "3", 
			52 => "4", 
			53 => "5", 
			54 => "6", 
			55 => "7", 
			56 => "8", 
			57 => "9", 
			58 => ":", 
			59 => ";", 
			60 => "<", 
			61 => "=", 
			62 => ">", 
			63 => "?", 
			64 => "@", 
			91 => "[", 
			92 => "\\", 
			93 => "]", 
			94 => "^", 
			95 => "_", 
			96 => "`", 
			97 => "A", 
			98 => "B", 
			99 => "C", 
			100 => "D", 
			101 => "E", 
			102 => "F", 
			103 => "G", 
			104 => "H", 
			105 => "I", 
			106 => "J", 
			107 => "K", 
			108 => "L", 
			109 => "M", 
			110 => "N0", 
			111 => "O", 
			112 => "P", 
			113 => "Q", 
			114 => "R", 
			115 => "S", 
			116 => "T", 
			117 => "U", 
			118 => "V", 
			119 => "W", 
			120 => "X", 
			121 => "Y", 
			122 => "Z", 
			127 => "Del", 
			_ => (key - 256) switch
			{
				0 => "K0", 
				1 => "K1", 
				2 => "K2", 
				3 => "K3", 
				4 => "K4", 
				5 => "K5", 
				6 => "K6", 
				7 => "K7", 
				8 => "K8", 
				9 => "K9", 
				10 => ".", 
				11 => "/", 
				12 => "*", 
				13 => "-", 
				14 => "+", 
				15 => "NT", 
				16 => "=", 
				17 => "UP", 
				18 => "DN", 
				19 => "LT", 
				20 => "RT", 
				21 => "Ins", 
				22 => "Home", 
				23 => "End", 
				24 => "PU", 
				25 => "PD", 
				26 => "F1", 
				27 => "F2", 
				28 => "F3", 
				29 => "F4", 
				30 => "F5", 
				31 => "F6", 
				32 => "F7", 
				33 => "F8", 
				34 => "F9", 
				35 => "F10", 
				36 => "F11", 
				37 => "F12", 
				38 => "F13", 
				39 => "F14", 
				40 => "F15", 
				44 => "Num", 
				45 => "Cap", 
				46 => "Scr", 
				47 => "RS", 
				48 => "LS", 
				49 => "RC", 
				50 => "LC", 
				51 => "RA", 
				52 => "LA", 
				67 => "M0", 
				68 => "M1", 
				69 => "M2", 
				70 => "M3", 
				71 => "M4", 
				72 => "M5", 
				73 => "M6", 
				74 => "(A)", 
				75 => "(B)", 
				76 => "(X)", 
				77 => "(Y)", 
				78 => "(RB)", 
				79 => "(LB)", 
				80 => "(Back)", 
				81 => "(Start)", 
				82 => "(LS)", 
				83 => "(RS)", 
				84 => "J10", 
				85 => "J11", 
				86 => "J12", 
				87 => "J13", 
				88 => "J14", 
				89 => "J15", 
				90 => "J16", 
				91 => "J17", 
				92 => "J18", 
				93 => "J19", 
				_ => null, 
			}, 
		};
	}

	public static T Draw<T>(string id, OnInitFunc<T> onInit = null) where T : UIWidget
	{
		if (mWidgets.TryGetValue(id, out var value) && Object.op_Implicit((Object)(object)value))
		{
			return (T)value;
		}
		if ((Object)(object)mRoot == (Object)null)
		{
			UICamera uICamera = null;
			UIRoot uIRoot = null;
			for (int i = 0; i < UIRoot.list.Count; i++)
			{
				UIRoot uIRoot2 = UIRoot.list[i];
				if (Object.op_Implicit((Object)(object)uIRoot2))
				{
					UICamera uICamera2 = UICamera.FindCameraForLayer(((Component)uIRoot2).gameObject.layer);
					if (Object.op_Implicit((Object)(object)uICamera2) && uICamera2.cachedCamera.orthographic)
					{
						uICamera = uICamera2;
						uIRoot = uIRoot2;
						break;
					}
				}
			}
			if ((Object)(object)uICamera == (Object)null)
			{
				mRoot = CreateUI(advanced3D: false, LayerMask.NameToLayer("UI"));
			}
			else
			{
				mRoot = ((Component)uIRoot).gameObject.AddChild<UIPanel>();
			}
			mRoot.depth = 100000;
			mGo = ((Component)mRoot).gameObject;
			((Object)mGo).name = "Immediate Mode GUI";
		}
		value = mGo.AddWidget<T>();
		((Object)value).name = id;
		mWidgets[id] = value;
		onInit?.Invoke((T)value);
		return (T)value;
	}

	public static Color GammaToLinearSpace(this Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)mColorSpace == -1)
		{
			mColorSpace = QualitySettings.activeColorSpace;
		}
		if ((int)mColorSpace == 1)
		{
			c.r = Mathf.GammaToLinearSpace(c.r);
			c.g = Mathf.GammaToLinearSpace(c.g);
			c.b = Mathf.GammaToLinearSpace(c.b);
			c.a = Mathf.GammaToLinearSpace(c.a);
		}
		return c;
	}
}
