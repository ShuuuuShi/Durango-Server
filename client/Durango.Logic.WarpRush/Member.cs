using Durango.Network;

namespace Durango.Logic.WarpRush;

public class Member
{
	private double _expiresAt;

	public string EntityId { get; private set; }

	public Point2 Tile { get; private set; }

	public bool IsOffline
	{
		get
		{
			if (PlayerBehavior.LocalPlayer.EntityId == EntityId)
			{
				return false;
			}
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			return predictedServerTime >= _expiresAt;
		}
	}

	public Member(string entityId)
	{
		EntityId = entityId;
	}
}
