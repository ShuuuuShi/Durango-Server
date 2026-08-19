using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Key Navigation")]
public class UIKeyNavigation : MonoBehaviour
{
	public enum Constraint
	{
		None,
		Vertical,
		Horizontal,
		Explicit
	}

	public static BetterList<UIKeyNavigation> list = new BetterList<UIKeyNavigation>();

	public Constraint constraint;

	public GameObject onUp;

	public GameObject onDown;

	public GameObject onLeft;

	public GameObject onRight;

	public GameObject onClick;

	public GameObject onTab;

	public bool startsSelected;

	[NonSerialized]
	private bool mStarted;

	public static int mLastFrame = 0;

	public static UIKeyNavigation current
	{
		get
		{
			GameObject hoveredObject = UICamera.hoveredObject;
			if ((Object)(object)hoveredObject == (Object)null)
			{
				return null;
			}
			return hoveredObject.GetComponent<UIKeyNavigation>();
		}
	}

	public bool isColliderEnabled
	{
		get
		{
			if (((Behaviour)this).enabled && ((Component)this).gameObject.activeInHierarchy)
			{
				Collider component = ((Component)this).GetComponent<Collider>();
				if ((Object)(object)component != (Object)null)
				{
					return component.enabled;
				}
				Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
				return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
			}
			return false;
		}
	}

	protected virtual void OnEnable()
	{
		list.Add(this);
		if (mStarted)
		{
			Start();
		}
	}

	private void Start()
	{
		mStarted = true;
		if (startsSelected && isColliderEnabled)
		{
			UICamera.hoveredObject = ((Component)this).gameObject;
		}
	}

	protected virtual void OnDisable()
	{
		list.Remove(this);
	}

	private static bool IsActive(GameObject go)
	{
		if (Object.op_Implicit((Object)(object)go) && go.activeInHierarchy)
		{
			Collider component = go.GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				return component.enabled;
			}
			Collider2D component2 = go.GetComponent<Collider2D>();
			return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
		}
		return false;
	}

	public GameObject GetLeft()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsActive(onLeft))
		{
			return onLeft;
		}
		if (constraint == Constraint.Vertical || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.left, 1f, 2f);
	}

	public GameObject GetRight()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsActive(onRight))
		{
			return onRight;
		}
		if (constraint == Constraint.Vertical || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.right, 1f, 2f);
	}

	public GameObject GetUp()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsActive(onUp))
		{
			return onUp;
		}
		if (constraint == Constraint.Horizontal || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.up, 2f);
	}

	public GameObject GetDown()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsActive(onDown))
		{
			return onDown;
		}
		if (constraint == Constraint.Horizontal || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.down, 2f);
	}

	public GameObject Get(Vector3 myDir, float x = 1f, float y = 1f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		myDir = transform.TransformDirection(myDir);
		Vector3 center = GetCenter(((Component)this).gameObject);
		float num = float.MaxValue;
		GameObject result = null;
		for (int i = 0; i < list.size; i++)
		{
			UIKeyNavigation uIKeyNavigation = list[i];
			if ((Object)(object)uIKeyNavigation == (Object)(object)this || uIKeyNavigation.constraint == Constraint.Explicit || !uIKeyNavigation.isColliderEnabled)
			{
				continue;
			}
			UIWidget component = ((Component)uIKeyNavigation).GetComponent<UIWidget>();
			if ((Object)(object)component != (Object)null && component.alpha == 0f)
			{
				continue;
			}
			Vector3 val = GetCenter(((Component)uIKeyNavigation).gameObject) - center;
			float num2 = Vector3.Dot(myDir, ((Vector3)(ref val)).normalized);
			if (!(num2 < 0.707f))
			{
				val = transform.InverseTransformDirection(val);
				val.x *= x;
				val.y *= y;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				if (!(sqrMagnitude > num))
				{
					result = ((Component)uIKeyNavigation).gameObject;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	protected static Vector3 GetCenter(GameObject go)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = go.GetComponent<UIWidget>();
		UICamera uICamera = UICamera.FindCameraForLayer(go.layer);
		if ((Object)(object)uICamera != (Object)null)
		{
			Vector3 val = go.transform.position;
			if ((Object)(object)component != (Object)null)
			{
				Vector3[] worldCorners = component.worldCorners;
				val = (worldCorners[0] + worldCorners[2]) * 0.5f;
			}
			val = uICamera.cachedCamera.WorldToScreenPoint(val);
			val.z = 0f;
			return val;
		}
		if ((Object)(object)component != (Object)null)
		{
			Vector3[] worldCorners2 = component.worldCorners;
			return (worldCorners2[0] + worldCorners2[2]) * 0.5f;
		}
		return go.transform.position;
	}

	public virtual void OnNavigate(KeyCode key)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected I4, but got Unknown
		if (!UIPopupList.isOpen && mLastFrame != Time.frameCount)
		{
			mLastFrame = Time.frameCount;
			GameObject val = null;
			switch (key - 273)
			{
			case 3:
				val = GetLeft();
				break;
			case 2:
				val = GetRight();
				break;
			case 0:
				val = GetUp();
				break;
			case 1:
				val = GetDown();
				break;
			}
			if ((Object)(object)val != (Object)null)
			{
				UICamera.hoveredObject = val;
			}
		}
	}

	public virtual void OnKey(KeyCode key)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		if (UIPopupList.isOpen || mLastFrame == Time.frameCount)
		{
			return;
		}
		mLastFrame = Time.frameCount;
		if ((int)key != 9)
		{
			return;
		}
		GameObject val = onTab;
		if ((Object)(object)val == (Object)null)
		{
			if (UICamera.GetKey((KeyCode)304) || UICamera.GetKey((KeyCode)303))
			{
				val = GetLeft();
				if ((Object)(object)val == (Object)null)
				{
					val = GetUp();
				}
				if ((Object)(object)val == (Object)null)
				{
					val = GetDown();
				}
				if ((Object)(object)val == (Object)null)
				{
					val = GetRight();
				}
			}
			else
			{
				val = GetRight();
				if ((Object)(object)val == (Object)null)
				{
					val = GetDown();
				}
				if ((Object)(object)val == (Object)null)
				{
					val = GetUp();
				}
				if ((Object)(object)val == (Object)null)
				{
					val = GetLeft();
				}
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			UICamera.currentScheme = UICamera.ControlScheme.Controller;
			UICamera.hoveredObject = val;
			UIInput component = val.GetComponent<UIInput>();
			if ((Object)(object)component != (Object)null)
			{
				component.isSelected = true;
			}
		}
	}

	protected virtual void OnClick()
	{
		if (NGUITools.GetActive(onClick))
		{
			UICamera.hoveredObject = onClick;
		}
	}
}
