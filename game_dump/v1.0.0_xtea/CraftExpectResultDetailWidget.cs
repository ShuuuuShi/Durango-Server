using ItemSystem;
using UnityEngine;

public class CraftExpectResultDetailWidget : MonoBehaviour
{
	public const string Unknown = "?";

	[SerializeField]
	private UISprite _successRateBar;

	[SerializeField]
	private UILabel _textSuccessRate;

	[SerializeField]
	private UISprite _iconDurability;

	[SerializeField]
	private UILabel _textDurability;

	[SerializeField]
	private UISprite _iconModifiableCount;

	[SerializeField]
	private UILabel _textModifiableCount;

	[SerializeField]
	private Color[] _successRateTextColors;

	[SerializeField]
	private int[] _successRatePercentages;

	[SerializeField]
	private string[] _durabilityIconNames;

	[SerializeField]
	private int[] _durabilityPercentages;

	[SerializeField]
	private string _modifiableIconName;

	[SerializeField]
	private string _unmodifiableIconName;

	public void Show(IExpectedResultInfo resultInfo)
	{
		ShowSuccessRate(resultInfo);
		ShowDurability(resultInfo);
		ShowModifiableInfo(resultInfo);
	}

	private void ShowSuccessRate(IExpectedResultInfo resultInfo)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		string text = "?";
		if (resultInfo != null)
		{
			num = Mathf.CeilToInt(resultInfo.SuccessRate * 100f);
			text = $"{num}%";
		}
		UIUtility.SetLabelText(_textSuccessRate, text);
		Color valueByPercentage = UIUtility.GetValueByPercentage(num, _successRatePercentages, _successRateTextColors);
		_successRateBar.color = valueByPercentage;
		_textSuccessRate.color = valueByPercentage;
	}

	private void ShowDurability(IExpectedResultInfo resultInfo)
	{
		if (resultInfo != null)
		{
			float durabilityCurrent = resultInfo.DurabilityCurrent;
			float durabilityMax = resultInfo.DurabilityMax;
			int percentage = ((durabilityMax > 0f) ? Mathf.CeilToInt(durabilityCurrent / durabilityMax * 100f) : 0);
			UIUtility.SetSpriteName(_iconDurability, UIUtility.GetValueByPercentage(percentage, _durabilityPercentages, _durabilityIconNames));
			UIUtility.SetLabelText(_textDurability, Util.LocalizedDurability(durabilityCurrent, durabilityMax));
		}
		else
		{
			UIUtility.SetSpriteName(_iconDurability, _durabilityIconNames[0]);
			UIUtility.SetLabelText(_textDurability, "?");
		}
	}

	private void ShowModifiableInfo(IExpectedResultInfo resultInfo)
	{
		if (resultInfo != null)
		{
			UIUtility.SetSpriteName(_iconModifiableCount, (resultInfo.ModifiableCount <= 0) ? _unmodifiableIconName : _modifiableIconName);
			UIUtility.SetLabelText(_textModifiableCount, Util.LocalizedModifiableCount(resultInfo.ModifiableCount));
		}
		else
		{
			UIUtility.SetSpriteName(_iconModifiableCount, _modifiableIconName);
			UIUtility.SetLabelText(_textModifiableCount, "?");
		}
	}
}
