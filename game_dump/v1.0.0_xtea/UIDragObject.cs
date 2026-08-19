using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Drag Object")]
public class UIDragObject : MonoBehaviour
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring
	}

	public Transform target;

	public UIPanel panelRegion;

	public Vector3 scrollMomentum = Vector3.zero;

	public bool restrictWithinPanel;

	public UIRect contentRect;

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	public float momentumAmount = 35f;

	[SerializeField]
	protected Vector3 scale = new Vector3(1f, 1f, 0f);

	[HideInInspector]
	[SerializeField]
	private float scrollWheelFactor;

	private Plane mPlane;

	private Vector3 mTargetPos;

	private Vector3 mLastPos;

	private Vector3 mMomentum = Vector3.zero;

	private Vector3 mScroll = Vector3.zero;

	private Bounds mBounds;

	private int mTouchID;

	private bool mStarted;

	private bool mPressed;

	public Vector3 dragMovement
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return scale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			scale = value;
		}
	}

	private void OnEnable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (scrollWheelFactor != 0f)
		{
			scrollMomentum = scale * scrollWheelFactor;
			scrollWheelFactor = 0f;
		}
		if ((Object)(object)contentRect == (Object)null && (Object)(object)target != (Object)null && Application.isPlaying)
		{
			UIWidget component = ((Component)target).GetComponent<UIWidget>();
			if ((Object)(object)component != (Object)null)
			{
				contentRect = component;
			}
		}
		mTargetPos = ((!((Object)(object)target != (Object)null)) ? Vector3.zero : target.position);
	}

	private void OnDisable()
	{
		mStarted = false;
	}

	private void FindPanel()
	{
		panelRegion = ((!((Object)(object)target != (Object)null)) ? null : UIPanel.Find(((Component)target).transform.parent));
		if ((Object)(object)panelRegion == (Object)null)
		{
			restrictWithinPanel = false;
		}
	}

	private void UpdateBounds()
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)contentRect))
		{
			Transform cachedTransform = panelRegion.cachedTransform;
			Matrix4x4 worldToLocalMatrix = cachedTransform.worldToLocalMatrix;
			Vector3[] worldCorners = contentRect.worldCorners;
			for (int i = 0; i < 4; i++)
			{
				ref Vector3 reference = ref worldCorners[i];
				reference = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(worldCorners[i]);
			}
			mBounds = new Bounds(worldCorners[0], Vector3.zero);
			for (int j = 1; j < 4; j++)
			{
				((Bounds)(ref mBounds)).Encapsulate(worldCorners[j]);
			}
		}
		else
		{
			mBounds = NGUIMath.CalculateRelativeWidgetBounds(panelRegion.cachedTransform, target);
		}
	}

	private void OnPress(bool pressed)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		if (UICamera.currentTouchID == -2 || UICamera.currentTouchID == -3)
		{
			return;
		}
		float timeScale = Time.timeScale;
		if ((timeScale < 0.01f && timeScale != 0f) || !((Behaviour)this).enabled || !NGUITools.GetActive(((Component)this).gameObject) || !((Object)(object)target != (Object)null))
		{
			return;
		}
		if (pressed)
		{
			if (!mPressed)
			{
				mTouchID = UICamera.currentTouchID;
				mPressed = true;
				mStarted = false;
				CancelMovement();
				if (restrictWithinPanel && (Object)(object)panelRegion == (Object)null)
				{
					FindPanel();
				}
				if (restrictWithinPanel)
				{
					UpdateBounds();
				}
				CancelSpring();
				Transform transform = ((Component)UICamera.currentCamera).transform;
				mPlane = new Plane(((!((Object)(object)panelRegion != (Object)null)) ? transform.rotation : panelRegion.cachedTransform.rotation) * Vector3.back, UICamera.lastWorldPosition);
			}
		}
		else if (mPressed && mTouchID == UICamera.currentTouchID)
		{
			mPressed = false;
			if (restrictWithinPanel && dragEffect == DragEffect.MomentumAndSpring && panelRegion.ConstrainTargetToBounds(target, ref mBounds, immediate: false))
			{
				CancelMovement();
			}
		}
	}

	private void OnDrag(Vector2 delta)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		if (!mPressed || mTouchID != UICamera.currentTouchID || !((Behaviour)this).enabled || !NGUITools.GetActive(((Component)this).gameObject) || !((Object)(object)target != (Object)null))
		{
			return;
		}
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
		Ray val = UICamera.currentCamera.ScreenPointToRay(Vector2.op_Implicit(UICamera.currentTouch.pos));
		float num = 0f;
		if (!((Plane)(ref mPlane)).Raycast(val, ref num))
		{
			return;
		}
		Vector3 point = ((Ray)(ref val)).GetPoint(num);
		Vector3 val2 = point - mLastPos;
		mLastPos = point;
		if (!mStarted)
		{
			mStarted = true;
			val2 = Vector3.zero;
		}
		if (val2.x != 0f || val2.y != 0f)
		{
			val2 = target.InverseTransformDirection(val2);
			((Vector3)(ref val2)).Scale(scale);
			val2 = target.TransformDirection(val2);
		}
		if (dragEffect != 0)
		{
			mMomentum = Vector3.Lerp(mMomentum, mMomentum + val2 * (0.01f * momentumAmount), 0.67f);
		}
		Vector3 localPosition = target.localPosition;
		Move(val2);
		if (restrictWithinPanel)
		{
			((Bounds)(ref mBounds)).center = ((Bounds)(ref mBounds)).center + (target.localPosition - localPosition);
			if (dragEffect != DragEffect.MomentumAndSpring && panelRegion.ConstrainTargetToBounds(target, ref mBounds, immediate: true))
			{
				CancelMovement();
			}
		}
	}

	private void Move(Vector3 worldDelta)
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)panelRegion != (Object)null)
		{
			mTargetPos += worldDelta;
			Transform parent = target.parent;
			Rigidbody component = ((Component)target).GetComponent<Rigidbody>();
			if ((Object)(object)parent != (Object)null)
			{
				Matrix4x4 worldToLocalMatrix = parent.worldToLocalMatrix;
				Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(mTargetPos);
				val.x = Mathf.Round(val.x);
				val.y = Mathf.Round(val.y);
				if ((Object)(object)component != (Object)null)
				{
					Matrix4x4 localToWorldMatrix = parent.localToWorldMatrix;
					val = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(val);
					component.position = val;
				}
				else
				{
					target.localPosition = val;
				}
			}
			else if ((Object)(object)component != (Object)null)
			{
				component.position = mTargetPos;
			}
			else
			{
				target.position = mTargetPos;
			}
			UIScrollView component2 = ((Component)panelRegion).GetComponent<UIScrollView>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.UpdateScrollbars(recalculateBounds: true);
			}
		}
		else
		{
			Transform obj = target;
			obj.position += worldDelta;
		}
	}

	private void LateUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		float deltaTime = RealTime.deltaTime;
		mMomentum -= mScroll;
		mScroll = NGUIMath.SpringLerp(mScroll, Vector3.zero, 20f, deltaTime);
		if (((Vector3)(ref mMomentum)).magnitude < 0.0001f)
		{
			return;
		}
		if (!mPressed)
		{
			if ((Object)(object)panelRegion == (Object)null)
			{
				FindPanel();
			}
			Move(NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime));
			if (restrictWithinPanel && (Object)(object)panelRegion != (Object)null)
			{
				UpdateBounds();
				if (panelRegion.ConstrainTargetToBounds(target, ref mBounds, dragEffect == DragEffect.None))
				{
					CancelMovement();
				}
				else
				{
					CancelSpring();
				}
			}
			NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime);
			if (((Vector3)(ref mMomentum)).magnitude < 0.0001f)
			{
				CancelMovement();
			}
		}
		else
		{
			NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime);
		}
	}

	public void CancelMovement()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target != (Object)null)
		{
			Vector3 localPosition = target.localPosition;
			localPosition.x = Mathf.RoundToInt(localPosition.x);
			localPosition.y = Mathf.RoundToInt(localPosition.y);
			localPosition.z = Mathf.RoundToInt(localPosition.z);
			target.localPosition = localPosition;
		}
		mTargetPos = ((!((Object)(object)target != (Object)null)) ? Vector3.zero : target.position);
		mMomentum = Vector3.zero;
		mScroll = Vector3.zero;
	}

	public void CancelSpring()
	{
		SpringPosition component = ((Component)target).GetComponent<SpringPosition>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = false;
		}
	}

	private void OnScroll(float delta)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject))
		{
			mScroll -= scrollMomentum * (delta * 0.05f);
		}
	}
}
