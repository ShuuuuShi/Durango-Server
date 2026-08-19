using Messages;
using UnityEngine;

public class DetectWarpHoleUI : KSingleton<DetectWarpHoleUI>
{
	[SerializeField]
	private GameObject _projectionObject;

	[SerializeField]
	private DetectWarpHoleScanner _scanner;

	[SerializeField]
	private DetectWarpHoleNearbyMarker _nearbyMarker;

	private Transform _transformCenter;

	private void Update()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		_transformCenter.localPosition = currentPosition;
		_scanner.UpdatePosition(currentPosition);
		_nearbyMarker.UpdatePosition(currentPosition);
	}

	public void ShowScanner(SearchResult[] results)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_scanner.Show(results, PlayerBehavior.LocalPlayer.CurrentPosition);
	}

	public void HideScanner()
	{
		_scanner.Hide();
	}

	protected override void OnAwake()
	{
		_transformCenter = _projectionObject.transform;
		_scanner.Init();
		GameSystem<MapSystem>.Instance().NearbyPOIUpdated += DetectWarpHoleUI_NearbyArtifactUpdated;
	}

	private void DetectWarpHoleUI_NearbyArtifactUpdated(POIUpdater.NearbyPOI? nearbyPOI)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (nearbyPOI.HasValue)
		{
			_nearbyMarker.Show(nearbyPOI.Value.Type, nearbyPOI.Value.Position);
			_nearbyMarker.UpdatePosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		}
		else
		{
			_nearbyMarker.Hide();
		}
	}
}
