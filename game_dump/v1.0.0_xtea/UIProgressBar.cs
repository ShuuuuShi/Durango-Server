using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/NGUI Progress Bar")]
[ExecuteInEditMode]
public class UIProgressBar : UIWidgetContainer
{
	public enum FillDirection
	{
		LeftToRight,
		RightToLeft,
		BottomToTop,
		TopToBottom
	}

	public delegate void OnDragFinished();

	public static UIProgressBar current;

	public OnDragFinished onDragFinished;

	public Transform thumb;

	[HideInInspector]
	[SerializeField]
	protected UIWidget mBG;

	[SerializeField]
	[HideInInspector]
	protected UIWidget mFG;

	[HideInInspector]
	[SerializeField]
	protected float mValue = 1f;

	[HideInInspector]
	[SerializeField]
	protected FillDirection mFill;

	[NonSerialized]
	protected bool mStarted;

	[NonSerialized]
	protected Transform mTrans;

	[NonSerialized]
	protected bool mIsDirty;

	[NonSerialized]
	protected Camera mCam;

	[NonSerialized]
	protected float mOffset;

	public int numberOfSteps;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	public Transform cachedTransform
	{
		get
		{
			if ((Object)(object)mTrans == (Object)null)
			{
				mTrans = ((Component)this).transform;
			}
			return mTrans;
		}
	}

	public Camera cachedCamera
	{
		get
		{
			if ((Object)(object)mCam == (Object)null)
			{
				mCam = NGUITools.FindCameraForLayer(((Component)this).gameObject.layer);
			}
			return mCam;
		}
	}

	public UIWidget foregroundWidget
	{
		get
		{
			return mFG;
		}
		set
		{
			if ((Object)(object)mFG != (Object)(object)value)
			{
				mFG = value;
				mIsDirty = true;
			}
		}
	}

	public UIWidget backgroundWidget
	{
		get
		{
			return mBG;
		}
		set
		{
			if ((Object)(object)mBG != (Object)(object)value)
			{
				mBG = value;
				mIsDirty = true;
			}
		}
	}

	public FillDirection fillDirection
	{
		get
		{
			return mFill;
		}
		set
		{
			if (mFill != value)
			{
				mFill = value;
				if (mStarted)
				{
					ForceUpdate();
				}
			}
		}
	}

	public float value
	{
		get
		{
			if (numberOfSteps > 1)
			{
				return Mathf.Round(mValue * (float)(numberOfSteps - 1)) / (float)(numberOfSteps - 1);
			}
			return mValue;
		}
		set
		{
			Set(value);
		}
	}

	public float alpha
	{
		get
		{
			if ((Object)(object)mFG != (Object)null)
			{
				return mFG.alpha;
			}
			if ((Object)(object)mBG != (Object)null)
			{
				return mBG.alpha;
			}
			return 1f;
		}
		set
		{
			if ((Object)(object)mFG != (Object)null)
			{
				mFG.alpha = value;
				if ((Object)(object)((Component)mFG).GetComponent<Collider>() != (Object)null)
				{
					((Component)mFG).GetComponent<Collider>().enabled = mFG.alpha > 0.001f;
				}
				else if ((Object)(object)((Component)mFG).GetComponent<Collider2D>() != (Object)null)
				{
					((Behaviour)((Component)mFG).GetComponent<Collider2D>()).enabled = mFG.alpha > 0.001f;
				}
			}
			if ((Object)(object)mBG != (Object)null)
			{
				mBG.alpha = value;
				if ((Object)(object)((Component)mBG).GetComponent<Collider>() != (Object)null)
				{
					((Component)mBG).GetComponent<Collider>().enabled = mBG.alpha > 0.001f;
				}
				else if ((Object)(object)((Component)mBG).GetComponent<Collider2D>() != (Object)null)
				{
					((Behaviour)((Component)mBG).GetComponent<Collider2D>()).enabled = mBG.alpha > 0.001f;
				}
			}
			if (!((Object)(object)thumb != (Object)null))
			{
				return;
			}
			UIWidget component = ((Component)thumb).GetComponent<UIWidget>();
			if ((Object)(object)component != (Object)null)
			{
				component.alpha = value;
				if ((Object)(object)((Component)component).GetComponent<Collider>() != (Object)null)
				{
					((Component)component).GetComponent<Collider>().enabled = component.alpha > 0.001f;
				}
				else if ((Object)(object)((Component)component).GetComponent<Collider2D>() != (Object)null)
				{
					((Behaviour)((Component)component).GetComponent<Collider2D>()).enabled = component.alpha > 0.001f;
				}
			}
		}
	}

