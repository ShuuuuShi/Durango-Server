using System.Collections;
using System.Collections.Generic;
using Messages;
using Shared.Etc;
using UnityEngine;

public class Bridge : ArtifactComponent
{
	private const string BridgeKey = "bridge";

	private static readonly Point2[] Dirs = new Point2[4]
	{
		Point2.left,
		Point2.down,
		Point2.right,
		Point2.up
	};

	private bool _isBegin;

	private readonly bool[] _neighborhood = new bool[4];

	private int _neighborhoodCount;

	private string _modelKey;

	private readonly float[] _depth = new float[4];

	private float _height;

	private readonly List<BoxCollider> _fences = new List<BoxCollider>();

	private bool _isDirty;

	protected override bool InteractionDisabled => true;

	public override void OverrideDepth(ref byte floor, ref float depth00, ref float depth10, ref float depth01, ref float depth11)
	{
		bool flag = false;
		if (_neighborhoodCount == 0)
		{
			flag = base.Artifact.Blueprint.Floor == 0;
			floor = 0;
		}
		else if (base.Artifact.Blueprint.Floor > 0)
		{
			if (floor > 0 || _isBegin)
			{
				floor = 1;
				flag = true;
			}
			else
			{
				floor = 0;
			}
		}
		else
		{
			floor = 0;
			flag = true;
		}
		if (flag)
		{
			float num = (0f - _height) / 200f;
			depth00 = _depth[0] * num;
			depth10 = _depth[1] * num;
			depth01 = _depth[2] * num;
			depth11 = _depth[3] * num;
		}
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		_modelKey = msg.Parts.Get("common");
		UpdateBridge();
		return true;
	}

	public override void OnCompleted()
	{
		UpdateBridge();
		UpdateNearBridges();
	}

	public override void OnRemoved()
	{
		UpdateNearBridges();
	}

	public override void ArtifactPlaced()
	{
		UpdateBridge();
		UpdateNearBridges();
	}

	private void UpdateNearBridges()
	{
		for (int i = 0; i < Dirs.Length; i++)
		{
			Point2 worldTile = base.Artifact.WorldTile + Dirs[i];
			TileObject tileObject = TerrainA6.GetTileObject(worldTile, warning: false);
			if (tileObject != null && !((Object)(object)tileObject.Artifact == (Object)null))
			{
				Bridge artifactComponent = tileObject.Artifact.GetArtifactComponent<Bridge>();
				if (artifactComponent != null && artifactComponent.Artifact.BuildCompleted)
				{
					artifactComponent.UpdateBridge();
				}
			}
		}
	}

	private int CheckNeighborhood()
	{
		int num = 0;
		for (int i = 0; i < Dirs.Length; i++)
		{
			_neighborhood[i] = false;
			Point2 worldTile = base.Artifact.WorldTile + Dirs[i];
			TileObject tileObject = TerrainA6.GetTileObject(worldTile);
			if (tileObject != null && !((Object)(object)tileObject.Artifact == (Object)null))
			{
				Bridge artifactComponent = tileObject.Artifact.GetArtifactComponent<Bridge>();
				if (artifactComponent != null && artifactComponent.Artifact.BuildCompleted && artifactComponent.Artifact.Blueprint.Floor != 0)
				{
					_neighborhood[i] = true;
					num++;
				}
			}
		}
		float tileMinDepth = TerrainA6.GetTileMinDepth(base.Artifact.WorldTile);
		TerrainWater.WaterDepthLevel waterDepthLevel = TerrainWater.GetWaterDepthLevel(tileMinDepth);
		_isBegin = num == 1 && waterDepthLevel <= TerrainWater.WaterDepthLevel.Foot;
		_neighborhoodCount = num;
		return num;
	}

	public void UpdateBridge()
	{
		if (!_isDirty)
		{
			_isDirty = false;
			((MonoBehaviour)base.Artifact).StartCoroutine(CoUpdateBridge());
		}
	}

	private IEnumerator CoUpdateBridge()
	{
		yield return null;
		LateUpdateBridge();
	}

	private void LateUpdateBridge()
	{
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		_isDirty = false;
		int num = CheckNeighborhood();
		Rotation rotation = Rotation.None;
		ModelComponent models = base.Artifact.Models;
		models.BeginLoad();
		int floor = base.Artifact.Blueprint.Floor;
		ModelComponent.IModel model;
		if (!base.Artifact.BuildCompleted)
		{
			model = models.PathLoad("bridge", base.Artifact.ScaffoldingAssetPath.Substring("Models/".Length));
		}
		else if (floor == 0)
		{
			model = models.Load("bridge", _modelKey, null);
		}
		else if (num == 0)
		{
			model = models.PathLoad("bridge", base.Artifact.ScaffoldingAssetPath.Substring("Models/".Length));
		}
		else
		{
			for (int i = 0; i < _depth.Length; i++)
			{
				_depth[i] = 1f;
			}
			if (_isBegin && floor > 0)
			{
				model = models.Load("bridge", _modelKey, "begin");
				for (int j = 0; j < _neighborhood.Length; j++)
				{
					if (_neighborhood[j])
					{
						rotation = (Rotation)j;
						break;
					}
				}
				switch (rotation)
				{
				case Rotation.None:
					_depth[1] = 0f;
					_depth[3] = 0f;
					break;
				case Rotation.Quarter:
					_depth[2] = 0f;
					_depth[3] = 0f;
					break;
				case Rotation.Half:
					_depth[0] = 0f;
					_depth[2] = 0f;
					break;
				case Rotation.ThreeQuarter:
					_depth[0] = 0f;
					_depth[1] = 0f;
					break;
				}
			}
			else
			{
				model = models.Load("bridge", _modelKey, null);
			}
		}
		Vector3 position = new Vector3((float)base.Artifact.Size.x, 0f, (float)base.Artifact.Size.y) * 200f * 0.5f;
		Vector3 angle = KUtility.DirectionToAngle(KUtility.RotationToDirection(rotation));
		model.SetPosition(position).SetAngle(angle);
		models.EndLoad();
		UpdateFence();
	}

	private void UpdateFence()
	{
		if (_fences.Count >= _neighborhood.Length)
		{
			for (int i = 0; i < _neighborhood.Length; i++)
			{
				((Collider)_fences[i]).enabled = !_neighborhood[i];
			}
		}
	}

	public override void ResourcesLoadCompleted()
	{
		GameObject @object = base.Artifact.Models.GetModel("bridge").GetObject();
		_height = 0f;
		_fences.Clear();
		if ((Object)(object)@object == (Object)null)
		{
			return;
		}
		BridgeModelInfo component = @object.GetComponent<BridgeModelInfo>();
		if (!((Object)(object)component == (Object)null))
		{
			_height = component.Height;
			GameObject fence = component.Fence;
			if (!((Object)(object)fence == (Object)null))
			{
				fence.GetComponents<BoxCollider>(_fences);
				UpdateFence();
			}
		}
	}
}
