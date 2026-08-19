using Durango.Logic.Faction;
using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Faction")]
public class FactionGroup : UIBase, INotificationable
{
	public enum Mode
	{
		Summary,
		History,
		SupportRequest
	}

	public static FactionType[] FactionOrder = new FactionType[7]
	{
		FactionType.SubStory,
		FactionType.RescueTf,
		FactionType.TheFirm,
		FactionType.ChlorophylForum,
		FactionType.ChamberOfPioneer,
		FactionType.TheCommittee,
		FactionType.Lama
	};

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private FactionSummaryPage _summaryPage;

	[SerializeField]
	private FactionTalksPage _factionTalksPage;

	[SerializeField]
	private FactionSupportRequestPage _supportRequestPage;

	private int _sentSupportRequestCount;

	private readonly Toggle _notification = new Toggle(Type.Normal);

	private Mode _currentMode;

	public Mode CurrentMode
	{
		get
		{
			return _currentMode;
		}
		private set
		{
			_currentMode = value;
			HasBack.Value = _currentMode != Mode.Summary;
		}
	}

	public Notification Notification => _notification;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Faction;
		GameSystem<FactionSystem>.Instance().FactionPointChanged += OnChangeFactionPoint;
		_summaryPage.TalksClicked += delegate(FactionType type)
		{
			ShowHistoryPage(type, null);
		};
		_summaryPage.SupportRequestClicked += ShowSupportRequestPage;
		_titleWidget.Object.SetTitle(T._("지원 단체"));
		GameSystem<FactionSystem>.Instance().FactionsUpdated += OnUpdateFactions;
		GameSystem<FactionSystem>.Instance().SupportRequestAvailableChanged += OnSupportRequestAvailableChanged;
		GameSystem<FactionSystem>.Instance().SupportRewardsAccepted += OnSupportRewardsAccepted;
		base.OnOpenSucceed += OnOpenSuceed;
		base.TryClose();
	}

	private void Update()
	{
		if (base.IsOpened && _sentSupportRequestCount < 3 && GameSystem<FactionSystem>.Instance().GetSupportRequestsSucceeded)
		{
			CheckSupportRequest();
		}
	}

	private void CheckSupportRequest()
	{
		if (GameSystem<FactionSystem>.Instance().CheckSupportRequests())
		{
			_sentSupportRequestCount++;
		}
	}

	private void OnOpenSuceed()
	{
		GameSystem<FactionSystem>.Instance().RequestFactions();
		CheckSupportRequest();
		_sentSupportRequestCount = 0;
		Refresh();
	}

	private void OnUpdateFactions()
	{
		if (base.IsOpened)
		{
			Refresh();
		}
		UpdateNotification();
	}

	private void Refresh()
	{
		_summaryPage.Refresh();
		_factionTalksPage.Refresh();
		_supportRequestPage.Refresh();
	}

	private void OnSupportRequestAvailableChanged()
	{
		UpdateNotification();
	}

	private void UpdateNotification()
	{
		bool flag = false;
		foreach (Durango.Logic.Faction.Faction faction in GameSystem<FactionSystem>.Instance().GetFactions())
		{
			if (faction.IsAvailable())
			{
				flag |= faction.IsSupportRequestAvailable();
				flag |= faction.GetTalkNotification();
			}
		}
		_notification.BeginSetting();
		_notification.Type = Type.Normal;
		_notification.On = flag;
		_notification.EndSetting();
	}

	public Transform GetSupportAvailableButtonTransform()
	{
		return _summaryPage.GetSupportAvailableButtonTransform();
	}

	public Transform GetRequestAvailableButtonTransform()
	{
		return _supportRequestPage.GetRequestAvailableButtonTransform();
	}

	public void OpenSupportRequestPage(FactionType factionType)
	{
		Open();
		ShowSupportRequestPage(factionType);
	}

	public void OpenTalksPage(FactionType factionType, Talks talks = null)
	{
		Open();
		ShowHistoryPage(factionType, talks);
	}

	public override bool Open()
	{
		bool result = base.Open();
		ShowMainPage();
		return result;
	}

	protected override bool TryClose()
	{
		switch (CurrentMode)
		{
		case Mode.History:
			if (_factionTalksPage.Back())
			{
				ShowMainPage();
			}
			return false;
		case Mode.SupportRequest:
			ShowMainPage();
			return false;
		default:
			return base.TryClose();
		}
	}

	private void ShowMainPage()
	{
		CurrentMode = Mode.Summary;
		_summaryPage.Show();
		_factionTalksPage.Hide();
		_supportRequestPage.Hide();
	}

	private void ShowHistoryPage(FactionType type, Talks talks)
	{
		CurrentMode = Mode.History;
		_summaryPage.Hide();
		_factionTalksPage.Show(type, talks);
		_supportRequestPage.Hide();
	}

	private void ShowSupportRequestPage(FactionType type)
	{
		CurrentMode = Mode.SupportRequest;
		_summaryPage.Hide();
		_factionTalksPage.Hide();
		_supportRequestPage.Show(type);
	}

	private void OnChangeFactionPoint(Durango.Logic.Faction.Faction faction, int diff)
	{
		Talks[] array = SingletonDict<FactionType, Talks[]>.Get(faction.Type);
		int i = 0;
		for (int size = KUtility.GetSize(array); i < size; i++)
		{
			Talks talk = array[i];
			if (KUtility.GetSize(talk.List) == 0 || faction.Point - diff >= talk.FriendshipPoint || talk.FriendshipPoint > faction.Point)
			{
				continue;
			}
			if (talk.NoticeType == TalkType.Modal)
			{
				UIManager.FindScript<DialogueGroupBase>().AddFactionTalks(talk);
				continue;
			}
			AlarmGroup alarmGroup = UIManager.FindScript<AlarmGroup>();
			string text = talk.Title;
			if (text.Length > 20)
			{
				text = text.Substring(0, 20) + "...";
			}
			alarmGroup.ShowNotify(text, "alarm_memo", major: true, 1.8f, delegate
			{
				OpenTalksPage(faction.Type, talk);
			});
		}
	}

	private static void OnSupportRewardsAccepted(AcceptedSupportRewards rewards)
	{
		UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowAcceptedSupportRewards(rewards);
	}

	public static string FactionTalksToString(Talk talk)
	{
		if (talk.Target.HasValue)
		{
			string arg = ((talk.Target.Value != 0) ? ((string)SingletonDict<Shared.Faction.Messenger, Yaml.Messenger>.Get(talk.Target.Value).Name) : PlayerBehavior.LocalPlayer.PlayerName);
			return $"<em>@{arg}</em> {talk.Message}";
		}
		return talk.Message.ToString();
	}
}
