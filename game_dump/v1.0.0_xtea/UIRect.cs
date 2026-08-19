using System;
using UnityEngine;

public abstract class UIRect : MonoBehaviour
{
	[Serializable]
	public class AnchorPoint
	{
		public Transform target;

		public float relative;

		public int absolute;

		[NonSerialized]
		public UIRect rect;

		[NonSerialized]
		public Camera targetCam;

		public AnchorPoint()
		{
		}

		public AnchorPoint(float relative)
		{
			this.relative = relative;
		}

		public void Set(float relative, float absolute)
		{
			this.relative = relative;
			this.absolute = Mathf.FloorToInt(absolute + 0.5f);
		}

		public void Set(Transform target, float relative, float absolute)
		{
			this.target = target;
			this.relative = relative;
			this.absolute = Mathf.FloorToInt(absolute + 0.5f);
		}

		public void SetToNearest(float abs0, float abs1, float abs2)
		{
			SetToNearest(0f, 0.5f, 1f, abs0, abs1, abs2);
		}

		public void SetToNearest(float rel0, float rel1, float rel2, float abs0, float abs1, float abs2)
		{
			float num = Mathf.Abs(abs0);
			float num2 = Mathf.Abs(abs1);
			float num3 = Mathf.Abs(abs2);
			if (num < num2 && num < num3)
			{
				Set(rel0, abs0);
			}
			else if (num2 < num && num2 < num3)
			{
				Set(rel1, abs1);
			}
			else
			{
				Set(rel2, abs2);
			}
		}

