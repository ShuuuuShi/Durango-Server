using System;

namespace com.adjust.sdk;

public interface IAdjust
{
	bool isEnabled();

	void onPause();

	void onResume();

	void sendFirstPackages();

	void setEnabled(bool enabled);

	void setOfflineMode(bool enabled);

	void setDeviceToken(string deviceToken);

	void start(AdjustConfig adjustConfig);

	void trackEvent(AdjustEvent adjustEvent);

	string getIdfa();

	void setReferrer(string referrer);

	void getGoogleAdId(Action<string> onDeviceIdsRead);
}
