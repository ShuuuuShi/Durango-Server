using System;
using Durango.Logic;
using Durango.Logic.Party;
using UnityEngine;

namespace Durango.UI;

public class PartyHudControl : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private ListObjectPool _memberPools;

	[SerializeField]
	private float _memberRepositionMargin;

	public event Action HudVisibilityChanged;

	void IUIInitializable.Init()
	{
		GameSystem<PartySystem>.Instance().MembersUpdated += Refresh;
		GameSystem<PartySystem>.Instance().ShowPartyHudChanged += Refresh;
		_memberPools.Init(delegate(GameObject o)
		{
			UIEventListener.Get(o).onClick = delegate
			{
				MenuHelper.Open(MenuType.Party);
			};
		});
		base.gameObject.SetActive(value: false);
	}

	private void Refresh()
	{
		PartySystem partySystem = GameSystem<PartySystem>.Instance();
		bool flag = !partySystem.NotInParty && !partySystem.IsInvited && partySystem.ShowPartyHud;
		bool num = base.gameObject.activeSelf != flag;
		base.gameObject.SetActive(flag);
		if (num && this.HudVisibilityChanged != null)
		{
			this.HudVisibilityChanged();
		}
		if (flag)
		{
			_memberPools.BeginLoad();
			int memberCount = partySystem.MemberCount;
			for (int i = 0; i < memberCount; i++)
			{
				Member member = partySystem.GetMember(i);
				_memberPools.GetNext().GetComponent<PartyHudPlayerWidget>().Set(member, i + 1);
			}
			_memberPools.EndLoad();
			UIUtility.WidgetsReposition(_memberPools, Vector3.down, _memberRepositionMargin);
		}
	}
}
