using System;
using Durango.Logic.Map;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.UI.InGame;

public class DetectWarpHoleUI : Singleton<DetectWarpHoleUI>
{
	private enum EnabledType
	{
		Scanner,
		NearbyMarker
	}

	[SerializeField]
	private GameObject _projectionObject;

	[SerializeField]
	private DetectWarpHoleScanner _scanner;

	[SerializeField]
	private DetectWarpHoleNearbyMarker _nearbyMarker;

	private Transform _transformCenter;

	private bool[] _enabledType;

	private void Start()
	{
		Singleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
	}

	private void Update()
	{
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		_transformCenter.localPosition = currentPosition;
		_scanner.UpdatePosition(currentPosition);
		_nearbyMarker.UpdatePosition(currentPosition);
	}

	public void ShowScanner(SearchResult[] results)
	{
		_scanner.Show(results, PlayerBehavior.LocalPlayer.CurrentPosition);
		Active(EnabledType.Scanner, active: true);
	}

	protected override void OnAwake()
	{
		_transformCenter = _projectionObject.transform;
		_scanner.Init();
		_scanner.Finished += OnScannerFinish;
		GameSystem<MapSystem>.Instance().NearbyPOIUpdated += DetectWarpHoleUI_NearbyArtifactUpdated;
		Array values = Enum.GetValues(typeof(EnabledType));
		_enabledType = new bool[values.Length];
	}

	private void Active(EnabledType type, bool active)
	{
		_enabledType[(int)type] = active;
		bool flag = false;
		for (int i = 0; i < _enabledType.Length; i++)
		{
			if (_enabledType[i])
			{
				flag = true;
				break;
			}
		}
		base.enabled = flag;
	}

	private void OnScannerFinish()
	{
		Active(EnabledType.Scanner, active: false);
	}

	private void DetectWarpHoleUI_NearbyArtifactUpdated(POIUpdater.NearbyPOI? nearbyPOI)
	{
		if (nearbyPOI.HasValue)
		{
			_nearbyMarker.Show(nearbyPOI.Value.Type, nearbyPOI.Value.Position);
			_nearbyMarker.UpdatePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
			Active(EnabledType.NearbyMarker, active: true);
		}
		else
		{
			_nearbyMarker.Hide();
			Active(EnabledType.NearbyMarker, active: false);
		}
	}

	private void PlayerController_MoveStarted()
	{
		if (_scanner.IsShow)
		{
			_scanner.Hide();
		}
	}
}
