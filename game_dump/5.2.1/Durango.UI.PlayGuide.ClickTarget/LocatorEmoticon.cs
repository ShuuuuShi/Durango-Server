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
		string currentPhase = base.CurrentPhase;
		if (!(currentPhase == "open_selector"))
		{
			if (currentPhase == "click_emoticon")
			{
				if (_menuWidget != null)
				{
					EmoticonWidget emoticonWidget = _menuWidget.FindEmoticonWidget(_emoticonId);
					base.TargetTransform = ((!(emoticonWidget != null)) ? null : emoticonWidget.transform);
				}
				else
				{
					base.TargetTransform = null;
				}
			}
		}
		else
		{
			base.TargetTransform = _menuWidget.CommunicationButton.transform;
		}
	}
}
