using System;
using System.Collections.Generic;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PlayerSearchBottomBar : MonoBehaviour
{
	[SerializeField]
	private SelectableButton _confirm;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UIWidget _selectedView;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	private KInfiniteScrollView.View<string, PlayerSearchSelectedWidget> _view;

	private int _maxCount;

	public event Action Confirmed
	{
		add
		{
			SelectableButton confirm = _confirm;
			confirm.Clicked = (Action)Delegate.Combine(confirm.Clicked, value);
		}
		remove
		{
			SelectableButton confirm = _confirm;
			confirm.Clicked = (Action)Delegate.Remove(confirm.Clicked, value);
		}
	}

	public event Action<string> SelectionCanceled;

	[UsedImplicitly]
	private void OnInitialize()
	{
		_view = _scrollView.Initialize(delegate(PlayerSearchSelectedWidget widget, string entityId)
		{
			widget.Set(entityId);
		}, delegate(PlayerSearchSelectedWidget widget)
		{
			widget.Canceled += delegate
			{
				if (this.SelectionCanceled != null)
				{
					this.SelectionCanceled(widget.GetEntityId());
				}
			};
		});
	}

	public void EnableSelectedView(bool enable)
	{
		_selectedView.gameObject.SetActive(enable);
	}

	public void SetDescription(string description)
	{
		_description.text = description;
		_selectedView.ResetAndUpdateAnchors();
	}

	public void SetMaxCount(int count)
	{
		_maxCount = count;
	}

	public void SetConfirmButton(string text, bool disabled)
	{
		_confirm.Text = text;
		_confirm.Disabled = disabled;
	}

	public void SetPlayers([CanBeNull] IList<string> list)
	{
		_view.SetList(list);
		int size = KUtility.GetSize(list);
		_scrollView.UpdateLayout();
		_scrollView.MoveToEnd(size, instant: false);
		SetDescription((_maxCount <= 0) ? string.Empty : T._("<em>{0}</em> / {1}", size, _maxCount));
		_confirm.Disabled = size == 0;
	}
}
