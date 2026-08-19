using System;
using UnityEngine;

public class RecipeSlotWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

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

	private bool _initialized;

	public SlotInfo SlotInfo { get; set; }

	public event Action<GameObject> OnClick;

	public void Init()
	{
		if (!_initialized)
		{
			UIEventListener.Get(((Component)this).gameObject).onClick = OnGameObjectClick;
			_initialized = true;
		}
	}

	public void Refresh(SlotInfo currentSlot)
	{
		if (SlotInfo != null)
		{
			bool flag = SlotInfo.State == SlotInfo.SlotState.FullSelected;
			UIUtility.SetLabelText(_textName, SlotInfo.TextName);
			UIUtility.SetLabelText(_textCount, SlotInfo.TextCount);
			ShowCheckedIcon(flag);
			RefreshColor(SlotInfo == currentSlot, flag);
		}
	}

	private void ShowCheckedIcon(bool show)
	{
		if ((Object)(object)_iconChecked != (Object)null)
		{
			_iconChecked.SetActive(show);
		}
	}

	private void RefreshColor(bool selected, bool ready)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_background != (Object)null)
		{
			_background.color = (selected ? ((!ready) ? UIManager.UIYellow : _colorSelectedReady) : ((!ready) ? _colorDefaultNormal : _colorDefaultReady));
		}
	}

	private void OnGameObjectClick(GameObject go)
	{
		if (this.OnClick != null)
		{
			this.OnClick(go);
		}
	}
}
