using System.Collections.Generic;
using ItemSystem;
using UnityEngine;

public class TagsPage : MonoBehaviour
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _majorTagControls;

	[SerializeField]
	private ListObjectPool _minorTagControls;

	[SerializeField]
	private UILabel _textDescription;

	public void ShowItemContent(ItemData itemData)
	{
		ShowTags(itemData);
		ShowDescription(itemData);
		_scrollView.Reposition(resetPosition: true, tween: false);
	}

	private void ShowTags(ItemData itemData)
	{
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)_majorTagControls.BaseObject.transform.parent).GetComponent<UIWidget>();
		_majorTagControls.Clear();
		_minorTagControls.Clear();
		object list;
		if (itemData.Capsule == null)
		{
			IList<TagData> tags = itemData.Tags;
			list = tags;
		}
		else
		{
			list = itemData.Capsule.Tags;
		}
		IList<TagData> list2 = (IList<TagData>)list;
		int count = list2.Count;
		for (int i = 0; i < count; i++)
		{
			TagData tagData = list2[i];
			if (tagData.Visible != TagData.VisibleType.Hide)
			{
				ListObjectPool listObjectPool = ((tagData.Display != 0) ? _minorTagControls : _majorTagControls);
				ItemTagControl itemTagControl = ((ListObjectPoolBase<GameObject>)listObjectPool).Add<ItemTagControl>();
				if (tagData.Display == TagData.DisplayType.Major)
				{
					itemTagControl.Name = tagData.LocalizedName;
					itemTagControl.Level = tagData.Level;
				}
				else
				{
					itemTagControl.Name = tagData.GetNameWithLevel();
					int width = itemTagControl.NameLabel.width + 30;
					itemTagControl.Widget.width = width;
				}
				itemTagControl.Icon = tagData.Icon;
			}
		}
		Vector3 localPosition = _majorTagControls.BaseObject.transform.localPosition;
		Vector3 localPosition2 = localPosition;
		int width2 = component.width;
		for (int j = 0; j < _majorTagControls.Count; j++)
		{
			UIWidget component2 = _majorTagControls[j].GetComponent<UIWidget>();
			if (localPosition2.x - localPosition.x + (float)component2.width > (float)width2)
			{
				localPosition2.x = localPosition.x;
				localPosition2.y -= (float)component2.height;
			}
			((Component)component2).transform.localPosition = localPosition2;
			localPosition2.x += (float)component2.width;
		}
		localPosition = _minorTagControls.BaseObject.transform.localPosition;
		if (_majorTagControls.Count == 0)
		{
			localPosition.y = _majorTagControls.BaseObject.transform.localPosition.y;
		}
		else
		{
			localPosition.y = _majorTagControls[_majorTagControls.Count - 1].transform.localPosition.y + (localPosition.y - _majorTagControls.BaseObject.transform.localPosition.y);
		}
		localPosition2 = localPosition;
		for (int k = 0; k < _minorTagControls.Count; k++)
		{
			UIWidget component3 = _minorTagControls[k].GetComponent<UIWidget>();
			if (localPosition2.x - localPosition.x + (float)component3.width > (float)width2)
			{
				localPosition2.x = localPosition.x;
				localPosition2.y -= (float)component3.height;
			}
			((Component)component3).transform.localPosition = localPosition2;
			localPosition2.x += (float)component3.width;
		}
		int num = 0;
		if (_minorTagControls.Count > 0)
		{
			UIWidget component4 = _minorTagControls[_minorTagControls.Count - 1].GetComponent<UIWidget>();
			num = (int)Mathf.Abs(component4.GetPosition(0f, 0f).y);
		}
		else if (_majorTagControls.Count > 0)
		{
			UIWidget component5 = _majorTagControls[_majorTagControls.Count - 1].GetComponent<UIWidget>();
			num = component5.height + (int)Mathf.Abs(((Component)component5).transform.localPosition.y);
		}
		if (num > 0)
		{
			component.height = num;
			((Component)component).gameObject.SetActive(true);
			UIUtility.UpdateAnchors(((Component)component).transform);
		}
		else
		{
			((Component)component).gameObject.SetActive(false);
		}
	}

	private void ShowDescription(ItemData itemData)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)((Component)_textDescription).transform.parent).GetComponent<UIWidget>();
		_textDescription.text = itemData.Description;
		component.height = _textDescription.height + (int)Mathf.Abs(((Component)_textDescription).transform.localPosition.y);
	}
}