	protected bool isHorizontal => mFill == FillDirection.LeftToRight || mFill == FillDirection.RightToLeft;

	protected bool isInverted => mFill == FillDirection.RightToLeft || mFill == FillDirection.TopToBottom;

	public void Set(float val, bool notify = true)
	{
		val = Mathf.Clamp01(val);
		if (mValue == val)
		{
			return;
		}
		float num = value;
		mValue = val;
		if (mStarted && num != value)
		{
			ForceUpdate();
			if (notify && NGUITools.GetActive((Behaviour)(object)this) && EventDelegate.IsValid(onChange))
			{
				current = this;
				EventDelegate.Execute(onChange);
				current = null;
			}
		}
	}

	public void Start()
	{
		if (mStarted)
		{
			return;
		}
		mStarted = true;
		Upgrade();
		if (Application.isPlaying)
		{
			if ((Object)(object)mBG != (Object)null)
			{
				mBG.autoResizeBoxCollider = true;
			}
			OnStart();
			if ((Object)(object)current == (Object)null && onChange != null)
			{
				current = this;
				EventDelegate.Execute(onChange);
				current = null;
			}
		}
		ForceUpdate();
	}

	protected virtual void Upgrade()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected void Update()
	{
		if (mIsDirty)
		{
			ForceUpdate();
		}
	}

	protected void OnValidate()
	{
		if (NGUITools.GetActive((Behaviour)(object)this))
		{
			Upgrade();
			mIsDirty = true;
			float num = Mathf.Clamp01(mValue);
			if (mValue != num)
			{
				mValue = num;
			}
			if (numberOfSteps < 0)
			{
				numberOfSteps = 0;
			}
			else if (numberOfSteps > 20)
			{
				numberOfSteps = 20;
			}
			ForceUpdate();
		}
		else
		{
			float num2 = Mathf.Clamp01(mValue);
			if (mValue != num2)
			{
				mValue = num2;
			}
			if (numberOfSteps < 0)
			{
				numberOfSteps = 0;
			}
			else if (numberOfSteps > 20)
			{
				numberOfSteps = 20;
			}
		}
	}

