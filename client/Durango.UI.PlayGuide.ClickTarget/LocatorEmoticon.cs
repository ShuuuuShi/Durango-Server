using Durango.Logic.PlayGuide;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorEmoticon : Locator
{
	private BottomMenuWidgetBase _menuWidget;

	private string _emoticonId;

	protected override void OnInitialized()
	{
		BottomLeftMenuGroupBase bottomLeftMenuGroupBase = UIManager.FindScript<BottomLeftMenuGroupBase>();
		if (bottomLeftMenuGroupBase != null)
		{
			_menuWidget = bottomLeftMenuGroupBase.BottomMenuWidget;
		}
		Parameter parameter = Parameters.Get("click_emoticon");
		if (parameter != null)
		{
			_emoticonId = parameter.id;
		}
	}

	protected override string SelectPhase()
	{
		if (_menuWidget != null && _menuWidget.IsEmotionSelectorVisible)
		{
			return "click_emoticon";
		}
		return "open_selector";
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "open_selector":
			base.TargetTransform = _menuWidget.CommunicationButton.transform;
			break;
		case "click_emoticon":
			if (_menuWidget != null)
			{
				EmoticonWidget emoticonWidget = _menuWidget.FindEmoticonWidget(_emoticonId);
				base.TargetTransform = ((!(emoticonWidget != null)) ? null : emoticonWidget.transform);
			}
			else
			{
				base.TargetTransform = null;
			}
			break;
		}
	}
}
