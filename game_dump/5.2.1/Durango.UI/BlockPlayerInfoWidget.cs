using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class BlockPlayerInfoWidget : PlayerInfoWidget
{
	[SerializeField]
	private SelectableButton _removeButton;

	public event Action<string> RemoveClicked;

	private void Start()
	{
		_removeButton.Clicked = delegate
		{
			if (this.RemoveClicked != null)
			{
				this.RemoveClicked(base.EntityId);
			}
		};
		_removeButton.Text = T._("차단 해제");
	}
}
