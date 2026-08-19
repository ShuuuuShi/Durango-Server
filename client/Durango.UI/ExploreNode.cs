using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Party;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class ExploreNode<T> : MonoBehaviour where T : class
{
	public Action<T> Clicked;

	[SerializeField]
	protected UILabel _nameLabel;

	[SerializeField]
	protected UISprite _biomeSprite;

	[SerializeField]
	protected GameObject _newMarker;

	[SerializeField]
	protected UISprite _seasonEmblem;

	[CanBeNull]
	[SerializeField]
	protected UIWidget _memberListParent;

	[CanBeNull]
	[SerializeField]
	protected ListObjectPool _memberList;

	[CanBeNull]
	[SerializeField]
	protected GameObject _storyEmblem;

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked(this as T);
		}
	}

	protected virtual void SetMarkers()
	{
	}

	protected void OnEnable()
	{
		GameSystem<PartySystem>.Instance().PartierStatusUpdated += SetMarkers;
	}

	protected void OnDisable()
	{
		GameSystem<PartySystem>.Instance().PartierStatusUpdated -= SetMarkers;
	}

	protected void Awake()
	{
		if (_memberList != null && !(_memberListParent == null))
		{
			_memberList.Init(null, _memberListParent.transform);
		}
	}

	protected void ShowPartyMembersMarker([CanBeNull] IEnumerable<Member> partyMembers)
	{
		if (_memberList == null || _memberListParent == null)
		{
			return;
		}
		if (partyMembers == null || !partyMembers.Any())
		{
			_memberListParent.gameObject.SetActive(value: false);
			return;
		}
		_memberListParent.gameObject.SetActive(value: true);
		_memberList.BeginLoad();
		foreach (Member partyMember in partyMembers)
		{
			if (partyMember.IsAccepted && !partyMember.IsOffline && partyMember.PlayerInfo != null && !(partyMember.PlayerInfo.EntityId == GameManager.PlayerId))
			{
				ExploreAreaPartyInfo component = _memberList.GetNext().GetComponent<ExploreAreaPartyInfo>();
				int memberIndex = GameSystem<PartySystem>.Instance().GetMemberIndex(partyMember);
				component.Set(memberIndex, partyMember.IsLeader);
			}
		}
		_memberList.EndLoad();
		float num = UIUtility.WidgetsReposition(_memberList, Vector3.right, _memberListParent.localCenter, 1f);
		_memberListParent.transform.localPosition = new Vector3((0f - num) / 2f, _memberListParent.transform.localPosition.y, 0f);
	}
}
