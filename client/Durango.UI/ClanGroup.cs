using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Logic.Notification;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;

namespace Durango.UI;

[Uri("Clan")]
public class ClanGroup : UIBase, INotificationable
{
	public enum ClanMenus
	{
		Invalid = -1,
		[T.EnumName("창설")]
		MakeClan,
		[T.EnumName("정보")]
		Info,
		[T.EnumName("부족관리")]
		Members,
		[T.EnumName("레벨")]
		Level,
		[T.EnumName("이력")]
		Timeline,
		[T.EnumName("동맹")]
		Ally,
		[T.EnumName("검색")]
		ClanList
	}

	[Serializable]
	[EnumType(typeof(ClanMenus))]
	private class MenuPages : EnumKeyList
	{
		[SerializeField]
		private List<ClanMenuPage> _values;

		public List<ClanMenuPage> Values => _values;

		public int IndexOf(ClanMenus menu)
		{
			return IndexOf((int)menu);
		}
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private MenuPages _menuPages;

	[SerializeField]
	private ClanMenuTabs _menuTabs;

	[SerializeField]
	private UIWidget _mainWidget;

	private bool _isWaiting;

	private Countable _notification;

	private ClanMenus _selectedMenu;

	public bool RedirectedFromAllyPage { get; private set; }

