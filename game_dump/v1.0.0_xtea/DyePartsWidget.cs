using System;
using ItemSystem;
using Shared.Item;
using UnityEngine;

public class DyePartsWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private GameObject _partsContainer;

	[SerializeField]
	private ListObjectPool _partButtons;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private Color _defaultColor;

	[SerializeField]
	private float _blinkPeriod;

	[SerializeField]
	private Color _selectColor1;

	[SerializeField]
	private Color _selectColor2;

	private ItemData _item;

	private bool _isVisibleParts;

	private bool _isInit;

	public int SelectedPart { get; private set; }

	public event Action SelectPartChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_partButtons.Init(InitPartsButton);
		}
	}

	private void InitPartsButton(GameObject obj)
	{
		DefaultSelectableButton component = obj.GetComponent<DefaultSelectableButton>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickPartsButton));
		component.Widget.SetAnchor((Transform)null);
	}

	private void LateUpdate()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (!_isVisibleParts)
		{
			return;
		}
		Color[] array = (Color[])(object)new Color[3];
		for (int i = 0; i < _partButtons.Count; i++)
		{
			Selectable component = _partButtons[i].GetComponent<Selectable>();
			component.Select = i == SelectedPart;
			if (i == SelectedPart)
			{
				float num = (Mathf.Sin(Time.time * 2f * (float)Math.PI * _blinkPeriod) + 1f) / 2f;
				ref Color reference = ref array[i];
				reference = Color.Lerp(_selectColor1, _selectColor2, num);
			}
			else
			{
				ref Color reference2 = ref array[i];
				reference2 = _defaultColor;
			}
		}
		_iconTexture.SetIcon(_item.Icon, array[0], array[1], array[2]);
	}

	public void Reset()
	{
		_noData.gameObject.SetActive(true);
		_partsContainer.gameObject.SetActive(false);
		SelectedPart = 0;
		_isVisibleParts = false;
	}

	public void Set(ItemData item)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_isVisibleParts = true;
		_noData.gameObject.SetActive(false);
		_partsContainer.gameObject.SetActive(true);
		_item = item;
		_partButtons.Clear();
		int num = -1;
		for (int i = 0; i < item.Colors.Count; i++)
		{
			Color val = item.Colors[i];
			if (val == Color.clear)
			{
				continue;
			}
			DefaultSelectableButton defaultSelectableButton = ((ListObjectPoolBase<GameObject>)_partButtons).Add<DefaultSelectableButton>();
			defaultSelectableButton.Text = (i + 1).ToString();
			if (_item.IsDyeable((ColorChannel)i))
			{
				defaultSelectableButton.Disable = false;
				if (num == -1)
				{
					num = i;
				}
			}
			else
			{
				defaultSelectableButton.Disable = true;
				if (i == SelectedPart)
				{
					SelectedPart = -1;
				}
			}
		}
		_partButtons.Reposition(Vector3.down, 20);
		SelectedPart = ((SelectedPart != -1) ? SelectedPart : num);
		Select(SelectedPart);
	}

	private void Select(int index)
	{
		SelectedPart = Mathf.Clamp(index, 0, _partButtons.Count - 1);
		Color[] array = (Color[])(object)new Color[3];
		for (int i = 0; i < _partButtons.Count; i++)
		{
			Selectable component = _partButtons[i].GetComponent<Selectable>();
			component.Select = i == SelectedPart;
		}
		if (this.SelectPartChanged != null)
		{
			this.SelectPartChanged();
		}
	}

	private void OnClickPartsButton()
	{
		if (!Selectable.Current.Disable)
		{
			int index = _partButtons.IndexOf(((Component)Selectable.Current).gameObject);
			Select(index);
		}
	}
}
