using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIManager")]
public class tk2dUIManager : MonoBehaviour
{
	private const int MAX_MULTI_TOUCH_COUNT = 5;

	private const string MOUSE_WHEEL_AXES_NAME = "Mouse ScrollWheel";

	public static double version = 1.0;

	public static int releaseId = 0;

	private static tk2dUIManager instance;

	[SerializeField]
	private Camera uiCamera;

	private static List<tk2dUICamera> allCameras = new List<tk2dUICamera>();

	private List<tk2dUICamera> sortedCameras = new List<tk2dUICamera>();

	public LayerMask raycastLayerMask = LayerMask.op_Implicit(-1);

	private bool inputEnabled = true;

	public bool areHoverEventsTracked = true;

	private tk2dUIItem pressedUIItem;

	private tk2dUIItem overUIItem;

	private tk2dUITouch firstPressedUIItemTouch;

	private bool checkForHovers = true;

	[SerializeField]
	private bool useMultiTouch;

	private tk2dUITouch[] allTouches = new tk2dUITouch[5];

	private List<tk2dUIItem> prevPressedUIItemList = new List<tk2dUIItem>();

	private tk2dUIItem[] pressedUIItems = new tk2dUIItem[5];

	private int touchCounter;

	private Vector2 mouseDownFirstPos = Vector2.zero;

	private tk2dUITouch primaryTouch = default(tk2dUITouch);

	private tk2dUITouch secondaryTouch = default(tk2dUITouch);

	private tk2dUITouch resultTouch = default(tk2dUITouch);

	private tk2dUIItem hitUIItem;

	private RaycastHit hit;

	private Ray ray;

	private tk2dUITouch currTouch;

	private tk2dUIItem currPressedItem;

	private tk2dUIItem prevPressedItem;

