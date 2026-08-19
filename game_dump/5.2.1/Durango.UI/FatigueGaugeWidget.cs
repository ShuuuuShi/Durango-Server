using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class FatigueGaugeWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _valueLabel;

	private int _maxValue;

	private int _currentValue;

	private void Update()
	{
		UpdateFatigue();
	}

	private void UpdateFatigue()
	{
		Fatigue fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		int num = Mathf.FloorToInt(fatigue.Max);
		int num2 = Mathf.FloorToInt(fatigue.Get());
		if (_maxValue != num || _currentValue != num2)
		{
			_maxValue = num;
			_currentValue = num2;
			_valueLabel.text = $"[FFD85B]{_currentValue}[-] [84847D]/[-] [E8E5DF]{_maxValue}[-]";
		}
	}
}
