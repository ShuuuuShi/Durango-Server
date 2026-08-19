using Messages;
using Shared.Etc;
using UnityEngine;

public class ArtifactComponent
{
	public Artifact Artifact { get; private set; }

	public string EntityId => Artifact.EntityId;

	public Point2 WorldTile => Artifact.WorldTile;

	public Point2 Size => Artifact.Size;

	public ModelComponent Models => Artifact.Models;

	protected virtual string ConsiteAssetPath => null;

	protected virtual string ScaffoldingAssetPath => null;

	protected virtual Artifact.Interaction InteractionType => Artifact.Interaction.Normal;

	public virtual string ContextIcon => null;

	public virtual int Height => 0;

	public virtual Vector3 InteractionPositionOffset => Vector3.zero;

	public void SetParent(Artifact artifact)
	{
		Artifact = artifact;
		if (!string.IsNullOrEmpty(ConsiteAssetPath))
		{
			Artifact.ConsiteAssetPath = ConsiteAssetPath;
		}
		if (!string.IsNullOrEmpty(ScaffoldingAssetPath))
		{
			Artifact.ScaffoldingAssetPath = ScaffoldingAssetPath;
		}
		Artifact.InteractionType = InteractionType;
		if (string.IsNullOrEmpty(Artifact.ContextIcon) && !string.IsNullOrEmpty(ContextIcon))
		{
			Artifact.ContextIcon = ContextIcon;
		}
	}

	public virtual void PreInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
	}

	public virtual void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
	}

	public virtual void Update()
	{
	}

	public virtual bool OnSelectArtifact(bool isSelect)
	{
		return false;
	}

	public virtual void OnUpdateCollider()
	{
	}

	public virtual bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		return false;
	}

	public virtual bool OnUpdateState(double eventTime)
	{
		return false;
	}

	public virtual string GetName()
	{
		return null;
	}

	public virtual void OnCompleted()
	{
	}

	public virtual void OnRemoved()
	{
	}

	public virtual void ArtifactPlaced()
	{
	}

	public virtual void ResourcesLoadCompleted()
	{
	}

	public virtual void OnUpdateBuildState()
	{
	}

	public virtual void OnPlayerEnter()
	{
	}

	public virtual void OnPlayerExit()
	{
	}

	public virtual void OnPlayerFloorChange()
	{
	}

	public virtual void OnChangeInterior()
	{
	}

	public virtual bool IsIgnoreWaterDepth()
	{
		return false;
	}

	public virtual Color GetColor()
	{
		return Color.white;
	}
}
