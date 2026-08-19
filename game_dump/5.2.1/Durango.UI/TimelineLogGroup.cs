using System;
using Durango.Logic.Timeline;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class TimelineLogGroup : UIBase
{
	private enum PlayerTab
	{
		[T.EnumName("전체")]
		All,
		[T.EnumName("재화")]
		Money
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private SelectableWidget _timelinePushButton;

	[SerializeField]
	private TimelineLogContainer _container;

	[SerializeField]
	private NestedPrefabLinker _iconTabListLinker;

	[SerializeField]
	private RectLayoutComponent _layout;

	private IconTabList _iconTabList;

	private TimelineOption? _option;

	private string _lastEntityId;

	private TimelineType _lastType;

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetTimeline, delegate(InteractionObject target)
		{
			if (InteractionSystem.CurrentMenu.Disabled)
			{
				UIManager.SystemMsg(T._("이력을 볼 수 있는 권한이 없습니다."));
			}
			else
			{
				Artifact targetComponent = target.GetTargetComponent<Artifact>();
				if (targetComponent != null)
				{
					OpenForArtifact(targetComponent);
				}
			}
		});
		SelectableWidget timelinePushButton = _timelinePushButton;
		timelinePushButton.Clicked = (Action)Delegate.Combine(timelinePushButton.Clicked, new Action(OnClickPushState));
		_iconTabList = _iconTabListLinker.Object.GetComponent<IconTabList>();
		_iconTabList.Clicked += IconTabList_Clicked;
		SetChildrenActive(activated: false);
	}

	public override bool Open()
	{
		if (SetPlayer(PlayerBehavior.LocalPlayer.EntityId))
		{
			return base.Open();
		}
		return false;
	}

	public void OpenForPlayer(string entityId)
	{
		if (SetPlayer(entityId))
		{
			base.Open();
		}
	}

	public void OpenForArtifact([NotNull] Artifact artifact)
	{
		if (SetArtifact(artifact))
		{
			base.Open();
		}
	}

	public void OpenForEstate(EstateLicense license)
	{
		if (SetEstate(license))
		{
			base.Open();
		}
	}

	private bool SetArtifact(Artifact artifact)
	{
		if (artifact == null)
		{
			return false;
		}
		SetTimelineOption(show: false);
		SetArtifactTitle(artifact);
		ShowTabList(showTab: false);
		SetTimeline(artifact.EntityId, TimelineType.Entity);
		return true;
	}

	private bool SetPlayer(string entityId)
	{
		if (string.IsNullOrEmpty(entityId))
		{
			return false;
		}
		SetTimelineOption(show: false);
		SetPlayerTitle(entityId);
		ShowTabList(showTab: true);
		_iconTabList.BeginLoad();
		PlayerTab[] array = Enums<PlayerTab>.All();
		foreach (PlayerTab playerTab in array)
		{
			_iconTabList.Add(null, playerTab.GetName());
		}
		_iconTabList.EndLoad();
		_iconTabList.Select(0);
		SetTimeline(entityId, TimelineType.Player);
		return true;
	}

	private bool SetEstate(EstateLicense license)
	{
		TimelineType type;
		switch (license.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			SetTimelineOption(show: true);
			type = TimelineType.Estate;
			break;
		case OwnerType.ClanEstate:
			SetTimelineOption(show: false);
			type = TimelineType.ClanEstate;
			break;
		default:
			return false;
		}
		SetEstateTitle(license);
		ShowTabList(showTab: false);
		SetTimeline(license.EstateId, type);
		return true;
	}

	private void SetTimeline(string entityId, TimelineType type)
	{
		_lastEntityId = entityId;
		_lastType = type;
		_container.SetTimeline(_lastEntityId, _lastType);
	}

	private void SetPlayerTitle([NotNull] string entityId)
	{
		_titleWidget.Object.SetTitle(T._("이력"));
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				_titleWidget.Object.SetTitle(T._("{0}의 이력", info.Name));
			}
		});
	}

	private void SetArtifactTitle(Artifact artifact)
	{
		_titleWidget.Object.SetTitle(T._("{0}의 이력", artifact.LocalizedName));
	}

	private void SetEstateTitle(EstateLicense license)
	{
		switch (license.Type)
		{
		case OwnerType.PersonalPlayer:
			_titleWidget.Object.SetTitle(T._("개인섬 사유지의 이력"));
			break;
		case OwnerType.Player:
			_titleWidget.Object.SetTitle(T._("안정섬 사유지의 이력"));
			break;
		case OwnerType.ClanEstate:
			_titleWidget.Object.SetTitle(T._("부족 영토의 이력"));
			break;
		case OwnerType.System:
		case OwnerType.ClanWarphole:
			break;
		}
	}

	private void SetTimelineOption(bool show)
	{
		_option = null;
		if (show)
		{
			_timelinePushButton.Widget.alpha = 0f;
			_timelinePushButton.gameObject.SetActive(value: true);
			TimelineLogList.GetOption(OnTimelineOption);
		}
		else
		{
			_timelinePushButton.gameObject.SetActive(value: false);
		}
	}

	private void OnTimelineOption(TimelineOption option)
	{
		_option = option;
		_timelinePushButton.Widget.alpha = 1f;
		_timelinePushButton.Selected = option.EstateNotification;
	}

	private void ShowTabList(bool showTab)
	{
		_iconTabListLinker.gameObject.SetActive(showTab);
		_layout.UpdateLayout();
	}

	private void OnClickPushState()
	{
		TimelineOption? option = _option;
		if (!option.HasValue)
		{
			return;
		}
		TimelineOption op = _option.Value;
		op.EstateNotification = !op.EstateNotification;
		_timelinePushButton.Disabled = true;
		TimelineLogList.SetOption(op, delegate(bool success)
		{
			if (success)
			{
				_timelinePushButton.Disabled = false;
				_timelinePushButton.Selected = op.EstateNotification;
				_option = op;
			}
		});
	}

	private void IconTabList_Clicked(int index)
	{
		_iconTabList.Select(index);
		_container.SetTimeline(_lastEntityId, _lastType, (index != 0) ? index.ToString() : null);
	}
}
