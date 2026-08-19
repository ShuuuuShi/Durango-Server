using System.Collections.Generic;
using Durango.Logic.PlayGuide;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorMissionDelivery : Locator
{
	private readonly LocatorInteraction _interaction = new LocatorInteraction();

	private DeliveryGroup _deliveryGroup;

	private bool _isDeliveryPhase;

	public override void Initialize(Dictionary<string, Parameter> dict)
	{
		_interaction.Initialize(dict);
		base.Initialize(dict);
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_deliveryGroup = UIManager.FindScript<DeliveryGroup>();
	}

	protected override string SelectPhase()
	{
		_interaction.Process();
		if (_deliveryGroup != null && _deliveryGroup.IsOpened)
		{
			_isDeliveryPhase = true;
			if (_deliveryGroup.CanDeliver)
			{
				return "delivery_button";
			}
			return "select_item";
		}
		_isDeliveryPhase = false;
		return _interaction.CurrentPhase;
	}

	protected override void UpdateTargetTransform()
	{
		if (_isDeliveryPhase)
		{
			string currentPhase = base.CurrentPhase;
			if (!(currentPhase == "select_item"))
			{
				if (currentPhase == "delivery_button")
				{
					base.TargetTransform = _deliveryGroup.GetConfirmButtonTransform();
				}
				else
				{
					base.UpdateTargetTransform();
				}
			}
			else
			{
				base.TargetTransform = _deliveryGroup.GetSelectableItemTranform();
			}
		}
		else
		{
			base.TargetTransform = _interaction.TargetTransform;
			base.CurrentParameter = _interaction.CurrentParameter;
		}
	}
}