	public Notification Notification
	{
		get
		{
			if (_notification == null)
			{
				_notification = new Countable(Durango.Logic.Notification.Type.Normal);
			}
			return _notification;
		}
	}

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		_titleWidget.Object.SetTitle(T._("부족"));
		_menuTabs.Selected += SelectMenu;
		List<ClanMenuPage> values = _menuPages.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (!(values[i] == null))
			{
				AnimationWidget.Get(values[i].gameObject, 0f, 0f, deactiveWhenFadeout: true);
			}
		}
		base.OnOpenSucceed += OnOpened;
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += OnUpdateClanInfo;
		SetChildrenActive(activated: false);
	}

	public void Open(ClanMenus menu)
	{
		Open();
		SelectMenu(menu);
	}

	protected override bool TryOpen()
	{
		List<ClanMenuPage> values = _menuPages.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (!(values[i] == null))
			{
				values[i].GetComponent<AnimationWidget>().SetAlpha(0f, useTween: false);
				values[i].gameObject.SetActive(value: false);
			}
		}
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		int num = _menuPages.IndexOf(_selectedMenu);
		if (num != -1)
		{
			ClanMenuPage clanMenuPage = _menuPages.Values[num];
			if (!clanMenuPage.Back())
			{
				return false;
			}
		}
		return base.TryClose();
	}

	private void OnOpened()
	{
		_isWaiting = true;
		UIManager.Popup.LoadingRing.AttachToWidget(_mainWidget.gameObject);
		_mainWidget.alpha = 0f;
		ClanSystem.RefreshPlayerClan();
		GameSystem<ClanSystem>.Instance().GetAllySlots();
	}

	private void OnUpdateClanInfo()
	{
		UpdateBadgeCount();
		if (base.IsOpened)
		{
			if (_isWaiting)
			{
				_isWaiting = false;
				UIManager.Popup.LoadingRing.DetachFromWidget(_mainWidget.gameObject);
				_mainWidget.alpha = 1f;
				UpdatePlayerClan();
			}
			else
			{
				UpdatePlayerClan();
			}
		}
	}

	private void UpdatePlayerClan()
	{
		UpdateTabs();
		UpdateWaitingClan();
	}

	public void SearchClansForAlliance()
	{
		RedirectedFromAllyPage = true;
		SelectMenu(ClanMenus.ClanList);
		RedirectedFromAllyPage = false;
	}

	private void SelectMenu(ClanMenus menu)
	{
		_selectedMenu = menu;
		_menuTabs.SelectMenu(menu);
		int num = _menuPages.IndexOf(menu);
		HasBack.Value = num >= 0;
		List<ClanMenuPage> values = _menuPages.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (!(values[i] == null))
			{
				AnimationWidget component = values[i].GetComponent<AnimationWidget>();
				if (i == num)
				{
					component.gameObject.SetActive(value: true);
					component.Delay = 0.1f;
					component.Duration = 0.2f;
					component.Alpha = 1f;
				}
				else
				{
					component.Delay = 0f;
					component.Duration = 0.2f;
					component.Alpha = 0f;
				}
			}
		}
	}

	public void SuggestAlly(string clanId)
	{
		bool closeIfCancel = false;
		if (!base.IsOpened)
		{
			Open();
			closeIfCancel = true;
		}
		MessageBox messageBox = UIManager.MessageBox;
		double num = OptionSystem.GetAllySuggestionCoolTime();
		ClanAllyInfoWidget clanAllyInfo = messageBox.ClanAllyInfo;
		clanAllyInfo.Set(clanId, num, isDelta: true, string.Format("[FFD85B][icon=icon_information] {0}[-]", T._("같은 부족을 대상으로 {0} 이내에 동맹 신청을 할 수 없습니다.", TimedeltaFormatter.Format(num))));
		messageBox.SetCustomWidget(clanAllyInfo, MessageBox.Position.Center);
		messageBox.Show(T._("다른 부족에게 동맹을 제안하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.SuggestAlly(clanId);
				SelectMenu(ClanMenus.Ally);
			}
			else if (closeIfCancel)
			{
				Close();
			}
		}, T._("제안"));
	}

	public void SuggestAllyCancel(AllySlot slot)
	{
		UIManager.SystemMsg(T._("동맹이 제안된 상태입니다. 상대편이 결정하지 않으면 24시간 후 거절로 처리됩니다."));
	}

	public void SuggestBreak(AllySlot slot)
	{
		MessageBox messageBox = UIManager.MessageBox;
		ClanAllyInfoWidget clanAllyInfo = messageBox.ClanAllyInfo;
		clanAllyInfo.Set(slot.ClanId, null, isDelta: true, string.Format("<alert>[icon=icon_make_alert] {0}</alert>", T._("동맹 파기를 제안하면 상대 부족이 거절하기 전까지 취소할 수 없습니다.")));
		messageBox.SetCustomWidget(clanAllyInfo, MessageBox.Position.Center);
		messageBox.Show(T._("상대 부족에게 동맹 파기를 제안하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.SuggestBreak(slot.ClanId);
			}
		}, T._("동맹 파기 제안"));
	}

	public void BreakAlly(AllySlot slot)
	{
		MessageBox messageBox = UIManager.MessageBox;
		ClanAllyInfoWidget clanAllyInfo = messageBox.ClanAllyInfo;
		clanAllyInfo.Set(slot.ClanId, slot.StateExpiresAt, isDelta: false, string.Format("<alert>[icon=icon_make_alert] {0}</alert>", T._("동맹을 합의 없이 강제로 파기하면 이 동맹 슬롯은 {0} 동안 사용할 수 없게 됩니다.", TimedeltaFormatter.Format(OptionSystem.GetAllyLockedAfterBreak()))));
		messageBox.SetCustomWidget(clanAllyInfo, MessageBox.Position.Center);
		messageBox.Show(T._("상대 부족과의 동맹을 강제로 파기하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.BreakAlly(slot.ClanId);
			}
		}, T._("확정"));
	}

	public void BeenSuggestedAlly(AllySlot slot)
	{
		MessageBox messageBox = UIManager.MessageBox;
		ClanAllyInfoWidget clanAllyInfo = messageBox.ClanAllyInfo;
		clanAllyInfo.Set(slot.ClanId, slot.StateExpiresAt, isDelta: false, string.Format("[362E2B][icon=icon_information] {0}[-]", T._("{0} 이내에 응답하지 않으면 거절한 것으로 결정됩니다.", TimedeltaFormatter.Format(OptionSystem.GetAllySuggestionExpireTime()))));
		messageBox.SetCustomWidget(clanAllyInfo, MessageBox.Position.Center);
		messageBox.Show(T._("다른 부족으로부터 동맹 제안이 왔습니다.\n수락하시겠습니까?"), delegate(int index)
		{
			switch (index)
			{
			case 0:
				ClanSystem.AcceptSuggestion(slot.ClanId);
				break;
			case 1:
				ClanSystem.RefuseSuggestion(slot.ClanId);
				break;
			}
		}, new MessageBox.Button(T._("수락")), new MessageBox.Button(T._("거절")), T._("다음에 결정"));
	}

	public void BeenBreakSuggestedAlly(AllySlot slot)
	{
		MessageBox messageBox = UIManager.MessageBox;
		ClanAllyInfoWidget clanAllyInfo = messageBox.ClanAllyInfo;
		clanAllyInfo.Set(slot.ClanId, slot.StateExpiresAt, isDelta: false, string.Format("[362E2B][icon=icon_information] {0}[-]", T._("{0} 이내에 응답하지 않으면 동맹 파기를 거절한 것으로 결정됩니다.", TimedeltaFormatter.Format(OptionSystem.GetAllySuggestionExpireTime()))));
		messageBox.SetCustomWidget(clanAllyInfo, MessageBox.Position.Center);
		messageBox.Show(T._("다른 부족으로부터 동맹 파기 제안이 왔습니다.\n수락하시겠습니까?"), delegate(int index)
		{
			switch (index)
			{
			case 0:
				ClanSystem.AcceptSuggestion(slot.ClanId);
				break;
			case 1:
				ClanSystem.RefuseSuggestion(slot.ClanId);
				break;
			}
		}, new MessageBox.Button(T._("수락")), new MessageBox.Button(T._("거절")), T._("다음에 결정"));
	}

	[Uri("Join")]
	public void JoinClan(string clanId)
	{
		ClanSystem.GetClanInfo(clanId, JoinClan);
	}

	public void JoinClan([CanBeNull] Clan clan)
	{
		if (clan == null)
		{
			UIManager.SystemMsg(T._("해당 부족의 정보가 없습니다."));
			return;
		}
		if (GameSystem<ClanSystem>.Instance().PlayerClan != null)
		{
			UIManager.SystemMsg(T._("이미 부족에 가입되어 있습니다."));
			return;
		}
		Clan waitingClan = GameSystem<ClanSystem>.Instance().WaitingClan;
		string mainText;
		if (waitingClan != null)
		{
			if (waitingClan.Id == clan.Id)
			{
				UIManager.SystemMsg(T._("이미 가입 신청된 부족입니다."));
				return;
			}
			mainText = T._("<em>{0}</em> 부족에 가입을 신청하면\n<em>{1}</em> 부족의 가입 신청은 취소됩니다.\n신청하시겠습니까?", clan.Name, waitingClan.Name);
		}
		else
		{
			mainText = T._("<em>{0}</em> 부족에 가입 신청 하시겠습니까?", clan.Name);
		}
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.JoinClan(clan);
			}
		});
	}

	private void UpdateBadgeCount()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
		int count = 0;
		if (playerClan != null && !string.IsNullOrEmpty(clan.ClanId) && playerClan.TryGetRole(clan.RoleId, out var role) && (role.GetPermissions() & Permissions.ApproveMember) != 0)
		{
			count = KUtility.GetSize(playerClan.Appliers);
		}
		Notification.Count = count;
	}

	private void UpdateWaitingClan()
	{
		if (_selectedMenu != ClanMenus.ClanList)
		{
			return;
		}
		int num = _menuPages.IndexOf(ClanMenus.ClanList);
		if (num != -1)
		{
			ClanListPage clanListPage = _menuPages.Values[num] as ClanListPage;
			if (!(clanListPage == null))
			{
				clanListPage.UpdateWaitingClan(GameSystem<ClanSystem>.Instance().WaitingClan);
			}
		}
	}

	private void UpdateTabs()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		ClanMenus[] array = ((playerClan == null) ? new ClanMenus[2]
		{
			ClanMenus.MakeClan,
			ClanMenus.ClanList
		} : new ClanMenus[6]
		{
			ClanMenus.Info,
			ClanMenus.Members,
			ClanMenus.Level,
			ClanMenus.Timeline,
			ClanMenus.ClanList,
			ClanMenus.Ally
		});
		_menuTabs.Set(array);
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == _selectedMenu)
			{
				num = i;
				break;
			}
		}
		SelectMenu((num != -1) ? array[num] : array[0]);
	}
}
