using System;
using Durango.Logic.WarpRush;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class WarpRushResultGroup : UIBase
{
	private const string HideKey = "WarpResult";

	private const string ContentsFormat = "[FFFFFFA0][size=24]{0}[/size][-][c]{1}[/c]{2}";

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subtitleLabel;

	[SerializeField]
	private RectLayoutComponent _contentsLayout;

	[SerializeField]
	private GameObject _overallRankingContainer;

	[SerializeField]
	private UISpriteLabel _overallRankingLabel;

	[SerializeField]
	private UISpriteLabel _suvivorCountLabel;

	[SerializeField]
	private UISpriteLabel _acquiredResourcesLabel;

	[SerializeField]
	private SelectableButton _exitButton;

	private Revision _revision;

	private MyRecord _myRecord;

	private void Start()
	{
		_exitButton.Text = T._("나가기");
		SelectableButton exitButton = _exitButton;
		exitButton.Clicked = (Action)Delegate.Combine(exitButton.Clicked, new Action(ExitButtonClicked));
		TryClose();
	}

	private void ExitButtonClicked()
	{
		Connections.Frontend.Send(default(S02Leave)).On<Error>(delegate
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(_exitButton.gameObject);
			_exitButton.Disabled = false;
		});
		UIManager.Popup.LoadingRing.AttachToWidget(_exitButton.gameObject);
		_exitButton.Disabled = true;
		StopAllCoroutines();
	}
}
