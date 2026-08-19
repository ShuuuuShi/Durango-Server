namespace NPA;

public interface INPBannerListener : INPListenerType
{
	void OnBannerClick(string landInfo);

	void OnBannerFailed(NPResult npResult);

	void OnBannerDismiss();
}
