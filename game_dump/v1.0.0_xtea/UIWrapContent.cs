using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Wrap Content")]
public class UIWrapContent : MonoBehaviour
{
	public delegate void OnInitializeItem(GameObject go, int wrapIndex, int realIndex);

	public int itemSize = 100;

	public bool cullContent = true;

	public int minIndex;

	public int maxIndex;

	public bool hideInactive;

	public OnInitializeItem onInitializeItem;

	protected Transform mTrans;

	protected UIPanel mPanel;

	protected UIScrollView mScroll;

	protected bool mHorizontal;

	protected bool mFirstTime = true;

	protected List<Transform> mChildren = new List<Transform>();

	protected virtual void Start()
	{
		SortBasedOnScrollMovement();
		WrapContent();
		if ((Object)(object)mScroll != (Object)null)
		{
			((Component)mScroll).GetComponent<UIPanel>().onClipMove = OnMove;
		}
		mFirstTime = false;
	}

	protected virtual void OnMove(UIPanel panel)
	{
		WrapContent();
	}

	[ContextMenu("Sort Based on Scroll Movement")]
	public virtual void SortBasedOnScrollMovement()
	{
		if (!CacheScrollView())
		{
			return;
		}
		mChildren.Clear();
		for (int i = 0; i < mTrans.childCount; i++)
		{
			Transform child = mTrans.GetChild(i);
			if (!hideInactive || ((Component)child).gameObject.activeInHierarchy)
			{
				mChildren.Add(child);
			}
		}
		if (mHorizontal)
		{
			mChildren.Sort(UIGrid.SortHorizontal);
		}
		else
		{
			mChildren.Sort(UIGrid.SortVertical);
		}
		ResetChildPositions();
	}

	[ContextMenu("Sort Alphabetically")]
	public virtual void SortAlphabetically()
	{
		if (!CacheScrollView())
		{
			return;
		}
		mChildren.Clear();
		for (int i = 0; i < mTrans.childCount; i++)
		{
			Transform child = mTrans.GetChild(i);
			if (!hideInactive || ((Component)child).gameObject.activeInHierarchy)
			{
				mChildren.Add(child);
			}
		}
		mChildren.Sort(UIGrid.SortByName);
		ResetChildPositions();
	}

	protected bool CacheScrollView()
	{
		mTrans = ((Component)this).transform;
		mPanel = NGUITools.FindInParents<UIPanel>(((Component)this).gameObject);
		mScroll = ((Component)mPanel).GetComponent<UIScrollView>();
		if ((Object)(object)mScroll == (Object)null)
		{
			return false;
		}
		if (mScroll.movement == UIScrollView.Movement.Horizontal)
		{
			mHorizontal = true;
		}
		else
		{
			if (mScroll.movement != UIScrollView.Movement.Vertical)
			{
				return false;
			}
			mHorizontal = false;
		}
		return true;
	}

