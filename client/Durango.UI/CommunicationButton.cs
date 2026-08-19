namespace Durango.UI;

public class CommunicationButton : CommunicationButtonBase
{
	private void OnClick()
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		if (_clicked != null)
		{
			_clicked();
		}
	}
}
