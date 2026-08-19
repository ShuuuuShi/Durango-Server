using Messages;

public class Fence : ArtifactComponent
{
	private WallJointMaterial _materialType;

	public override void OnUpdateCollider()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.Artifact.CreateCollider();
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		string text = null;
		if (msg.Parts.ContainsKey("common"))
		{
			text = msg.Parts.Get("common");
		}
		else if (msg.Parts.ContainsKey("pillar"))
		{
			text = msg.Parts.Get("pillar");
		}
		if (!string.IsNullOrEmpty(text))
		{
			UpdateWallMaterial(text);
		}
		return false;
	}

	private void UpdateWallMaterial(string assetPath)
	{
		WallJointMaterial materialType = _materialType;
		_materialType = KSingleton<WallJointGridManager>.Instance().GetMaterialByPath(assetPath);
		if (materialType != _materialType)
		{
			AddWallJoint();
		}
	}

	private void AddWallJoint()
	{
		if (_materialType != 0)
		{
			KSingleton<WallJointGridManager>.Instance().AddWallJoint(base.Artifact.WorldTile, _materialType);
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		KSingleton<WallJointGridManager>.Instance().RemoveWallJoint(base.Artifact.WorldTile);
	}
}
