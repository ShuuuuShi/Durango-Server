namespace NPA;

public interface INPEndingBannerListener : INPListenerType
{
	void OnEndingBannerClick(string landInfo);

	void OnEndingBannerFailed(NPResult npResult);

	void OnEndingBannerDismiss();

	void OnEndingBannerExit();
}