	public static tk2dUIManager Instance
	{
		get
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				instance = Object.FindObjectOfType(typeof(tk2dUIManager)) as tk2dUIManager;
				if ((Object)(object)instance == (Object)null)
				{
					GameObject val = new GameObject("tk2dUIManager");
					instance = val.AddComponent<tk2dUIManager>();
				}
			}
			return instance;
		}
	}

	public static tk2dUIManager Instance__NoCreate => instance;

	public Camera UICamera
	{
		get
		{
			return uiCamera;
		}
		set
		{
			uiCamera = value;
		}
	}

	public bool InputEnabled
	{
		get
		{
			return inputEnabled;
		}
		set
		{
			if (inputEnabled && !value)
			{
				SortCameras();
				inputEnabled = value;
				if (useMultiTouch)
				{
					CheckMultiTouchInputs();
				}
				else
				{
					CheckInputs();
				}
			}
			else
			{
				inputEnabled = value;
			}
		}
	}

	public tk2dUIItem PressedUIItem
	{
		get
		{
			if (useMultiTouch)
			{
				if (pressedUIItems.Length > 0)
				{
					return pressedUIItems[pressedUIItems.Length - 1];
				}
				return null;
			}
			return pressedUIItem;
		}
	}

	public tk2dUIItem[] PressedUIItems => pressedUIItems;

	public bool UseMultiTouch
	{
		get
		{
			return useMultiTouch;
		}
		set
		{
			if (useMultiTouch != value && inputEnabled)
			{
				InputEnabled = false;
				useMultiTouch = value;
				InputEnabled = true;
			}
			else
			{
				useMultiTouch = value;
			}
		}
	}

	public event Action OnAnyPress;

	public event Action OnInputUpdate;

	public event Action<float> OnScrollWheelChange;

	public Camera GetUICameraForControl(GameObject go)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		int num = 1 << go.layer;
		int count = allCameras.Count;
		for (int i = 0; i < count; i++)
		{
			tk2dUICamera tk2dUICamera2 = allCameras[i];
			if ((LayerMask.op_Implicit(tk2dUICamera2.FilteredMask) & num) != 0)
			{
				return tk2dUICamera2.HostCamera;
			}
		}
		Debug.LogError((object)("Unable to find UI camera for " + ((Object)go).name));
		return null;
	}

	public static void RegisterCamera(tk2dUICamera cam)
	{
		allCameras.Add(cam);
	}

	public static void UnregisterCamera(tk2dUICamera cam)
	{
		allCameras.Remove(cam);
	}

	private void SortCameras()
	{
		sortedCameras.Clear();
		int count = allCameras.Count;
		for (int i = 0; i < count; i++)
		{
			tk2dUICamera tk2dUICamera2 = allCameras[i];
			if ((Object)(object)tk2dUICamera2 != (Object)null)
			{
				sortedCameras.Add(tk2dUICamera2);
			}
		}
		sortedCameras.Sort((tk2dUICamera a, tk2dUICamera b) => ((Component)b).GetComponent<Camera>().depth.CompareTo(((Component)a).GetComponent<Camera>().depth));
	}

	private void Awake()
	{
		if ((Object)(object)instance == (Object)null)
		{
			instance = this;
			if (((Component)instance).transform.childCount != 0)
			{
				Debug.LogError((object)"You should not attach anything to the tk2dUIManager object. The tk2dUIManager will not get destroyed between scene switches and any children will persist as well.");
			}
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			}
		}
		else if ((Object)(object)instance != (Object)(object)this)
		{
			if ((Object)(object)uiCamera != (Object)null)
			{
				HookUpLegacyCamera(uiCamera);
				uiCamera = null;
			}
			Object.Destroy((Object)(object)this);
			return;
		}
		tk2dUITime.Init();
		Setup();
	}

	private void HookUpLegacyCamera(Camera cam)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)cam).GetComponent<tk2dUICamera>() == (Object)null)
		{
			tk2dUICamera tk2dUICamera2 = ((Component)cam).gameObject.AddComponent<tk2dUICamera>();
			tk2dUICamera2.AssignRaycastLayerMask(raycastLayerMask);
		}
	}

	private void Start()
	{
		if ((Object)(object)uiCamera != (Object)null)
		{
			HookUpLegacyCamera(uiCamera);
			uiCamera = null;
		}
		if (allCameras.Count == 0)
		{
			Debug.LogError((object)"Unable to find any tk2dUICameras, and no cameras are connected to the tk2dUIManager. You will not be able to interact with the UI.");
		}
	}

	private void Setup()
	{
		if (!areHoverEventsTracked)
		{
			checkForHovers = false;
		}
	}

	private void Update()
	{
		tk2dUITime.Update();
		if (!inputEnabled)
		{
			return;
		}
		SortCameras();
		if (useMultiTouch)
		{
			CheckMultiTouchInputs();
		}
		else
		{
			CheckInputs();
		}
		if (this.OnInputUpdate != null)
		{
			this.OnInputUpdate();
		}
		if (this.OnScrollWheelChange != null)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis != 0f)
			{
				this.OnScrollWheelChange(axis);
			}
		}
	}

	private void CheckInputs()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Invalid comparison between Unknown and I4
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		primaryTouch = default(tk2dUITouch);
		secondaryTouch = default(tk2dUITouch);
		resultTouch = default(tk2dUITouch);
		hitUIItem = null;
		if (inputEnabled)
		{
			int touchCount = Input.touchCount;
			if (Input.touchCount > 0)
			{
				for (int i = 0; i < touchCount; i++)
				{
					Touch touch = Input.GetTouch(i);
					if ((int)((Touch)(ref touch)).phase == 0)
					{
						primaryTouch = new tk2dUITouch(touch);
						flag = true;
						flag3 = true;
					}
					else if ((Object)(object)pressedUIItem != (Object)null && ((Touch)(ref touch)).fingerId == firstPressedUIItemTouch.fingerId)
					{
						secondaryTouch = new tk2dUITouch(touch);
						flag2 = true;
					}
				}
				checkForHovers = false;
			}
			else if (Input.GetMouseButtonDown(0))
			{
				primaryTouch = new tk2dUITouch((TouchPhase)0, 9999, Vector2.op_Implicit(Input.mousePosition), Vector2.zero, 0f);
				flag = true;
				flag3 = true;
			}
			else if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
			{
				Vector2 val = Vector2.zero;
				TouchPhase phase = (TouchPhase)1;
				if ((Object)(object)pressedUIItem != (Object)null)
				{
					val = firstPressedUIItemTouch.position - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				}
				if (Input.GetMouseButtonUp(0))
				{
					phase = (TouchPhase)3;
				}
				else if (val == Vector2.zero)
				{
					phase = (TouchPhase)2;
				}
				secondaryTouch = new tk2dUITouch(phase, 9999, Vector2.op_Implicit(Input.mousePosition), val, tk2dUITime.deltaTime);
				flag2 = true;
			}
		}
		if (flag)
		{
			resultTouch = primaryTouch;
		}
		else if (flag2)
		{
			resultTouch = secondaryTouch;
		}
		if (flag || flag2)
		{
			hitUIItem = RaycastForUIItem(resultTouch.position);
			if ((int)resultTouch.phase == 0)
			{
				if ((Object)(object)pressedUIItem != (Object)null)
				{
					pressedUIItem.CurrentOverUIItem(hitUIItem);
					if ((Object)(object)pressedUIItem != (Object)(object)hitUIItem)
					{
						pressedUIItem.Release();
						pressedUIItem = null;
					}
					else
					{
						firstPressedUIItemTouch = resultTouch;
					}
				}
				if ((Object)(object)hitUIItem != (Object)null)
				{
					hitUIItem.Press(resultTouch);
				}
				pressedUIItem = hitUIItem;
				firstPressedUIItemTouch = resultTouch;
			}
			else if ((int)resultTouch.phase == 3)
			{
				if ((Object)(object)pressedUIItem != (Object)null)
				{
					pressedUIItem.CurrentOverUIItem(hitUIItem);
					pressedUIItem.UpdateTouch(resultTouch);
					pressedUIItem.Release();
					pressedUIItem = null;
				}
			}
			else if ((Object)(object)pressedUIItem != (Object)null)
			{
				pressedUIItem.CurrentOverUIItem(hitUIItem);
				pressedUIItem.UpdateTouch(resultTouch);
			}
		}
		else if ((Object)(object)pressedUIItem != (Object)null)
		{
			pressedUIItem.CurrentOverUIItem(null);
			pressedUIItem.Release();
			pressedUIItem = null;
		}
		if (checkForHovers)
		{
			if (inputEnabled)
			{
				if (!flag && !flag2 && (Object)(object)hitUIItem == (Object)null && !Input.GetMouseButton(0))
				{
					hitUIItem = RaycastForUIItem(Vector2.op_Implicit(Input.mousePosition));
				}
				else if (Input.GetMouseButton(0))
				{
					hitUIItem = null;
				}
			}
			if ((Object)(object)hitUIItem != (Object)null)
			{
				if (hitUIItem.isHoverEnabled)
				{
					if (!hitUIItem.HoverOver(overUIItem) && (Object)(object)overUIItem != (Object)null)
					{
						overUIItem.HoverOut(hitUIItem);
					}
					overUIItem = hitUIItem;
				}
				else if ((Object)(object)overUIItem != (Object)null)
				{
					overUIItem.HoverOut(null);
				}
			}
			else if ((Object)(object)overUIItem != (Object)null)
			{
				overUIItem.HoverOut(null);
			}
		}
		if (flag3 && this.OnAnyPress != null)
		{
			this.OnAnyPress();
		}
	}

	private void CheckMultiTouchInputs()
	{
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Invalid comparison between Unknown and I4
		bool flag = false;
		int num = -1;
		bool flag2 = false;
		bool flag3 = false;
		touchCounter = 0;
		if (inputEnabled)
		{
			if (Input.touchCount > 0)
			{
				Touch[] touches = Input.touches;
				foreach (Touch touch in touches)
				{
					if (touchCounter < 5)
					{
						ref tk2dUITouch reference = ref allTouches[touchCounter];
						reference = new tk2dUITouch(touch);
						touchCounter++;
						continue;
					}
					break;
				}
			}
			else if (Input.GetMouseButtonDown(0))
			{
				ref tk2dUITouch reference2 = ref allTouches[touchCounter];
				reference2 = new tk2dUITouch((TouchPhase)0, 9999, Vector2.op_Implicit(Input.mousePosition), Vector2.zero, 0f);
				mouseDownFirstPos = Vector2.op_Implicit(Input.mousePosition);
				touchCounter++;
			}
			else if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
			{
				Vector2 val = mouseDownFirstPos - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				TouchPhase phase = (TouchPhase)1;
				if (Input.GetMouseButtonUp(0))
				{
					phase = (TouchPhase)3;
				}
				else if (val == Vector2.zero)
				{
					phase = (TouchPhase)2;
				}
				ref tk2dUITouch reference3 = ref allTouches[touchCounter];
				reference3 = new tk2dUITouch(phase, 9999, Vector2.op_Implicit(Input.mousePosition), val, tk2dUITime.deltaTime);
				touchCounter++;
			}
		}
		for (int j = 0; j < touchCounter; j++)
		{
			pressedUIItems[j] = RaycastForUIItem(allTouches[j].position);
		}
		for (int k = 0; k < prevPressedUIItemList.Count; k++)
		{
			prevPressedItem = prevPressedUIItemList[k];
			if (!((Object)(object)prevPressedItem != (Object)null))
			{
				continue;
			}
			num = prevPressedItem.Touch.fingerId;
			flag2 = false;
			for (int l = 0; l < touchCounter; l++)
			{
				currTouch = allTouches[l];
				if (currTouch.fingerId != num)
				{
					continue;
				}
				flag2 = true;
				currPressedItem = pressedUIItems[l];
				if ((int)currTouch.phase == 0)
				{
					prevPressedItem.CurrentOverUIItem(currPressedItem);
					if ((Object)(object)prevPressedItem != (Object)(object)currPressedItem)
					{
						prevPressedItem.Release();
						prevPressedUIItemList.RemoveAt(k);
						k--;
					}
				}
				else if ((int)currTouch.phase == 3)
				{
					prevPressedItem.CurrentOverUIItem(currPressedItem);
					prevPressedItem.UpdateTouch(currTouch);
					prevPressedItem.Release();
					prevPressedUIItemList.RemoveAt(k);
					k--;
				}
				else
				{
					prevPressedItem.CurrentOverUIItem(currPressedItem);
					prevPressedItem.UpdateTouch(currTouch);
				}
				break;
			}
			if (!flag2)
			{
				prevPressedItem.CurrentOverUIItem(null);
				prevPressedItem.Release();
				prevPressedUIItemList.RemoveAt(k);
				k--;
			}
		}
		for (int m = 0; m < touchCounter; m++)
		{
			currPressedItem = pressedUIItems[m];
			currTouch = allTouches[m];
			if ((int)currTouch.phase == 0)
			{
				if ((Object)(object)currPressedItem != (Object)null && currPressedItem.Press(currTouch))
				{
					prevPressedUIItemList.Add(currPressedItem);
				}
				flag = true;
			}
		}
		if (flag && this.OnAnyPress != null)
		{
			this.OnAnyPress();
		}
	}

	private tk2dUIItem RaycastForUIItem(Vector2 screenPos)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		int count = sortedCameras.Count;
		for (int i = 0; i < count; i++)
		{
			tk2dUICamera tk2dUICamera2 = sortedCameras[i];
			if (tk2dUICamera2.RaycastType == tk2dUICamera.tk2dRaycastType.Physics3D)
			{
				ray = tk2dUICamera2.HostCamera.ScreenPointToRay(Vector2.op_Implicit(screenPos));
				if (Physics.Raycast(ray, ref hit, tk2dUICamera2.HostCamera.farClipPlane - tk2dUICamera2.HostCamera.nearClipPlane, LayerMask.op_Implicit(tk2dUICamera2.FilteredMask)))
				{
					return ((Component)((RaycastHit)(ref hit)).collider).GetComponent<tk2dUIItem>();
				}
			}
			else if (tk2dUICamera2.RaycastType == tk2dUICamera.tk2dRaycastType.Physics2D)
			{
				Collider2D val = Physics2D.OverlapPoint(Vector2.op_Implicit(tk2dUICamera2.HostCamera.ScreenToWorldPoint(Vector2.op_Implicit(screenPos))), LayerMask.op_Implicit(tk2dUICamera2.FilteredMask));
				if ((Object)(object)val != (Object)null)
				{
					return ((Component)val).GetComponent<tk2dUIItem>();
				}
			}
		}
		return null;
	}

	public void OverrideClearAllChildrenPresses(tk2dUIItem item)
	{
		if (useMultiTouch)
		{
			for (int i = 0; i < pressedUIItems.Length; i++)
			{
				tk2dUIItem tk2dUIItem2 = pressedUIItems[i];
				if ((Object)(object)tk2dUIItem2 != (Object)null && item.CheckIsUIItemChildOfMe(tk2dUIItem2))
				{
					tk2dUIItem2.CurrentOverUIItem(item);
				}
			}
		}
		else if ((Object)(object)pressedUIItem != (Object)null && item.CheckIsUIItemChildOfMe(pressedUIItem))
		{
			pressedUIItem.CurrentOverUIItem(item);
		}
	}
}
