using System;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class CampPinboardGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private PinboardLineList _pinboardLineList;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private RectLayoutComponent _inputLayout;

	[SerializeField]
	private UIInput _textInput;

	[SerializeField]
	private SelectableButton _submitButton;

	private Artifact _artifact;

	private void Awake()
	{
		_submitButton.Text = T._("전송");
		_submitButton.MinWidth = 200;
		_submitButton.ToPreferredSize();
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("게시판"));
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.ReadPinboard, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (targetComponent != null)
			{
				Open(targetComponent);
			}
		});
		_pinboardLineList.Init();
		EventDelegate.Add(_textInput.onSubmit, OnSubmit);
		SelectableButton submitButton = _submitButton;
		submitButton.Clicked = (Action)Delegate.Combine(submitButton.Clicked, new Action(OnSubmit));
		SetChildrenActive(activated: false);
		base.OnOpenSucceed += Opened;
	}

	public void Open([NotNull] Artifact artifact)
	{
		_artifact = artifact;
		Open();
	}

	private void Opened()
	{
		UpdateLayout();
		RequestPinboardContents(clear: true);
		if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			_textInput.isSelected = true;
		}
	}

	private void UpdateLayout()
	{
		_inputLayout.UpdateLayout();
	}

	private void RequestPinboardContents(bool clear)
	{
		if (clear)
		{
			_pinboardLineList.Clear();
		}
		_noData.SetActive(value: false);
		ShowLoadingRing(show: true);
		Http.RequestYml(GameManager.GatewayUrl + "/regions/" + GameManager.Region.Id + "/pinboard", delegate(PinboardLineList.ReadPinboard readPinboard)
		{
			_pinboardLineList.Refresh(readPinboard);
			_noData.SetActive(_pinboardLineList.Count == 0);
			ShowLoadingRing(show: false);
		});
	}

	private void ShowLoadingRing(bool show)
	{
		if (show)
		{
			UIManager.Popup.LoadingRing.AttachToWidget(_noData, _mainWidget.gameObject);
		}
		else
		{
			UIManager.Popup.LoadingRing.DetachFromWidget(_mainWidget.gameObject);
		}
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		if (base.IsOpened)
		{
			UpdateLayout();
		}
	}

	private void OnSubmit()
	{
		string value = _textInput.value;
		_textInput.value = string.Empty;
		if (!string.IsNullOrEmpty(value) && !(_artifact == null))
		{
			WriteToRegionPinboard writeToRegionPinboard = default(WriteToRegionPinboard);
			writeToRegionPinboard.EntityId = _artifact.EntityId;
			writeToRegionPinboard.Tile = _artifact.WorldTile;
			writeToRegionPinboard.PlayerId = GameManager.PlayerId;
			writeToRegionPinboard.Content = value;
			WriteToRegionPinboard msg = writeToRegionPinboard;
			Connections.Frontend.Send(msg).On<OK>(delegate
			{
				RequestPinboardContents(clear: false);
			});
		}
	}
}
