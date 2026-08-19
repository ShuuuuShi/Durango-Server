using L10N;

namespace Durango.UI;

public class BuildGridGroup : BuildGridGroupBase
{
	protected override void Start()
	{
		base.Start();
		_commentTitle = T._("건설 부지를 선택하세요.");
	}

	protected override void SetButtons(bool rotatable)
	{
		base.SetButtons(rotatable);
		if (rotatable)
		{
			_rotatePreviewButton.transform.localPosition = _buttonPositions[0];
			_confirmGridSelectionButton.transform.localPosition = _buttonPositions[1];
			_cancelGridSelectionButton.transform.localPosition = _buttonPositions[2];
		}
		else
		{
			_confirmGridSelectionButton.transform.localPosition = _buttonPositions[0];
			_cancelGridSelectionButton.transform.localPosition = _buttonPositions[2];
		}
	}
}
