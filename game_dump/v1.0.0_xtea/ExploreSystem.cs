using System;
using ExploreData;
using K1Network;
using L10N;
using Messages;
using Shared.System;

public class ExploreSystem : GameSystem<ExploreSystem>
{
	public static ulong LastFoundRegion;

	public event Action<ExploreData.Region> FoundRegion;

	public event Action<Routes> RoutesUpdated;

	public event Action<ExploreData.Route> Traveled;

	private void Awake()
	{
		Connections.Frontend.On<Routes>(OnRoutes);
		Connections.Frontend.On<RegionExpirationAlarm>(OnRegionExpirationAlarm);
		Connections.Frontend.On<RegionExpired>(OnRegionExpiration);
		Connections.Frontend.On<RegionMovedByExpiration>(OnRegionMovedByExpiration);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.SailingWithdraw, delegate(InteractionObject target)
		{
			UIManager.MessageBox.Show(T._("출발했던 항구로 돌아가시겠습니까?"), delegate(bool ok)
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				if (ok)
				{
					Connections.Frontend.Send(new Withdraw
					{
						EntityId = target.EntityId,
						Tile = new Point2(target.Tile)
					});
				}
			});
		});
	}

	public void TravelRegion(Port port, ExploreData.Route route)
	{
		Connections.Frontend.Send(new TravelByRegion
		{
			EntityId = port.Id,
			Tile = port.Tile,
			RegionId = route.Region.Id
		});
		if (this.Traveled != null)
		{
			this.Traveled(route);
		}
	}

	public void RecommendRegion(Port port, string templateId)
	{
		Connections.Frontend.Send(new RecommendRegion
		{
			EntityId = port.Id,
			Tile = port.Tile,
			TemplateId = templateId
		}).On<Messages.Region>(OnFoundRegion).On<Error>(NotFoundRegion);
	}

	private void OnFoundRegion(Messages.Region region, PacketHeader header)
	{
		LastFoundRegion = region.Id;
		if (this.FoundRegion != null)
		{
			this.FoundRegion(new ExploreData.Region(region));
		}
	}

	private void NotFoundRegion(Error msg, PacketHeader header)
	{
		GameManager.DefaultErrorHandler(msg, header);
		if (this.FoundRegion != null)
		{
			this.FoundRegion(null);
		}
	}

	private void OnRoutes(Routes routes, PacketHeader header)
	{
		if (this.RoutesUpdated != null)
		{
			this.RoutesUpdated(routes);
		}
	}

	public void RequestPort(ulong entityId, Point2 tile)
	{
		Connections.Frontend.Send(new GetRoutes
		{
			EntityId = entityId,
			Tile = tile
		});
	}

	private void OnRegionExpirationAlarm(RegionExpirationAlarm msg, PacketHeader header)
	{
		int num = (int)msg.After;
		string comment;
		if (num >= 60)
		{
			num /= 60;
			comment = T._("#{0}초 후 섬이 사라집니다.", num);
		}
		else
		{
			comment = T._("{0}분 후 섬이 사라집니다.", num);
		}
		UIManager.SystemMsg(comment, 4f);
	}

	private void OnRegionExpiration(RegionExpired msg, PacketHeader header)
	{
		UIManager.SystemMsg(T._("섬이 곧 사라집니다. {0}초 후 집으로 이동합니다.", ((int)msg.After).ToString()), 4f);
	}

	private void OnRegionMovedByExpiration(RegionMovedByExpiration msg, PacketHeader header)
	{
		UIManager.MessageBox.Show(T._("섬이 사라져 집으로 돌아왔습니다."));
	}
}
