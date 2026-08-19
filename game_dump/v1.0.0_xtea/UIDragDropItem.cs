using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag and Drop Item")]
public class UIDragDropItem : MonoBehaviour
{
	public enum Restriction
	{
		None,
		Horizontal,
		Vertical,
		PressAndHold
	}

	public Restriction restriction;

	public bool cloneOnDrag;

	[HideInInspector]
	public float pressAndHoldDelay = 1f;

	public bool interactable = true;

	[NonSerialized]
	protected Transform mTrans;

	[NonSerialized]
	protected Transform mParent;

	[NonSerialized]
	protected Collider mCollider;

	[NonSerialized]
	protected Collider2D mCollider2D;

	[NonSerialized]
	protected UIButton mButton;

	[NonSerialized]
	protected UIRoot mRoot;

	[NonSerialized]
	protected UIGrid mGrid;

	[NonSerialized]
	protected UITable mTable;

	[NonSerialized]
	protected float mDragStartTime;

	[NonSerialized]
	protected UIDragScrollView mDragScrollView;

	[NonSerialized]
	protected bool mPressed;

	[NonSerialized]
	protected bool mDragging;

	[NonSerialized]
	protected UICamera.MouseOrTouch mTouch;

	public static List<UIDragDropItem> draggedItems = new List<UIDragDropItem>();

	protected virtual void Awake()
	{
		mTrans = ((Component)this).transform;
		mCollider = ((Component)this).gameObject.GetComponent<Collider>();
		mCollider2D = ((Component)this).gameObject.GetComponent<Collider2D>();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		if (mDragging)
		{
			StopDragging(UICamera.hoveredObject);
		}
	}

	protected virtual void Start()
	{
		mButton = ((Component)this).GetComponent<UIButton>();
		mDragScrollView = ((Component)this).GetComponent<UIDragScrollView>();
	}

	protected virtual void OnPress(bool isPressed)
	{
		if (!interactable || UICamera.currentTouchID == -2 || UICamera.currentTouchID == -3)
		{
			return;
		}
		if (isPressed)
		{
			if (!mPressed)
			{
				mTouch = UICamera.currentTouch;
				mDragStartTime = RealTime.time + pressAndHoldDelay;
				mPressed = true;
			}
		}
		else if (mPressed && mTouch == UICamera.currentTouch)
		{
			mPressed = false;
			mTouch = null;
		}
	}

	protected virtual void Update()
	{
		if (restriction == Restriction.PressAndHold && mPressed && !mDragging && mDragStartTime < RealTime.time)
		{
			StartDragging();
		}
	}

