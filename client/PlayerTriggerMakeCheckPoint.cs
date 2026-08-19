using Durango.Network;
using Durango.Terrain;
using Messages;
using UnityEngine;

public class PlayerTriggerMakeCheckPoint : PlayerTriggerBase
{
	protected override void DoTriggerEnter(Collider other)
	{
		Vector2 vector = Util.ClientPositionToTilePosition(other.transform.position);
		Connections.Frontend.Send(new SetReturningPoint
		{
			Tile = new Point2((int)vector.x, (int)vector.y)
		});
	}

	protected override void DoTriggerExit(Collider other)
	{
	}
}
