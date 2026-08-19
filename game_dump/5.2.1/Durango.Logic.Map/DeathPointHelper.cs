using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.Logic.Map;

public class DeathPointHelper
{
	private const string DeathPointKey = "Points:Death";

	private bool _isLocalPlayerAlive;

	private Vector3? _deathPosition;

	private MapSystem _parent;

	private DelayedFunction _delayedUpdateDeathPoint;

	public void Init(MapSystem parent)
	{
		_parent = parent;
		_delayedUpdateDeathPoint = new DelayedFunction(RefreshDeathPoint, new WaitForSeconds(1f));
		GameSystem<MapSystem>.Instance().PointsUpdated += delegate
		{
			_delayedUpdateDeathPoint.Call(_parent);
		};
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += delegate(CharacterBehavior character)
		{
			bool isAlive = character.IsAlive;
			if (isAlive != _isLocalPlayerAlive)
			{
				_isLocalPlayerAlive = isAlive;
				_delayedUpdateDeathPoint.Call(_parent);
			}
		};
		Singleton<PlayerController>.Instance().MoveEnded += OnPlayerMoveEnded;
	}

	private void RefreshDeathPoint()
	{
		RegionTile? deathPoint = GameSystem<MapSystem>.Instance().Points.DeathPoint;
		NavigateGroup navigateGroup = UIManager.FindScript<NavigateGroup>();
		if (PlayerBehavior.LocalPlayer.IsAlive && deathPoint.HasValue && GameManager.Region.Id == deathPoint.Value.Region.Id)
		{
			_deathPosition = Util.TilePositionToClientPosition(deathPoint.Value.Tile, tileCenter: true);
			navigateGroup.Point.SetTarget("Points:Death", new PointTargetController.Arguments
			{
				Position = _deathPosition.Value,
				Icon = "todo_icon_player_dead",
				BorderColor = PresetColor.UIWhite
			});
		}
		else
		{
			_deathPosition = null;
			navigateGroup.Point.ClearTarget("Points:Death");
		}
	}

	private void OnPlayerMoveEnded()
	{
		if (_deathPosition.HasValue && !((_deathPosition.Value - PlayerBehavior.LocalPlayer.CurrentPosition).sqrMagnitude > 360000f))
		{
			_deathPosition = null;
			MapSystem.RemoveDeathPoint();
		}
	}
}
