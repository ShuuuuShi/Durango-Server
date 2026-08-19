using System.Collections.Generic;
using Durango.Logic.PlayGuide;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorMissionStart : Locator
{
	private readonly LocatorInteraction _interaction = new LocatorInteraction();

	private MissionGroup _missionGroup;

	private bool _isMissionPhase;

	public override void Initialize(Dictionary<string, Parameter> dict)
	{
		_interaction.Initialize(dict);
		base.Initialize(dict);
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_missionGroup = UIManager.FindScript<MissionGroup>();
	}

	protected override string SelectPhase()
	{
		_interaction.Process();
		if (_missionGroup != null && _missionGroup.IsOpened)
		{
			_isMissionPhase = true;
			if (UIManager.Popup.FindTooltip<MissionInfoPopup>().IsVisible)
			{
				return "confirm_mission";
			}
			return "start_mission";
		}
		_isMissionPhase = false;
		return _interaction.CurrentPhase;
	}

	protected override void UpdateTargetTransform()
	{
		if (_isMissionPhase)
		{
			switch (base.CurrentPhase)
			{
			case "start_mission":
				base.TargetTransform = _missionGroup.GetStartButtonTransform();
				break;
			case "confirm_mission":
			{
				MissionInfoPopup missionInfoPopup = UIManager.Popup.FindTooltip<MissionInfoPopup>();
				base.TargetTransform = missionInfoPopup.ConfirmButton.transform;
				break;
			}
			default:
				base.UpdateTargetTransform();
				break;
			}
		}
		else
		{
			base.TargetTransform = _interaction.TargetTransform;
			base.CurrentParameter = _interaction.CurrentParameter;
		}
	}
}
