using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Root")]
[ExecuteInEditMode]
public class UIRoot : MonoBehaviour
{
	public enum Scaling
	{
		Flexible,
		Constrained,
		ConstrainedOnMobiles
	}

	public enum Constraint
	{
		Fit,
		Fill,
		FitWidth,
		FitHeight
	}

	public static List<UIRoot> list = new List<UIRoot>();

	public Scaling scalingStyle;

	public int manualWidth = 1280;

	public int manualHeight = 720;

	public int minimumHeight = 320;

	public int maximumHeight = 1536;

	public bool fitWidth;

	public bool fitHeight = true;

	public bool adjustByDPI;

	public bool shrinkPortraitUI;

	private Transform mTrans;

	public Constraint constraint
	{
		get
		{
			if (fitWidth)
			{
				if (fitHeight)
				{
					return Constraint.Fit;
				}
				return Constraint.FitWidth;
			}
			if (fitHeight)
			{
				return Constraint.FitHeight;
			}
			return Constraint.Fill;
		}
	}

	public Scaling activeScaling
	{
		get
		{
			Scaling scaling = scalingStyle;
			if (scaling == Scaling.ConstrainedOnMobiles)
			{
				return Scaling.Constrained;
			}
			return scaling;
		}
	}

	public int activeHeight
	{
		get
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			if (activeScaling == Scaling.Flexible)
			{
				Vector2 screenSize = NGUITools.screenSize;
				float num = screenSize.x / screenSize.y;
				if (screenSize.y < (float)minimumHeight)
				{
					screenSize.y = minimumHeight;
					screenSize.x = screenSize.y * num;
				}
				else if (screenSize.y > (float)maximumHeight)
				{
					screenSize.y = maximumHeight;
					screenSize.x = screenSize.y * num;
				}
				int num2 = Mathf.RoundToInt((!shrinkPortraitUI || !(screenSize.y > screenSize.x)) ? screenSize.y : (screenSize.y / num));
				return (!adjustByDPI) ? num2 : NGUIMath.AdjustByDPI(num2);
			}
			Constraint constraint = this.constraint;
			if (constraint == Constraint.FitHeight)
			{
				return manualHeight;
			}
			Vector2 screenSize2 = NGUITools.screenSize;
			float num3 = screenSize2.x / screenSize2.y;
			float num4 = (float)manualWidth / (float)manualHeight;
			return constraint switch
			{
				Constraint.FitWidth => Mathf.RoundToInt((float)manualWidth / num3), 
				Constraint.Fit => (!(num4 > num3)) ? manualHeight : Mathf.RoundToInt((float)manualWidth / num3), 
				Constraint.Fill => (!(num4 < num3)) ? manualHeight : Mathf.RoundToInt((float)manualWidth / num3), 
				_ => manualHeight, 
			};
		}
	}

	public float pixelSizeAdjustment
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			int num = Mathf.RoundToInt(NGUITools.screenSize.y);
			return (num != -1) ? GetPixelSizeAdjustment(num) : 1f;
		}
	}

	public static float GetPixelSizeAdjustment(GameObject go)
	{
		UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(go);
		return (!((Object)(object)uIRoot != (Object)null)) ? 1f : uIRoot.pixelSizeAdjustment;
	}

	public float GetPixelSizeAdjustment(int height)
	{
		height = Mathf.Max(2, height);
		if (activeScaling == Scaling.Constrained)
		{
			return (float)activeHeight / (float)height;
		}
		if (height < minimumHeight)
		{
			return (float)minimumHeight / (float)height;
		}
		if (height > maximumHeight)
		{
			return (float)maximumHeight / (float)height;
		}
		return 1f;
	}

	protected virtual void Awake()
	{
		mTrans = ((Component)this).transform;
	}

	protected virtual void OnEnable()
	{
		list.Add(this);
	}

	protected virtual void OnDisable()
	{
		list.Remove(this);
	}

	protected virtual void Start()
	{
		UIOrthoCamera componentInChildren = ((Component)this).GetComponentInChildren<UIOrthoCamera>();
		if ((Object)(object)componentInChildren != (Object)null)
		{
			Camera component = ((Component)componentInChildren).gameObject.GetComponent<Camera>();
			((Behaviour)componentInChildren).enabled = false;
			if ((Object)(object)component != (Object)null)
			{
				component.orthographicSize = 1f;
			}
		}
		else
		{
			UpdateScale(updateAnchors: false);
		}
	}

	private void Update()
	{
		UpdateScale();
	}

	public void UpdateScale(bool updateAnchors = true)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mTrans != (Object)null))
		{
			return;
		}
		float num = activeHeight;
		if (!(num > 0f))
		{
			return;
		}
		float num2 = 2f / num;
		Vector3 localScale = mTrans.localScale;
		if (!(Mathf.Abs(localScale.x - num2) <= float.Epsilon) || !(Mathf.Abs(localScale.y - num2) <= float.Epsilon) || !(Mathf.Abs(localScale.z - num2) <= float.Epsilon))
		{
			mTrans.localScale = new Vector3(num2, num2, num2);
			if (updateAnchors)
			{
				((Component)this).BroadcastMessage("UpdateAnchors");
			}
		}
	}

	public static void Broadcast(string funcName)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIRoot uIRoot = list[i];
			if ((Object)(object)uIRoot != (Object)null)
			{
				((Component)uIRoot).BroadcastMessage(funcName, (SendMessageOptions)1);
			}
		}
	}

	public static void Broadcast(string funcName, object param)
	{
		if (param == null)
		{
			Debug.LogError((object)"SendMessage is bugged when you try to pass 'null' in the parameter field. It behaves as if no parameter was specified.");
			return;
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIRoot uIRoot = list[i];
			if ((Object)(object)uIRoot != (Object)null)
			{
				((Component)uIRoot).BroadcastMessage(funcName, param, (SendMessageOptions)1);
			}
		}
	}
}
