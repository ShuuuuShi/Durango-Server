using ExploreData;
using Messages;
using TerrainData;
using UnityEngine;

public class ExploreRegionNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _islandSprite;

	[SerializeField]
	private UILabel _lvLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private GameObject _newMaker;

	[SerializeField]
	private GameObject _homeSprite;

	private Vector3 _baseLvPos;

	private bool _isInit;

	public ExploreData.Route Route { get; private set; }

	private void Init()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_baseLvPos = ((Component)_lvLabel).transform.localPosition;
		}
	}

	public void Set(ExploreData.Route route)
	{
		Route = route;
		Set(route.Region);
	}

	public void Set(ExploreData.Region region)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Init();
		Biome biome = region.MajorBiome();
		RoutesViewer.RegionInfo regionInfo = RoutesViewer.RegionOptions[(int)(biome + 1)];
		Color color = regionInfo.Color;
		_islandSprite.color = color;
		_lvLabel.text = LocalizeUtil.FormatLevel(region.Level);
		_nameLabel.text = region.Name;
		_nameLabel.color = regionInfo.FontColor;
		if (string.IsNullOrEmpty(region.Name))
		{
			Vector3 baseLvPos = _baseLvPos;
			baseLvPos.y = ((Component)_nameLabel).transform.localPosition.y;
			((Component)_lvLabel).transform.localPosition = baseLvPos;
		}
		else
		{
			((Component)_lvLabel).transform.localPosition = _baseLvPos;
		}
		regionInfo.Icon.Set(_iconSprite);
		if ((Object)(object)_homeSprite != (Object)null)
		{
			EntityTile? homePoint = GameSystem<MapSystem>.Instance().Points.HomePoint;
			Messages.Region region2 = ((!homePoint.HasValue) ? GameSystem<MapSystem>.Instance().Points.ReturningPoint.Region : homePoint.Value.Region);
			_homeSprite.gameObject.SetActive(region2.Id == region.Id);
		}
		if ((Object)(object)_newMaker != (Object)null)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			_newMaker.gameObject.SetActive(predictedServerTime - region.CreatedAt < 3600.0);
		}
	}
}
