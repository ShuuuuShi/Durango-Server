using Messages;
using Shared.System;

namespace Durango.Logic.Timeline;

public struct TimelineLog
{
	public struct ArtifactDigest
	{
		public string PrototypeId;

		public string EntityId;

		public string RegionId;

		public int[] Tile;

		public Messages.ArtifactDigest? ToArtifactDigest()
		{
			Point2 tile = ((Tile != null && Tile.Length < 2) ? new Point2(Tile[0], Tile[2]) : Point2.zero);
			Messages.ArtifactDigest value = default(Messages.ArtifactDigest);
			value.EntityId = EntityId;
			value.PrototypeId = PrototypeId;
			value.RegionId = RegionId;
			value.Tile = tile;
			return value;
		}
	}

	public TimelineEvent Type;

	public double At;

	public ArtifactDigest? TargetArtifact;

	public string TargetEntityId;

	public string AgentEntityId;

	public Gettext[] Params;
}
