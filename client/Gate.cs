using System.Collections.Generic;
using Durango.Utils;
using Messages;
using UnityEngine;

public class Gate : ArtifactComponent
{
	private WallJointMaterial _jointMaterial;

	private bool _openStateInitialized;

	private bool _opened;

	private Animation _anim;

	private Animation Anim
	{
		get
		{
			if (_anim == null)
			{
				Animation componentInChildren = base.Artifact.gameObject.GetComponentInChildren<Animation>(includeInactive: true);
				if (componentInChildren != null)
				{
					_anim = componentInChildren;
				}
			}
			return _anim;
		}
	}

	public override void OnUpdateCollider()
	{
		base.Artifact.CreateCollider();
	}

	public override void ResourcesLoadCompleted()
	{
		UpdateOpenState();
		_openStateInitialized = true;
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

	public override bool OnUpdateState(double eventAt)
	{
		UpdateOpenState();
		return false;
	}

	private void UpdateWallJoint()
	{
		Point2 worldTile = base.Artifact.WorldTile;
		Singleton<WallJointGridManager>.Instance().SetWallJoint(worldTile, _jointMaterial);
		if (base.Size.x > 1)
		{
			Singleton<WallJointGridManager>.Instance().SetWallJoint(worldTile + new Point2(base.Size.x - 1, 0), _jointMaterial, banXAlign: true);
		}
		if (base.Size.y > 1)
		{
			Singleton<WallJointGridManager>.Instance().SetWallJoint(worldTile + new Point2(0, base.Size.y - 1), _jointMaterial, banXAlign: false, banYAlign: true);
		}
		if (base.Size.x > 1 && base.Size.y > 1)
		{
			Singleton<WallJointGridManager>.Instance().SetWallJoint(worldTile + new Point2(base.Size.x - 1, base.Size.y - 1), _jointMaterial, banXAlign: true, banYAlign: true);
		}
	}

	private void UpdateOpenState()
	{
		if (base.Artifact.ArtifactState.GateOpened)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	private void Open()
	{
		if (!_opened || !_openStateInitialized)
		{
			_opened = true;
			if (Anim != null)
			{
				Anim.Play("open_gate");
			}
		}
	}

	private void Close()
	{
		if (_opened || !_openStateInitialized)
		{
			_opened = false;
			if (Anim != null)
			{
				Anim.Play("close_gate");
			}
		}
	}
}
