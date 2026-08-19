using Durango.Logic.InputSystem;

namespace Durango.UI;

public class BuildGridGroup_PC : BuildGridGroupBase
{
	protected override void Start()
	{
		base.Start();
		UILabel componentInChildren = _confirmGridSelectionButton.GetComponentInChildren<UILabel>();
		if (componentInChildren != null)
		{
			componentInChildren.text = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(InputCommand.BuildGridActionOK, Layer.BuildGridUI);
		}
		UILabel componentInChildren2 = _rotatePreviewButton.GetComponentInChildren<UILabel>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.text = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(InputCommand.BuildGridActionRotation, Layer.BuildGridUI);
		}
		UILabel componentInChildren3 = _cancelGridSelectionButton.GetComponentInChildren<UILabel>();
		if (componentInChildren3 != null)
		{
			componentInChildren3.text = GameSystem<InputSystem>.Instance().Keyboard.GetKeyCaption(InputCommand.Back);
		}
		GameSystem<InputSystem>.Instance().On(InputCommand.BuildGridActionOK, OnActionOK);
		GameSystem<InputSystem>.Instance().On(InputCommand.BuildGridActionRotation, OnActionRotation);
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, OnActionCancel);
	}

	private void OnActionOK(InputCommandMessage message)
	{
		ConfirmGridSelection_OnClick();
	}

	private void OnActionRotation(InputCommandMessage message)
	{
		RotatePreview_OnClick();
	}

	private void OnActionCancel(InputCommandMessage message)
	{
		OnCanceled();
	}
}
