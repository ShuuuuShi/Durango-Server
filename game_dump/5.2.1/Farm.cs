using Durango.Logic.Timer;
using Durango.Network;
using Durango.Render.Particle;
using Durango.Render.Sprite;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Messages;
using Shared.Etc;
using UnityEngine;

public class Farm : ArtifactComponent
{
	private const string FertilizerParticlePath = "Particle/FX_Smoke_Fertilizer_01.prefab";

	private Durango.Render.Sprite.Sprite _cropSprite;

	private string _cropSpriteName;

	private Point2 _size;

	private Durango.Logic.Timer.Timer _growTimer;

	private int _fertilizerParticleId;

	private bool _fertilizedEnough;

	private bool FertilizedEnough
	{
		get
		{
			return _fertilizedEnough;
		}
		set
		{
			if (_fertilizedEnough != value)
			{
				_fertilizedEnough = value;
				if (_fertilizedEnough)
				{
					EmitFertilizerPaticle();
				}
				else
				{
					StopFertilizerParticle();
				}
			}
		}
	}

	public override void PreInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
		InitCropSprite(size);
	}

	private void InitCropSprite(Point2 size)
	{
		_cropSprite = Singleton<SpriteManager>.Instance().CreateSprite(base.Artifact.gameObject.transform, selectable: false);
		_size = size;
		_cropSprite.TransformUpdated += CropSprite_TransformUpdated;
	}

	public override string GetName()
	{
		if (!base.Artifact.ArtifactState.Farming.HasValue)
		{
			return null;
		}
		return base.Artifact.ArtifactState.Farming.Value.PlantName;
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		if (_cropSprite == null)
		{
			return false;
		}
		_cropSpriteName = msg.Crop;
		if (string.IsNullOrEmpty(_cropSpriteName))
		{
			_cropSprite.GameObject.SetActive(value: false);
			return false;
		}
		SpriteCollectionInfo spriteCollectionInfo = Singleton<SpriteManager>.Instance().GetSpriteCollectionInfo(_cropSpriteName);
		if (spriteCollectionInfo == null)
		{
			return false;
		}
		_cropSprite.SetSpriteByName(spriteCollectionInfo.SpriteObjectType, _cropSpriteName);
		spriteCollectionInfo.Loaded += SpriteCollectionsInfoLoaded;
		return false;
	}

	private void SpriteCollectionsInfoLoaded(SpriteCollectionInfo info)
	{
		info.Loaded -= SpriteCollectionsInfoLoaded;
		if (!(base.Artifact == null))
		{
			_cropSprite.CheckLoaded();
		}
	}

	public override void ArtifactPlaced()
	{
		UpdateSpriteTransformParams();
	}

	private void UpdateSpriteTransformParams()
	{
		TerrainChunk_Mobile terrainChunk_Mobile = Singleton<TerrainBase>.Instance().GetChunkFromTile(base.WorldTile) as TerrainChunk_Mobile;
		if (terrainChunk_Mobile != null)
		{
			Point2 point = terrainChunk_Mobile.FromWorldTile(base.WorldTile);
			terrainChunk_Mobile.SetSpriteTransformParams(_cropSprite, point.x, point.y, 0);
		}
		_cropSprite.UpdateTransform();
	}

	private void CropSprite_TransformUpdated()
	{
		_cropSprite.GameObject.transform.localPosition = new Vector3(_size.x, 0f, _size.y) * 200f * 0.5f;
	}

	public override bool OnUpdateState(double eventAt)
	{
		UpdateGrowTimer();
		UpdateFertilizedEnough();
		return false;
	}

	public override void OnRemoved()
	{
		StopFertilizerParticle();
		base.OnRemoved();
	}

	private void UpdateFertilizedEnough()
	{
		ArtifactState artifactState = base.Artifact.ArtifactState;
		Farming? farming = artifactState.Farming;
		FertilizedEnough = farming.HasValue && artifactState.Farming.Value.FertilizedRatio >= 1f;
	}

	private void EmitFertilizerPaticle()
	{
		if (_fertilizerParticleId == 0)
		{
			_fertilizerParticleId = ParticleManager.Emit("Particle/FX_Smoke_Fertilizer_01.prefab", base.Artifact.Center, Quaternion.identity, comeForwardToCamera: false, groundDecal: false, default(Vector3), reusable: true, limit: false);
		}
	}

	private void StopFertilizerParticle()
	{
		if (_fertilizerParticleId != 0)
		{
			ParticleManager.Stop(_fertilizerParticleId);
			_fertilizerParticleId = 0;
		}
	}

	private void UpdateGrowTimer()
	{
		ArtifactState artifactState = base.Artifact.ArtifactState;
		Farming? farming = artifactState.Farming;
		if (!farming.HasValue)
		{
			if (_growTimer != null && !_growTimer.IsStop)
			{
				_growTimer.Stop();
			}
			return;
		}
		Farming value = artifactState.Farming.Value;
		double num = value.PlantedAt;
		double growsUntil = value.GrowsUntil;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (num <= 0.0)
		{
			num = predictedServerTime;
		}
		if (growsUntil <= predictedServerTime || num >= growsUntil)
		{
			if (_growTimer != null && !_growTimer.IsStop)
			{
				_growTimer.Stop();
			}
			return;
		}
		float num2 = (float)(growsUntil - num);
		float ratio = (float)(predictedServerTime - num) / num2;
		if (_growTimer == null || _growTimer.IsStop)
		{
			_growTimer = new Durango.Logic.Timer.Timer(base.Artifact.EntityId, "grow", num2, ratio);
			Durango.Logic.Timer.Timer.Play<TimerProgressGauge>(_growTimer).SetTarget(base.Artifact.gameObject, new Vector3((float)(200 * base.Artifact.Size.y) * 0.5f, 50f, (float)(200 * base.Artifact.Size.x) * 0.5f));
		}
		else
		{
			_growTimer.SetDuration(base.Artifact.EntityId, "grow", num2, ratio);
		}
	}

	public override Color GetColor()
	{
		Farming? farming = base.Artifact.ArtifactState.Farming;
		if (farming.HasValue)
		{
			if (farming.Value.Water.x >= farming.Value.Water.y)
			{
				return new Color(0.6f, 0.6f, 0.6f);
			}
			return Color.white;
		}
		return Color.white;
	}
}
