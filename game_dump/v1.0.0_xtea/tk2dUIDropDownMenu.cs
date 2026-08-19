using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIDropDownMenu")]
public class tk2dUIDropDownMenu : MonoBehaviour
{
	public tk2dUIItem dropDownButton;

	public tk2dTextMesh selectedTextMesh;

	[HideInInspector]
	public float height;

	public tk2dUIDropDownItem dropDownItemTemplate;

	[SerializeField]
	private string[] startingItemList;

	[SerializeField]
	private int startingIndex;

	private List<string> itemList = new List<string>();

	public string SendMessageOnSelectedItemChangeMethodName = string.Empty;

	private int index;

	private List<tk2dUIDropDownItem> dropDownItems = new List<tk2dUIDropDownItem>();

	private bool isExpanded;

	[HideInInspector]
	[SerializeField]
	private tk2dUILayout menuLayoutItem;

	[HideInInspector]
	[SerializeField]
	private tk2dUILayout templateLayoutItem;

	public List<string> ItemList
	{
		get
		{
			return itemList;
		}
		set
		{
			itemList = value;
		}
	}

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = Mathf.Clamp(value, 0, ItemList.Count - 1);
			SetSelectedItem();
		}
	}

	public string SelectedItem
	{
		get
		{
			if (index >= 0 && index < itemList.Count)
			{
				return itemList[index];
			}
			return string.Empty;
		}
	}

	public GameObject SendMessageTarget
	{
		get
		{
			if ((Object)(object)dropDownButton != (Object)null)
			{
				return dropDownButton.sendMessageTarget;
			}
			return null;
		}
		set
		{
			if ((Object)(object)dropDownButton != (Object)null && (Object)(object)dropDownButton.sendMessageTarget != (Object)(object)value)
			{
				dropDownButton.sendMessageTarget = value;
			}
		}
	}

	public tk2dUILayout MenuLayoutItem
	{
		get
		{
			return menuLayoutItem;
		}
		set
		{
			menuLayoutItem = value;
		}
	}

	public tk2dUILayout TemplateLayoutItem
	{
		get
		{
			return templateLayoutItem;
		}
		set
		{
			templateLayoutItem = value;
		}
	}

	public event Action OnSelectedItemChange;

	private void Awake()
	{
		string[] array = startingItemList;
		foreach (string item in array)
		{
			itemList.Add(item);
		}
		index = startingIndex;
		((Component)dropDownItemTemplate).gameObject.SetActive(false);
		UpdateList();
	}

	private void OnEnable()
	{
		dropDownButton.OnDown += ExpandButtonPressed;
	}

	private void OnDisable()
	{
		dropDownButton.OnDown -= ExpandButtonPressed;
	}

	public void UpdateList()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (dropDownItems.Count > ItemList.Count)
		{
			for (int i = ItemList.Count; i < dropDownItems.Count; i++)
			{
				((Component)dropDownItems[i]).gameObject.SetActive(false);
			}
		}
		while (dropDownItems.Count < ItemList.Count)
		{
			dropDownItems.Add(CreateAnotherDropDownItem());
		}
		for (int j = 0; j < ItemList.Count; j++)
		{
			tk2dUIDropDownItem tk2dUIDropDownItem2 = dropDownItems[j];
			Vector3 localPosition = ((Component)tk2dUIDropDownItem2).transform.localPosition;
			if ((Object)(object)menuLayoutItem != (Object)null && (Object)(object)templateLayoutItem != (Object)null)
			{
				localPosition.y = menuLayoutItem.bMin.y - (float)j * (templateLayoutItem.bMax.y - templateLayoutItem.bMin.y);
			}
			else
			{
				localPosition.y = 0f - height - (float)j * tk2dUIDropDownItem2.height;
			}
			((Component)tk2dUIDropDownItem2).transform.localPosition = localPosition;
			if ((Object)(object)tk2dUIDropDownItem2.label != (Object)null)
			{
				tk2dUIDropDownItem2.LabelText = itemList[j];
			}
			tk2dUIDropDownItem2.Index = j;
		}
		SetSelectedItem();
	}

	public void SetSelectedItem()
	{
		if (index < 0 || index >= ItemList.Count)
		{
			index = 0;
		}
		if (index >= 0 && index < ItemList.Count)
		{
			selectedTextMesh.text = ItemList[index];
			selectedTextMesh.Commit();
		}
		else
		{
			selectedTextMesh.text = string.Empty;
			selectedTextMesh.Commit();
		}
		if (this.OnSelectedItemChange != null)
		{
			this.OnSelectedItemChange();
		}
		if ((Object)(object)SendMessageTarget != (Object)null && SendMessageOnSelectedItemChangeMethodName.Length > 0)
		{
			SendMessageTarget.SendMessage(SendMessageOnSelectedItemChangeMethodName, (object)this, (SendMessageOptions)0);
		}
	}

	private tk2dUIDropDownItem CreateAnotherDropDownItem()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(((Component)dropDownItemTemplate).gameObject);
		((Object)val).name = "DropDownItem";
		val.transform.parent = ((Component)this).transform;
		val.transform.localPosition = ((Component)dropDownItemTemplate).transform.localPosition;
		val.transform.localRotation = ((Component)dropDownItemTemplate).transform.localRotation;
		val.transform.localScale = ((Component)dropDownItemTemplate).transform.localScale;
		tk2dUIDropDownItem component = val.GetComponent<tk2dUIDropDownItem>();
		component.OnItemSelected += ItemSelected;
		(component.upDownHoverBtn = val.GetComponent<tk2dUIUpDownHoverButton>()).OnToggleOver += DropDownItemHoverBtnToggle;
		return component;
	}

	private void ItemSelected(tk2dUIDropDownItem item)
	{
		if (isExpanded)
		{
			CollapseList();
		}
		Index = item.Index;
	}

	private void ExpandButtonPressed()
	{
		if (isExpanded)
		{
			CollapseList();
		}
		else
		{
			ExpandList();
		}
	}

	private void ExpandList()
	{
		isExpanded = true;
		int num = Mathf.Min(ItemList.Count, dropDownItems.Count);
		for (int i = 0; i < num; i++)
		{
			((Component)dropDownItems[i]).gameObject.SetActive(true);
		}
		tk2dUIDropDownItem tk2dUIDropDownItem2 = dropDownItems[index];
		if ((Object)(object)tk2dUIDropDownItem2.upDownHoverBtn != (Object)null)
		{
			tk2dUIDropDownItem2.upDownHoverBtn.IsOver = true;
		}
	}

	private void CollapseList()
	{
		isExpanded = false;
		foreach (tk2dUIDropDownItem dropDownItem in dropDownItems)
		{
			((Component)dropDownItem).gameObject.SetActive(false);
		}
	}

	private void DropDownItemHoverBtnToggle(tk2dUIUpDownHoverButton upDownHoverButton)
	{
		if (!upDownHoverButton.IsOver)
		{
			return;
		}
		foreach (tk2dUIDropDownItem dropDownItem in dropDownItems)
		{
			if ((Object)(object)dropDownItem.upDownHoverBtn != (Object)(object)upDownHoverButton && (Object)(object)dropDownItem.upDownHoverBtn != (Object)null)
			{
				dropDownItem.upDownHoverBtn.IsOver = false;
			}
		}
	}

	private void OnDestroy()
	{
		foreach (tk2dUIDropDownItem dropDownItem in dropDownItems)
		{
			dropDownItem.OnItemSelected -= ItemSelected;
			if ((Object)(object)dropDownItem.upDownHoverBtn != (Object)null)
			{
				dropDownItem.upDownHoverBtn.OnToggleOver -= DropDownItemHoverBtnToggle;
			}
		}
	}
}
