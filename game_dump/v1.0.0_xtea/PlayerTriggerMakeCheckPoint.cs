using Messages;
using UnityEngine;

public class PlayerTriggerMakeCheckPoint : PlayerTriggerBase
{
	protected override void DoTriggerEnter(Collider other)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = TerrainA6.ClientPositionToTilePosition(((Component)other).transform.position);
		Connections.Frontend.Send(new SetReturningPoint
		{
			Tile = new Point2((int)val.x, (int)val.y)
		});
	}

	protected override void DoTriggerExit(Collider other)
	{
	}
}
