using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIDropDownItem")]
public class tk2dUIDropDownItem : tk2dUIBaseItemControl
{
	public tk2dTextMesh label;

	public float height;

	public tk2dUIUpDownHoverButton upDownHoverBtn;

	private int index;

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public string LabelText
	{
		get
		{
			return label.text;
		}
		set
		{
			label.text = value;
			label.Commit();
		}
	}

	public event Action<tk2dUIDropDownItem> OnItemSelected;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnClick += ItemSelected;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)uiItem))
		{
			uiItem.OnClick -= ItemSelected;
		}
	}

	private void ItemSelected()
	{
		if (this.OnItemSelected != null)
		{
			this.OnItemSelected(this);
		}
	}
}
