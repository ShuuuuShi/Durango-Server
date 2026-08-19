using Messages;
using Shared.Etc;
using UnityEngine;

public class ArtifactComponent
{
	public Artifact Artifact { get; private set; }

	public ulong EntityId => Artifact.EntityId;

	public Point2 WorldTile => Artifact.WorldTile;

	public Point2 Size => Artifact.Size;

	public ModelComponent Models => Artifact.Models;

	protected virtual string ConsiteAssetPath => null;

	protected virtual string ScaffoldingAssetPath => null;

	protected virtual bool InteractionDisabled => false;

	protected virtual bool HasShadow => true;

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
		Artifact.InteractionDisabled |= InteractionDisabled;
		Artifact.HasShadow &= HasShadow;
	}

	public virtual void PreInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
	{
	}

	public virtual void PostInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
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

	public virtual bool ShadowSkipFunction(MeshRenderer meshRenderer)
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

	public virtual void OverrideDepth(ref byte floor, ref float depth00, ref float depth10, ref float depth01, ref float depth11)
	{
	}
}