	protected virtual void ResetChildPositions()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = mChildren.Count; i < count; i++)
		{
			Transform val = mChildren[i];
			val.localPosition = ((!mHorizontal) ? new Vector3(0f, (float)(-i * itemSize), 0f) : new Vector3((float)(i * itemSize), 0f, 0f));
			UpdateItem(val, i);
		}
	}

	public virtual void WrapContent()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(itemSize * mChildren.Count) * 0.5f;
		Vector3[] worldCorners = mPanel.worldCorners;
		for (int i = 0; i < 4; i++)
		{
			Vector3 val = worldCorners[i];
			val = mTrans.InverseTransformPoint(val);
			worldCorners[i] = val;
		}
		Vector3 val2 = Vector3.Lerp(worldCorners[0], worldCorners[2], 0.5f);
		bool flag = true;
		float num2 = num * 2f;
		if (mHorizontal)
		{
			float num3 = worldCorners[0].x - (float)itemSize;
			float num4 = worldCorners[2].x + (float)itemSize;
			int j = 0;
			for (int count = mChildren.Count; j < count; j++)
			{
				Transform val3 = mChildren[j];
				float num5 = val3.localPosition.x - val2.x;
				if (num5 < 0f - num)
				{
					Vector3 localPosition = val3.localPosition;
					localPosition.x += num2;
					num5 = localPosition.x - val2.x;
					int num6 = Mathf.RoundToInt(localPosition.x / (float)itemSize);
					if (minIndex == maxIndex || (minIndex <= num6 && num6 <= maxIndex))
					{
						val3.localPosition = localPosition;
						UpdateItem(val3, j);
					}
					else
					{
						flag = false;
					}
				}
				else if (num5 > num)
				{
					Vector3 localPosition2 = val3.localPosition;
					localPosition2.x -= num2;
					num5 = localPosition2.x - val2.x;
					int num7 = Mathf.RoundToInt(localPosition2.x / (float)itemSize);
					if (minIndex == maxIndex || (minIndex <= num7 && num7 <= maxIndex))
					{
						val3.localPosition = localPosition2;
						UpdateItem(val3, j);
					}
					else
					{
						flag = false;
					}
				}
				else if (mFirstTime)
				{
					UpdateItem(val3, j);
				}
				if (cullContent)
				{
					num5 += mPanel.clipOffset.x - mTrans.localPosition.x;
					if (!UICamera.IsPressed(((Component)val3).gameObject))
					{
						NGUITools.SetActive(((Component)val3).gameObject, num5 > num3 && num5 < num4, compatibilityMode: false);
					}
				}
			}
		}
		else
		{
			float num8 = worldCorners[0].y - (float)itemSize;
			float num9 = worldCorners[2].y + (float)itemSize;
			int k = 0;
			for (int count2 = mChildren.Count; k < count2; k++)
			{
				Transform val4 = mChildren[k];
				float num10 = val4.localPosition.y - val2.y;
				if (num10 < 0f - num)
				{
					Vector3 localPosition3 = val4.localPosition;
					localPosition3.y += num2;
					num10 = localPosition3.y - val2.y;
					int num11 = Mathf.RoundToInt(localPosition3.y / (float)itemSize);
					if (minIndex == maxIndex || (minIndex <= num11 && num11 <= maxIndex))
					{
						val4.localPosition = localPosition3;
						UpdateItem(val4, k);
					}
					else
					{
						flag = false;
					}
				}
				else if (num10 > num)
				{
					Vector3 localPosition4 = val4.localPosition;
					localPosition4.y -= num2;
					num10 = localPosition4.y - val2.y;
					int num12 = Mathf.RoundToInt(localPosition4.y / (float)itemSize);
					if (minIndex == maxIndex || (minIndex <= num12 && num12 <= maxIndex))
					{
						val4.localPosition = localPosition4;
						UpdateItem(val4, k);
					}
					else
					{
						flag = false;
					}
				}
				else if (mFirstTime)
				{
					UpdateItem(val4, k);
				}
				if (cullContent)
				{
					num10 += mPanel.clipOffset.y - mTrans.localPosition.y;
					if (!UICamera.IsPressed(((Component)val4).gameObject))
					{
						NGUITools.SetActive(((Component)val4).gameObject, num10 > num8 && num10 < num9, compatibilityMode: false);
					}
				}
			}
		}
		mScroll.restrictWithinPanel = !flag;
		mScroll.InvalidateBounds();
	}

	private void OnValidate()
	{
		if (maxIndex < minIndex)
		{
			maxIndex = minIndex;
		}
		if (minIndex > maxIndex)
		{
			maxIndex = minIndex;
		}
	}

	protected virtual void UpdateItem(Transform item, int index)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (onInitializeItem != null)
		{
			int realIndex = ((mScroll.movement != UIScrollView.Movement.Vertical) ? Mathf.RoundToInt(item.localPosition.x / (float)itemSize) : Mathf.RoundToInt(item.localPosition.y / (float)itemSize));
			onInitializeItem(((Component)item).gameObject, index, realIndex);
		}
	}
}
