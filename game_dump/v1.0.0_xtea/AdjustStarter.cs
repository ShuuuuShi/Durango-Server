using UnityEngine;
using com.adjust.sdk;

public class AdjustStarter : MonoBehaviour
{
	public string appToken = "uct3l1zw6ozk";

	private void Start()
	{
		AdjustConfig adjustConfig = new AdjustConfig(appToken, AdjustEnvironment.Production);
		adjustConfig.setLogLevel(AdjustLogLevel.Info);
		adjustConfig.setSendInBackground(sendInBackground: false);
		adjustConfig.setEventBufferingEnabled(eventBufferingEnabled: false);
		adjustConfig.setLaunchDeferredDeeplink(launchDeferredDeeplink: true);
		Adjust.start(adjustConfig);
	}
}
