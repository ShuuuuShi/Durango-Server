using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Environment;

public class WindManager : Singleton<WindManager>
{
	private const int NearRange = 5;

	[SerializeField]
	private float _windPeriod = 15f;

	[SerializeField]
	private float _windForce = 0.1f;

	[SerializeField]
	private AnimationCurve _windCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _windTime = 10f;

	private SimpleTimer _windTimer;

	private void Start()
	{
		_windTimer = new SimpleTimer(_windPeriod + _windTime);
	}

	private void Update()
	{
		if (_windTimer.CheckTime())
		{
			RiseWind();
		}
	}

	[ExposedInEditor(null)]
	private void RiseWind()
	{
		SwayNearShrubs();
	}

	private void SwayNearShrubs()
	{
		Point2 currentTile = PlayerBehavior.LocalPlayer.CurrentTile;
		Point2 worldTile = new Point2(0, 0);
		for (int i = -5; i <= 5; i++)
		{
			for (int j = -5; j <= 5; j++)
			{
				worldTile.x = currentTile.x + i;
				worldTile.y = currentTile.y + j;
				TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(worldTile, warning: false);
				if (tileObject == null)
				{
					continue;
				}
				NaturalSpriteObject naturalSpriteObject = tileObject.NaturalObject as NaturalSpriteObject;
				if (!(naturalSpriteObject == null))
				{
					ShrubComponent shrubComponent = naturalSpriteObject.NaturalComponent as ShrubComponent;
					if (shrubComponent != null && IsSwayable(naturalSpriteObject))
					{
						shrubComponent.Sway(_windTime);
					}
				}
			}
		}
	}

	private bool IsSwayable(NaturalSpriteObject naturalSprite)
	{
		if (naturalSprite.Sprite != null)
		{
			return naturalSprite.Sprite.IsSwayable;
		}
		return false;
	}

	public float GetWindValue(float offset)
	{
		return _windCurve.Evaluate(offset) * _windForce;
	}
}
