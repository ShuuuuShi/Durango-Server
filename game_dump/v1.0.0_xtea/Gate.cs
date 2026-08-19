using System.Collections.Generic;
using Messages;
using Shared.Etc;
using UnityEngine;

public class Gate : ArtifactComponent
{
	private WallJointMaterial _materialType;

	private Point2[] _wallJoints;

	private bool _openStateInitialized;

	private bool _opened;

	private Animation _anim;

	private Animation Anim
	{
		get
		{
			if ((Object)(object)_anim == (Object)null)
			{
				Animation componentInChildren = ((Component)base.Artifact).gameObject.GetComponentInChildren<Animation>(true);
				if ((Object)(object)componentInChildren != (Object)null)
				{
					_anim = componentInChildren;
				}
			}
			return _anim;
		}
	}

	public override void PostInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
	{
		InitWallJointPoints();
	}

	private void InitWallJointPoints()
	{
		List<Point2> list = new List<Point2>();
		Point2 worldTile = base.Artifact.WorldTile;
		list.Add(worldTile);
		Point2 size = base.Artifact.Size;
		if (size.x > 1)
		{
			list.Add(worldTile + new Point2(size.x - 1, 0));
		}
		if (size.y > 1)
		{
			list.Add(worldTile + new Point2(0, size.y - 1));
		}
		if (size.x > 1 && size.y > 1)
		{
			list.Add(worldTile + new Point2(size.x - 1, size.y - 1));
		}
		_wallJoints = list.ToArray();
	}

	public override void OnUpdateCollider()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		base.Artifact.CreateCollider();
	}

	public override void ResourcesLoadCompleted()
	{
		UpdateOpenState();
		_openStateInitialized = true;
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		string text = msg.Parts.Get("common");
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
			AddWallJoints();
		}
	}

	private void AddWallJoints()
	{
		if (_materialType != 0)
		{
			int i = 0;
			for (int size = KUtility.GetSize(_wallJoints); i < size; i++)
			{
				KSingleton<WallJointGridManager>.Instance().AddWallJoint(_wallJoints[i], _materialType);
			}
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		int i = 0;
		for (int size = KUtility.GetSize(_wallJoints); i < size; i++)
		{
			KSingleton<WallJointGridManager>.Instance().RemoveWallJoint(_wallJoints[i]);
		}
	}

	public override bool OnUpdateState(double eventAt)
	{
		UpdateOpenState();
		return false;
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

	public void Open()
	{
		if (!_opened || !_openStateInitialized)
		{
			_opened = true;
			if ((Object)(object)Anim != (Object)null)
			{
				Anim.Play("open_gate");
			}
		}
	}

	public void Close()
	{
		if (_opened || !_openStateInitialized)
		{
			_opened = false;
			if ((Object)(object)Anim != (Object)null)
			{
				Anim.Play("close_gate");
			}
		}
	}
}
