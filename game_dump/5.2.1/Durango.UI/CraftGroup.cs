using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class CraftGroup : CraftGroupBase
{
	[SerializeField]
	private RectLayoutComponent _mainLayout;

	[SerializeField]
	private RecipeStepSelectWidget _recipeStepSelectHorizontalWidget;

	protected override RecipeStepSelectWidget RecipeStepSelectWidget
	{
		get
		{
			if (base.IsPortrait)
			{
				return _recipeStepSelectHorizontalWidget;
			}
			return _recipeStepSelectVerticalWidget;
		}
	}

	protected override bool TryOpen()
	{
		bool result = base.TryOpen();
		Singleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
		_mainLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		return result;
	}

	protected override bool TryClose()
	{
		if (Singleton<PlayerController>.HasInstance())
		{
			Singleton<PlayerController>.Instance().MoveStarted -= PlayerController_MoveStarted;
		}
		return base.TryClose();
	}

	public override void OnChangeScreenSize()
	{
		_recipeStepSelectHorizontalWidget.gameObject.SetActive(base.IsPortrait);
		_recipeStepSelectVerticalWidget.gameObject.SetActive(!base.IsPortrait);
	}

	private void PlayerController_MoveStarted()
	{
		Close();
	}
}
