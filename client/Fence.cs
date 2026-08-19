using System.Collections.Generic;
using Durango.Utils;
using Messages;

public class Fence : ArtifactComponent
{
	private WallJointMaterial _jointMaterial;

	public override void OnUpdateCollider()
	{
		base.Artifact.CreateCollider();
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		string model = null;
		string pattern = null;
		foreach (KeyValuePair<string, string> part in msg.Parts)
		{
			if (string.IsNullOrEmpty(part.Value))
			{
				continue;
			}
			model = part.Value;
			if (msg.Textures != null)
			{
				pattern = msg.Textures.Get(part.Key);
			}
			break;
		}
		_jointMaterial = new WallJointMaterial(model, pattern);
		UpdateWallJoint();
		return false;
	}

	public override void ArtifactPlaced()
	{
		UpdateWallJoint();
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		_jointMaterial = default(WallJointMaterial);
		UpdateWallJoint();
	}

	private void UpdateWallJoint()
	{
		Point2 worldTile = base.Artifact.WorldTile;
		Singleton<WallJointGridManager>.Instance().SetWallJoint(worldTile, _jointMaterial);
	}
}
