using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Market;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InventoryFilterPopup : TooltipBase
{
	[SerializeField]
	private TweenerPlayer _showTweener;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private SelectableButton _closeButton;

	[SerializeField]
	private VerticalLayoutWidget _filterContainer;

	[SerializeField]
	private RectLayoutComponent _layout;

	[SerializeField]
	private UIEventListener _swapToTagSelectorButton;

	private HashSet<Category.Main> _selectedCategories = new HashSet<Category.Main>();

	private Category.Main[] _mainCategories;

	private bool _isInit;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	private void Init()
	{
		_isInit = true;
		_confirmButton.Text = T._("필터적용");
		_closeButton.Text = T._("취소");
		_mainCategories = GameSystem<MarketSystem>.Instance().CategoryYamlData.Select((Category elem) => elem.MainCategory).ToArray();
		_closeButton.Clicked = delegate
		{
			_selectedCategories.Clear();
			Hide();
		};
	}

	public InventoryFilterPopup Set([NotNull] HashSet<Category.Main> selectedCategories, [CanBeNull] Action<HashSet<Category.Main>> applyCategoryFilter)
	{
		if (!_isInit)
		{
			Init();
		}
		_selectedCategories.Clear();
		_selectedCategories.AddRange(selectedCategories);
		_confirmButton.Clicked = delegate
		{
			if (applyCategoryFilter != null)
			{
				applyCategoryFilter(_selectedCategories);
			}
			Hide();
		};
		_filterContainer.SetGrids(_mainCategories, delegate(Category.Main data, InventoryCategoryWidget obj, int idx)
		{
			obj.Set(data, _selectedCategories.Contains(data), delegate(Category.Main selected)
			{
				if (_selectedCategories.Contains(selected))
				{
					_selectedCategories.Remove(selected);
					obj.SetSelection(isSelected: false);
				}
				else
				{
					_selectedCategories.Add(selected);
					obj.SetSelection(isSelected: true);
				}
			});
		});
		_swapToTagSelectorButton.gameObject.SetActive(value: false);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		return this;
	}

	public void SetTagSelector([NotNull] HashSet<string> selectedTags, [CanBeNull] Action<HashSet<string>> applyTag, [CanBeNull] HashSet<string> existingTags)
	{
		_swapToTagSelectorButton.onClick = delegate
		{
			Hide();
			TagSelectPopup tagSelectPopup = UIManager.Popup.Tooltip<TagSelectPopup>();
			tagSelectPopup.Show();
			tagSelectPopup.Set(selectedTags.ToList(), applyTag, existingTags);
		};
		_swapToTagSelectorButton.gameObject.SetActive(value: true);
	}

	protected override void OnShow()
	{
		base.OnShow();
		_showTweener.Play();
	}

	protected override void OnTryConfirmOnModal()
	{
		if (_confirmButton != null && _confirmButton.Clicked != null)
		{
			_confirmButton.Clicked();
		}
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _closeButton;
	}
}
