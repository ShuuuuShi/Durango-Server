using UnityEngine;

namespace Durango.UI;

public class ExploreAreaPartyInfo : MonoBehaviour
{
	[SerializeField]
	private UISprite _memberIndex;

	[SerializeField]
	private GameObject _partyLeaderMarker;

	public void Set(int memberIndex, bool isPartyLeader)
	{
		_memberIndex.spriteName = $"num_{memberIndex + 1}";
		_partyLeaderMarker.SetActive(isPartyLeader);
	}
}
