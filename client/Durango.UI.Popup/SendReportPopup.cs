using System;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class SendReportPopup : TooltipBase
{
	private enum Step
	{
		None,
		SelectReason,
		InputReport,
		Sending
	}

	[SerializeField]
	private UILabel _textTitle;

	[SerializeField]
	private GameObject _reportTarget;

	[SerializeField]
	private UILabel _textTargetName;

	[SerializeField]
	private GameObject _serverInfo;

	[SerializeField]
	private UILabel _serverInfoLabel;

	[SerializeField]
	private GameObject _selectReason;

	[SerializeField]
	private KWidgetScrollView _reasonScrollVIew;

	[SerializeField]
	private UILabel _labelSelectReason;

	[SerializeField]
	private SendReportReasonWidget _reasonWidget;

	[SerializeField]
	private GameObject _inputPane;

	[SerializeField]
	private GameObject _inputReport;

	[SerializeField]
	private UILabel _labelInquiry;

	[SerializeField]
	private UIWidget _backButton;

	[SerializeField]
	private UILabel _textReason;

	[SerializeField]
	private UILabel _textCharCount;

	[SerializeField]
	private UIInput _inputText;

	[SerializeField]
	private UILabel _textForInput;

	[SerializeField]
	private KWidgetScrollView _inputSrollView;

	[SerializeField]
	private SelectableButton _buttonSend;

	[SerializeField]
	private SelectableButton _buttonCancel;

	private ListObjectPool<SendReportReasonWidget> _reasonWidgets = new ListObjectPool<SendReportReasonWidget>();

	private int _heightTextForInput;

	private Step _currentStep;

	private string _title;

	private string _entityid;

	private string _targetName;

	private SendReportSystem.ReportType _reportType;

	private SendReportSystem.PlayerReportCategory _category;

	public override bool DragLock => true;

	public void SetForPlayer(PlayerInfo playerInfo)
	{
		_title = T._("플레이어 신고");
		_entityid = playerInfo.EntityId;
		_targetName = playerInfo.GetNameFreq(21, "FFFFFF7F");
		_reportType = SendReportSystem.ReportType.Players;
	}

	public void SetForClan(Clan clan)
	{
		_title = T._("부족 신고");
		_entityid = clan.Id;
		_targetName = T._("{0} 부족", clan.Name);
		_reportType = SendReportSystem.ReportType.Clans;
	}

	public void SetForScribbles(string playerName, Artifact artifact)
	{
		_title = T._("그림 신고");
		_entityid = artifact.EntityId;
		_targetName = T._("{0}의 {1}", playerName, artifact.LocalizedName);
		_reportType = SendReportSystem.ReportType.Scribbles;
	}

	public void SetForArtifactName(string playerName, Artifact artifact, bool clanWarehouse)
	{
		_title = ((!clanWarehouse) ? T._("이름 신고") : T._("부족 창고 신고"));
		_entityid = artifact.EntityId;
		_targetName = T._("{0}의 {1}", playerName, artifact.LocalizedName);
		_reportType = SendReportSystem.ReportType.Nameables;
	}

	public void SetForServerStatus()
	{
		_title = T._("튕김(끊김)");
		_entityid = string.Empty;
		_targetName = string.Empty;
		_reportType = SendReportSystem.ReportType.ServerStatus;
	}

	public void SetForSuggestion()
	{
		_title = T._("건의");
		_entityid = string.Empty;
		_targetName = string.Empty;
		_reportType = SendReportSystem.ReportType.Suggestion;
	}

	protected override void Start()
	{
		base.Start();
		EventDelegate.Set(_inputText.onChange, OnInputTextChanged);
		UIEventListener uIEventListener = UIEventListener.Get(_backButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickBackButton));
		_reasonWidgets.BaseObject = _reasonWidget;
		_reasonWidgets.UseBase = true;
		_reasonWidgets.BeginLoad();
		AddReasonWidget();
		AddReasonWidget(SendReportSystem.PlayerReportCategory.ImproperName, T._("부적절한 캐릭터/부족명"));
		AddReasonWidget(SendReportSystem.PlayerReportCategory.Insult, T._("욕설, 음란한 내용 등으로 인해 불쾌합니다"));
		AddReasonWidget(SendReportSystem.PlayerReportCategory.Cheating, T._("부정 행위의 소지가 있습니다"));
		AddReasonWidget(SendReportSystem.PlayerReportCategory.Spam, T._("광고입니다"));
		AddReasonWidget(SendReportSystem.PlayerReportCategory.Etc, T._("기타"));
		_reasonWidgets.EndLoad();
		SelectableButton buttonSend = _buttonSend;
		buttonSend.Clicked = (Action)Delegate.Combine(buttonSend.Clicked, new Action(Send));
		SelectableButton buttonCancel = _buttonCancel;
		buttonCancel.Clicked = (Action)Delegate.Combine(buttonCancel.Clicked, new Action(Hide));
	}

	protected override void FillData()
	{
		_textTitle.text = _title;
		_textTargetName.text = _targetName;
		bool flag = _reportType != SendReportSystem.ReportType.ServerStatus && _reportType != SendReportSystem.ReportType.Suggestion;
		_reportTarget.SetActive(flag);
		_serverInfo.SetActive(!flag);
		_inputText.value = string.Empty;
		switch (_reportType)
		{
		case SendReportSystem.ReportType.ServerStatus:
			_serverInfoLabel.text = T._("* 게임 이용 중 발생한 튕김/끊김 현상에 대해서 제보해 주세요.\n<em>* 제보창구로 접수해주신 내용에는 별도의 답변을 드리지 않습니다.</em>\n* 결제/계정/복구 문의는 [고객센터-결제/기타]를 통해 접수해 주세요.");
			_inputText.defaultText = T._("접수 시 개인 정보(이름, 전화번호, 이메일 등)가 포함될 경우 개인 정보 보호를 위해 내용이 삭제될 수 있습니다.\n\n<em>1. 사용하시는 휴대 기기 및 OS 정보를 입력해주세요.\n2.겪으신 현상을 자세하게 기재해주세요.</em>\nex) 사유지 메뉴에서 '부족 영토로 귀환'을 터치하자 앱이 종료되었어요.");
			break;
		case SendReportSystem.ReportType.Suggestion:
			_serverInfoLabel.text = T._("* 접수해주신 건의는 게임 서비스 개선을 위해 사용됩니다.\n<em>* 제보창구로 접수해주신 내용에는 별도의 답변을 드리지 않습니다.</em>\n* 결제/계정/복구 문의는 [고객센터-결제/기타]를 통해 접수해 주세요.");
			_inputText.defaultText = T._("접수 시 개인 정보(이름, 전화번호, 이메일 등)가 포함될 경우 개인 정보 보호를 위해 내용이 삭제될 수 있습니다.\n\n<em>건의하고자 하는 내용을 상세히 적어주세요.\n개선 내역은 별도 공지나 업데이트 내용을 통해 확인하실 수 있습니다.</em>");
			break;
		default:
			_inputText.defaultText = T._("신고 사유를 자세히 입력해주세요.");
			break;
		}
		_buttonSend.Text = ((!flag) ? T._("접수") : T._("신고"));
		SetCurrentStep(flag ? Step.SelectReason : Step.InputReport);
	}

	private void SetCurrentStep(Step step)
	{
		_currentStep = step;
		switch (_currentStep)
		{
		case Step.SelectReason:
			_reasonScrollVIew.ResetPosition();
			break;
		case Step.InputReport:
		{
			bool flag = _reportType != SendReportSystem.ReportType.ServerStatus && _reportType != SendReportSystem.ReportType.Suggestion;
			_labelInquiry.gameObject.SetActive(!flag);
			_backButton.gameObject.SetActive(flag);
			_textReason.gameObject.SetActive(flag);
			RefreshInputScrollView();
			RefreshCharCountText();
			break;
		}
		case Step.Sending:
			ShowLoadingRing(show: true);
			break;
		}
		RefreshInputPane();
		RefreshSendButton();
	}

	private void AddReasonWidget(SendReportSystem.PlayerReportCategory category = SendReportSystem.PlayerReportCategory.None, string textReason = null)
	{
		if (string.IsNullOrEmpty(textReason))
		{
			_reasonScrollVIew.Widgets.Add(_labelSelectReason);
			return;
		}
		SendReportReasonWidget next = _reasonWidgets.GetNext();
		next.Set(category, textReason);
		next.ReasonClicked = ReasonWidgetClicked;
		_reasonScrollVIew.Widgets.Add(next.Widget);
	}

	private void RefreshInputScrollView()
	{
		_inputSrollView.UpdateLayout();
		_inputSrollView.MoveTo(_inputSrollView.ContentsLength, instant: false);
		_heightTextForInput = _textForInput.height;
	}

	private void RefreshCharCountText()
	{
		_textCharCount.text = $"<em>{_inputText.value.Length}</em> / {_inputText.characterLimit}";
	}

	private void RefreshInputPane()
	{
		_selectReason.SetActive(_currentStep == Step.SelectReason);
		_inputReport.SetActive(_currentStep == Step.InputReport);
	}

	private void RefreshSendButton()
	{
		_buttonSend.Disabled = _currentStep == Step.Sending || string.IsNullOrEmpty(_inputText.value);
	}

	private void Send()
	{
		string value = _inputText.value;
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		switch (_reportType)
		{
		case SendReportSystem.ReportType.ServerStatus:
			GameSystem<SendReportSystem>.Instance().SendServerStatus(value, "서버상황제보", delegate(bool result)
			{
				string resultText = ((!result) ? null : T._("제보해 주셔서 감사합니다.\n서버 상황 제보 외 문의(결제, 플레이 등)는 게임 내 고객센터 1:1 문의를 통해 접수해 주세요."));
				HideWithResultMsg(resultText);
			});
			break;
		case SendReportSystem.ReportType.Suggestion:
			GameSystem<SendReportSystem>.Instance().SendServerStatus(value, "건의/신고하기", delegate(bool result)
			{
				string resultText2 = ((!result) ? null : T._("제보해 주셔서 감사합니다."));
				HideWithResultMsg(resultText2);
			});
			break;
		default:
			GameSystem<SendReportSystem>.Instance().SendReport(_reportType, _entityid, _category, value, delegate(SendReportSystem.Response response)
			{
				string resultText3 = null;
				switch (response)
				{
				case SendReportSystem.Response.Done:
					resultText3 = T._("신고를 접수했습니다.");
					break;
				case SendReportSystem.Response.BadRequest:
					resultText3 = T._("신고 사유를 적어주세요.");
					break;
				case SendReportSystem.Response.NotFound:
					resultText3 = T._("대상을 확인할 수 없습니다.");
					break;
				case SendReportSystem.Response.Conflict:
					resultText3 = T._("신고 횟수를 초과했습니다.");
					break;
				}
				HideWithResultMsg(resultText3);
			});
			break;
		}
		SetCurrentStep(Step.Sending);
	}

	private void ShowLoadingRing(bool show)
	{
		LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
		if (show)
		{
			loadingRing.AttachToWidget(_inputPane);
			loadingRing.ShowInstantly();
		}
		else
		{
			loadingRing.DetachFromWidget(_inputPane);
		}
	}

	private void HideWithResultMsg([CanBeNull] string resultText)
	{
		UIManager.SystemMsg((!string.IsNullOrEmpty(resultText)) ? resultText : T._("내용을 접수하지 못했습니다. 통신 상태를 확인해 주세요."), 4f);
		ShowLoadingRing(show: false);
		Hide();
	}

	private void OnInputTextChanged()
	{
		if (_heightTextForInput != _textForInput.height)
		{
			RefreshInputScrollView();
		}
		RefreshCharCountText();
		RefreshSendButton();
	}

	private void OnClickBackButton(GameObject obj)
	{
		SetCurrentStep(Step.SelectReason);
	}

	private void ReasonWidgetClicked(SendReportSystem.PlayerReportCategory category, string text)
	{
		_textReason.text = text;
		_category = category;
		SetCurrentStep(Step.InputReport);
	}
}