		public void SetHorizontal(Transform parent, float localPos)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)rect))
			{
				Vector3[] sides = rect.GetSides(parent);
				float num = Mathf.Lerp(sides[0].x, sides[2].x, relative);
				absolute = Mathf.FloorToInt(localPos - num + 0.5f);
				return;
			}
			Vector3 val = target.position;
			if ((Object)(object)parent != (Object)null)
			{
				val = parent.InverseTransformPoint(val);
			}
			absolute = Mathf.FloorToInt(localPos - val.x + 0.5f);
		}

		public void SetVertical(Transform parent, float localPos)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)rect))
			{
				Vector3[] sides = rect.GetSides(parent);
				float num = Mathf.Lerp(sides[3].y, sides[1].y, relative);
				absolute = Mathf.FloorToInt(localPos - num + 0.5f);
				return;
			}
			Vector3 val = target.position;
			if ((Object)(object)parent != (Object)null)
			{
				val = parent.InverseTransformPoint(val);
			}
			absolute = Mathf.FloorToInt(localPos - val.y + 0.5f);
		}

		public Vector3[] GetSides(Transform relativeTo)
		{
			if ((Object)(object)target != (Object)null)
			{
				if ((Object)(object)rect != (Object)null)
				{
					return rect.GetSides(relativeTo);
				}
				if ((Object)(object)((Component)target).GetComponent<Camera>() != (Object)null)
				{
					return ((Component)target).GetComponent<Camera>().GetSides(relativeTo);
				}
			}
			return null;
		}
	}

	public enum AnchorUpdate
	{
		OnEnable,
		OnUpdate,
		OnStart
	}

	public AnchorPoint leftAnchor = new AnchorPoint();

	public AnchorPoint rightAnchor = new AnchorPoint(1f);

	public AnchorPoint bottomAnchor = new AnchorPoint();

	public AnchorPoint topAnchor = new AnchorPoint(1f);

	public AnchorUpdate updateAnchors;

	[NonSerialized]
	protected GameObject mGo;

	[NonSerialized]
	protected Transform mTrans;

	[NonSerialized]
	protected BetterList<UIRect> mChildren = new BetterList<UIRect>();

	[NonSerialized]
	protected bool mChanged = true;

	[NonSerialized]
	protected bool mParentFound;

	[NonSerialized]
	private bool mUpdateAnchors = true;

	[NonSerialized]
	private int mUpdateFrame = -1;

	[NonSerialized]
	private bool mAnchorsCached;

	[NonSerialized]
	private UIRoot mRoot;

	[NonSerialized]
	private UIRect mParent;

	[NonSerialized]
	private bool mRootSet;

	[NonSerialized]
	protected Camera mCam;

	protected bool mStarted;

	[NonSerialized]
	public float finalAlpha = 1f;

	protected static Vector3[] mSides = (Vector3[])(object)new Vector3[4];

	public GameObject cachedGameObject
	{
		get
		{
			if ((Object)(object)mGo == (Object)null)
			{
				mGo = ((Component)this).gameObject;
			}
			return mGo;
		}
	}

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

	public Camera anchorCamera
	{
		get
		{
			if (!mAnchorsCached)
			{
				ResetAnchors();
			}
			return mCam;
		}
	}

	public bool isFullyAnchored => Object.op_Implicit((Object)(object)leftAnchor.target) && Object.op_Implicit((Object)(object)rightAnchor.target) && Object.op_Implicit((Object)(object)topAnchor.target) && Object.op_Implicit((Object)(object)bottomAnchor.target);

	public virtual bool isAnchoredHorizontally => Object.op_Implicit((Object)(object)leftAnchor.target) || Object.op_Implicit((Object)(object)rightAnchor.target);

	public virtual bool isAnchoredVertically => Object.op_Implicit((Object)(object)bottomAnchor.target) || Object.op_Implicit((Object)(object)topAnchor.target);

	public virtual bool canBeAnchored => true;

	public UIRect parent
	{
		get
		{
			if (!mParentFound)
			{
				mParentFound = true;
				mParent = NGUITools.FindInParents<UIRect>(cachedTransform.parent);
			}
			return mParent;
		}
	}

	public UIRoot root
	{
		get
		{
			if ((Object)(object)parent != (Object)null)
			{
				return mParent.root;
			}
			if (!mRootSet)
			{
				mRootSet = true;
				mRoot = NGUITools.FindInParents<UIRoot>(cachedTransform);
			}
			return mRoot;
		}
	}

	public bool isAnchored => (Object.op_Implicit((Object)(object)leftAnchor.target) || Object.op_Implicit((Object)(object)rightAnchor.target) || Object.op_Implicit((Object)(object)topAnchor.target) || Object.op_Implicit((Object)(object)bottomAnchor.target)) && canBeAnchored;

	public abstract float alpha { get; set; }

	public abstract Vector3[] localCorners { get; }

	public abstract Vector3[] worldCorners { get; }

	protected float cameraRayDistance
	{
		get
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)anchorCamera == (Object)null)
			{
				return 0f;
			}
			if (!mCam.orthographic)
			{
				Transform val = cachedTransform;
				Transform transform = ((Component)mCam).transform;
				Plane val2 = default(Plane);
				((Plane)(ref val2))._002Ector(val.rotation * Vector3.back, val.position);
				Ray val3 = default(Ray);
				((Ray)(ref val3))._002Ector(transform.position, transform.rotation * Vector3.forward);
				float result = default(float);
				if (((Plane)(ref val2)).Raycast(val3, ref result))
				{
					return result;
				}
			}
			return Mathf.Lerp(mCam.nearClipPlane, mCam.farClipPlane, 0.5f);
		}
	}

	public abstract float CalculateFinalAlpha(int frameID);

	public virtual void Invalidate(bool includeChildren)
	{
		mChanged = true;
		if (includeChildren)
		{
			for (int i = 0; i < mChildren.size; i++)
			{
				mChildren.buffer[i].Invalidate(includeChildren: true);
			}
		}
	}

	public virtual Vector3[] GetSides(Transform relativeTo)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)anchorCamera != (Object)null)
		{
			return mCam.GetSides(cameraRayDistance, relativeTo);
		}
		Vector3 position = cachedTransform.position;
		for (int i = 0; i < 4; i++)
		{
			mSides[i] = position;
		}
		if ((Object)(object)relativeTo != (Object)null)
		{
			for (int j = 0; j < 4; j++)
			{
				ref Vector3 reference = ref mSides[j];
				reference = relativeTo.InverseTransformPoint(mSides[j]);
			}
		}
		return mSides;
	}

	protected Vector3 GetLocalPos(AnchorPoint ac, Transform trans)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)anchorCamera == (Object)null || (Object)(object)ac.targetCam == (Object)null)
		{
			return cachedTransform.localPosition;
		}
		Rect rect = ac.targetCam.rect;
		Vector3 val = ac.targetCam.WorldToViewportPoint(ac.target.position);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(val.x * ((Rect)(ref rect)).width + ((Rect)(ref rect)).x, val.y * ((Rect)(ref rect)).height + ((Rect)(ref rect)).y, val.z);
		val2 = mCam.ViewportToWorldPoint(val2);
		if ((Object)(object)trans != (Object)null)
		{
			val2 = trans.InverseTransformPoint(val2);
		}
		val2.x = Mathf.Floor(val2.x + 0.5f);
		val2.y = Mathf.Floor(val2.y + 0.5f);
		return val2;
	}

	protected virtual void OnEnable()
	{
		mUpdateFrame = -1;
		if (updateAnchors == AnchorUpdate.OnEnable)
		{
			mAnchorsCached = false;
			mUpdateAnchors = true;
		}
		if (mStarted)
		{
			OnInit();
		}
		mUpdateFrame = -1;
	}

	protected virtual void OnInit()
	{
		mChanged = true;
		mRootSet = false;
		mParentFound = false;
		if ((Object)(object)parent != (Object)null)
		{
			mParent.mChildren.Add(this);
		}
	}

	protected virtual void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)mParent))
		{
			mParent.mChildren.Remove(this);
		}
		mParent = null;
		mRoot = null;
		mRootSet = false;
		mParentFound = false;
	}

	protected virtual void Awake()
	{
		mStarted = false;
		mGo = ((Component)this).gameObject;
		mTrans = ((Component)this).transform;
	}

	protected void Start()
	{
		mStarted = true;
		OnInit();
		OnStart();
	}

	public void Update()
	{
		if (!mAnchorsCached)
		{
			ResetAnchors();
		}
		int frameCount = Time.frameCount;
		if (mUpdateFrame != frameCount)
		{
			if (updateAnchors == AnchorUpdate.OnUpdate || mUpdateAnchors)
			{
				UpdateAnchorsInternal(frameCount);
			}
			OnUpdate();
		}
	}

	protected void UpdateAnchorsInternal(int frame)
	{
		mUpdateFrame = frame;
		mUpdateAnchors = false;
		bool flag = false;
		if (Object.op_Implicit((Object)(object)leftAnchor.target))
		{
			flag = true;
			if ((Object)(object)leftAnchor.rect != (Object)null && leftAnchor.rect.mUpdateFrame != frame)
			{
				leftAnchor.rect.Update();
			}
		}
		if (Object.op_Implicit((Object)(object)bottomAnchor.target))
		{
			flag = true;
			if ((Object)(object)bottomAnchor.rect != (Object)null && bottomAnchor.rect.mUpdateFrame != frame)
			{
				bottomAnchor.rect.Update();
			}
		}
		if (Object.op_Implicit((Object)(object)rightAnchor.target))
		{
			flag = true;
			if ((Object)(object)rightAnchor.rect != (Object)null && rightAnchor.rect.mUpdateFrame != frame)
			{
				rightAnchor.rect.Update();
			}
		}
		if (Object.op_Implicit((Object)(object)topAnchor.target))
		{
			flag = true;
			if ((Object)(object)topAnchor.rect != (Object)null && topAnchor.rect.mUpdateFrame != frame)
			{
				topAnchor.rect.Update();
			}
		}
		if (flag)
		{
			OnAnchor();
		}
	}

	public void UpdateAnchors()
	{
		if (isAnchored)
		{
			mUpdateFrame = -1;
			mUpdateAnchors = true;
			UpdateAnchorsInternal(Time.frameCount);
		}
	}

	protected abstract void OnAnchor();

	public void SetAnchor(Transform t)
	{
		leftAnchor.target = t;
		rightAnchor.target = t;
		topAnchor.target = t;
		bottomAnchor.target = t;
		ResetAnchors();
		UpdateAnchors();
	}

	public void SetAnchor(GameObject go)
	{
		Transform target = ((!((Object)(object)go != (Object)null)) ? null : go.transform);
		leftAnchor.target = target;
		rightAnchor.target = target;
		topAnchor.target = target;
		bottomAnchor.target = target;
		ResetAnchors();
		UpdateAnchors();
	}

	public void SetAnchor(GameObject go, int left, int bottom, int right, int top)
	{
		Transform target = ((!((Object)(object)go != (Object)null)) ? null : go.transform);
		leftAnchor.target = target;
		rightAnchor.target = target;
		topAnchor.target = target;
		bottomAnchor.target = target;
		leftAnchor.relative = 0f;
		rightAnchor.relative = 1f;
		bottomAnchor.relative = 0f;
		topAnchor.relative = 1f;
		leftAnchor.absolute = left;
		rightAnchor.absolute = right;
		bottomAnchor.absolute = bottom;
		topAnchor.absolute = top;
		ResetAnchors();
		UpdateAnchors();
	}

	public void SetAnchor(GameObject go, float left, float bottom, float right, float top)
	{
		Transform target = ((!((Object)(object)go != (Object)null)) ? null : go.transform);
		leftAnchor.target = target;
		rightAnchor.target = target;
		topAnchor.target = target;
		bottomAnchor.target = target;
		leftAnchor.relative = left;
		rightAnchor.relative = right;
		bottomAnchor.relative = bottom;
		topAnchor.relative = top;
		leftAnchor.absolute = 0;
		rightAnchor.absolute = 0;
		bottomAnchor.absolute = 0;
		topAnchor.absolute = 0;
		ResetAnchors();
		UpdateAnchors();
	}

	public void SetAnchor(GameObject go, float left, int leftOffset, float bottom, int bottomOffset, float right, int rightOffset, float top, int topOffset)
	{
		Transform target = ((!((Object)(object)go != (Object)null)) ? null : go.transform);
		leftAnchor.target = target;
		rightAnchor.target = target;
		topAnchor.target = target;
		bottomAnchor.target = target;
		leftAnchor.relative = left;
		rightAnchor.relative = right;
		bottomAnchor.relative = bottom;
		topAnchor.relative = top;
		leftAnchor.absolute = leftOffset;
		rightAnchor.absolute = rightOffset;
		bottomAnchor.absolute = bottomOffset;
		topAnchor.absolute = topOffset;
		ResetAnchors();
		UpdateAnchors();
	}

	public void SetAnchor(float left, int leftOffset, float bottom, int bottomOffset, float right, int rightOffset, float top, int topOffset)
	{
		Transform target = cachedTransform.parent;
		leftAnchor.target = target;
		rightAnchor.target = target;
		topAnchor.target = target;
		bottomAnchor.target = target;
		leftAnchor.relative = left;
		rightAnchor.relative = right;
		bottomAnchor.relative = bottom;
		topAnchor.relative = top;
		leftAnchor.absolute = leftOffset;
		rightAnchor.absolute = rightOffset;
		bottomAnchor.absolute = bottomOffset;
		topAnchor.absolute = topOffset;
		ResetAnchors();
		UpdateAnchors();
	}

	public void SetScreenRect(int left, int top, int width, int height)
	{
		SetAnchor(0f, left, 1f, -top - height, 0f, left + width, 1f, -top);
	}

	public void ResetAnchors()
	{
		mAnchorsCached = true;
		leftAnchor.rect = ((!Object.op_Implicit((Object)(object)leftAnchor.target)) ? null : ((Component)leftAnchor.target).GetComponent<UIRect>());
		bottomAnchor.rect = ((!Object.op_Implicit((Object)(object)bottomAnchor.target)) ? null : ((Component)bottomAnchor.target).GetComponent<UIRect>());
		rightAnchor.rect = ((!Object.op_Implicit((Object)(object)rightAnchor.target)) ? null : ((Component)rightAnchor.target).GetComponent<UIRect>());
		topAnchor.rect = ((!Object.op_Implicit((Object)(object)topAnchor.target)) ? null : ((Component)topAnchor.target).GetComponent<UIRect>());
		mCam = NGUITools.FindCameraForLayer(cachedGameObject.layer);
		FindCameraFor(leftAnchor);
		FindCameraFor(bottomAnchor);
		FindCameraFor(rightAnchor);
		FindCameraFor(topAnchor);
		mUpdateAnchors = true;
	}

	public void ResetAndUpdateAnchors()
	{
		ResetAnchors();
		UpdateAnchors();
	}

	public abstract void SetRect(float x, float y, float width, float height);

	private void FindCameraFor(AnchorPoint ap)
	{
		if ((Object)(object)ap.target == (Object)null || (Object)(object)ap.rect != (Object)null)
		{
			ap.targetCam = null;
		}
		else
		{
			ap.targetCam = NGUITools.FindCameraForLayer(((Component)ap.target).gameObject.layer);
		}
	}

	public virtual void ParentHasChanged()
	{
		mParentFound = false;
		UIRect uIRect = NGUITools.FindInParents<UIRect>(cachedTransform.parent);
		if ((Object)(object)mParent != (Object)(object)uIRect)
		{
			if (Object.op_Implicit((Object)(object)mParent))
			{
				mParent.mChildren.Remove(this);
			}
			mParent = uIRect;
			if (Object.op_Implicit((Object)(object)mParent))
			{
				mParent.mChildren.Add(this);
			}
			mRootSet = false;
		}
	}

	protected abstract void OnStart();

	protected virtual void OnUpdate()
	{
	}
}
