using UnityEngine;

public class WindManager : KSingleton<WindManager>
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
				TileObject tileObject = TerrainA6.GetTileObject(worldTile, warning: false);
				if (tileObject != null && tileObject.TileType == TileObject.Type.NaturalObject)
				{
					NaturalObject naturalObject = tileObject.NaturalObject;
					ShrubComponent shrubComponent = ((!((Object)(object)naturalObject != (Object)null)) ? null : (naturalObject.NaturalComponent as ShrubComponent));
					if (shrubComponent != null && IsSwayable(naturalObject))
					{
						shrubComponent.Sway(_windTime);
					}
				}
			}
		}
	}

	private bool IsSwayable(NaturalObject naturalObject)
	{
		return TerrainDataHelper.GetBiomeSpriteInfo(naturalObject.EntityType)?.IsSwayable ?? false;
	}

	public float GetWindValue(float offset)
	{
		return _windCurve.Evaluate(offset) * _windForce;
	}
}
