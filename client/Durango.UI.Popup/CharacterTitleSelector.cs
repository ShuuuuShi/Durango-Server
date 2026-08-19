using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Statistics;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class CharacterTitleSelector : TooltipBase
{
	private class FavoritePrimalComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			FavoriteTitles favoriteTitles = GameSystem<StatisticsSystem>.Instance().FavoriteTitles;
			return (favoriteTitles.IsFavorite(x) != favoriteTitles.IsFavorite(y)) ? ((!favoriteTitles.IsFavorite(x)) ? 1 : (-1)) : 0;
		}
	}

	private const string BlurKey = "CharacterTitleSelector";

	private static FavoritePrimalComparer _comparer = new FavoritePrimalComparer();

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _favoriteCountLabel;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayout _layout;

	private KInfiniteScrollView.View<Title, CharacterTitleSelectorItem> _view;

	private string _selectedTitleId;

	private bool _favoriteChanged;

	private readonly List<Title> _avalaiableTitles = new List<Title>();

	protected override void OnAwake()
	{
		base.OnAwake();
		_confirmButton.Text = T._("확인");
		_view = _scrollView.Initialize(delegate(CharacterTitleSelectorItem comp, Title curTitle)
		{
			bool isSelected = ((curTitle != null) ? (curTitle.Id == _selectedTitleId) : string.IsNullOrEmpty(_selectedTitleId));
			bool isFavorite = curTitle != null && GameSystem<StatisticsSystem>.Instance().FavoriteTitles.IsFavorite(curTitle.Id);
			comp.Set(curTitle, isSelected, isFavorite);
		}, delegate(CharacterTitleSelectorItem comp)
		{
			comp.Clicked += SelectItem;
			comp.FavoriteClicked += ToggleFavorite;
		});
	}

	public void Set([CanBeNull] string selectedTitleId, [NotNull] Action<string> confirmed)
	{
		_selectedTitleId = selectedTitleId;
		_confirmButton.Clicked = delegate
		{
			confirmed(_selectedTitleId);
			Hide();
		};
		_avalaiableTitles.Clear();
		_avalaiableTitles.Add(null);
		_avalaiableTitles.AddRange(GameSystem<StatisticsSystem>.Instance().Titles.Where((Title elem) => elem.Enabled).OrderBy((Title elem) => elem.Id, _comparer));
		_view.SetList(_avalaiableTitles);
		_layout.UpdateLayout();
		UpdateFavoriteCount(GameSystem<StatisticsSystem>.Instance().FavoriteTitles.Count);
	}

	private void SelectItem(CharacterTitleSelectorItem comp)
	{
		_selectedTitleId = ((comp.TargetTitle != null) ? comp.TargetTitle.Id : null);
		_view.Redraw();
	}

	private void ToggleFavorite(CharacterTitleSelectorItem comp)
	{
		GameSystem<StatisticsSystem>.Instance().FavoriteTitles.Toggle(comp.TargetTitle.Id);
		_view.Redraw();
		_favoriteChanged = true;
		UpdateFavoriteCount(GameSystem<StatisticsSystem>.Instance().FavoriteTitles.Count);
	}

	private void UpdateFavoriteCount(int count)
	{
		_favoriteCountLabel.text = string.Format("{0} {1}", "craft_icon_star_enable_big".ToEncodedIcon(), count.ToString().ToEncodedColor(Color.black));
	}

	protected override void UpdateLayout()
	{
		base.transform.localPosition = Vector3.zero;
		int safeHeight = UIManager.SafeHeight;
		_layout.UpdateLayout(null, Mathf.Min((int)((float)safeHeight * 0.8f), safeHeight - 160));
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnShow()
	{
		base.OnShow();
		BlurController.BlurOn("CharacterTitleSelector", BlurController.Mask.UI);
		_scrollView.ResetPosition();
	}

	protected override void OnHide()
	{
		base.OnHide();
		BlurController.BlurOff("CharacterTitleSelector");
		if (_favoriteChanged)
		{
			_favoriteChanged = false;
			GameSystem<StatisticsSystem>.Instance().FavoriteTitles.Save();
		}
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
}
