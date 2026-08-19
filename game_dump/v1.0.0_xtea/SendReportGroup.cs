using System;
using ClanData;
using L10N;
using Player;
using Shared.System;
using UnityEngine;

public class SendReportGroup : UIBase
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private UILabel _labelTitle;

	[SerializeField]
	private UILabel _labelTargetText;

	[SerializeField]
	private UILabel _labelReason;

	[SerializeField]
	private UIInput _inputReason;

	[SerializeField]
	private DefaultSelectableButton _buttonSend;

	[SerializeField]
	private DefaultSelectableButton _buttonCancel;

	private void Awake()
	{
		DefaultSelectableButton buttonSend = _buttonSend;
		buttonSend.Clicked = (Action)Delegate.Combine(buttonSend.Clicked, (Action)delegate
		{
			GameSystem<SendReportSystem>.Instance().SendReport(_inputReason.value);
			_inputReason.value = string.Empty;
			Close();
		});
		DefaultSelectableButton buttonCancel = _buttonCancel;
		buttonCancel.Clicked = (Action)Delegate.Combine(buttonCancel.Clicked, (Action)delegate
		{
			_inputReason.value = string.Empty;
			Close();
		});
		_container.gameObject.SetActive(false);
	}

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SendReport, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if ((Object)(object)targetComponent != (Object)null)
			{
				if (targetComponent.ArtifactState.Scribble.HasValue)
				{
					OpenForScribble(targetComponent);
				}
				else if (targetComponent.ArtifactState.ChangedName != null)
				{
					OpenForNameable(targetComponent);
				}
				else if (((Object)targetComponent).name == "clan_warehouse")
				{
					OpenForWarehouse(targetComponent);
				}
			}
		});
	}

	private void OnEnable()
	{
		GameSystem<SendReportSystem>.Instance().ResponseReceived += SendReportGroup_ResponseReceived;
	}

	private void OnDisable()
	{
		GameSystem<SendReportSystem>.Instance().ResponseReceived -= SendReportGroup_ResponseReceived;
	}

	public void OpenForPlayer(PlayerInfo playerInfo)
	{
		Open(SendReportSystem.ReportType.Players, playerInfo.EntityId, T._("플레이어 신고"), string.Format(T._("{0} [71716B]({1:0000} KHZ)[-]"), playerInfo.Name, playerInfo.Freq));
	}

	public void OpenForScribble(Artifact artifact)
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(artifact.ArtifactState.Scribble.Value.Scribbler, delegate(PlayerInfo info)
		{
			if (info.Valid)
			{
				Open(SendReportSystem.ReportType.Scribbles, artifact.EntityId, T._("그림 신고"), LocalizeSystem.Format("#send_report_target_artifact", info.Name, artifact.LocalizedName));
			}
		});
	}

	public void OpenForNameable(Artifact artifact)
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(artifact.FounderId, delegate(PlayerInfo info)
		{
			if (info.Valid)
			{
				Open(SendReportSystem.ReportType.Nameables, artifact.EntityId, T._("이름 신고"), LocalizeSystem.Format("#send_report_target_artifact", info.Name, artifact.LocalizedName));
			}
		});
	}

	public void OpenForWarehouse(Artifact artifact)
	{
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(artifact.FounderId, delegate(PlayerInfo info)
		{
			if (info.Valid)
			{
				Open(SendReportSystem.ReportType.Nameables, artifact.EntityId, T._("부족 창고 신고"), LocalizeSystem.Format("#send_report_target_artifact", info.Name, artifact.LocalizedName));
			}
		});
	}

	public void OpenForClan(Clan clan)
	{
		Open(SendReportSystem.ReportType.Clans, clan.Id, T._("부족 신고"), LocalizeSystem.Format("#send_report_target_clans", clan.Name));
	}

	private void Open(SendReportSystem.ReportType type, ulong entityId, string title, string targetName)
	{
		SendReportSystem sendReportSystem = GameSystem<SendReportSystem>.Instance();
		int characterLimit = _inputReason.characterLimit;
		UIUtility.SetLabelText(_labelTitle, title);
		UIUtility.SetLabelText(_labelTargetText, targetName);
		UIUtility.SetLabelText(_labelReason, LocalizeSystem.Format("#send_report_reason", characterLimit.ToString()));
		if (entityId != sendReportSystem.EntityId)
		{
			_inputReason.value = string.Empty;
		}
		sendReportSystem.SetTarget(type, entityId);
		Open();
	}

	protected override bool OnOpen()
	{
		_container.gameObject.SetActive(true);
		return true;
	}

	protected override bool OnClose()
	{
		_container.gameObject.SetActive(false);
		return true;
	}

	private void SendReportGroup_ResponseReceived(SendReportSystem.Response response)
	{
		string comment = T._("신고를 접수하지 못했습니다. 통신 상태를 확인해 주세요.");
		switch (response)
		{
		case SendReportSystem.Response.Done:
			comment = T._("신고를 접수했습니다.");
			break;
		case SendReportSystem.Response.BadRequest:
			comment = T._("신고 사유를 적어주세요.");
			break;
		case SendReportSystem.Response.Conflict:
			comment = T._("신고 회수를 초과했습니다.");
			break;
		}
		UIManager.SystemMsg(comment);
	}
}