	protected float ScreenToValue(Vector2 screenPos)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Transform val = cachedTransform;
		Plane val2 = default(Plane);
		((Plane)(ref val2))._002Ector(val.rotation * Vector3.back, val.position);
		Ray val3 = cachedCamera.ScreenPointToRay(Vector2.op_Implicit(screenPos));
		float num = default(float);
		if (!((Plane)(ref val2)).Raycast(val3, ref num))
		{
			return value;
		}
		return LocalToValue(Vector2.op_Implicit(val.InverseTransformPoint(((Ray)(ref val3)).GetPoint(num))));
	}

	protected virtual float LocalToValue(Vector2 localPos)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mFG != (Object)null)
		{
			Vector3[] localCorners = mFG.localCorners;
			Vector3 val = localCorners[2] - localCorners[0];
			if (isHorizontal)
			{
				float num = (localPos.x - localCorners[0].x) / val.x;
				return (!isInverted) ? num : (1f - num);
			}
			float num2 = (localPos.y - localCorners[0].y) / val.y;
			return (!isInverted) ? num2 : (1f - num2);
		}
		return value;
	}

	public virtual void ForceUpdate()
	{
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		mIsDirty = false;
		bool flag = false;
		if ((Object)(object)mFG != (Object)null)
		{
			UIBasicSprite uIBasicSprite = mFG as UIBasicSprite;
			if (isHorizontal)
			{
				if ((Object)(object)uIBasicSprite != (Object)null && uIBasicSprite.type == UIBasicSprite.Type.Filled)
				{
					if (uIBasicSprite.fillDirection == UIBasicSprite.FillDirection.Horizontal || uIBasicSprite.fillDirection == UIBasicSprite.FillDirection.Vertical)
					{
						uIBasicSprite.fillDirection = UIBasicSprite.FillDirection.Horizontal;
						uIBasicSprite.invert = isInverted;
					}
					uIBasicSprite.fillAmount = value;
				}
				else
				{
					mFG.drawRegion = ((!isInverted) ? new Vector4(0f, 0f, value, 1f) : new Vector4(1f - value, 0f, 1f, 1f));
					((Behaviour)mFG).enabled = true;
					flag = value < 0.001f;
				}
			}
			else if ((Object)(object)uIBasicSprite != (Object)null && uIBasicSprite.type == UIBasicSprite.Type.Filled)
			{
				if (uIBasicSprite.fillDirection == UIBasicSprite.FillDirection.Horizontal || uIBasicSprite.fillDirection == UIBasicSprite.FillDirection.Vertical)
				{
					uIBasicSprite.fillDirection = UIBasicSprite.FillDirection.Vertical;
					uIBasicSprite.invert = isInverted;
				}
				uIBasicSprite.fillAmount = value;
			}
			else
			{
				mFG.drawRegion = ((!isInverted) ? new Vector4(0f, 0f, 1f, value) : new Vector4(0f, 1f - value, 1f, 1f));
				((Behaviour)mFG).enabled = true;
				flag = value < 0.001f;
			}
		}
		if ((Object)(object)thumb != (Object)null && ((Object)(object)mFG != (Object)null || (Object)(object)mBG != (Object)null))
		{
			Vector3[] array = ((!((Object)(object)mFG != (Object)null)) ? mBG.localCorners : mFG.localCorners);
			Vector4 val = ((!((Object)(object)mFG != (Object)null)) ? mBG.border : mFG.border);
			ref Vector3 reference = ref array[0];
			reference.x += val.x;
			ref Vector3 reference2 = ref array[1];
			reference2.x += val.x;
			ref Vector3 reference3 = ref array[2];
			reference3.x -= val.z;
			ref Vector3 reference4 = ref array[3];
			reference4.x -= val.z;
			ref Vector3 reference5 = ref array[0];
			reference5.y += val.y;
			ref Vector3 reference6 = ref array[1];
			reference6.y -= val.w;
			ref Vector3 reference7 = ref array[2];
			reference7.y -= val.w;
			ref Vector3 reference8 = ref array[3];
			reference8.y += val.y;
			Transform val2 = ((!((Object)(object)mFG != (Object)null)) ? mBG.cachedTransform : mFG.cachedTransform);
			for (int i = 0; i < 4; i++)
			{
				ref Vector3 reference9 = ref array[i];
				reference9 = val2.TransformPoint(array[i]);
			}
			if (isHorizontal)
			{
				Vector3 val3 = Vector3.Lerp(array[0], array[1], 0.5f);
				Vector3 val4 = Vector3.Lerp(array[2], array[3], 0.5f);
				SetThumbPosition(Vector3.Lerp(val3, val4, (!isInverted) ? value : (1f - value)));
			}
			else
			{
				Vector3 val5 = Vector3.Lerp(array[0], array[3], 0.5f);
				Vector3 val6 = Vector3.Lerp(array[1], array[2], 0.5f);
				SetThumbPosition(Vector3.Lerp(val5, val6, (!isInverted) ? value : (1f - value)));
			}
		}
		if (flag)
		{
			((Behaviour)mFG).enabled = false;
		}
	}

	protected void SetThumbPosition(Vector3 worldPos)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = thumb.parent;
		if ((Object)(object)parent != (Object)null)
		{
			worldPos = parent.InverseTransformPoint(worldPos);
			worldPos.x = Mathf.Round(worldPos.x);
			worldPos.y = Mathf.Round(worldPos.y);
			worldPos.z = 0f;
			if (Vector3.Distance(thumb.localPosition, worldPos) > 0.001f)
			{
				thumb.localPosition = worldPos;
			}
		}
		else if (Vector3.Distance(thumb.position, worldPos) > 1E-05f)
		{
			thumb.position = worldPos;
		}
	}

	public virtual void OnPan(Vector2 delta)
	{
		if (((Behaviour)this).enabled)
		{
			switch (mFill)
			{
			case FillDirection.LeftToRight:
			{
				float num8 = (value = Mathf.Clamp01(mValue + delta.x));
				mValue = num8;
				break;
			}
			case FillDirection.RightToLeft:
			{
				float num6 = (value = Mathf.Clamp01(mValue - delta.x));
				mValue = num6;
				break;
			}
			case FillDirection.BottomToTop:
			{
				float num4 = (value = Mathf.Clamp01(mValue + delta.y));
				mValue = num4;
				break;
			}
			case FillDirection.TopToBottom:
			{
				float num2 = (value = Mathf.Clamp01(mValue - delta.y));
				mValue = num2;
				break;
			}
			}
		}
	}
}