	protected virtual void OnDragStart()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!interactable || !((Behaviour)this).enabled || mTouch != UICamera.currentTouch)
		{
			return;
		}
		if (restriction != 0)
		{
			if (restriction == Restriction.Horizontal)
			{
				Vector2 totalDelta = mTouch.totalDelta;
				if (Mathf.Abs(totalDelta.x) < Mathf.Abs(totalDelta.y))
				{
					return;
				}
			}
			else if (restriction == Restriction.Vertical)
			{
				Vector2 totalDelta2 = mTouch.totalDelta;
				if (Mathf.Abs(totalDelta2.x) > Mathf.Abs(totalDelta2.y))
				{
					return;
				}
			}
			else if (restriction == Restriction.PressAndHold)
			{
				return;
			}
		}
		StartDragging();
	}

	public virtual void StartDragging()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (!interactable || mDragging)
		{
			return;
		}
		if (cloneOnDrag)
		{
			mPressed = false;
			GameObject val = ((Component)((Component)this).transform.parent).gameObject.AddChild(((Component)this).gameObject);
			val.transform.localPosition = ((Component)this).transform.localPosition;
			val.transform.localRotation = ((Component)this).transform.localRotation;
			val.transform.localScale = ((Component)this).transform.localScale;
			UIButtonColor component = val.GetComponent<UIButtonColor>();
			if ((Object)(object)component != (Object)null)
			{
				component.defaultColor = ((Component)this).GetComponent<UIButtonColor>().defaultColor;
			}
			if (mTouch != null && (Object)(object)mTouch.pressed == (Object)(object)((Component)this).gameObject)
			{
				mTouch.current = val;
				mTouch.pressed = val;
				mTouch.dragged = val;
				mTouch.last = val;
			}
			UIDragDropItem component2 = val.GetComponent<UIDragDropItem>();
			component2.mTouch = mTouch;
			component2.mPressed = true;
			component2.mDragging = true;
			component2.Start();
			component2.OnClone(((Component)this).gameObject);
			component2.OnDragDropStart();
			if (UICamera.currentTouch == null)
			{
				UICamera.currentTouch = mTouch;
			}
			mTouch = null;
			UICamera.Notify(((Component)this).gameObject, "OnPress", false);
			UICamera.Notify(((Component)this).gameObject, "OnHover", false);
		}
		else
		{
			mDragging = true;
			OnDragDropStart();
		}
	}

	protected virtual void OnClone(GameObject original)
	{
	}

	protected virtual void OnDrag(Vector2 delta)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (interactable && mDragging && ((Behaviour)this).enabled && mTouch == UICamera.currentTouch)
		{
			if ((Object)(object)mRoot != (Object)null)
			{
				OnDragDropMove(delta * mRoot.pixelSizeAdjustment);
			}
			else
			{
				OnDragDropMove(delta);
			}
		}
	}

	protected virtual void OnDragEnd()
	{
		if (interactable && ((Behaviour)this).enabled && mTouch == UICamera.currentTouch)
		{
			StopDragging(UICamera.hoveredObject);
		}
	}

	public void StopDragging(GameObject go)
	{
		if (mDragging)
		{
			mDragging = false;
			OnDragDropRelease(go);
		}
	}

	protected virtual void OnDragDropStart()
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (!draggedItems.Contains(this))
		{
			draggedItems.Add(this);
		}
		if ((Object)(object)mDragScrollView != (Object)null)
		{
			((Behaviour)mDragScrollView).enabled = false;
		}
		if ((Object)(object)mButton != (Object)null)
		{
			mButton.isEnabled = false;
		}
		else if ((Object)(object)mCollider != (Object)null)
		{
			mCollider.enabled = false;
		}
		else if ((Object)(object)mCollider2D != (Object)null)
		{
			((Behaviour)mCollider2D).enabled = false;
		}
		mParent = mTrans.parent;
		mRoot = NGUITools.FindInParents<UIRoot>(mParent);
		mGrid = NGUITools.FindInParents<UIGrid>(mParent);
		mTable = NGUITools.FindInParents<UITable>(mParent);
		if ((Object)(object)UIDragDropRoot.root != (Object)null)
		{
			mTrans.parent = UIDragDropRoot.root;
		}
		Vector3 localPosition = mTrans.localPosition;
		localPosition.z = 0f;
		mTrans.localPosition = localPosition;
		TweenPosition component = ((Component)this).GetComponent<TweenPosition>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = false;
		}
		SpringPosition component2 = ((Component)this).GetComponent<SpringPosition>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Behaviour)component2).enabled = false;
		}
		NGUITools.MarkParentAsChanged(((Component)this).gameObject);
		if ((Object)(object)mTable != (Object)null)
		{
			mTable.repositionNow = true;
		}
		if ((Object)(object)mGrid != (Object)null)
		{
			mGrid.repositionNow = true;
		}
	}

	protected virtual void OnDragDropMove(Vector2 delta)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Transform obj = mTrans;
		obj.localPosition += Vector2.op_Implicit(delta);
	}

	protected virtual void OnDragDropRelease(GameObject surface)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (!cloneOnDrag)
		{
			if ((Object)(object)mButton != (Object)null)
			{
				mButton.isEnabled = true;
			}
			else if ((Object)(object)mCollider != (Object)null)
			{
				mCollider.enabled = true;
			}
			else if ((Object)(object)mCollider2D != (Object)null)
			{
				((Behaviour)mCollider2D).enabled = true;
			}
			UIDragDropContainer uIDragDropContainer = ((!Object.op_Implicit((Object)(object)surface)) ? null : NGUITools.FindInParents<UIDragDropContainer>(surface));
			if ((Object)(object)uIDragDropContainer != (Object)null)
			{
				mTrans.parent = ((!((Object)(object)uIDragDropContainer.reparentTarget != (Object)null)) ? ((Component)uIDragDropContainer).transform : uIDragDropContainer.reparentTarget);
				Vector3 localPosition = mTrans.localPosition;
				localPosition.z = 0f;
				mTrans.localPosition = localPosition;
			}
			else
			{
				mTrans.parent = mParent;
			}
			mParent = mTrans.parent;
			mGrid = NGUITools.FindInParents<UIGrid>(mParent);
			mTable = NGUITools.FindInParents<UITable>(mParent);
			if ((Object)(object)mDragScrollView != (Object)null)
			{
				((MonoBehaviour)this).Invoke("EnableDragScrollView", 0.001f);
			}
			NGUITools.MarkParentAsChanged(((Component)this).gameObject);
			if ((Object)(object)mTable != (Object)null)
			{
				mTable.repositionNow = true;
			}
			if ((Object)(object)mGrid != (Object)null)
			{
				mGrid.repositionNow = true;
			}
		}
		else
		{
			NGUITools.Destroy((Object)(object)((Component)this).gameObject);
		}
		OnDragDropEnd();
	}

	protected virtual void OnDragDropEnd()
	{
		draggedItems.Remove(this);
	}

	protected void EnableDragScrollView()
	{
		if ((Object)(object)mDragScrollView != (Object)null)
		{
			((Behaviour)mDragScrollView).enabled = true;
		}
	}
}
