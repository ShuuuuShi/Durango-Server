using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpGemRepairWidget : MonoBehaviour
{
	[SerializeField]
	private SelectableWidget _radioButton;

	[SerializeField]
	private UILabel _labelWarpGemRepair;

	private bool _initialized;

	private string _formatWarpGemRepair;

	public bool IsChecked
	{
		get
		{
			return _radioButton.Selected;
		}
		set
		{
			if (_radioButton.Selected != value)
			{
				_radioButton.Selected = value;
				if (this.RadioButtonStateChanged != null)
				{
					this.RadioButtonStateChanged();
				}
			}
		}
	}

	public event Action RadioButtonStateChanged;

	public void Init()
	{
		if (!_initialized)
		{
			_formatWarpGemRepair = _labelWarpGemRepair.text;
			SelectableWidget radioButton = _radioButton;
			radioButton.Clicked = (Action)Delegate.Combine(radioButton.Clicked, new Action(RadioButton_Clicked));
			UIEventListener.Get(_labelWarpGemRepair.gameObject).onClick = OnClick_LabelWarpGemRepair;
			_initialized = true;
		}
	}

	public void Refresh(RepairRequirement repairRequirement)
	{
		Refresh(repairRequirement.RepairPerformance);
	}

	public void Refresh([NotNull] Artifact artifact)
	{
		int repairRequirementPerformance = Singleton<Constants>.Instance.Repair.GetRepairRequirementPerformance(artifact.Blueprint.RepairRequirement, artifact.ArtifactState.Level);
		Refresh(repairRequirementPerformance);
	}

	private void Refresh(long warpGemPerformance)
	{
		_labelWarpGemRepair.text = T._(_formatWarpGemRepair, Durango.Logic.Item.Inventory.CurrencyFormat(warpGemPerformance, Currency.Gem));
		_radioButton.Selected = false;
	}

	private void RadioButton_Clicked()
	{
		IsChecked = !IsChecked;
	}

	private void OnClick_LabelWarpGemRepair(GameObject obj)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		IsChecked = !IsChecked;
	}
}
