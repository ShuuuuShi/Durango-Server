using Durango.Logic.Clan;
using Durango.Logic.Timeline;
using UnityEngine;

namespace Durango.UI;

public class ClanTimelinePage : ClanMenuPage
{
	[SerializeField]
	private TimelineLogContainer _container;

	protected override void OnEnable()
	{
		base.OnEnable();
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			_container.SetTimeline(playerClan.Id, TimelineType.Clan);
		}
		else
		{
			_container.Clear();
		}
	}
}
