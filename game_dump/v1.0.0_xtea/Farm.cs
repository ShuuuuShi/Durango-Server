using Messages;
using Shared.Etc;
using TimerData;
using UnityEngine;

public class Farm : ArtifactComponent
{
	private tk2dSprite _cropSprite;

	private string _crop;

	private TimerData.Timer _growTimer;

	protected override bool HasShadow => false;

	public override void PreInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
	{
		InitCropSprite(size);
	}

	private void InitCropSprite(Point2 size)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("CropSprite");
		val.transform.parent = ((Component)base.Artifact).gameObject.transform;
		val.transform.localPosition = new Vector3((float)size.x, 0f, (float)size.y) * 200f * 0.5f;
		val.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
		val.transform.localScale = new Vector3(0.5f, 0.61f, 1f);
		_cropSprite = val.AddComponent<tk2dSprite>();
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
		if ((Object)(object)_cropSprite == (Object)null)
		{
			return false;
		}
		_crop = msg.Crop;
		if (string.IsNullOrEmpty(_crop))
		{
			((Component)_cropSprite).gameObject.SetActive(false);
			return false;
		}
		((Component)_cropSprite).gameObject.SetActive(true);
		SpriteCollectionInfo spriteCollectionInfo = KSingleton<SpriteManager>.Instance().GetSpriteCollectionInfo(_crop);
		if (spriteCollectionInfo == null)
		{
			return false;
		}
		if ((Object)(object)spriteCollectionInfo.SpriteCollectionData != (Object)null)
		{
			_cropSprite.SetSprite(spriteCollectionInfo.SpriteCollectionData, _crop);
		}
		else
		{
			spriteCollectionInfo.Loaded += SpriteCollectionsInfoLoaded;
		}
		return false;
	}

	private void SpriteCollectionsInfoLoaded(SpriteCollectionInfo info)
	{
		info.Loaded -= SpriteCollectionsInfoLoaded;
		if (!string.IsNullOrEmpty(_crop))
		{
			int spriteIdByName = info.SpriteCollectionData.GetSpriteIdByName(_crop, -1);
			if (info.SpriteCollectionData.IsValidSpriteId(spriteIdByName))
			{
				_cropSprite.SetSprite(info.SpriteCollectionData, spriteIdByName);
			}
		}
	}

	public override bool OnUpdateState(double eventAt)
	{
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		ArtifactState artifactState = base.Artifact.ArtifactState;
		Farming? farming = artifactState.Farming;
		if (!farming.HasValue)
		{
			if (_growTimer != null && !_growTimer.IsStop)
			{
				_growTimer.Stop();
			}
			return true;
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
			return true;
		}
		float num2 = (float)(growsUntil - num);
		float ratio = (float)(predictedServerTime - num) / num2;
		if (_growTimer == null || _growTimer.IsStop)
		{
			_growTimer = new TimerData.Timer(base.Artifact.EntityId, "grow", num2, ratio);
			TimerProgressGauge timerProgressGauge = TimerData.Timer.Play<TimerProgressGauge>(_growTimer);
			timerProgressGauge.SetTarget(((Component)base.Artifact).gameObject, new Vector3((float)(200 * base.Artifact.Size.y) * 0.5f, 50f, (float)(200 * base.Artifact.Size.x) * 0.5f));
		}
		else
		{
			_growTimer.SetDuration(base.Artifact.EntityId, "grow", num2, ratio);
		}
		return true;
	}
}
