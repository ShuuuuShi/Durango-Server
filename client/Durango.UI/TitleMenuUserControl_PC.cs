using System;
using Durango.Logic.Clusters;
using Durango.System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitleMenuUserControl_PC : TitleMenuUserControlBase
{
	[SerializeField]
	private SelectableButton _startButton;

	[SerializeField]
	private UILabel _serverSelectionLabel;

	[SerializeField]
	private UILabel _playerSelectionLabel;

	[SerializeField]
	private UILabel _exitGameLabel;

	private bool _showPlayerButton;

	private bool _showClusterButton;

	protected override void Start()
	{
		base.Start();
		_startButton.Clicked = OnConfirm;
		_startButton.Text = ManualTranslator.Start;
		_exitGameLabel.text = ManualTranslator.ExitGame;
		_serverSelectionLabel.text = ManualTranslator.SelectServer;
		_playerSelectionLabel.text = ManualTranslator.SelectCharacter;
		_logoutButton.GetComponent<UIRect>().rightAnchor.Set(_exitGameLabel.transform, 1f, 0f);
		_logoutButton.GetComponent<UIRect>().UpdateAnchors();
	}

	public override void OnStateChanged(TitleMenuGroup.State state)
	{
		base.OnStateChanged(state);
		UpdateStartButton();
		UpdateExplainLabel();
	}

	protected override void OnConfirm()
	{
		if (LastState == TitleMenuGroup.State.Error)
		{
			if (base.QuitWhenErrorOccurred)
			{
				Platform.Instance.Quit();
			}
			else
			{
				RetryConnect = true;
			}
			IsAccountReady = false;
		}
		else
		{
			base.OnConfirm();
		}
	}

	public override void ShowCluster(Action onConfirm, Action onPlayerSelection, Action onLogout, bool autoConfirm)
	{
		base.ShowCluster(onConfirm, onPlayerSelection, onLogout, autoConfirm);
		_logoutButton.Clicked = ShowConfirmMessageBox;
	}

	protected override void UpdateButtonLayout(bool showPlayerButton)
	{
		base.UpdateButtonLayout(showPlayerButton);
		_showClusterButton = Clusters.Count >= 2;
		_playerSelectionLabel.gameObject.SetActive(showPlayerButton);
		_serverSelectionLabel.gameObject.SetActive(_showClusterButton);
		UpdateStartButton();
		UpdateExplainLabel();
	}

	protected override void OnClusterAccountUpdated(Account account)
	{
		base.OnClusterAccountUpdated(account);
		_showPlayerButton = account != null && account.MaxPlayerSlotCount > 1 && account.PlayerSlotCount >= 1;
		UpdateStartButton();
	}

	private void UpdateStartButton()
	{
		if (LastState == TitleMenuGroup.State.Error)
		{
			base.IsLoginProcess = true;
		}
		_startButton.gameObject.SetActive(base.IsLoginProcess);
		if (!base.IsLoginProcess)
		{
			return;
		}
		_startButton.Disabled = LastState != TitleMenuGroup.State.SelectCluster && LastState != TitleMenuGroup.State.Error;
		if (_startButton.Disabled)
		{
			return;
		}
		_startButton.Text = ManualTranslator.Start;
		_startButton.Disabled = !IsAccountReady && (!_showClusterButton || !_showPlayerButton);
		if (!_showClusterButton || !_showPlayerButton)
		{
			if (LastState == TitleMenuGroup.State.Error)
			{
				_startButton.Text = ((!base.QuitWhenErrorOccurred) ? ManualTranslator.Retry : ManualTranslator.Close);
				_startButton.Disabled = false;
			}
		}
		else if (_showClusterButton && _showPlayerButton && LastState == TitleMenuGroup.State.Error)
		{
			_startButton.Text = ((!base.QuitWhenErrorOccurred) ? ManualTranslator.Retry : ManualTranslator.Close);
			_startButton.Disabled = false;
		}
	}

	private void UpdateExplainLabel()
	{
		_explainLabel.gameObject.SetActive(value: true);
		if (LastState == TitleMenuGroup.State.SelectCluster)
		{
			_explainLabel.gameObject.SetActive(value: false);
		}
	}

	public override bool ShowMaintenance()
	{
		_startButton.gameObject.SetActive(value: false);
		return base.ShowMaintenance();
	}

	protected override void HideOutlinks()
	{
		_startButton.gameObject.SetActive(value: true);
		base.HideOutlinks();
	}

	protected override void OnReceiveBackMessage(InputCommandMessage message)
	{
		if (_mainContent.gameObject.activeSelf)
		{
			ShowConfirmMessageBox();
		}
		else
		{
			base.OnReceiveBackMessage(message);
		}
	}

	private void ShowConfirmMessageBox()
	{
		ShowMessageBox(ManualTranslator.ExitGame, ManualTranslator.WantToQuit, Platform.Instance.Quit, CloseMessageBox, ManualTranslator.ExitGame);
	}
}
