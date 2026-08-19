using Durango.Terrain;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetCageRegionInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _regionLabel;

	[SerializeField]
	private UISprite _regionSprite;

	[SerializeField]
	private UISprite _biomeSprite;

	private bool _isWait;

	private CageInfo _cage;

	private void Start()
	{
		_titleLabel.text = T._("축사 위치");
	}

	public void Set(CageInfo cage)
	{
		_cage = cage;
		_isWait = true;
		GameSystem<MapSystem>.Instance().GetRegion(cage.RegionId, SetRegion);
		Vector2 vector = MapPositionParser.PositionToHumaneTile(Util.TilePositionToWorldPosition(cage.Tile));
		_regionLabel.text = $"[icon=icon_map_pinpoint] {cage.RegionName} ({vector.x:0}, {vector.y:0})";
		if (_isWait)
		{
			SetWaitRegion();
		}
	}

	private void SetWaitRegion()
	{
		_regionSprite.alpha = 0f;
		_biomeSprite.alpha = 0f;
	}

	private void SetRegion(Region region)
	{
		if (!(_cage.RegionId != region.Id))
		{
			_isWait = false;
			_regionSprite.alpha = 1f;
			_biomeSprite.alpha = 1f;
			RegionTemplate obj = ((region.TemplateId != null) ? SingletonDict<string, RegionTemplate>.Get(region.TemplateId) : null);
			RoutesViewer.RegionLayout regionLayout = RoutesViewer.BiomeLayouts.Get(obj?.MajorBiome() ?? Biome.Invalid);
			_regionSprite.color = regionLayout.Color;
			regionLayout.Sprite.Set(_biomeSprite);
		}
	}
}
