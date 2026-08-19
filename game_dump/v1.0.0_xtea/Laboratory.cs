using System;
using K1Network;
using Messages;
using Yaml;
using Yaml.Util;

public class Laboratory : ArtifactComponent
{
	public string ResearchId { get; private set; }

	public double ResearchSince { get; private set; }

	public double ResearchUntil { get; private set; }

	public bool GetNowResearching()
	{
		if (ResearchId != null)
		{
			return ResearchUntil > Connections.Frontend.GetPredictedServerTime();
		}
		return false;
	}

	public override void OnCompleted()
	{
		RefreshResearchState();
	}

	public void RefreshResearchState(Action callback = null)
	{
		Connections.Frontend.Send(new GetResearchState
		{
			EntityId = base.EntityId,
			Tile = base.WorldTile
		}).On(delegate(ResearchState msg, PacketHeader header)
		{
			ResearchId = msg.ResearchId;
			ResearchSince = msg.ResearchStartAt;
			ResearchUntil = msg.ResearchStartAt + (double)GetDuration(msg.ResearchId);
			if (callback != null)
			{
				callback();
			}
		});
	}

	private static int GetDuration(string researchId)
	{
		if (researchId != null)
		{
			ClanResearch clanResearch = SingletonDict<string, ClanResearch>.Get(researchId);
			if (clanResearch != null)
			{
				return clanResearch.duration;
			}
		}
		return 0;
	}
}
