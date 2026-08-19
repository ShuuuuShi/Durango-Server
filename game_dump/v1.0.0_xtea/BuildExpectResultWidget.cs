using Building_;
using ItemSystem;
using L10N;
using UnityEngine;

public class BuildExpectResultWidget : MonoBehaviour
{
	private const string Unknown = "?";

	[SerializeField]
	private UISprite _iconResult;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private UILabel _textDurability;

	[SerializeField]
	private UILabel _textBuildTime;

	private BuildSlotContainer _slotContainer;

	public void Set(BuildSlotContainer slotContainer)
	{
		_slotContainer = slotContainer;
	}

	public void Refresh()
	{
		BuildSlotContainer slotContainer = _slotContainer;
		Blueprint blueprint = slotContainer.Blueprint;
		IExpectedResultInfo expectedResult = slotContainer.ExpectedResult;
		ShowNameAndIcon(blueprint, expectedResult);
		ShowDurability(expectedResult);
		ShowBuildTime(blueprint);
	}

	private void ShowNameAndIcon(Blueprint blueprint, IExpectedResultInfo resultInfo)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string arg = NGUIText.EncodeColor(T.Format("{0:lv:}", (resultInfo == null) ? "?" : resultInfo.Level.ToString()), UIManager.UIYellow);
		UIUtility.SetSpriteName(_iconResult, (blueprint != null) ? blueprint.Icon : "icon_question");
		UIUtility.SetLabelText(_textName, (blueprint != null) ? $"{blueprint.LocalizedName} {arg}" : string.Empty);
	}

	private void ShowDurability(IExpectedResultInfo resultInfo)
	{
		string text = ((resultInfo == null) ? "?" : Util.LocalizedDurability(resultInfo.DurabilityMax, resultInfo.DurabilityMax));
		UIUtility.SetLabelText(_textDurability, text);
	}

	private void ShowBuildTime(Blueprint blueprint)
	{
		int num = blueprint.PostprocessTime / 60;
		int num2 = blueprint.PostprocessTime % 60;
		UIUtility.SetLabelText(_textBuildTime, $"{num:D2}:{num2:D2}");
	}
}
