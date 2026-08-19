using ClanData;
using UnityEngine;

public class ClanTimelinePage : MonoBehaviour
{
	[SerializeField]
	private TimelineLogContainer _container;

	private void Awake()
	{
		_container.Init();
	}

	private void OnEnable()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			_container.SetTimeline(playerClan.Id, TimelineLogSystem.TimelineType.Clan);
		}
		else
		{
			_container.Clear();
		}
	}
}
