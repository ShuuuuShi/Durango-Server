using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class RecipeSlotWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite[] _backgrounds;

	[SerializeField]
	private GameObject _iconChecked;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textCount;

	[SerializeField]
	private Color _colorDefaultNormal;

	[SerializeField]
	private Color _colorDefaultReady;

	[SerializeField]
	private Color _colorSelectedReady;

	public SlotInfo SlotInfo { get; private set; }

	public event Action<GameObject> Clicked;

	private void Start()
	{
		UIEventListener.Get(base.gameObject).onClick = OnGameObjectClick;
	}

	public void Set([NotNull] SlotInfo slot, bool selected)
	{
		SlotInfo = slot;
		Refresh(selected);
	}

	public void Refresh(bool selected)
	{
		bool flag = SlotInfo.State == SlotInfo.SlotState.FullSelected;
		if ((bool)_textName)
		{
			_textName.text = SlotInfo.Name;
		}
		if ((bool)_textCount)
		{
			_textCount.text = $"{SlotInfo.CurrentCount} / {SlotInfo.TotalCount}";
		}
		ShowCheckedIcon(flag);
		RefreshColor(selected, flag);
	}

	private void ShowCheckedIcon(bool show)
	{
		if (_iconChecked != null)
		{
			_iconChecked.SetActive(show);
		}
	}

	private void RefreshColor(bool selected, bool ready)
	{
		Color color = (selected ? ((!ready) ? PresetColor.UIYellow : _colorSelectedReady) : ((!ready) ? _colorDefaultNormal : _colorDefaultReady));
		int i = 0;
		for (int size = KUtility.GetSize(_backgrounds); i < size; i++)
		{
			_backgrounds[i].color = color;
		}
	}

	private void OnGameObjectClick(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonMedium);
		if (this.Clicked != null)
		{
			this.Clicked(go);
		}
	}
}
