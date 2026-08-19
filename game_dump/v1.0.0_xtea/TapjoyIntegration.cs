using TapjoyUnity;
using UnityEngine;

public class TapjoyIntegration : MonoBehaviour
{
	private static bool _applaunchPlacementShowed;

	private TJPlacement _applaunchPlacement;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		TJPlacement.OnRequestSuccess += new OnRequestSuccessHandler(HandlePlacementRequestSuccess);
		TJPlacement.OnRequestFailure += new OnRequestFailureHandler(HandlePlacementRequestFailure);
		TJPlacement.OnContentReady += new OnContentReadyHandler(HandlePlacementContentReady);
		TJPlacement.OnContentShow += new OnContentShowHandler(HandlePlacementContentShow);
		TJPlacement.OnContentDismiss += new OnContentDismissHandler(HandlePlacementContentDismiss);
		Tapjoy.OnConnectSuccess += new OnConnectSuccessHandler(OnConnectSuccess);
	}

	public static void TrackEvent(string category, string name, long value)
	{
		Tapjoy.TrackEvent(category, name, ToyLoginHelper.NPA, (string)null, value);
	}

	public static void TrackEvent(string name, long value)
	{
		Tapjoy.TrackEvent((string)null, name, ToyLoginHelper.NPA, (string)null, value);
	}

	private void OnConnectSuccess()
	{
		if (!_applaunchPlacementShowed)
		{
			_applaunchPlacement = TJPlacement.CreatePlacement("AppLaunch");
			if (_applaunchPlacement != null)
			{
				_applaunchPlacement.RequestContent();
			}
		}
	}

	private void HandlePlacementContentDismiss(TJPlacement placement)
	{
	}

	private void HandlePlacementContentShow(TJPlacement placement)
	{
	}

	private void HandlePlacementContentReady(TJPlacement placement)
	{
		if (placement.IsContentAvailable() && placement.GetName() == "AppLaunch")
		{
			placement.ShowContent();
			_applaunchPlacementShowed = true;
		}
	}

	private void HandlePlacementRequestFailure(TJPlacement placement, string error)
	{
	}

	private void HandlePlacementRequestSuccess(TJPlacement placement)
	{
		if (!placement.IsContentAvailable())
		{
		}
	}
}
