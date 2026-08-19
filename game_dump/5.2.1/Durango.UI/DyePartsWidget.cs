using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

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

	private Color[] _rgb = new Color[3];

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
		SelectableButton component = obj.GetComponent<SelectableButton>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickPartsButton));
		component.Widget.SetAnchor((Transform)null);
	}

	private void LateUpdate()
	{
		if (!_isVisibleParts)
		{
			return;
		}
		Color[] rgb = _rgb;
		for (int i = 0; i < _partButtons.Count; i++)
		{
			_partButtons[i].GetComponent<Selectable>().Selected = i == SelectedPart;
		}
		ItemIcon icon = _item.Icon;
		for (int j = 0; j < 3; j++)
		{
			if (j < icon.Colors.Count && j != SelectedPart)
			{
				ref Color reference = ref rgb[j];
				reference = _defaultColor;
			}
			else
			{
				float t = (Mathf.Sin(Time.time * 2f * (float)Math.PI * _blinkPeriod) + 1f) / 2f;
				ref Color reference2 = ref rgb[j];
				reference2 = Color.Lerp(_selectColor1, _selectColor2, t);
			}
		}
		icon.Colors = new ItemColor(rgb[0], rgb[1], rgb[2]);
		_iconTexture.SetIcon(icon);
	}

	public void Reset()
	{
		_noData.gameObject.SetActive(value: true);
		_partsContainer.gameObject.SetActive(value: false);
		SelectedPart = 0;
		_isVisibleParts = false;
	}

	public void Set(ItemData item)
	{
		Init();
		_isVisibleParts = true;
		_noData.gameObject.SetActive(value: false);
		_partsContainer.gameObject.SetActive(value: true);
		_item = item;
		_partButtons.BeginLoad();
		int num = -1;
		for (int i = 0; i < item.Colors.Count; i++)
		{
			if (item.Colors[i] == Color.clear)
			{
				continue;
			}
			SelectableButton component = _partButtons.GetNext().GetComponent<SelectableButton>();
			component.Text = (i + 1).ToString();
			if (_item.IsDyeable((ColorChannel)i))
			{
				component.Disabled = false;
				if (num == -1)
				{
					num = i;
				}
			}
			else
			{
				component.Disabled = true;
				if (i == SelectedPart)
				{
					SelectedPart = -1;
				}
			}
		}
		_partButtons.EndLoad();
		_partButtons.Reposition(Vector3.down, 20);
		SelectedPart = ((SelectedPart != -1) ? SelectedPart : num);
		Select(SelectedPart);
	}

	private void Select(int index)
	{
		SelectedPart = Mathf.Clamp(index, 0, _partButtons.Count - 1);
		for (int i = 0; i < _partButtons.Count; i++)
		{
			_partButtons[i].GetComponent<Selectable>().Selected = i == SelectedPart;
		}
		if (this.SelectPartChanged != null)
		{
			this.SelectPartChanged();
		}
	}

	private void OnClickPartsButton()
	{
		int index = _partButtons.IndexOf(Selectable.Current.gameObject);
		Select(index);
	}
}
